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
using System.Windows.Shapes;

namespace ds_project_wpf
{
    /// <summary>
    /// Interaction logic for AlbumEditView.xaml
    /// </summary>
    public partial class AlbumEditView : Window
    {
        private Album _old_album { get; set; } = new();
        private Album _new_album { get; set; } = new();

        private ObservableCollection<Interpret> interprets { get; set; } = new();

        private bool Add { get; set; }

        public AlbumEditView(Album album, bool add = false)
        {

            InitializeComponent();

            _old_album = album;
            Add = add;

            if (_old_album != null)
            {

                _new_album.Album_id = _old_album.Album_id;
                _new_album.Name = _old_album.Name;
                _new_album.Available_quantity = _old_album.Available_quantity;
                _new_album.Current_price = _old_album.Current_price;

                _new_album.Interpret_id = _old_album.Interpret_id;
                Interpret i = InterpretTable.SelectID(_old_album.Interpret_id);
                interprets.Add(i);

                _new_album.Date_released = _old_album.Date_released;
            }

            Panel_Interpret.ItemsSource = interprets;
            Panel_Interpret.DisplayMemberPath = "Name";

            this.DataContext = _new_album;
            Panel_Interpret.SelectedIndex = 0;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = Panel_Interpret.SelectedIndex;
                _new_album.Interpret_id = interprets[index].Interpret_id;

                int i = -1;
                if (Add)
                {
                    i = AlbumTable.Insert(_new_album);
                }
                else
                {
                    i = AlbumTable.UpdateAlbum(1, _new_album);
                }

                _old_album.Name = _new_album.Name;
                _old_album.Available_quantity = _new_album.Available_quantity;
                _old_album.Current_price = _new_album.Current_price;
                _old_album.Date_released = _new_album.Date_released;
                _old_album.Interpret_id = _new_album.Interpret_id;

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Saving failed.", "Save error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Interpret_DropDownOpened(object sender, EventArgs e)
        {
            if (interprets.Count <= 1)
            {
                interprets.Clear();
                try
                {
                    var temp = InterpretTable.Select();
                    foreach (Interpret item in temp)
                    {
                        interprets.Add(item);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
