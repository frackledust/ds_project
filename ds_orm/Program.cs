
using DAO;
using DTO;

namespace ds_orm
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Database db = new Database();
            db.Connect();

            //Resets database to default values before tests
            BaseTable.PrepareDatabase();

            CustomerTests(db);

            EmployeeTests(db);

            InterpretTests(db);

            AlbumTests(db);

            PurchaseTests(db);

            ItemTests(db);

            db.Close();
        }

        static void CustomerTests(Database? db = null)
        {
            int countbefore = CustomerTable.Select(db).Count;
            Console.WriteLine($"1.2 count: {countbefore}");

            int rows = CustomerTable.Insert(new Customer
            { Name = "Ferda", Surname = "Mravenec", Customer_id = 6, Address = "Na Paloučku 4", City = "Praha", Email = "ferda@vsb.cz" }, db);

            int countafter = CustomerTable.Select(db).Count;
            Console.WriteLine("1.1 rows: " + rows + " count: " + countafter);

            Customer inserted = CustomerTable.SelectID(6, db);
            Console.WriteLine("1.3 - " + inserted.Name);

            inserted.Name = "Beruška";
            rows = CustomerTable.Update(inserted, db);
            Console.WriteLine("1.4 rows: " + rows);

            CustomerTable.Delete(6, db);
            countafter = CustomerTable.Select(db).Count;
            Console.WriteLine("1.1 rows: " + rows + " count: " + countafter);
            Console.WriteLine();
        }

        static void EmployeeTests(Database? db = null)
        {
            int count = EmployeeTable.Select(db).Count;
            Console.WriteLine("2.2 - rows " + count);

            int rows = EmployeeTable.Insert(new Employee { Employee_id = 5, Name = "Ferda", Surname = "Mravenec" }, db);
            Console.WriteLine("2.1 - rows " + rows);

            var inserted = EmployeeTable.SelectID(5, db);
            inserted.Name = "Beruška";

            rows = EmployeeTable.Update(inserted, db);
            Console.WriteLine("2.3 - rows " + rows);
            Console.WriteLine();

        }

        static void InterpretTests(Database? db = null)
        {
            int rows = InterpretTable.Insert(new Interpret { Name = "SandWitch", Interpret_id = 5, Nationality = "czech" }, db);
            Console.WriteLine($"3.1 rows:{rows}");

            var inserted = InterpretTable.SelectID(5);
            Console.WriteLine($"3.2 Name: {inserted.Name}");

            inserted.Name = "Maniac";
            rows = InterpretTable.Update(inserted, db);
            Console.WriteLine($"3.3 rows:{rows} \n");

            var result = InterpretTable.Stats(pDb: db);
            Console.WriteLine(result);

            rows = InterpretTable.Delete(5, db);
            Console.WriteLine($"3.4 rows:{rows}");
            Console.WriteLine();
        }

        static void AlbumTests(Database? db = null)
        {
            int countbefore = AlbumTable.Select(db).Count;
            AlbumTable.Insert(new Album
            {
                Name = "Test",
                Album_id = 8,
                Available_quantity = 20,
                Current_price = 200,
                Date_released = DateTime.Now,
                Interpret_id = 2
            }, db);
            Album inserted = AlbumTable.SelectID(8, db);
            Console.WriteLine("4.1 - Add album");
            Console.WriteLine($"4.2 - #C before: {countbefore}");
            Console.WriteLine($"4.3 - #C after: {inserted.Album_id} - {inserted.Name}");

            inserted.Current_price = 300;
            AlbumTable.UpdateAlbum(1, inserted, db);

            string price_history = AlbumTable.SelectHistoryByID(8, db);
            Console.WriteLine("4.6 History: " + price_history);

            int done = AlbumTable.DeleteAlbum(8);
            int count = AlbumTable.Select(db).Count;
            Console.WriteLine($"4.5 Rows affected: {done} - Count albums: {count}");
            price_history = AlbumTable.SelectHistoryByID(8, db);
            Console.WriteLine(" History: " + price_history);

            var result = AlbumTable.Filter(interpret_name: "BTS").Count;
            Console.WriteLine("7.2 Filter: " + result);
            Console.WriteLine();
        }

        static void PurchaseTests(Database? db = null)
        {
            int count = PurchaseTable.Select(db).Count;
            Console.WriteLine($"5.2 - count: {count}");

            int rows = PurchaseTable.Insert(new Purchase { Customer_id = 1, Employee_id = 1, Purchase_id = 12, Status = 'P' }, db);
            Console.WriteLine($"5.1 - rows: {rows}");

            var inserted = PurchaseTable.Select(db, customer_id: 1).ToList();
            Console.WriteLine($"{inserted.Count} - {inserted[2].Status}");

            inserted[2].Status = 'S';
            PurchaseTable.Update(inserted[2], db);

            inserted = PurchaseTable.Select(db, customer_id: 1).ToList();
            Console.WriteLine($"5.3 {inserted.Count} - {inserted[2].Status}");

            rows = PurchaseTable.Delete(12, db);
            inserted = PurchaseTable.Select(db, customer_id: 1).ToList();
            Console.WriteLine($"5.4 rows: {rows} count: {inserted.Count}");
            Console.WriteLine();
        }

        static void ItemTests(Database? db = null)
        {
            int countbeforeP = ItemTable.SelectByPurchase(11, db).Count;

            ItemTable.AddItem(11, 7, 10, db);

            var inserted = ItemTable.SelectByPurchase(11, db);
            Console.WriteLine($"6.1 - #C before: {countbeforeP}");
            Console.WriteLine($"6.2 - #C after: {inserted.Count}");

            Console.WriteLine($"6.3 - quantity before: {inserted[0].Quantity}");
            ItemTable.ChangeItemQuantity(inserted[0].Purchase_id, inserted[0].Album_id, 5, db);
            inserted = ItemTable.SelectByPurchase(11, db);
            Console.WriteLine($"6.3 - quantity after: {inserted[0].Quantity}");

            int countbefore = ItemTable.SelectByAlbum(4, db).Count;

            ItemTable.GroupItemsDelete(4, db);

            int countafter = ItemTable.SelectByAlbum(4, db).Count;
            Console.WriteLine($"6.2 - #C before: {countbefore}");
            Console.WriteLine($"6.4 - #C after: {countafter}");
            Console.WriteLine();
        }
    }
}