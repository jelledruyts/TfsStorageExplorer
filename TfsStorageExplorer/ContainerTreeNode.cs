using Microsoft.VisualStudio.Services.FileContainer;
using Microsoft.VisualStudio.Services.FileContainer.Client;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace TfsStorageExplorer
{
    [DebuggerDisplay("Container {FullPath}")]
    public class ContainerTreeNode : StorageTreeNode
    {
        public FileContainer Container { get; private set; }

        public ContainerTreeNode(FileContainerHttpClient service, FileContainer container)
            : base(service, container.Name, "Resources/Container.ico", true)
        {
            this.Container = container;
            this.Description = container.Description;
            this.DateCreated = container.DateCreated;
            this.DateLastModified = this.DateCreated;
            this.Size = container.Size;
        }

        protected async override void LoadChildren()
        {
            MainWindowViewModel.Instance.StatusText = "Loading...";
            var items = await this.Service.QueryContainerItemsAsync(this.Container.Id);
            this.Children.Clear();
            var numFiles = 0;
            var numFolders = 0;
            foreach (var item in items.OrderBy(i => i.Path))
            {
                numFiles += (item.ItemType == ContainerItemType.File ? 1 : 0);
                numFolders += (item.ItemType == ContainerItemType.Folder ? 1 : 0);
                AddItem(item, 0);
            }
            this.IsLoading = false;
            MainWindowViewModel.Instance.StatusText = string.Format(CultureInfo.CurrentCulture, "Loaded {0} folder(s) and {1} file(s) for container \"{2}\"", numFolders, numFiles, this.Container.Name);
        }
    }
}