using DAO;
using DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for StorageView.xaml
    /// </summary>
    public partial class StorageView : Page
    {
        public ObservableCollection<Album> albums { get; set; } = new();

        public ObservableCollection<Interpret> interprets { get; set; } = new();

        public StorageView()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        //Load functions
        private void Albums_Load(object sender, RoutedEventArgs e)
        {
            try
            {
                albums = new ObservableCollection<Album>(AlbumTable.Select());
            }
            catch (Exception)
            {
                MessageBox.Show("VPN not activated", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            DrawAlbumGrid();
        }

        private void DrawAlbumGrid()
        {
            ItemsDG.ItemsSource = albums;

            AlbumNameCol.Binding = new Binding(nameof(Album.Name));
            AlbumInterpretCol.Binding = new Binding("Interpret.Name");

            AlbumPriceCol.Binding = new Binding(nameof(Album.Current_price));
            AlbumPriceCol.Binding.StringFormat = "{0:C2}";

            AlbumQuantityCol.Binding = new Binding(nameof(Album.Available_quantity));
            AlbumQuantityCol.Binding.StringFormat = "{0:0} Ks";

            AlbumReleasedCol.Binding = new Binding(nameof(Album.Date_released));
            AlbumReleasedCol.Binding.StringFormat = "dd/MM/yyyy";

            ItemsDG.Visibility = Visibility.Visible;
            AlbumBtns.Visibility = Visibility.Visible;
        }

        //Albums CRUD
        private void Items_Add(object sender, RoutedEventArgs e)
        {
            Album added = new Album() { Album_id = albums.Count + 1 };
            AlbumEditView editwindow = new(added, add: true);
            editwindow.ShowDialog();

            if (added.Interpret_id > -1)
            {
                albums.Add(added);
            }
        }

        private void Items_Edit(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            Album? edited = btn.DataContext as Album;
            if(edited is not null)
            {
                AlbumEditView ew = new AlbumEditView(edited);
                ew.ShowDialog();
            }

            ItemsDG.Items.Refresh();
        }

        private void Item_Delete(object sender, RoutedEventArgs e)
        {
            MessageBoxResult msb = MessageBox.Show($"Do you want to delete {ItemsDG.SelectedItems.Count} elements?", "Info", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (msb == MessageBoxResult.Yes)
            {
                try
                {
                    Button? btn = sender as Button;
                    Album? deleted = btn.DataContext as Album;
                    int i = AlbumTable.DeleteAlbum(deleted.Album_id);
                    if(i > 0)
                    {
                        albums.Remove(deleted);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Deleting failed", "Deleting", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }


    }
}
