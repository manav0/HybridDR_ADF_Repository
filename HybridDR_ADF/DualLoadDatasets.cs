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

        public void createDataSet_SourceFolder(String container, String folderPath, String file, int i)
        {
            Console.WriteLine("Creating " + DualLoadConfig.DATASET_SOURCEFOLDER + "_" + i);
            client.Datasets.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                new DatasetCreateOrUpdateParameters()
                {
                    Dataset = new Dataset()
                    {
                        Name = DualLoadConfig.DATASET_SOURCEFOLDER + "_" + i,
                        Properties = new DatasetProperties()
                        {
                            LinkedServiceName = DualLoadConfig.LINKEDSERVICE_BlobStore_Name,
                            TypeProperties = new AzureBlobDataset
                            {
                                FolderPath = container + folderPath,//"root/",
                                FileName = file//"test.csv"
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

        public void createDataSet_ToBeProcessedPath(String container, String toBeProcessedFolderPath, String file, String pipelineName, int i)
        {
            Console.WriteLine("Creating " + DualLoadConfig.DATASET_ToBeProcessedFolder + "_" + pipelineName);
            bool isExternal = pipelineName.Equals(DualLoadConfig.PIPELINE_ARCHIVE + "_" + i) ? true : false;
            Console.WriteLine("isExternal= " + isExternal);
            if (isExternal)
                client.Datasets.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                    new DatasetCreateOrUpdateParameters()
                    {
                        Dataset = new Dataset()
                        {
                            Name = DualLoadConfig.DATASET_ToBeProcessedFolder + "_" + pipelineName,
                            Properties = new DatasetProperties()
                            {
                                LinkedServiceName = DualLoadConfig.LINKEDSERVICE_BlobStore_Name,
                                External = isExternal,
                                TypeProperties = new AzureBlobDataset
                                {
                                    FolderPath = container + "/" + toBeProcessedFolderPath, //"root/ToBeProcessedPath/"
                                    FileName = file
                                },
                                Availability = new Availability()
                                {
                                    Frequency = SchedulePeriod.Hour,
                                    Interval = 1,
                                }
                            }
                        }
                    });
            else
            {
                client.Datasets.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
    new DatasetCreateOrUpdateParameters()
    {
        Dataset = new Dataset()
        {
            Name = DualLoadConfig.DATASET_ToBeProcessedFolder + "_" + pipelineName,
            Properties = new DatasetProperties()
            {
                LinkedServiceName = DualLoadConfig.LINKEDSERVICE_BlobStore_Name,
                TypeProperties = new AzureBlobDataset
                {
                    FolderPath = container + "/" + toBeProcessedFolderPath, //"root/ToBeProcessedPath/"
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
        }

        public void createDataSet_ArchivedFolder(String container, String folderPath, String file, int i)
        {
            Console.WriteLine("Creating " + DualLoadConfig.DATASET_ArchivedFolder + "_" + i);
            client.Datasets.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                new DatasetCreateOrUpdateParameters()
                {
                    Dataset = new Dataset()
                    {
                        Name = DualLoadConfig.DATASET_ArchivedFolder + "_" + i,
                        Properties = new DatasetProperties()
                        {
                            LinkedServiceName = DualLoadConfig.LINKEDSERVICE_BlobStore_Name,
                            TypeProperties = new AzureBlobDataset
                            {
                                FolderPath = container + "/" + folderPath,//"root/",
                                                                          //FileName = file//"test.csv"
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


        public void createDataSet_Init_SqlDummy(int i)
        {
            Console.WriteLine("Creating " + DualLoadConfig.DATASET_INIT_SQLDUMMY + "_" + i);
            client.Datasets.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                new DatasetCreateOrUpdateParameters()
                {
                    Dataset = new Dataset()
                    {
                        Name = DualLoadConfig.DATASET_INIT_SQLDUMMY + "_" + i,
                        Properties = new DatasetProperties()
                        {
                            LinkedServiceName = DualLoadConfig.LINKEDSERVICE_ControlDB_Name,
                            TypeProperties = new AzureSqlTableDataset
                            {
                                TableName = "Init_Dummy",
                            }
                            ,
                            //External = true,
                            Availability = new Availability()
                            {
                                Frequency = SchedulePeriod.Hour,
                                Interval = 1,
                            }
                        }
                    }
                });
        }


        public void createDataSet_Load_1_SqlDummy(int i)
        {
            Console.WriteLine("Creating " + DualLoadConfig.DATASET_LOAD_1_SQLDUMMY + "_" + i);
            client.Datasets.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                new DatasetCreateOrUpdateParameters()
                {
                    Dataset = new Dataset()
                    {
                        Name = DualLoadConfig.DATASET_LOAD_1_SQLDUMMY + "_" + i,
                        Properties = new DatasetProperties()
                        {
                            LinkedServiceName = DualLoadConfig.LINKEDSERVICE_ControlDB_Name,
                            TypeProperties = new AzureSqlTableDataset
                            {
                                TableName = "Load_1_Dummy",
                            }
                            ,
                            Availability = new Availability()
                            {
                                Frequency = SchedulePeriod.Hour,
                                Interval = 1,
                            }
                        }
                    }
                });
        }


        public void createDataSet_Load_2_SqlDummy(int i)
        {
            Console.WriteLine("Creating " + DualLoadConfig.DATASET_LOAD_2_SQLDUMMY + "_" + i);
            client.Datasets.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                new DatasetCreateOrUpdateParameters()
                {
                    Dataset = new Dataset()
                    {
                        Name = DualLoadConfig.DATASET_LOAD_2_SQLDUMMY + "_" + i,
                        Properties = new DatasetProperties()
                        {
                            LinkedServiceName = DualLoadConfig.LINKEDSERVICE_ControlDB_Name,
                            TypeProperties = new AzureSqlTableDataset
                            {
                                TableName = "Load_2_Dummy",
                            }
                            ,
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

        public void createDataSet_Archive_1_SqlDummy(int i)
        {
            Console.WriteLine("Creating " + DualLoadConfig.DATASET_ARCHIVE_1_SQLDUMMY + "_" + i);
            client.Datasets.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                new DatasetCreateOrUpdateParameters()
                {
                    Dataset = new Dataset()
                    {
                        Name = DualLoadConfig.DATASET_ARCHIVE_1_SQLDUMMY + "_" + i,
                        Properties = new DatasetProperties()
                        {
                            LinkedServiceName = DualLoadConfig.LINKEDSERVICE_ControlDB_Name,
                            TypeProperties = new AzureSqlTableDataset
                            {
                                TableName = "Archive_1_Dummy",
                            }
                            ,
                            //External = true,
                            Availability = new Availability()
                            {
                                Frequency = SchedulePeriod.Hour,
                                Interval = 1,
                            }
                        }
                    }
                });
        }


        /**
        * not used currently
        */
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
