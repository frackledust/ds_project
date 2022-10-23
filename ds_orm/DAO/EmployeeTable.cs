using System.Data.SqlClient;
using DTO;
using ds_orm;

namespace DAO
{
    internal class EmployeeTable
    {
        //2.1. Nový zaměstnanec
        public static String SQL_INSERT = @"insert into Employee ([employee_id], [name], [surname]) values (@Employee_id, @Name, @Surname);";
        //2.2. Seznam zaměstnanců
        public static String SQL_SELECT = @"Select employee_id, name, surname FROM Employee";
        public static String SQL_SELECT_ID = @"Select name, surname FROM Employee WHERE employee_id=@Employee_id;";
        //2.3. Aktualizace zaměstnance
        public static String SQL_UPDATE = @"update Employee set name=@Name, surname=@Surname WHERE employee_id=@Employee_id;";


        public static int Insert(Employee e, Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);
            db.BeginTransaction();

            SqlCommand command = db.CreateCommand(SQL_INSERT);
            BaseTable.PrepareCommand(command, e, e.GetType());
            int ret = db.ExecuteNonQuery(command);

            db.EndTransaction();
            if (pDb == null){   db.Close(); }

            return ret;
        }

        public static Employee SelectID(int id, Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);
            Employee e = new Employee();
            using (SqlCommand command = db.CreateCommand(SQL_SELECT))
            {
                command.Parameters.AddWithValue("@Employe_id", id);
                using (SqlDataReader reader = db.Select(command))
                {
                    if (reader.Read())
                    {
                        e.Employee_id = reader.GetInt32(reader.GetOrdinal("employee_id"));
                        e.Name = reader.GetString(reader.GetOrdinal("name"));
                        e.Surname = reader.GetString(reader.GetOrdinal("surname")); 
                    }
                }

            }
            if (pDb == null) { db.Close(); }
            return e;
        }

        public static List<Employee> Select(Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);

            List<Employee> employees = new();
            SqlCommand command = db.CreateCommand(SQL_SELECT);
            using (SqlDataReader reader = db.Select(command))
            {
                while (reader.Read())
                {
                    Employee e  = new ();
                    e.Employee_id = reader.GetInt32(reader.GetOrdinal("employee_id"));
                    e.Name = reader.GetString(reader.GetOrdinal("name"));
                    e.Surname = reader.GetString(reader.GetOrdinal("surname"));

                    employees.Add(e);
                }
            }

            if (pDb == null) {  db.Close(); }
            return employees;
        }

        public static int Update(Employee e, Database? pDb = null)
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
    }
}
