using System.Data.SqlClient;
using DTO;
using ds_orm;
using System.Data;

namespace DAO
{
	public class CustomerTable
	{
		//1.1. Nový zákazník
		public static String SQL_INSERT = @"insert into Customer (customer_id, [name], surname, email, address, city) values (@Customer_id, @Name, @Surname, @Email, @Address, @City);";
		//1.2. Seznam zákazníků
		public static String SQL_SELECT = @"select customer_id, name, surname, email, address, city from Customer";
		//1.3. Detail zákazníka
		public static String SQL_SELECT_ID = SQL_SELECT + @" WHERE customer_id = @Customer_id";
		//1.4. Aktualizace zákazníka
		public static String SQL_UPDATE = @"update Customer set [name]=@Name, surname=@Surname, email=@Email, address=@Address, city=@City WHERE customer_id=@Customer_id";

		//1.5. Smazání zákazníka – pouze pokud má všechny objednávky starší než půl roku, dojde na vyžádání k smazání účtu, všech objednávek a položek těchto objednávek
		public static String SQL_DELETE_ID = @"delete from Customer where customer_id=@Customer_id";

        public static int Insert(Customer e, Database? pDb = null)
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

        public static List<Customer> Select(Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);

            List<Customer> items = new();
            SqlCommand command = db.CreateCommand(SQL_SELECT);
            using (SqlDataReader reader = db.Select(command))
            {
                while (reader.Read())
                {
                    Customer e = new();
                    e.Customer_id = reader.GetInt32(reader.GetOrdinal("customer_id"));
                    e.Name = reader.GetString(reader.GetOrdinal("name"));
                    e.Surname = reader.GetString(reader.GetOrdinal("surname"));
                    e.Email = reader.GetString(reader.GetOrdinal("email"));
                    e.Address = reader.GetString(reader.GetOrdinal("address"));
                    e.City = reader.GetString(reader.GetOrdinal("city"));

                    items.Add(e);
                }
            }

            if (pDb == null) { db.Close(); }
            return items;
        }

        public static Customer SelectID(int id, Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);
            Customer e = new();
            using (SqlCommand command = db.CreateCommand(SQL_SELECT_ID))
            {
                command.Parameters.AddWithValue("@Customer_id", id);
                using (SqlDataReader reader = db.Select(command))
                {
                    if (reader.Read())
                    {
                        e.Customer_id = reader.GetInt32(reader.GetOrdinal("customer_id"));
                        e.Name = reader.GetString(reader.GetOrdinal("name"));
                        e.Surname = reader.GetString(reader.GetOrdinal("surname"));
                        e.Email = reader.GetString(reader.GetOrdinal("email"));
                        e.Address = reader.GetString(reader.GetOrdinal("address"));
                        e.City = reader.GetString(reader.GetOrdinal("city"));
                    }
                }

            }
            if (pDb == null) { db.Close(); }
            return e;
        }

        public static int Update(Customer e, Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);
            db.BeginTransaction();

            SqlCommand command = db.CreateCommand(SQL_UPDATE);
            BaseTable.PrepareCommand(command, e, e.GetType());
            int ret = db.ExecuteNonQuery(command);

            db.EndTransaction();
            if (pDb == null)
            {
                db.Close();
            }

            return ret;
        }

        public static int Delete(int id, Database pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);
            db.BeginTransaction();

            // 1.  create a command object identifying the stored procedure
            SqlCommand command = db.CreateCommand("PDeleteCustomer");

            // 2. set the command object so it knows to execute a stored procedure
            command.CommandType = CommandType.StoredProcedure;

            // 3. create input parameters
            SqlParameter input = new SqlParameter();
            input.ParameterName = "@customer_id";
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

    }
}
