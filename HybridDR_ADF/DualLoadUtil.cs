using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;
using System.Collections.ObjectModel;

using Microsoft.Azure.Management.DataFactories;
using Microsoft.Azure.Management.DataFactories.Models;
using Microsoft.Azure.Management.DataFactories.Common.Models;

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Azure;


namespace HybridDR_ADF
{
    class DualLoadUtil
    {
        private DataFactoryManagementClient client;

        public DataFactoryManagementClient getClient()
        {
            return (client);
        }

        public DataFactoryManagementClient createDataFactoryManagementClient()
        {
            TokenCloudCredentials aadTokenCredentials = new TokenCloudCredentials(DualLoadConfig.SUBSCRIPTION_ID, GetAuthorizationHeader(DualLoadConfig.AD_TENANT_ID));

            Uri resourceManagerUri = new Uri(ConfigurationManager.AppSettings["ResourceManagerEndpoint"]);

            // create data factory management client
            return (client = new DataFactoryManagementClient(aadTokenCredentials, resourceManagerUri));
        }


        public void showInteractiveOutput(DateTime PipelineActivePeriodStartTime, DateTime PipelineActivePeriodEndTime)
        {
            // Pulling status within a timeout threshold
            DateTime start = DateTime.Now;
            bool done = false;

            while (DateTime.Now - start < TimeSpan.FromMinutes(5) && !done)
            {
                Console.WriteLine("Pulling the slice status");
                // wait before the next status check
                Thread.Sleep(1000 * 12);

                var datalistResponse = client.DataSlices.List(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name, DualLoadConfig.DATASET_ToBeProcessedPath,
                    new DataSliceListParameters()
                    {
                        DataSliceRangeStartTime = PipelineActivePeriodStartTime.ConvertToISO8601DateTimeString(),
                        DataSliceRangeEndTime = PipelineActivePeriodEndTime.ConvertToISO8601DateTimeString()
                    });

                foreach (DataSlice slice in datalistResponse.DataSlices)
                {
                    if (slice.State == DataSliceState.Failed || slice.State == DataSliceState.Ready)
                    {
                        Console.WriteLine("Slice execution is done with status: {0}", slice.State);
                        done = true;
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Slice status is: {0}", slice.State);
                    }
                }
            }
            Console.WriteLine("Getting run details of a data slice");

            // give it a few minutes for the output slice to be ready
            Console.WriteLine("\nGive it a few minutes for the output slice to be ready and press any key.");
            Console.ReadKey();

            var datasliceRunListResponse = client.DataSliceRuns.List(
                    DualLoadConfig.RESOURCEGROUP_Name,
                    DualLoadConfig.DATAFACTORY_Name,
                    DualLoadConfig.DATASET_ToBeProcessedPath,
                    new DataSliceRunListParameters()
                    {
                        DataSliceStartTime = PipelineActivePeriodStartTime.ConvertToISO8601DateTimeString()
                    }
                );

            foreach (DataSliceRun run in datasliceRunListResponse.DataSliceRuns)
            {
                Console.WriteLine("Status: \t\t{0}", run.Status);
                Console.WriteLine("DataSliceStart: \t{0}", run.DataSliceStart);
                Console.WriteLine("DataSliceEnd: \t\t{0}", run.DataSliceEnd);
                Console.WriteLine("ActivityId: \t\t{0}", run.ActivityName);
                Console.WriteLine("ProcessingStartTime: \t{0}", run.ProcessingStartTime);
                Console.WriteLine("ProcessingEndTime: \t{0}", run.ProcessingEndTime);
                Console.WriteLine("ErrorMessage: \t{0}", run.ErrorMessage);
            }
        }
        public void createDataFactory()
        {
            // create a data factory
            Console.WriteLine("Creating a new data factory: " + DualLoadConfig.DATAFACTORY_Name);
            client.DataFactories.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name,
                new DataFactoryCreateOrUpdateParameters()
                {
                    DataFactory = new DataFactory()
                    {
                        Name = DualLoadConfig.DATAFACTORY_Name,
                        Location = "WestUS",
                        Properties = new DataFactoryProperties() { }
                    }
                }
            );
        }

        public void createLinkedService_ControlDB()
        {
            Console.WriteLine("Creating " + DualLoadConfig.LINKEDSERVICE_ControlDB_Name);
            client.LinkedServices.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                new LinkedServiceCreateOrUpdateParameters()
                {
                    LinkedService = new LinkedService()
                    {
                        Name = DualLoadConfig.LINKEDSERVICE_ControlDB_Name,
                        Properties = new LinkedServiceProperties
                        (
                            new AzureSqlDatabaseLinkedService(DualLoadConfig.CONNECTION_STRING_ControlDB)
                        )
                    }
                }
            );
        }

        public void createLinkedService_BlobStorage()
        {
            Console.WriteLine("Creating " + DualLoadConfig.LINKEDSERVICE_BlobStore_Name);
            client.LinkedServices.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                new LinkedServiceCreateOrUpdateParameters()
                {
                    LinkedService = new LinkedService()
                    {
                        Name = DualLoadConfig.LINKEDSERVICE_BlobStore_Name,
                        Properties = new LinkedServiceProperties
                        (
                            new AzureStorageLinkedService(DualLoadConfig.CONNECTION_STRING_StorageAccount)
                        )
                    }
                }
            );
        }

        public static string GetAuthorizationHeader(String AD_TENANT_ID)
        {
            AuthenticationResult result = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var context = new AuthenticationContext(ConfigurationManager.AppSettings["ActiveDirectoryEndpoint"] + AD_TENANT_ID);

                    result = context.AcquireToken(
                        resource: ConfigurationManager.AppSettings["WindowsManagementUri"],
                        clientId: ConfigurationManager.AppSettings["AdfClientId"],
                        redirectUri: new Uri(ConfigurationManager.AppSettings["RedirectUri"]),
                        promptBehavior: PromptBehavior.Always);
                }
                catch (Exception threadEx)
                {
                    Console.WriteLine(threadEx.Message);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Name = "AcquireTokenThread";
            thread.Start();
            thread.Join();

            if (result != null)
            {
                return result.AccessToken;
            }

            throw new InvalidOperationException("Failed to acquire token");
        }
    }
}
