using System.Data.SqlClient;
using DTO;
using ds_orm;
using System.Data;
using System.Reflection;
using System.Text;

namespace DAO
{
	public class AlbumTable
	{
		//4.1. Nové album
		public static String SQL_INSERT = @"insert into Album (album_id, name, description, date_released, current_price, available_quantity, interpret_id)
VALUES (@Album_id, @Name, @Description, @Date_released, @Current_price, @Available_quantity, @Interpret_id)";

		//4.2. Seznam alb
		public static String SQL_SELECT = @"select album_id, name, description, date_released, current_price, available_quantity, interpret_id FROM Album";
		//4.3. Detail alba
		public static String SQL_SELECT_ID = SQL_SELECT + " where @Album_id = album_id";
        //4.4. Aktualizace alba - pokud se mění cena, uvede se v historii změn

        //4.5. Smazání alba – smazání pouze pokud album není v žádné objednávce, jinak pouze nastavení počtu
        //kusů na nula, kaskádové mazání všech záznamů alba v historii změn

        //4.6. Zobrazení historie cen alba
        public static String SQL_HISTORY = @"select album_id, old_price, new_price, modified_at, employee_id FROM Album_history where @Album_id = album_id";

        //7.2. Filtrování alb – zákazník může dle parametrů vyfiltrovat potřebná alba

        public static int Insert(Album e, Database? pDb = null)
		{
			Database db = BaseTable.GetDatabase(pDb);
            db.BeginTransaction();

            SqlCommand command = db.CreateCommand(SQL_INSERT);
			BaseTable.PrepareCommand(command, e, e.GetType());
			int ret = db.ExecuteNonQuery(command);

            db.EndTransaction();
			if (pDb == null) { db.Close(); }

			return ret;
		}

        public static List<Album> Select(Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);

            List<Album> albums = new();
            SqlCommand command = db.CreateCommand(SQL_SELECT);
            using (SqlDataReader reader = db.Select(command))
            {
                while (reader.Read())
                {
                    Album e = new();

                    e.Album_id = reader.GetInt32(reader.GetOrdinal("album_id"));
                    e.Name = reader.GetString(reader.GetOrdinal("name"));
                    e.Date_released = reader.GetDateTime(reader.GetOrdinal("date_released"));
                    e.Current_price = reader.GetDecimal(reader.GetOrdinal("current_price"));
                    e.Available_quantity = reader.GetInt32(reader.GetOrdinal("available_quantity"));
                    e.Interpret_id = reader.GetInt32(reader.GetOrdinal("interpret_id"));

                    if (!reader.IsDBNull(reader.GetOrdinal("description")))
                    {
                        e.Description = reader.GetString(reader.GetOrdinal("description"));
                    }

                    albums.Add(e);
                }
            }

            if (pDb == null) { db.Close(); }
            return albums;
        }

