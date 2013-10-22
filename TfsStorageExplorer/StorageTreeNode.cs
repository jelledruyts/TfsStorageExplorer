using Microsoft.VisualStudio.Services.FileContainer;
using Microsoft.VisualStudio.Services.FileContainer.Client;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TfsStorageExplorer
{
    public abstract class StorageTreeNode : INotifyPropertyChanged
    {
        #region Constants

        private const char PathSeparator = '/';

        #endregion

        #region Fields

        private bool isExpanded;
        private bool isLoadNeeded;
        private bool isLoading;
        private ObservableCollection<StorageTreeNode> children;

        #endregion

        #region Properties

        protected FileContainerHttpClient Service { get; private set; }
        public string FullPath { get; private set; }
        public string Name { get; private set; }
        public string Description { get; protected set; }
        public DateTime DateCreated { get; protected set; }
        public DateTime DateLastModified { get; protected set; }
        public long Size { get; protected set; }
        public string Icon { get; private set; }
        public ObservableCollection<StorageTreeNode> Children
        {
            get
            {
                EnsureChildren();
                return this.children;
            }
        }
        public ObservableCollection<StorageTreeNode> NonFileChildrenLazy { get; private set; }

        public bool IsExpanded
        {
            get
            {
                return this.isExpanded;
            }
            set
            {
                if (value)
                {
                    EnsureChildren();
                }
                this.isExpanded = value;
            }
        }
        public bool IsLoading
        {
            get
            {
                return this.isLoading;
            }
            set
            {
                this.isLoading = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Helper Methods

        private void EnsureChildren()
        {
            if (this.isLoadNeeded)
            {
                this.isLoadNeeded = false;
                this.IsLoading = true;
                LoadChildren();
            }
        }

        public static string GetName(string fullPath)
        {
            return fullPath.Substring(fullPath.LastIndexOf(PathSeparator) + 1);
        }

        #endregion

        #region Constructors

        protected StorageTreeNode(FileContainerHttpClient service, string fullPath, string icon, bool isLazy)
        {
            this.Service = service;
            this.FullPath = fullPath;
            this.Icon = icon;
            this.Name = GetName(this.FullPath);
            this.children = new ObservableCollection<StorageTreeNode>();
            this.children.CollectionChanged += (sender, e) =>
            {
                this.NonFileChildrenLazy.Clear();
                foreach (var child in this.Children.Where(c => !(c is FileTreeNode)))
                {
                    this.NonFileChildrenLazy.Add(child);
                }
            };
            this.NonFileChildrenLazy = new ObservableCollection<StorageTreeNode>();
            if (isLazy)
            {
                this.isLoadNeeded = true;
                this.NonFileChildrenLazy.Add(new PlaceholderTreeNode(service));
            }
            else
            {
                this.isLoadNeeded = false;
            }
        }

        #endregion

        #region Protected Helper Methods

        protected virtual void LoadChildren()
        {
        }

        protected void AddItem(FileContainerItem item, int level)
        {
            // Find the folder that contains the path of the requested item (which can itself be a file or folder).
            var folderPath = item.ItemType == ContainerItemType.Folder ? item.Path : item.Path.Substring(0, item.Path.LastIndexOf(PathSeparator));
            var containingFolder = this.Children.SingleOrDefault(f => string.Equals(f.Name, folderPath.Split(PathSeparator)[level], StringComparison.OrdinalIgnoreCase));
            if (containingFolder == null)
            {
                // There is no containing folder yet, create one for the current item.
                containingFolder = new FolderTreeNode(this.Service, item);
                this.Children.Add(containingFolder);
            }

            // Check if the containing folder matches the requested folder path entirely.
            if (!string.Equals(containingFolder.FullPath, folderPath, StringComparison.OrdinalIgnoreCase))
            {
                // We don't have a full match yet, go deeper.
                containingFolder.AddItem(item, level + 1);
            }
            else
            {
                // We have a full match so we have the folder hierarchy done.
                // If we're adding a file, add it to the containing folder.
                if (item.ItemType == ContainerItemType.File)
                {
                    containingFolder.Children.Add(new FileTreeNode(this.Service, item));
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region PlaceholderTreeNode Class

        private class PlaceholderTreeNode : StorageTreeNode
        {
            public PlaceholderTreeNode(FileContainerHttpClient service)
                : base(service, "(Loading...)", null, false)
            {
            }
        }

        #endregion
    }
}