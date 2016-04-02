using Microsoft.Azure.Management.DataFactories;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Text;

//Nuget WindowsAzure.Storage
namespace HybridDR_ADF
{
    class AzureStorageController
    {

        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(DualLoadConfig.CONNECTION_STRING_StorageAccount);
        private static CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

        static void Main(string[] args)
        {
            AzureStorageController storageController = new AzureStorageController();
            //getFilePathList("/");
            //storageController.getFilePathList("dimemployee", "TOBEPROCESSEDPATH/");
            storageController.deleteBlob("dimemployee", "/", "test.csv");


            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(DualLoadConfig.CONNECTION_STRING_StorageAccount);

            // Create the blob client.
            //CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            //writeAzureDirectoriesToConsole(blobClient.ListContainers());



            //list<dictionary<string, object>> resultlist = dbquery.getresultlist(dualloadconfig.query_loadprocess_2.replace("$pdwid", "1").replace("$controlprocess", "'dimemployee'"));

            //foreach (dictionary<string, object> result in resultlist)
            //{
            //    foreach (keyvaluepair<string, object> kvp in result)
            //    {
            //        string key = kvp.key;
            //        object value = kvp.value;
            //        console.writeline("key: " + key + ", value: " + value);
            //    }
            //}

            //List<Dictionary<string, object>> resultList = dbQuery.getResultList(DualLoadConfig.QUERY_LOADPROCESS_1.Replace('?', '2'));
            //Console.WriteLine("resultList: " + resultList);

            //foreach (Dictionary<string, object> result in resultList)
            //{
            //    foreach (KeyValuePair<string, object> kvp in result)
            //    {
            //        string key = kvp.Key;
            //        object value = kvp.Value;
            //        Console.WriteLine("Key: " + key + ", Value: " + value);
            //    }
            //}
            Console.ReadKey();
        }


        //public static DateTime GetBlockBlobDateTime(string baseURL, string container, string directory, string fileName)
        //{
        //    CloudBlobClient blobClient = new CloudBlobClient(baseURL);
        //    CloudBlobDirectory blobDir = blobClient.GetBlobDirectoryReference(container);
        //    CloudBlobDirectory subDirectory = blobDir.GetSubdirectory(directory);
        //    CloudBlockBlob cloudBlockBlob = subDirectory.GetBlockBlobReference(fileName);
        //    cloudBlockBlob.FetchAttributes();
        //    DateTime cloudTimeStampUTC = cloudBlockBlob.Properties.LastModifiedUtc;
        //    return cloudTimeStampUTC;
        //}



        static void writeAzureDirectoriesToConsole(IEnumerable<CloudBlobContainer> containers)
        {
            foreach (var container in containers)
            {
                string indent = "";
                Console.WriteLine("Container: " + container.Name);
                // Pass Ienumerable to recursive function to get "subdirectories":
                //Console.WriteLine(getContainerDirectories(container.ListBlobs(), indent));
                //List<String> filePathList = getFilePathList(container.ListBlobs(), "ToBeProcessedPath/");
                //List<String> filePathList = getFilePathList(container.ListBlobs(), "/");
                //foreach (String filePath in filePathList)
                {
                    //Console.WriteLine("filePath= " + filePath);
                }
            }
        }