        public static Album SelectID(int id, Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);
            Album e = new();
            using (SqlCommand command = db.CreateCommand(SQL_SELECT_ID))
            {
                command.Parameters.AddWithValue("@Album_id", id);
                using (SqlDataReader reader = db.Select(command))
                {
                    if (reader.Read())
                    {
                        e.Album_id = reader.GetInt32(reader.GetOrdinal("album_id"));
                        e.Name = reader.GetString(reader.GetOrdinal("name"));
                        e.Date_released = reader.GetDateTime(reader.GetOrdinal("date_released"));
                        e.Current_price = reader.GetDecimal(reader.GetOrdinal("current_price"));
                        e.Available_quantity = reader.GetInt32(reader.GetOrdinal("available_quantity"));
                        e.Interpret_id = reader.GetInt32(reader.GetOrdinal("interpret_id"));

                        if (!reader.IsDBNull(reader.GetOrdinal("description")))
                        {
                            e.Description = reader.GetString(reader.GetOrdinal("description"));
                        }
                    }
                }

            }
            if (pDb == null) { db.Close(); }
            return e;
        }

        public static int UpdateAlbum(int employee_id, Album e, Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);
            db.BeginTransaction();
            // 1.  create a command object identifying the stored procedure
            SqlCommand command = db.CreateCommand("PUpdateAlbum");

            // 2. set the command object so it knows to execute a stored procedure
            command.CommandType = CommandType.StoredProcedure;

            // 3. create input parameters
            SqlParameter input = new SqlParameter();
            input.ParameterName = "@employee_id";
            input.DbType = DbType.Int32;
            input.Value = employee_id;
            input.Direction = ParameterDirection.Input;
            command.Parameters.Add(input);

            foreach (PropertyInfo pi in typeof(Album).GetProperties())
            {
                if(pi.PropertyType.IsClass && pi.PropertyType != typeof(string)) { continue; }
                input = new SqlParameter();
                input.ParameterName = "@" + pi.Name;
                
                input.Value = pi.GetValue(e) == null ? DBNull.Value : pi.GetValue(e);
                input.Direction = ParameterDirection.Input;

                if(pi.PropertyType == typeof(int)) { input.DbType = DbType.Int32; }
                else if(pi.PropertyType == typeof(DateTime)) { input.DbType = DbType.DateTime; }
                else if(pi.PropertyType == typeof(Decimal)) { input.DbType = DbType.Decimal; }
                else { input.DbType = DbType.String; }

                command.Parameters.Add(input);
            }

            // 4. execute procedure
            int ret = db.ExecuteNonQuery(command);
            db.EndTransaction();
            if (pDb == null) { db.Close(); }

            return ret;
        }

        public static int DeleteAlbum(int id, Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);
            db.BeginTransaction();
            // 1.  create a command object identifying the stored procedure
            SqlCommand command = db.CreateCommand("PDeleteAlbum");

            // 2. set the command object so it knows to execute a stored procedure
            command.CommandType = CommandType.StoredProcedure;

            // 3. create input parameters
            SqlParameter input = new SqlParameter();
            input.ParameterName = "@album_id";
            input.DbType = DbType.Int32;
            input.Value = id;
            input.Direction = ParameterDirection.Input;
            command.Parameters.Add(input);

            // 4. execute procedure
            int ret = db.ExecuteNonQuery(command);
            db.EndTransaction();
            if (pDb == null) { db.Close(); }

            return ret;
        }

        public static string SelectHistoryByID(int id, Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);
            StringBuilder sb = new();
            using (SqlCommand command = db.CreateCommand(SQL_HISTORY))
            {
                command.Parameters.AddWithValue("@Album_id", id);
                using (SqlDataReader reader = db.Select(command))
                {
                    while (reader.Read())
                    {
                        //album_id, old_price, new_price, modified_at, employee_id
                        sb.Append(reader.GetDecimal(reader.GetOrdinal("old_price")).ToString());
                        sb.Append(" -> ");
                        sb.Append(reader.GetDecimal(reader.GetOrdinal("new_price")).ToString());
                        sb.Append(" changed at: ");
                        sb.Append(reader.GetDateTime(reader.GetOrdinal("modified_at")).ToString());
                        sb.Append(" by: ");
                        sb.Append(reader.GetInt32(reader.GetOrdinal("employee_id")).ToString());
                        sb.AppendLine();
                    }
                }

            }
            if (pDb == null) { db.Close(); }
            return sb.ToString();
        }

        public static List<string> Filter(int? customer_id = null,
            string? album_name = null,
            DateTime? released_after = null, DateTime? released_before = null,
            decimal? price_below = null,
            string? interpret_name = null, string? interpret_nationality = null,
            bool? not_bought = null, bool? only_available = null,
            Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);

            List<string> elements = new();
            string filter = GetFilterCommand(customer_id, album_name, released_after, released_before, price_below, interpret_name, interpret_nationality, not_bought, only_available);
            
            SqlCommand command = db.CreateCommand(filter);

            PrepareCommand(command, customer_id, album_name, released_after, released_before, price_below, interpret_name, interpret_nationality, not_bought, only_available);


            using (SqlDataReader reader = db.Select(command))
            {
                while (reader.Read())
                {
                    string e = reader.GetString(reader.GetOrdinal("album_name"));
                    e += reader.GetString(reader.GetOrdinal("interpret"));

                    e += reader.GetDecimal(reader.GetOrdinal("price")).ToString();
                    e += reader.GetString(reader.GetOrdinal("quantitystate"));
                    elements.Add(e);
                }
            }

            if (pDb == null) { db.Close(); }
            return elements;
        }

        public static List<Album> Filter2(int? customer_id = null,
            string? album_name = null,
            DateTime? released_after = null, DateTime? released_before = null,
            decimal? price_below = null,
            string? interpret_name = null, string? interpret_nationality = null,
            bool? not_bought = null, bool? only_available = null,
            Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);

            List<Album> elements = new();
            string filter = GetFilterCommand(customer_id, album_name, released_after, released_before, price_below, interpret_name, interpret_nationality, not_bought, only_available);

            SqlCommand command = db.CreateCommand(filter);

            PrepareCommand(command, customer_id, album_name, released_after, released_before, price_below, interpret_name, interpret_nationality, not_bought, only_available);


            using (SqlDataReader reader = db.Select(command))
            {
                while (reader.Read())
                {
                    Album e = new();
                    e.Album_id = reader.GetInt32(reader.GetOrdinal("album_id"));
                    e.Name = reader.GetString(reader.GetOrdinal("album_name"));
                    e.Interpret_id = reader.GetInt32(reader.GetOrdinal("interpret_id"));
                    e.Current_price = reader.GetDecimal(reader.GetOrdinal("price"));
                    e.Available_quantity = reader.GetInt32(reader.GetOrdinal("available_quantity"));
                    elements.Add(e);
                }
            }

            if (pDb == null) { db.Close(); }
            return elements;
        }

        private static void PrepareCommand(SqlCommand command, int? customer_id = null,
            string? album_name = null,
            DateTime? released_after = null, DateTime? released_before = null,
            decimal? price_below = null,
            string? interpret_name = null, string? interpret_nationality = null,
            bool? not_bought = null, bool? only_available = null,
            Database? pDb = null)
        {
            if (customer_id is not null) command.Parameters.AddWithValue("@Customer_id", customer_id);

            if (album_name != null) command.Parameters.AddWithValue("@Album_name", album_name);

            if (released_after != null) command.Parameters.AddWithValue("@Album_released_after", released_after);


            if (released_before != null) command.Parameters.AddWithValue("@Album_released_before", released_before);


            if (price_below != null) command.Parameters.AddWithValue("@Album_price_below", price_below);


            if (interpret_name != null) command.Parameters.AddWithValue("@Interpret_name", interpret_name);

            if (interpret_nationality != null) command.Parameters.AddWithValue("@Interpret_nationality", interpret_nationality);
        }

        private static string GetFilterCommand(int? customer_id = null,
            string? album_name = null,
            DateTime? released_after = null, DateTime? released_before = null,
            decimal? price_below = null,
            string? interpret_name = null, string? interpret_nationality = null,
            bool? not_bought = null, bool? only_available = null
            )
        {
            StringBuilder sb = new StringBuilder("SELECT ");
            sb.AppendLine("Album.album_id as album_id,");
            sb.AppendLine("Album.name as album_name,");
            sb.AppendLine("Album.available_quantity as available_quantity,");
            sb.AppendLine("Interpret.interpret_id as interpret_id,");
            sb.AppendLine("Interpret.name as interpret,");
            sb.AppendLine("Album.current_price as price,");
            sb.AppendLine("Case ");
            sb.AppendLine("WHEN Album.available_quantity = 0 THEN 'sold out'");
            sb.AppendLine("WHEN Album.available_quantity < 5 THEN('last ' + cast(Album.available_quantity as varchar(10)) + '!')");
            sb.AppendLine("ELSE 'stocked'");
            sb.AppendLine("END AS quantitystate");
            sb.AppendLine("FROM Album");
            sb.AppendLine("JOIN Interpret ON Album.interpret_id = Interpret.interpret_id");
            sb.AppendLine("WHERE ");

            if (not_bought == true && customer_id is not null)
            {
                sb.AppendLine("(NOT EXISTS (SELECT * FROM Item JOIN Purchase on Item.purchase_id = Purchase_purchase_id WHERE Album.album_id = Item.album_id AND Purchase.customer_id = @Customer_id))");
                sb.Append("AND ");
            }

            if(album_name != null)
            {
                sb.AppendLine("(Album.name LIKE '%' + @Album_name + '%')");
                sb.Append("AND ");
            }

            if(released_after != null)
            {
                sb.AppendLine("(Album.date_released > @Album_released_after)");
                sb.Append("AND ");
            }


            if (released_before != null)
            {
                sb.AppendLine("(Album.date_released < @Album_released_before)");
                sb.Append("AND ");
            }

            if (price_below != null)
            {
                sb.AppendLine("(Album.current_price < @Album_price_below)");
                sb.Append("AND ");
            }

            if (interpret_name != null)
            {
                sb.AppendLine("(Interpret.name LIKE '%' + @Interpret_name + '%')");
                sb.Append("AND ");
            }

            if (interpret_nationality != null)
            {
                sb.AppendLine("(Interpret.nationality LIKE '%' + @Interpret_nationality + '%')");
                sb.Append("AND ");
            }

            if(only_available == true)
            {
                sb.AppendLine("(Album.available_quantity > 0)");
                sb.Append("AND ");
            }

            sb.Append("1=1 ");

            sb.AppendLine("ORDER BY Album.date_released");

            return sb.ToString();
        }
    }
}
