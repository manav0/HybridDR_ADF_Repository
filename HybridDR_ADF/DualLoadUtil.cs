using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Azure.Management.DataFactories;
using Microsoft.Azure.Management.DataFactories.Models;
using Microsoft.Azure.Management.DataFactories.Common.Models;


using Microsoft.Azure;


namespace HybridDR_ADF
{
    /**
    * utility methods for Dual Load Data Factory
    */
    class DualLoadUtil
    {
        private DualLoadDatasets datasets;
        private static DataFactoryManagementClient client;
        private AzureLoginController loginController;

        public DualLoadUtil()
        {
            loginController = new AzureLoginController();
        }

        public void showInteractiveOutput(DateTime PipelineActivePeriodStartTime, DateTime PipelineActivePeriodEndTime, String outputDataSetName)
        {
            // Pulling status within a timeout threshold
            DateTime start = DateTime.Now;
            bool done = false;

            while (DateTime.Now - start < TimeSpan.FromMinutes(5) && !done)
            {
                Console.WriteLine("Pulling the slice status");
                // wait before the next status check
                Thread.Sleep(1000 * 12);

                var datalistResponse = client.DataSlices.List(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name, outputDataSetName,
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
                    outputDataSetName,
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
            client = AzureLoginController.createDataFactoryManagementClient();
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

        public DataFactoryManagementClient getClient()
        {
            return (loginController.getClient());
        }

        public void tearDown(DataFactoryManagementClient client, String pipelineName)
        {
            Console.WriteLine("Tearing down " + pipelineName + "_0");
            client.Pipelines.Delete(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name, pipelineName + "_0");
            client.Datasets.Delete(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name, DualLoadConfig.DATASET_ETL_ControlDetail);
            //client.Datasets.Delete(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name, DualLoadConfig.DATASET_INIT_SQLDUMMY);
        }

        public DualLoadDatasets getDatasets()
        {
            return (datasets);
        }

        public void setDatasets(DualLoadDatasets datasets)
        {
            this.datasets = datasets;
        }
    }
}
