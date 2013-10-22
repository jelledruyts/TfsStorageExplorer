using Microsoft.VisualStudio.Services.FileContainer;
using Microsoft.VisualStudio.Services.FileContainer.Client;
using System.Diagnostics;

namespace TfsStorageExplorer
{
    [DebuggerDisplay("Folder {FullPath}")]
    public class FolderTreeNode : ContainerItemTreeNode
    {
        public FolderTreeNode(FileContainerHttpClient service, FileContainerItem item)
            : base(service, item, "Resources/Folder.ico")
        {
        }
    }
}