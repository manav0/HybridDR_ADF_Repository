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
    /**
    * config params for Dual Load system
    */
    class DualLoadConfig
    {

        public static string
                            SUBSCRIPTION_ID = ConfigurationManager.AppSettings["SubscriptionId"],
                            AD_TENANT_ID = ConfigurationManager.AppSettings["ActiveDirectoryTenantId"];

        //DUAL LOAD STATUS CODES
        public static int
                            STATUS_NP = 1,
                            STATUS_PROCESSING = 2,
                            STATUS_COMPLETE = 3,
                            STATUS_ARCHIVED = 4;

        //DATA FACTORY & LINKED SERVICES
        public static string
                            DATAFACTORY_Name = "Data-Factory-Cloud-DR",
                            RESOURCEGROUP_Name = "clouddr-resourcegroup",
                            LINKEDSERVICE_ControlDB_Name = "Linked-Service-ControlDB",
                            LINKEDSERVICE_BlobStore_Name = "Linked-Service-BlobStore";

        //PIPELINES
        public static string
                            PIPELINE_INIT = "Pipeline-Init",
                            PIPELINE_LOADPROCESS = "Pipeline-LoadProcess",
                            PIPELINE_ARCHIVE = "Pipeline-Archive";

        //ACTIVITIES
        public static string
                            ACTIVITY_PDW_QUERY = "Activity-PDWquery",
                            ACTIVITY_INIT_1 = "Activity-Init-1",
                            ACTIVITY_INIT_3 = "Activity-Init-3",
                            ACTIVITY_INIT_4 = "Activity-Init-4";

        public static string
                            ACTIVITY_LOADPROCESS_3 = "Activity-LoadProcess-3",
                            ACTIVITY_LOADPROCESS_5 = "Activity-LoadProcess-5";

        public static string
                            ACTIVITY_ARCHIVE_2 = "Activity-Archive-2",
                            ACTIVITY_ARCHIVE_3 = "Activity-Archive-3";

        //DATASETS
        public static string
                            DATASET_Source = "Dataset-BlobSource",
                            DATASET_SOURCEFOLDER = "Dataset-SourceFolder",
                            DATASET_ToBeProcessedFolder = "Dataset-ToBeProcessedFolder",
                            DATASET_ArchivedFolder = "Dataset-ArchivedFolder";

        public static string
                            DATASET_INIT_SQLDUMMY = "Dataset-Init-SqlDummy",
                            DATASET_LOAD_1_SQLDUMMY = "Dataset-Load-1-SqlDummy",
                            DATASET_LOAD_2_SQLDUMMY = "Dataset-Load-2-SqlDummy",
                            DATASET_ARCHIVE_1_SQLDUMMY = "Dataset-Archive-1-SqlDummy";

        public static string
                            DATASET_ETL_Control = "Dataset-ETLControl",
                            DATASET_ETL_ControlDetail = "Dataset-ETLControlDetail",
                            DATASET_Destination = "Dataset-BlobDestination";

        public static string
                        TABLE_PDW = "PDW";



        //Connection String to Control Database and Azure Storage Account
        public static string
                         CONNECTION_STRING_ControlDB = "Server=tcp:clouddr-dbserver.database.windows.net,1433;Database=clouddr-control-db;User ID=clouddr-dbserver-admin@clouddr-dbserver;Password=Welcome1;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
                         CONNECTION_STRING_StorageAccount = "DefaultEndpointsProtocol=https;AccountName=clouddrstorage;AccountKey=FxWy5CdDsCv19CQ5mcSE2dYmH8hpX0Q8RjqFSrLtdFSOh1yYSIimW9tDYXN/qlLDf+SrPa+tzdBmT9XbNqaTXw==";


        //QUERIES
        public static String
                        QUERY_INIT_1 = "select id, ControlProcess, LastRunDate, FileNameLike, FilePath, ToBeProcessedPath, ArchivePath from[dbo].[ETLControl]",
                        QUERY_LOADPROCESS_1 = "select PDWIPAddress, PrimaryPDW from PDW where ID = ?",
                        QUERY_LOADPROCESS_2 = "if($PdwId = 1) Begin select ECD.ID as ETLControlDetailID, FileName from [dbo].[ETLControl] EC join [dbo].[ETLControlDetail] ECD on ec.id = ECD.ETLControlID Where ControlProcess = $ControlProcess and PrimaryAPSStatus = 1 order by ecd.id end Else begin select ECD.ID as ETLControlDetailID, FileName from ETLControl EC join ETLControlDetail ECD on ec.id = ECD.ETLControlID Where ControlProcess = $ControlProcess and SecondaryAPSStatus = 1 order by ecd.id End",
                        QUERY_ARCHIVE_1 = "select ECD.ID as ETLControlDetailID, FileName, EC.ArchivePath from [dbo].[ETLControl] EC join [dbo].[ETLControlDetail] ECD on ec.id = ECD.ETLControlID  Where ControlProcess = $ControlProcess and PrimaryAPSStatus = 3 and SecondaryAPSStatus = 3 order by ecd.id",
                        QUERY_ARCHIVE_3 = "Update [dbo].[ETLControlDetail] Set SecondaryAPSStatus = 4, PrimaryAPSStatus = 4, status = 3, FileName = ? Where ID = ?";

    }
}
