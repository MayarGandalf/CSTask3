using MahApps.Metro.Controls;
using System.Windows;
using WpfApp3.ViewModels;

namespace WpfApp3
{
    public partial class MainWindow : MetroWindow
    {
        public MainViewModel ViewModel { get; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            DataContext = ViewModel;
        }
    }
}