using System.Data.SqlClient;
using DTO;
using ds_orm;
using System.Data;

namespace DAO
{
    public class PurchaseTable
    {
        //5.1. Nová objednávka Zodpovědnost: zákazník
        public static String SQL_INSERT = @"insert into Purchase (purchase_id, [status], date_completed, employee_id, customer_id) values (@Purchase_id, @Status, @Date_completed, @Employee_id, @Customer_id);";
        //5.2. Seznam objednávek Zodpovědnost: zaměstnananci, zákazník pouze své
        public static String SQL_SELECT = @"select purchase_id, [status], date_completed, employee_id, customer_id FROM Purchase";
        public static String SQL_SELECT_CUSTOMER = " where customer_id =@Customer_id";
        //5.3. Aktualizace stavu objednávky Zodpovědnost: zaměstnanci, zákazník pouze své
        public static String SQL_UPDATE = @"update Purchase set [status]=@Status, date_completed=@Date_completed, employee_id=@Employee_id";
        //5.4. Smazání objednávky - kaskádové mazání všech položek objednávky pouze ve stavu S

        public static int Insert(Purchase e, Database? pDb = null)
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

        public static List<Purchase> Select(Database ? pDb = null, int? customer_id = null)
        {
            Database db = BaseTable.GetDatabase(pDb);

            List<Purchase> Purchases = new();
            string cmd = SQL_SELECT;
            if(customer_id != null) cmd += SQL_SELECT_CUSTOMER;
 
            SqlCommand command = db.CreateCommand(cmd);

            if (customer_id != null) command.Parameters.AddWithValue("@Customer_id", customer_id);

            using (SqlDataReader reader = db.Select(command))
            {
                while (reader.Read())
                {
                    Purchase e = new();
                    e.Purchase_id = reader.GetInt32(reader.GetOrdinal("Purchase_id"));
                    e.Status = reader.GetString(reader.GetOrdinal("Status"))[0];
                    e.Customer_id = reader.GetInt32(reader.GetOrdinal("Customer_id"));
                    e.Employee_id = reader.GetInt32(reader.GetOrdinal("Employee_id"));

                    if (!reader.IsDBNull(reader.GetOrdinal("Date_completed")))
                    {
                        e.Date_completed = reader.GetDateTime(reader.GetOrdinal("Date_completed"));
                    }

                    Purchases.Add(e);
                }
            }

            if (pDb == null) { db.Close(); }
            return Purchases;
        }

        public static int Update(Purchase e, Database? pDb = null)
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

        public static int Delete(int purchase_id, Database pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);
            db.BeginTransaction();

            SqlCommand command = db.CreateCommand("PDeletePurchase");

            // 2. set the command object so it knows to execute a stored procedure
            command.CommandType = CommandType.StoredProcedure;

            // 3. create input parameters
            SqlParameter input = new SqlParameter();
            input.ParameterName = "@purchase_id";
            input.DbType = DbType.Int32;
            input.Value = purchase_id;
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
