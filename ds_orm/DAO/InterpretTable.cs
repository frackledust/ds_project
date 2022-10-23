using System.Data.SqlClient;
using DTO;
using ds_orm;
using System.Text;
using System.Data.SqlTypes;
using System.Data;

namespace DAO
{
	public class InterpretTable
	{
		//3.1. Nový interpret
		public static String SQL_INSERT = @"insert into Interpret (interpret_id, [name], nationality, [description]) values (@Interpret_id, @Name, @Nationality, @Description);";
        //3.2. Detail interpreta
        public static String SQL_SELECT = @"select interpret_id, name, nationality, description FROM Interpret";
        public static String SQL_SELECT_ID = SQL_SELECT + " WHERE interpret_id = @Interpret_id;";
		//3.3. Aktualizace interpreta
		public static String SQL_UPDATE = @"update Interpret set [name]=@Name, nationality=@Nationality, [description]=@Description WHERE interpret_id = @Interpret_id";
		//3.4. Smazání interpreta – pouze pokud jdou smazat všechna alba interpreta
		public static String SQL_DELETE_ID = @"delete from Interpret where interpret_id = @interpret_id";

        //transakce- 7.1. Statistika popularity interpretů – vygenerování listu nejprodávanějších interpretů

        public static int Insert(Interpret e, Database? pDb = null)
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

        public static List<Interpret> Select(Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);
            List<Interpret> result = new();
            using (SqlCommand command = db.CreateCommand(SQL_SELECT))
            {
                using (SqlDataReader reader = db.Select(command))
                {
                    while (reader.Read())
                    {
                        Interpret e = new();
                        e.Interpret_id = reader.GetInt32(reader.GetOrdinal("Interpret_id"));
                        e.Name = reader.GetString(reader.GetOrdinal("Name"));

                        if (!reader.IsDBNull(reader.GetOrdinal("Nationality")))
                        {
                            e.Nationality = reader.GetString(reader.GetOrdinal("Nationality"));
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("Description")))
                        {
                            e.Description = reader.GetString(reader.GetOrdinal("Description"));
                        }

                        result.Add(e);
                    }
                }

            }
            if (pDb == null) { db.Close(); }
            return result;
        }

        public static Interpret SelectID(int id, Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);
            Interpret e = new();
            using (SqlCommand command = db.CreateCommand(SQL_SELECT_ID))
            {
                command.Parameters.AddWithValue("@Interpret_id", id);
                using (SqlDataReader reader = db.Select(command))
                {
                    if (reader.Read())
                    {
                        e.Interpret_id = reader.GetInt32(reader.GetOrdinal("Interpret_id"));
                        e.Name = reader.GetString(reader.GetOrdinal("Name"));

                        if (!reader.IsDBNull(reader.GetOrdinal("Nationality")))
                        {
                            e.Nationality = reader.GetString(reader.GetOrdinal("Nationality"));
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("Description")))
                        {
                            e.Description = reader.GetString(reader.GetOrdinal("Description"));
                        }
                    }
                }

            }
            if (pDb == null) { db.Close(); }
            return e;
        }

        public static int Update(Interpret e, Database? pDb = null)
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

        public static int Delete(int interpret_id, Database pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);
            db.BeginTransaction();

            // 1.  create a command object identifying the stored procedure
            SqlCommand command = db.CreateCommand("PDeleteInterpret");

            // 2. set the command object so it knows to execute a stored procedure
            command.CommandType = CommandType.StoredProcedure;

            // 3. create input parameters
            SqlParameter input = new SqlParameter();
            input.ParameterName = "@interpret_id";
            input.DbType = DbType.Int32;
            input.Value = interpret_id;
            input.Direction = ParameterDirection.Input;
            command.Parameters.Add(input);

            // 4. execute procedure
            int ret = db.ExecuteNonQuery(command);

            db.EndTransaction();
            if (pDb == null) { db.Close(); }

            return ret;
        }

        public static string Stats(DateTime? date_from = null, DateTime? date_to = null, Database pDb = null)
        {
            string query = @"
                                        WITH albums_stats AS
                            (
                            SELECT Album.album_id, Album.name, Album.interpret_id,
                            SUM(Item.quantity) as item_quantity, SUM(Item.price_per_item * Item.quantity) as profit
                            FROM Item
                            JOIN Album ON Item.album_id = Album.album_id
                            WHERE Item.date_added > @Date_from AND Item.date_added < @Date_to
                            GROUP BY Album.album_id, Album.name, Album.interpret_id
                            )
                            SELECT Interpret.interpret_id,
                                Interpret.name as interpret_name,
                                SUM(albums_stats.item_quantity) as sold_albums,
                                SUM(albums_stats.profit) as total_profit,
                                (SELECT TOP 1[name]
                                 FROM albums_stats s1 where s1.interpret_id = Interpret.interpret_id
                                 ) as top_album
                            FROM Interpret
                            JOIN albums_stats ON albums_stats.interpret_id = Interpret.interpret_id
                            GROUP BY Interpret.interpret_id, Interpret.name
                            ORDER BY total_profit desc
                            ";

            Database db = BaseTable.GetDatabase(pDb);
            StringBuilder sb = new();

            if (date_from == null) date_from = new DateTime(2008, 1, 1);
            if (date_to == null) date_to = DateTime.Now;

            using (SqlCommand command = db.CreateCommand(query))
            {
                command.Parameters.AddWithValue("@Date_from", date_from);
                command.Parameters.AddWithValue("@Date_to", date_to);
                using (SqlDataReader reader = db.Select(command))
                {
                    int i = 1;
                    while (reader.Read())
                    {
                        sb.Append(" " + i + "; ");
                        sb.Append(reader.GetString(reader.GetOrdinal("interpret_name")));
                        sb.Append("; ");
                        sb.Append(reader.GetInt32(reader.GetOrdinal("sold_albums")).ToString());
                        sb.Append("; ");
                        sb.Append(reader.GetDecimal(reader.GetOrdinal("total_profit")).ToString());
                        sb.Append("; ");
                        sb.Append(reader.GetString(reader.GetOrdinal("top_album")));
                        sb.AppendLine("; ");

                        i++;
                    }
                }

            }
            if (pDb == null) { db.Close(); }
            return sb.ToString();
        }
    }
}