        public void deleteBlob(String container, String directoryPath, String file)
        {
            IEnumerable<CloudBlobContainer> containers = blobClient.ListContainers();
            IEnumerable<IListBlobItem> blobList = null;
            foreach (var containerItem in containers)
            {
                Console.WriteLine("Container: " + containerItem.Name);
                if (containerItem.Name.Equals(container))
                {
                    blobList = containerItem.ListBlobs();
                }

            }

            Console.WriteLine("directory param: " + directoryPath.ToUpper());
            //List<String> filePathList = new List<string>();
            if ("/".Equals(directoryPath.ToUpper()))
            {
                Console.WriteLine("retrieving blobs from container root");
                foreach (var item in
                    blobList.Where((blobItem, type) => blobItem is CloudBlockBlob))
                {
                    var blobFile = item as CloudBlockBlob;
                    if (file.Equals(blobFile.Name))
                    {
                        Console.WriteLine("deleting blobFile.Name= " + blobFile.Name);
                        blobFile.Delete();
                    }

                }
                return;
            }

            foreach (var item in blobList.Where((blobItem, type) => blobItem is CloudBlobDirectory))
            {
                var directory = item as CloudBlobDirectory;
                String directoryString = directory.Prefix.ToUpper();
                Console.WriteLine("Found directory: " + directoryString);

                if (directoryString.Equals(directoryPath.ToUpper()))
                {
                    Console.WriteLine("retrieving blobs from container directory: " + directoryString);
                    foreach (var subitem in directory.ListBlobs().Where((blobsubItem, type) => blobsubItem is CloudBlockBlob))
                    {
                        var blobFile = subitem as CloudBlockBlob;
                        if (file.Equals(blobFile.Name))
                        {
                            Console.WriteLine("deleting blobFile.Name= " + blobFile.Name);
                            blobFile.Delete();
                        }
                    }
                }
            }

        }

        public List<String> getFilePathList(String container, String directoryPath)
        {
            IEnumerable<CloudBlobContainer> containers = blobClient.ListContainers();
            IEnumerable<IListBlobItem> blobList = null;
            foreach (var containerItem in containers)
            {
                Console.WriteLine("Container: " + containerItem.Name);
                if (containerItem.Name.Equals(container))
                {
                    blobList = containerItem.ListBlobs();
                }

            }

            Console.WriteLine("directory param: " + directoryPath.ToUpper());
            List<String> filePathList = new List<string>();
            if ("/".Equals(directoryPath.ToUpper()))
            {
                Console.WriteLine("retrieving blobs from container root");
                foreach (var item in
                    blobList.Where((blobItem, type) => blobItem is CloudBlockBlob))
                {
                    var blobFile = item as CloudBlockBlob;
                    filePathList.Add(blobFile.Name);
                    Console.WriteLine("blobFile.Name= " + blobFile.Name);
                }
                return (filePathList);
            }

            foreach (var item in blobList.Where((blobItem, type) => blobItem is CloudBlobDirectory))
            {
                var directory = item as CloudBlobDirectory;
                String directoryString = directory.Prefix.ToUpper();
                Console.WriteLine("Found directory: " + directoryString);

                if (directoryString.Equals(directoryPath.ToUpper()))
                {
                    Console.WriteLine("retrieving blobs from container directory: " + directoryString);
                    foreach (var subitem in directory.ListBlobs().Where((blobsubItem, type) => blobsubItem is CloudBlockBlob))
                    {
                        var blobFile = subitem as CloudBlockBlob;
                        filePathList.Add(blobFile.Name);
                        Console.WriteLine("blobFile.Name= " + blobFile.Name);
                    }
                }
            }

            return (filePathList);
        }

        static string getContainerDirectories(IEnumerable<IListBlobItem> blobList, string indent)
        {
            indent = indent + "  "; // Indent each item in the output for the current subdirectory:
            StringBuilder sb = new StringBuilder("");
            // First list all the actual FILES within the current blob list. No recursion needed:
            foreach (var item in blobList.Where((blobItem, type) => blobItem is CloudBlockBlob))
            {
                var blobFile = item as CloudBlockBlob;
                sb.AppendLine(indent + blobFile.Name);
            }
            // List all additional subdirectories in the current directory, and call recursively:
            foreach (var item in blobList.Where((blobItem, type) => blobItem is CloudBlobDirectory))
            {
                var directory = item as CloudBlobDirectory;
                sb.AppendLine(indent + directory.Prefix.ToUpper());
                // Call this method recursively to retrieve subdirectories within the current:
                sb.AppendLine(getContainerDirectories(directory.ListBlobs(), indent));
            }
            return sb.ToString();
        }
    }
}
