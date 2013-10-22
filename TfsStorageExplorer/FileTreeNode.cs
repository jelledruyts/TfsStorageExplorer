using Microsoft.VisualStudio.Services.FileContainer;
using Microsoft.VisualStudio.Services.FileContainer.Client;
using System.Diagnostics;

namespace TfsStorageExplorer
{
    [DebuggerDisplay("File {FullPath}")]
    public class FileTreeNode : ContainerItemTreeNode
    {
        public FileTreeNode(FileContainerHttpClient service, FileContainerItem item)
            : base(service, item, "Resources/File.png")
        {
        }
    }
}