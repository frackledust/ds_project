using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ds_project_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        StorageView storageView { get; set; }

        CustomerView customerView { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            storageView = new StorageView();
            customerView = new CustomerView();
            MainFrame.Content = customerView;
        }

        private void Get_Storage_View(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = storageView;
        }

        private void Get_Customer_View(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = customerView;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ButtonWindowState_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Maximized)
                this.WindowState = WindowState.Maximized;
            else
                this.WindowState = WindowState.Normal;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}
