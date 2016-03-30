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
    class DualLoadConfig
    {

        public static string SUBSCRIPTION_ID = ConfigurationManager.AppSettings["SubscriptionId"];
        public static string AD_TENANT_ID = ConfigurationManager.AppSettings["ActiveDirectoryTenantId"];

        public static string DATAFACTORY_Name = "Data-Factory-Cloud-DR";
        public static string RESOURCEGROUP_Name = "clouddr-resourcegroup";
        public static string LINKEDSERVICE_ControlDB_Name = "Linked-Service-ControlDB";
        public static string LINKEDSERVICE_BlobStore_Name = "Linked-Service-BlobStore";

        public static string DATASET_Source = "Dataset-BlobSource";
        public static string DATASET_SOURCEFOLDER = "Dataset-SourceFolder";
        public static string DATASET_ToBeProcessedFolder = "Dataset-ToBeProcessedFolder";
        public static string DATASET_SQLOUTPUT = "Dataset-SqlOutput";
        public static string DATASET_SQLDUMMY = "Dataset-SqlDummy";
        public static string DATASET_ETL_Control = "Dataset-ETLControl";
        public static string DATASET_ETL_ControlDetail = "Dataset-ETLControlDetail";
        public static string DATASET_Destination = "Dataset-BlobDestination";
        //public static string DATASET_PDW = "clouddr-pdw-ds";

        public static string ACTIVITY_PDW_QUERY = "Activity-PDWquery";

        public static string ACTIVITY_INIT_1 = "Activity-Init-1";
        public static string ACTIVITY_INIT_3 = "Activity-Init-3";
        public static string ACTIVITY_INIT_4 = "Activity-Init-4";

        public static string ACTIVITY_LOADPROCESS_3 = "Activity-LoadProcess-3";
        public static string ACTIVITY_LOADPROCESS_5 = "Activity-LoadProcess-5";

        public static string TABLE_PDW = "PDW";

        public static string PIPELINE_INIT = "Pipeline-Init";
        public static string PIPELINE_LOADPROCESS = "Pipeline-LoadProcess";
        public static string PIPELINE_ARCHIVE = "Pipeline-Archive";


        public static string CONNECTION_STRING_ControlDB = "Server=tcp:clouddr-dbserver.database.windows.net,1433;Database=clouddr-control-db;User ID=clouddr-dbserver-admin@clouddr-dbserver;Password=Welcome1;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        //public static string CONNECTION_STRING_StorageAccount = "DefaultEndpointsProtocol=https;AccountName=clouddrstorage;AccountKey=FxWy5CdDsCv19CQ5mcSE2dYmH8hpX0Q8RjqFSrLtdFSOh1yYSIimW9tDYXN/qlLDf+SrPa+tzdBmT9XbNqaTXw==";

        public static string CONNECTION_STRING_StorageAccount = "DefaultEndpointsProtocol=https;AccountName=clouddrstorage;AccountKey=FxWy5CdDsCv19CQ5mcSE2dYmH8hpX0Q8RjqFSrLtdFSOh1yYSIimW9tDYXN/qlLDf+SrPa+tzdBmT9XbNqaTXw==";

        public static String QUERY_INIT_1 = "select id, ControlProcess, LastRunDate, FileNameLike, FilePath, ToBeProcessedPath, ArchivePath from[dbo].[ETLControl]";

        public static String QUERY_LOADPROCESS_1 = "select PDWIPAddress, PrimaryPDW from PDW where ID = ?";
        public static String QUERY_LOADPROCESS_2 = "if($PdwId = 1) Begin select ECD.ID as ETLControlDetailID, FileName from [dbo].[ETLControl] EC join [dbo].[ETLControlDetail] ECD on ec.id = ECD.ETLControlID Where ControlProcess = $ControlProcess and PrimaryAPSStatus = 1 order by ecd.id end Else begin select ECD.ID as ETLControlDetailID, FileName from ETLControl EC join ETLControlDetail ECD on ec.id = ECD.ETLControlID Where ControlProcess = $ControlProcess and SecondaryAPSStatus = 1 order by ecd.id End";
        //public static string query_loadprocess_3 = "if(? = 1) begin  update [dbo].[etlcontroldetail] set primaryapsstatus = 2 where id = ? end else begin update [dbo].[etlcontroldetail] set secondaryapsstatus = 2 where id = ? end";
        //public static string query_loadprocess_5 = "if(? = 1) begin  update [dbo].[etlcontroldetail] set primaryapsstatus = 3 where id = ? end else begin update [dbo].[etlcontroldetail] set secondaryapsstatus = 3 where id = ? end";

        public static String QUERY_ARCHIVE_1 = "select ECD.ID as ETLControlDetailID, FileName, EC.ArchivePath from [dbo].[ETLControl] EC join [dbo].[ETLControlDetail] ECD on ec.id = ECD.ETLControlID  Where ControlProcess = ? and PrimaryAPSStatus = 3 and SecondaryAPSStatus = 3 order by ecd.id";

        public static String QUERY_ARCHIVE_3 = "Update [dbo].[ETLControlDetail] Set SecondaryAPSStatus = 4, PrimaryAPSStatus = 4, status = 3, FileName = ? Where ID = ?";

        private Activity create_Record_SProc_Activity()
        {
            IDictionary<string, string> sprocParams = new Dictionary<string, string>();
            sprocParams.Add("@ETLControlID", "1");
            sprocParams.Add("@FileName", "Z:\\DimEmployee\\DimEmployee1.csv");

            Activity record_SProc_Activity = new Activity()
            //{
            //    Name = DualLoadConfig.ACTIVITY_SP_RECORD,
            //    Outputs = new List<ActivityOutput>()
            //                        {
            //                            new ActivityOutput() {
            //                            //Name = DualLoadConfig.DATASET_BLOBSTORE
            //                            Name = DualLoadConfig.DATASET_SQLOUTPUT
            //                            }
            //                        },
            //    TypeProperties = new SqlServerStoredProcedureActivity
            //    {
            //        StoredProcedureName = "dbo.usp_RecordFilesToBeProcessed",

            //        StoredProcedureParameters = sprocParams

            //    }
            //}
            ;
            record_SProc_Activity.Name = DualLoadConfig.ACTIVITY_INIT_3;
            record_SProc_Activity.Outputs = new List<ActivityOutput>()
                                    {
                                        new ActivityOutput() {
                                        //Name = DualLoadConfig.DATASET_BLOBSTORE
                                        Name = DualLoadConfig.DATASET_SQLOUTPUT
                                        }
                                    };
            record_SProc_Activity.TypeProperties = new SqlServerStoredProcedureActivity
            {
                StoredProcedureName = "dbo.usp_RecordFilesToBeProcessed",

                StoredProcedureParameters = sprocParams

            };

            return (record_SProc_Activity);
        }
    }
}
