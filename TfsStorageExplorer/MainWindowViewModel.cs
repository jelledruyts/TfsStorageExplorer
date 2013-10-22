using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.FileContainer.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using TfsStorageExplorer.Infrastructure;

namespace TfsStorageExplorer
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Fields

        private FileContainerHttpClient service;
        private string statusText;

        #endregion

        #region Properties

        public IList<string> AvailableTeamProjectCollectionUrls { get; private set; }
        public string TeamProjectCollectionUrl { get; set; }
        public bool IgnoreBuildLogs { get; set; }
        public ObservableCollection<StorageTreeNode> Nodes { get; private set; }
        public RelayCommand RefreshContainersCommand { get; private set; }
        public RelayCommand DownloadSelectedFilesCommand { get; private set; }
        public IList<StorageTreeNode> SelectedNodes { get; set; }
        public string StatusText { get { return this.statusText; } set { this.statusText = value; OnPropertyChanged(); } }

        #endregion

        #region Constructors

        private static MainWindowViewModel instance;
        public static MainWindowViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MainWindowViewModel();
                }
                return instance;
            }
        }

        private MainWindowViewModel()
        {
            this.AvailableTeamProjectCollectionUrls = RegisteredTfsConnections.GetProjectCollections().Select(c => c.Uri.ToString()).OrderBy(u => u).ToArray();
            this.TeamProjectCollectionUrl = this.AvailableTeamProjectCollectionUrls.FirstOrDefault();
            this.Nodes = new ObservableCollection<StorageTreeNode>();
            this.RefreshContainersCommand = new RelayCommand(RefreshContainers, CanRefreshContainers);
            this.DownloadSelectedFilesCommand = new RelayCommand(DownloadSelectedFiles, CanDownloadSelectedFiles);
            this.StatusText = "Ready";
        }

        #endregion

        #region RefreshContainers Command

        private bool CanRefreshContainers(object argument)
        {
            return !string.IsNullOrEmpty(this.TeamProjectCollectionUrl);
        }

        private async void RefreshContainers(object argument)
        {
            try
            {
                this.StatusText = "Loading...";
                var serverUri = new Uri(this.TeamProjectCollectionUrl);
                this.service = new FileContainerHttpClient(serverUri, new VssCredentials(true));
                var containers = await service.QueryContainersAsync(null);
                this.Nodes.Clear();
                foreach (var container in containers.OrderBy(c => c.Name))
                {
                    if (!IgnoreBuildLogs || !container.ArtifactUri.ToString().StartsWith("vstfs:///Build/Build/", StringComparison.OrdinalIgnoreCase))
                    {
                        this.Nodes.Add(new ContainerTreeNode(service, container));
                    }
                }
                this.StatusText = string.Format(CultureInfo.CurrentCulture, "Loaded {0} container(s) from \"{1}\"", this.Nodes.Count, serverUri.ToString());
            }
            catch (Exception exc)
            {
                HandleException(exc);
            }
        }

        #endregion

        #region DownloadSelectedFiles Command

        private bool CanDownloadSelectedFiles(object argument)
        {
            return this.SelectedNodes != null && this.SelectedNodes.Any() && this.SelectedNodes.All(n => n is FileTreeNode);
        }

        private async void DownloadSelectedFiles(object argument)
        {
            try
            {
                var files = this.SelectedNodes.OfType<FileTreeNode>().Select(n => n.Item).ToArray();
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.Description = "Please select the path where to export the Work Item Categories files (*.xml). They will be stored in a folder per Team Project.";
                var result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    this.StatusText = "Downloading...";
                    var rootFolder = dialog.SelectedPath;
                    var cancellationTokenSource = new CancellationTokenSource();
                    var cancellationToken = cancellationTokenSource.Token;
                    foreach (var file in files)
                    {
                        var fileName = Path.Combine(rootFolder, StorageTreeNode.GetName(file.Path));
                        using (var fileStream = await this.service.DownloadFileAsync(file.ContainerId, file.Path, cancellationToken))
                        using (var fileStreamWriter = File.OpenWrite(fileName))
                        {
                            await fileStream.CopyToAsync(fileStreamWriter);
                        }
                    }
                    this.StatusText = string.Format(CultureInfo.CurrentCulture, "Downloaded {0} file(s) to \"{1}\"", files.Length, rootFolder);
                }
            }
            catch (Exception exc)
            {
                HandleException(exc);
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

        #region Helper Methods

        private static void HandleException(Exception exc)
        {
            MessageBox.Show("An unexpected error occurred." + Environment.NewLine + Environment.NewLine + exc.Message, "Unexpected Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion
    }
}