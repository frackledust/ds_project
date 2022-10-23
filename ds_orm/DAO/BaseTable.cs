using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ds_orm;
using DTO;

namespace DAO
{
    internal static class BaseTable
    {
        public static Database GetDatabase(Database? pDb)
        {
            if (pDb == null)
            {
                Database db = new Database();
                db.Connect();
                return db;
            }
            else
            {
                return pDb;
            }
        }

        public static int Update(Object obj, Database? db = null)
        {
            if (db == null) { db = new Database(); }
            db.Connect();


            SqlCommand command = db.CreateCommand("");

            PrepareCommand(command, obj, obj.GetType());
            int ret = db.ExecuteNonQuery(command);
            db.Close();
            return ret;
        }
        public static List<Object> Read(SqlDataReader reader, Type type)
        {
            List<Object> result = new();

            foreach (PropertyInfo pi in type.GetProperties())
            {
                Object? entry = Activator.CreateInstance(type);

                foreach (PropertyInfo pf in type.GetProperties())
                {
                    int col = reader.GetOrdinal(pf.Name);
                    if (reader.IsDBNull(col))
                    {
                        pf.SetValue(entry, null);
                    }
                    else
                    {
                        if (pf.PropertyType == typeof(int))
                        {
                            int value = reader.GetInt32(col);
                            pf.SetValue(entry, value);
                        }
                        else if(pf.PropertyType == typeof(char))
                        {
                            char value = reader.GetChar(col);
                            pf.SetValue(entry, value);
                        }
                        else if (pf.PropertyType == typeof(DateTime))
                        {
                            DateTime dateTime = reader.GetDateTime(col);
                            pf.SetValue(entry, dateTime);
                        }
                        else
                        {
                            string value = reader.GetString(col);
                            pf.SetValue(entry, value);
                        }
                    }

                    if(entry != null) { result.Add(entry); }
                }
            }

            return result;
        }

        public static void PrepareCommand(SqlCommand command, Object obj, Type type)
        {
            foreach(PropertyInfo pi in type.GetProperties())
            {
                if(pi.PropertyType.IsClass && pi.PropertyType != typeof(string))
                {
                    continue;
                }
                else
                {
                    string name = pi.Name;
                    var param = pi.GetValue(obj) == null ? DBNull.Value : pi.GetValue(obj);
                    command.Parameters.AddWithValue(name, param);
                }
            }
        }

        public static int PrepareDatabase(Database? pDb = null)
        {
            Database db = BaseTable.GetDatabase(pDb);
            // 1.  create a command object identifying the stored procedure
            SqlCommand command = db.CreateCommand("PDataDelete");
            SqlCommand command2 = db.CreateCommand("PDataInsert");

            command.CommandType = CommandType.StoredProcedure;
            command2.CommandType = CommandType.StoredProcedure;

            int res = db.ExecuteNonQuery(command);
            res *= db.ExecuteNonQuery(command2);

            if (pDb == null) { db.Close(); }

            return res;
        }
    }
}
