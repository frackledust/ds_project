using DAO;
using ds_orm;
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
    /// Interaction logic for CustomerView.xaml
    /// </summary>
    public partial class CustomerView : Page
    {
        Customer customer = new();

        Album selected_album = new();

        public CustomerView()
        {
            Database db = new();
            db.Connect();
            InitializeComponent();
            customer = CustomerTable.SelectID(3);
            CustomerLabel.Content = customer.Name;
            PurchasePanel.ItemsSource = PurchaseTable.Select(customer_id: customer.Customer_id, pDb: db);

            //Items
            AlbumCol.Binding = new Binding("Album.Name");
            InterpretCol.Binding = new Binding("Album.Interpret.Name");
            QuantityCol.Binding = new Binding("Quantity");

            Album_AlbumCol.Binding = new Binding("Name");
            Album_InterpretCol.Binding = new Binding("Interpret.Name");
            Album_PriceCol.Binding = new Binding("Current_price");
            Album_PriceCol.Binding.StringFormat = "{0:C2}";

            Album_QuantityCol.Binding = new Binding("Available_quantity");
            Album_QuantityCol.Binding.StringFormat = "{0:0 ks}";
            AlbumsDG.ItemsSource = AlbumTable.Select(db);


            Load_Trends();
            db.Close();
        }

        private void Load_Trends()
        {
            var data = InterpretTable.Stats().Split('\n');
            string[] labels = new string[3];

            for (int i = 0; i < 3; i++)
            {
                var row = data[i].Split(';');
                labels[i] = row[0] + "\n" + row[1] + "\n" + row[4];
            }

            Trend1.Content = new TextBlock
            {
                Text = labels[1],
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            Trend2.Content = new TextBlock
            {
                Text = labels[0],
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            Trend3.Content = new TextBlock
            {
                Text = labels[2],
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

        }

        private void PurchasePanel_DropDownClosed(object sender, EventArgs e)
        {
            Purchase selected = PurchasePanel.SelectedItem as Purchase;
            if(selected != null)
            { 
                ItemsDG.ItemsSource = ItemTable.SelectByPurchase(selected.Purchase_id);
                ItemsDG.UpdateLayout();

                if (selected.Status != 'S')
                {
                    BtnCol.Visibility = Visibility.Hidden;
                    OrderPanel.Visibility = Visibility.Hidden;
                }
                else
                {
                    BtnCol.Visibility = Visibility.Visible;
                    OrderPanel.Visibility = Visibility.Visible;
                }
            }
        }

        private void New_Purchase(object sender, RoutedEventArgs e)
        {
            int ID = PurchaseTable.Select().Count() + 10;
            Purchase p = new Purchase
            {
                Purchase_id = ID,
                Customer_id = customer.Customer_id,
                Status = 'S',
                Employee_id = 1
            };
            PurchaseTable.Insert(p);

            PurchasePanel.ItemsSource = PurchaseTable.Select(customer_id: customer.Customer_id);
            PurchasePanel.UpdateLayout();
            PurchasePanel.SelectedIndex = PurchasePanel.Items.Count - 1;

            ItemsDG.ItemsSource = new List<Item>();
            ItemsDG.UpdateLayout();


            BtnCol.Visibility = Visibility.Visible;
            OrderPanel.Visibility = Visibility.Visible;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            Item? edited = btn.DataContext as Item;
            if (edited is not null)
            {

                Database db = new Database();
                db.Connect();

                edited.Quantity += 1;
                ItemTable.ChangeItemQuantity(edited.Purchase_id, edited.Album_id, edited.Quantity);
                
                Purchase selected = PurchasePanel.SelectedItem as Purchase;
                ItemsDG.ItemsSource = ItemTable.SelectByPurchase(selected.Purchase_id);
                ItemsDG.UpdateLayout();

                db.Close();
                Search(sender, e);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            Item? edited = btn.DataContext as Item;
            if (edited is not null)
            {
                Database db = new Database();
                db.Connect();

                edited.Quantity -= 1;
                ItemTable.ChangeItemQuantity(edited.Purchase_id, edited.Album_id, edited.Quantity);
                ItemsDG.UpdateLayout();

                Purchase selected = PurchasePanel.SelectedItem as Purchase;
                ItemsDG.ItemsSource = ItemTable.SelectByPurchase(selected.Purchase_id);
                ItemsDG.UpdateLayout();

                db.Close();
                Search(sender, e);
            }
        }

        private void Order_Purchase(object sender, RoutedEventArgs e)
        {
            Purchase selected = PurchasePanel.SelectedItem as Purchase;
            selected.Status = 'P';
            PurchaseTable.Update(selected);

            PurchasePanel.ItemsSource = PurchaseTable.Select(customer_id: customer.Customer_id);
            PurchasePanel.UpdateLayout();
            PurchasePanel.SelectedIndex = PurchasePanel.Items.Count - 1;

            BtnCol.Visibility = Visibility.Hidden;
            OrderPanel.Visibility = Visibility.Hidden;
        }

        private void Delete_Purchase(object sender, RoutedEventArgs e)
        {
            Purchase selected = PurchasePanel.SelectedItem as Purchase;
            PurchaseTable.Delete(selected.Purchase_id);
            MessageBox.Show("Purchase deleted.");

            PurchasePanel.ItemsSource = PurchaseTable.Select(customer_id: customer.Customer_id);
            PurchasePanel.UpdateLayout();

            BtnCol.Visibility = Visibility.Hidden;
            OrderPanel.Visibility = Visibility.Hidden;
        }

        private void Search(object sender, RoutedEventArgs e)
        {
            string? album_name = Filter_AlbumName.Text.Length > 0 ? Filter_AlbumName.Text : null;
            string? interpret_name = Filter_InterpretName.Text.Length > 0 ? Filter_InterpretName.Text : null;
            DateTime? date = Filter_DateAfter.Text.Length > 0 ? Filter_DateAfter.DisplayDate : null;
            string? nationality = Filter_InterpretNationality.Text.Length > 0 ? Filter_InterpretNationality.Text : null;
            string? price_val = Filter_MaxPrice.Text.Length > 0 ? (Filter_MaxPrice.Text) : null;

            decimal? price = null;
            if (price_val is not null)
            {
                price = Decimal.Parse(price_val);
            }


            AlbumsDG.ItemsSource = AlbumTable.Filter2(customer_id: customer.Customer_id,
                album_name: album_name,
                interpret_name: interpret_name,
                interpret_nationality: nationality,
                price_below: price,
                released_after: date,
                only_available: true);

            AlbumsDG.UpdateLayout();
            SelectedCol.Width = new GridLength(0);
        }

        private void AlbumsDG_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            selected_album = (Album) AlbumsDG.SelectedItem;

            if(selected_album != null)
            {
                SelectedCol.Width = new GridLength(2, GridUnitType.Star);
                SelectedTitle.Text = selected_album.Name.ToString() + " - " + selected_album.Interpret.Name.ToString();
                SelectedDescription.Text = selected_album.Description + " - " + selected_album.Interpret.Description;
                SelectedPrice.Text = String.Format("{0:C2}", selected_album.Current_price);
            }
            else
            {
                SelectedCol.Width = new GridLength(0);
            }
        }

        private void AddToPurchase(object sender, RoutedEventArgs e)
        {
            Purchase selected_purchase = PurchasePanel.SelectedItem as Purchase;

            if (selected_purchase is not null && selected_purchase.Status == 'S')
            {
                
                int row = ItemTable.AddItem(selected_purchase.Purchase_id, selected_album.Album_id, 1);

                ItemsDG.ItemsSource = ItemTable.SelectByPurchase(selected_purchase.Purchase_id);
                ItemsDG.UpdateLayout();

                Search(sender, e);
            }
        }

        private void SetTrend(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            if (btn != null)
            {
                TextBlock tb = btn.Content as TextBlock;
                var label_text = tb.Text.Split('\n');
                Filter_AlbumName.Text = label_text[2].Trim();
                Filter_InterpretName.Text = label_text[1].Trim();
            }
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
