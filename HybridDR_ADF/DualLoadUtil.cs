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
        private ADFLoginController loginController;
        private ADFOutputMonitor adfMonitor;

        public DualLoadUtil()
        {
            loginController = new ADFLoginController();
        }

        public void createDataFactory()
        {
            // create a data factory
            Console.WriteLine("Creating a new data factory: " + DualLoadConfig.DATAFACTORY_Name);
            client = ADFLoginController.createDataFactoryManagementClient();
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

        public ADFOutputMonitor getADFMonitor()
        {
            return (adfMonitor);
        }

        public void setADFMonitor(ADFOutputMonitor adfMonitor)
        {
            this.adfMonitor = adfMonitor;
        }
    }
}
