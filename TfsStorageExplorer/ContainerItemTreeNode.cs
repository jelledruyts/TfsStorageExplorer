using Microsoft.VisualStudio.Services.FileContainer;
using Microsoft.VisualStudio.Services.FileContainer.Client;

namespace TfsStorageExplorer
{
    public class ContainerItemTreeNode : StorageTreeNode
    {
        public FileContainerItem Item { get; private set; }

        public ContainerItemTreeNode(FileContainerHttpClient service, FileContainerItem item, string icon)
            : base(service, item.Path, icon, false)
        {
            this.Item = item;
            this.DateCreated = item.DateCreated;
            this.DateLastModified = item.DateLastModified;
            this.Size = item.FileLength;
        }
    }
}