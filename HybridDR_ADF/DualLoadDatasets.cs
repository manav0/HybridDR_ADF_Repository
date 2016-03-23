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
    class DualLoadDatasets
    {

        private DataFactoryManagementClient client;
        public DualLoadDatasets(DataFactoryManagementClient client)
        {
            this.client = client;
        }

        public void createDataSet_ETLControl()
        {
            Console.WriteLine("Creating " + DualLoadConfig.DATASET_ETL_Control);
            client.Datasets.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                new DatasetCreateOrUpdateParameters()
                {
                    Dataset = new Dataset()
                    {
                        Name = DualLoadConfig.DATASET_ETL_Control,
                        Properties = new DatasetProperties()
                        {
                            LinkedServiceName = DualLoadConfig.LINKEDSERVICE_ControlDB_Name,
                            TypeProperties = new AzureSqlTableDataset
                            {
                                TableName = "dbo.ETLControl",
                            }
                            ,
                            External = true,
                            Availability = new Availability()
                            {
                                Frequency = SchedulePeriod.Hour,
                                Interval = 1,
                            }
                            //,

                            //Policy = new Policy()
                            //{
                            //    Validation = new ValidationPolicy()
                            //    {
                            //        MinimumRows = 1
                            //    }
                            //}
                        }
                    }
                });
        }

        public void createDataSet_ETLControlDetail()
        {
            Console.WriteLine("Creating " + DualLoadConfig.DATASET_ETL_ControlDetail);
            client.Datasets.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                new DatasetCreateOrUpdateParameters()
                {
                    Dataset = new Dataset()
                    {
                        Name = DualLoadConfig.DATASET_ETL_ControlDetail,
                        Properties = new DatasetProperties()
                        {
                            LinkedServiceName = DualLoadConfig.LINKEDSERVICE_ControlDB_Name,
                            TypeProperties = new AzureSqlTableDataset
                            {
                                TableName = "dbo.ETLControlDetail",
                            }
                            ,
                            External = true,
                            Availability = new Availability()
                            {
                                Frequency = SchedulePeriod.Hour,
                                Interval = 1,
                            }
                            //,

                            //Policy = new Policy()
                            //{
                            //    Validation = new ValidationPolicy()
                            //    {
                            //        MinimumRows = 1
                            //    }
                            //}
                        }
                    }
                });
        }

        public void createDataSet_root()
        {
            Console.WriteLine("Creating " + DualLoadConfig.DATASET_ROOT);
            client.Datasets.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                new DatasetCreateOrUpdateParameters()
                {
                    Dataset = new Dataset()
                    {
                        Name = DualLoadConfig.DATASET_ROOT,
                        Properties = new DatasetProperties()
                        {
                            LinkedServiceName = DualLoadConfig.LINKEDSERVICE_BlobStore_Name,
                            TypeProperties = new AzureBlobDataset
                            {
                                FolderPath = "root/",
                                FileName = "test.csv"

                            },
                            External = true,
                            Availability = new Availability()
                            {
                                Frequency = SchedulePeriod.Hour,
                                Interval = 1,
                            }
                        }
                    }
                });
        }

        public void createDataSet_ToBeProcessedPath()
        {
            Console.WriteLine("Creating " + DualLoadConfig.DATASET_ToBeProcessedPath);
            client.Datasets.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                new DatasetCreateOrUpdateParameters()
                {
                    Dataset = new Dataset()
                    {
                        Name = DualLoadConfig.DATASET_ToBeProcessedPath,
                        Properties = new DatasetProperties()
                        {
                            LinkedServiceName = DualLoadConfig.LINKEDSERVICE_BlobStore_Name,
                            TypeProperties = new AzureBlobDataset
                            {
                                FolderPath = "root/ToBeProcessedPath/"
                            },
                            Availability = new Availability()
                            {
                                Frequency = SchedulePeriod.Hour,
                                Interval = 1,
                            }
                        }
                    }
                });
        }

        public void createDataSet_SqlOutput()
        {
            Console.WriteLine("Creating " + DualLoadConfig.DATASET_SQLOUTPUT);
            client.Datasets.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                new DatasetCreateOrUpdateParameters()
                {
                    Dataset = new Dataset()
                    {
                        Name = DualLoadConfig.DATASET_SQLOUTPUT,
                        Properties = new DatasetProperties()
                        {
                            LinkedServiceName = DualLoadConfig.LINKEDSERVICE_ControlDB_Name,
                            TypeProperties = new AzureSqlTableDataset
                            {
                                TableName = "Output",


                            }
                            ,
                            //External = true,
                            Availability = new Availability()
                            {
                                Frequency = SchedulePeriod.Hour,
                                Interval = 1,
                            }
                            //,

                            //Policy = new Policy()
                            //{
                            //    Validation = new ValidationPolicy()
                            //    {
                            //        MinimumRows = 1
                            //    }
                            //}
                        }
                    }
                });
        }

        public void createDataSet_SqlDummy()
        {
            Console.WriteLine("Creating " + DualLoadConfig.DATASET_SQLDUMMY);
            client.Datasets.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                new DatasetCreateOrUpdateParameters()
                {
                    Dataset = new Dataset()
                    {
                        Name = DualLoadConfig.DATASET_SQLDUMMY,
                        Properties = new DatasetProperties()
                        {
                            LinkedServiceName = DualLoadConfig.LINKEDSERVICE_ControlDB_Name,
                            TypeProperties = new AzureSqlTableDataset
                            {
                                TableName = "Dummy",


                            }
                            ,
                            //External = true,
                            Availability = new Availability()
                            {
                                Frequency = SchedulePeriod.Hour,
                                Interval = 1,
                            }
                            //,

                            //Policy = new Policy()
                            //{
                            //    Validation = new ValidationPolicy()
                            //    {
                            //        MinimumRows = 1
                            //    }
                            //}
                        }
                    }
                });
        }
        public void createOutputDataSet()
        {
            Console.WriteLine("Creating output dataset");
            client.Datasets.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                new DatasetCreateOrUpdateParameters()
                {
                    Dataset = new Dataset()
                    {
                        Name = DualLoadConfig.DATASET_Destination,
                        Properties = new DatasetProperties()
                        {

                            LinkedServiceName = "LinkedService-AzureStorage",
                            TypeProperties = new AzureBlobDataset()
                            {
                                FolderPath = "adftutorial/apifactoryoutput/{Slice}",
                                PartitionedBy = new Collection<Partition>()
                                {
                        new Partition()
                        {
                            Name = "Slice",
                            Value = new DateTimePartitionValue()
                            {
                                Date = "SliceStart",
                                Format = "yyyyMMdd-HH"
                            }
                        }
                                }
                            },

                            Availability = new Availability()
                            {
                                Frequency = SchedulePeriod.Hour,
                                Interval = 1,
                            },
                        }
                    }
                });
        }
    }
}
