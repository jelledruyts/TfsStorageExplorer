using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TfsStorageExplorer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.ViewModel = MainWindowViewModel.Instance;
        }

        public MainWindowViewModel ViewModel
        {
            get { return (MainWindowViewModel)this.DataContext; }
            set { this.DataContext = value; }
        }

        private void childrenDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ViewModel.SelectedNodes = this.childrenDataGrid.SelectedItems.Cast<StorageTreeNode>().ToArray();
        }
    }
}