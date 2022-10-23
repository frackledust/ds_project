using System.Data.SqlClient;
using DTO;
using ds_orm;
using System.Data;

namespace DAO
{
	public class ItemTable
	{
        //transakce - 6.1. Přidání položky - kontrola stavu objednávky, zda na objednávce, přičtení kusů, jinak vytvoření, odečtení dostupných kusů na skladu
        //@purchase_id INT, @album_id INT, @add_quantity INT

        //6.2. Seznam položek objednávky
        public static String SQL_SELECT = @"SELECT purchase_id, album_id, quantity, price_per_item, date_added FROM Item";
		public static String SQL_SELECT_PURCHASE = SQL_SELECT + @" where purchase_id = @Purchase_id";
		public static String SQL_SELECT_ALBUM = SQL_SELECT + @" where album_id = @Album_id";

        //6.3. Přidání a odebrání počtu kusů - kontrola stavu objednávky, snížení počtu, navýšení počtu, dostupných kusů, pokud nastane počet kusů 0 pak smazání záznamu

        //transakce - 6.4. Hromadné mazání položek(kontrola stavu, odstranění položky, nastavení počtu kusů alba na nulu, vypsání seznamu uživatelů, kterým patřily objednávky)
        
        public static int AddItem(int purchase_id, int album_id, int add_quantity, Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);
            db.BeginTransaction();

            // 1.  create a command object identifying the stored procedure
            SqlCommand command = db.CreateCommand("PAddItem");

            // 2. set the command object so it knows to execute a stored procedure
            command.CommandType = CommandType.StoredProcedure;

            // 3. create input parameters
            SqlParameter input = new SqlParameter();
            input.ParameterName = "@album_id";
            input.DbType = DbType.Int32;
            input.Value = album_id;
            input.Direction = ParameterDirection.Input;
            command.Parameters.Add(input);

            input = new SqlParameter();
            input.ParameterName = "@purchase_id";
            input.DbType = DbType.Int32;
            input.Value = purchase_id;
            input.Direction = ParameterDirection.Input;
            command.Parameters.Add(input);

            input = new SqlParameter();
            input.ParameterName = "@add_quantity";
            input.DbType = DbType.Int32;
            input.Value = add_quantity;
            input.Direction = ParameterDirection.Input;
            command.Parameters.Add(input);

            // 4. execute procedure
            int ret = db.ExecuteNonQuery(command);

            db.EndTransaction();
            if (pDb == null) { db.Close(); }

            return ret;
        }

        public static List<Item> SelectByPurchase(int purchase_id, Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);

            List<Item> items = new();
            SqlCommand command = db.CreateCommand(SQL_SELECT_PURCHASE);
            command.Parameters.AddWithValue("@Purchase_id", purchase_id);
            using (SqlDataReader reader = db.Select(command))
            {
                while (reader.Read())
                {
                    Item e = new();
                    e.Purchase_id = reader.GetInt32(reader.GetOrdinal("Purchase_id"));
                    e.Album_id = reader.GetInt32(reader.GetOrdinal("Album_id"));
                    e.Quantity = reader.GetInt32(reader.GetOrdinal("Quantity"));
                    e.Price_per_item = reader.GetDecimal(reader.GetOrdinal("Price_per_item"));
                    e.Date_added = reader.GetDateTime(reader.GetOrdinal("Date_added"));

                    items.Add(e);
                }
            }

            if (pDb == null) { db.Close(); }
            return items;
        }
        public static List<Item> SelectByAlbum(int album_id, Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);

            List<Item> items = new();
            SqlCommand command = db.CreateCommand(SQL_SELECT_ALBUM);
            command.Parameters.AddWithValue("@Album_id", album_id);
            using (SqlDataReader reader = db.Select(command))
            {

                while (reader.Read())
                {
                    Item e = new();
                    e.Purchase_id = reader.GetInt32(reader.GetOrdinal("Purchase_id"));
                    e.Album_id = reader.GetInt32(reader.GetOrdinal("Album_id"));
                    e.Price_per_item = reader.GetDecimal(reader.GetOrdinal("Price_per_item"));
                    e.Date_added = reader.GetDateTime(reader.GetOrdinal("Date_added"));

                    items.Add(e);
                }
            }

            if (pDb == null) { db.Close(); }
            return items;
        }

        public static int ChangeItemQuantity(int purchase_id, int album_id, int new_quantity, Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);
            db.BeginTransaction();

            // 1.  create a command object identifying the stored procedure
            SqlCommand command = db.CreateCommand("PSubstractItem");

            // 2. set the command object so it knows to execute a stored procedure
            command.CommandType = CommandType.StoredProcedure;

            // 3. create input parameters
            SqlParameter input = new SqlParameter();
            input.ParameterName = "@album_id";
            input.DbType = DbType.Int32;
            input.Value = album_id;
            input.Direction = ParameterDirection.Input;
            command.Parameters.Add(input);

            input = new SqlParameter();
            input.ParameterName = "@purchase_id";
            input.DbType = DbType.Int32;
            input.Value = purchase_id;
            input.Direction = ParameterDirection.Input;
            command.Parameters.Add(input);

            input = new SqlParameter();
            input.ParameterName = "@new_quantity";
            input.DbType = DbType.Int32;
            input.Value = new_quantity;
            input.Direction = ParameterDirection.Input;
            command.Parameters.Add(input);

            // 4. execute procedure
            int ret = db.ExecuteNonQuery(command);

            db.EndTransaction();
            if (pDb == null) { db.Close(); }

            return ret;
        }

        public static void GroupItemsDelete(int album_id, Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);
            // 1.  create a command object identifying the stored procedure
            SqlCommand command = db.CreateCommand("PCancelOrders");

            // 2. set the command object so it knows to execute a stored procedure
            command.CommandType = CommandType.StoredProcedure;

            // 3. create input parameters
            SqlParameter input = new SqlParameter();
            input.ParameterName = "@album_id";
            input.DbType = DbType.Int32;
            input.Value = album_id;
            input.Direction = ParameterDirection.Input;
            command.Parameters.Add(input);

            // 4. execute procedure
            int ret = db.ExecuteNonQuery(command);

            if (pDb == null){   db.Close();}
        }
    }
}
