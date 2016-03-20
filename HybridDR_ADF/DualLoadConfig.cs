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
        public static string DATASET_ROOT = "Dataset-Root";
        public static string DATASET_ToBeProcessedPath = "Dataset-ToBeProcessedPath";
        public static string DATASET_SQLOUTPUT = "Dataset-SqlOutput";
        public static string DATASET_Destination = "Dataset-BlobDestination";
        //public static string DATASET_PDW = "clouddr-pdw-ds";

        public static string ACTIVITY_PDW_QUERY = "Activity-PDWquery";
        public static string ACTIVITY_SP_RECORD = "Activity-SProc-Record";
        public static string ACTIVITY_QuerySQL_ETLControl = "Activity-QuerySQL-ETLControl";
        public static string ACTIVITY_MOVE_FILES = "Activity-MoveFiles";

        public static string TABLE_PDW = "PDW";
        public static string PIPELINE_INIT_Name = "Pipeline-Init";


        public static string CONNECTION_STRING_ControlDB = "Server=tcp:clouddr-dbserver.database.windows.net,1433;Database=clouddr-control-db;User ID=clouddr-dbserver-admin@clouddr-dbserver;Password=Welcome1;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        public static string CONNECTION_STRING_StorageAccount = "DefaultEndpointsProtocol=https;AccountName=clouddrstorage;AccountKey=FxWy5CdDsCv19CQ5mcSE2dYmH8hpX0Q8RjqFSrLtdFSOh1yYSIimW9tDYXN/qlLDf+SrPa+tzdBmT9XbNqaTXw==";


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
            record_SProc_Activity.Name = DualLoadConfig.ACTIVITY_SP_RECORD;
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
