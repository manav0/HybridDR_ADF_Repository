using Microsoft.Azure.Management.DataFactories;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace HybridDR_ADF
{
    class DBQuery
    {
        static void Main(string[] args)
        {
            DBQuery dbQuery = new DBQuery();
            dbQuery.getResults(DualLoadConfig.QUERY_INIT_1);
        }

        public void getResults(String queryString)
        {
            Console.WriteLine("Processing Query: " + queryString);
            SqlConnection connection = new SqlConnection(DualLoadConfig.CONNECTION_STRING_ControlDB);
            var command = connection.CreateCommand();
            command.CommandText = queryString;

            //command.Parameters.AddWithValue("@Name", "SQL Server Express");
            //command.Parameters.AddWithValue("@Number", "SQLEXPRESS1");
            //command.Parameters.AddWithValue("@Cost", 0);
            //command.Parameters.AddWithValue("@Price", 0);

            connection.Open();
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                int rowsReturned = reader.FieldCount;
                Console.WriteLine("rowsReturned: " + rowsReturned);
                for (int lp = 0; lp < rowsReturned; lp++)
                {
                    String name = reader.GetName(lp);
                    Object value = reader.GetValue(lp);
                    dict.Add(name, value);
                    Console.WriteLine(name + ": " + value);
                }
            }

            reader.Close();
            connection.Close();
            Console.ReadKey();
        }

        private void init1(String queryString)
        {
            using (SqlConnection connection = new SqlConnection(DualLoadConfig.CONNECTION_STRING_ControlDB))
            {
                SqlCommand command = new SqlCommand(queryString);
                command.Connection = connection;
                connection.Open();
                Console.WriteLine("connection opened");

                //queryResultCloud = command.ExecuteReader();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine("reader = " + reader);
                        //Console.WriteLine("ID: {0} LastRunDate: {1} FileNameLike: {2} FilePath: {3} ToBeProcessedPath: {4} ArchivePath: {5}", reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5));
                    }
                }
                connection.Close();
            }
        }
    }
}
