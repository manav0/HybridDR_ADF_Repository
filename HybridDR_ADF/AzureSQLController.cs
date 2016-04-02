using Microsoft.Azure.Management.DataFactories;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace HybridDR_ADF
{
    class AzureSQLController
    {

        static void Main(string[] args)
        {
            AzureSQLController sqlController = new AzureSQLController();
            sqlController.executeDBQuery_Step1();
            Console.ReadKey();
        }

        private void executeDBQuery_Step1()
        {
            int CONTROLDETAIL_ID = 0;
            String FILENAME = "", ARCHIVE_FOLDER_PATH = "";


            List<Dictionary<string, object>> resultList = getResultList(DualLoadConfig.QUERY_ARCHIVE_1);

            List<Object> controlIdList = new List<Object>();
            foreach (Dictionary<string, object> result in resultList)
            {
                foreach (KeyValuePair<string, object> kvp in result)
                {
                    string key = kvp.Key;
                    object value = kvp.Value;
                    //Console.WriteLine("Key: " + key + ", value: " + value);
                    CONTROLDETAIL_ID = ("ETLControlDetailID".Equals(key)) ? (int)value : CONTROLDETAIL_ID;
                    FILENAME = ("FileName".Equals(key)) ? value.ToString() : FILENAME;
                    ARCHIVE_FOLDER_PATH = ("ArchivePath".Equals(key)) ? value.ToString() : ARCHIVE_FOLDER_PATH;
                    Console.WriteLine("CONTROLDETAIL_ID = " + CONTROLDETAIL_ID);
                    Console.WriteLine("FILENAME = " + FILENAME);
                    Console.WriteLine("ARCHIVE_FOLDER_PATH = " + ARCHIVE_FOLDER_PATH);
                }
            }


        }

        public List<Dictionary<String, object>> getResultList(String queryString)
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
            List<Dictionary<string, object>> resultList = new List<Dictionary<string, object>>();

            int rowsReturned = 0;
            while (reader.Read())
            {
                rowsReturned++;
                Dictionary<string, object> dict = new Dictionary<string, object>();
                int fieldCount = reader.FieldCount;
                Console.WriteLine("fieldCount= " + fieldCount);
                for (int lp = 0; lp < fieldCount; lp++)
                {
                    String name = reader.GetName(lp);
                    Object value = reader.GetValue(lp);
                    dict.Add(name, value);
                    Console.WriteLine("name: " + name + ", value: " + value);
                }
                resultList.Add(dict);
            }

            Console.WriteLine("rowsReturned: " + rowsReturned);

            reader.Close();
            connection.Close();
            return (resultList);
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
