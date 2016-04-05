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
    * all activities used by Dual Load data Factory & pipelines
    */
    class DualLoadActivities
    {

        //INIT PIPELINE ACTIVITIES

        /**
        * not used currently
        */
        public Activity create_Activity_Init_1()
        {
            Console.WriteLine("Creating " + DualLoadConfig.ACTIVITY_INIT_1);

            Activity activity = new Activity();

            List<ActivityInput> activityInputs = new List<ActivityInput>();
            ActivityInput activityInput = new ActivityInput();
            activityInput.Name = DualLoadConfig.DATASET_ETL_Control;
            activityInputs.Add(activityInput);
            SqlSource source = new SqlSource();
            source.SqlReaderQuery = DualLoadConfig.QUERY_INIT_1;

            List<ActivityOutput> activityOutputs = new List<ActivityOutput>();
            ActivityOutput activityOutput = new ActivityOutput();
            activityOutput.Name = DualLoadConfig.DATASET_LOAD_1_SQLDUMMY;
            activityOutputs.Add(activityOutput);
            SqlSink sink = new SqlSink();

            CopyActivity copyActivity = new CopyActivity();
            copyActivity.Source = source;
            copyActivity.Sink = sink;

            activity.Name = DualLoadConfig.ACTIVITY_INIT_1;
            activity.Inputs = activityInputs;
            activity.Outputs = activityOutputs;
            activity.TypeProperties = copyActivity;

            return (activity);
        }

        /**
        * Stored Proc Activity - usp_RecordFilesToBeProcessed
        * Add record to the ETLControlDetail table for the files that need to be processed
        */
        public Activity create_Activity_Init_3(int controlID, String filePath, int i)
        {
            Console.WriteLine("Creating " + DualLoadConfig.ACTIVITY_INIT_3 + "_" + i);
            IDictionary<string, string> sprocParams = new Dictionary<string, string>();
            //sprocParams.Add("@ETLControlID", "1");
            //sprocParams.Add("@FileName", "Z:\\DimEmployee\\DimEmployee1.csv");

            sprocParams.Add("@ETLControlID", controlID.ToString());
            sprocParams.Add("@FileName", filePath);

            Activity activity = new Activity();

            List<ActivityOutput> activityOutputs = new List<ActivityOutput>();
            ActivityOutput activityOutput = new ActivityOutput();
            activityOutput.Name = DualLoadConfig.DATASET_INIT_SQLDUMMY + "_" + i;
            activityOutputs.Add(activityOutput);

            SqlServerStoredProcedureActivity sqlserverStoredProcActivity = new SqlServerStoredProcedureActivity();
            sqlserverStoredProcActivity.StoredProcedureName = "dbo.usp_RecordFilesToBeProcessed";
            sqlserverStoredProcActivity.StoredProcedureParameters = sprocParams;


            activity.Name = DualLoadConfig.ACTIVITY_INIT_3 + "_" + i;
            activity.Outputs = activityOutputs;
            activity.TypeProperties = sqlserverStoredProcActivity;

            return (activity);
        }

        /**
        * CopyActivity from Blob source to Blob sink from Source Folder to ToBeProcessed Folder
        */
        public Activity create_Activity_Init_4(String sourceFolderDataset, String toBeProcessedCompleteFolderDataset, int i)
        {
            Console.WriteLine("Creating " + DualLoadConfig.ACTIVITY_INIT_4 + "_" + i);

            Activity activity = new Activity();

            List<ActivityInput> activityInputs = new List<ActivityInput>();
            ActivityInput activityInput = new ActivityInput();
            activityInput.Name = sourceFolderDataset;
            activityInputs.Add(activityInput);

            List<ActivityOutput> activityOutputs = new List<ActivityOutput>();
            ActivityOutput activityOutput = new ActivityOutput();
            activityOutput.Name = toBeProcessedCompleteFolderDataset;
            activityOutputs.Add(activityOutput);

            CopyActivity copyActivity = new CopyActivity();
            BlobSource blobSource = new BlobSource();
            copyActivity.Source = blobSource;
            //blobSource.Recursive = true;

            BlobSink sink = new BlobSink();
            sink.CopyBehavior = "PreserveHierarchy";

            //sink.WriteBatchSize = 10000;
            //sink.WriteBatchTimeout = TimeSpan.FromMinutes(10);
            copyActivity.Sink = sink;

            //Scheduler scheduler = new Scheduler();
            //scheduler.Frequency = SchedulePeriod.Hour;
            //scheduler.Interval = 1;


            activity.Name = DualLoadConfig.ACTIVITY_INIT_4 + "_" + i;
            activity.Inputs = activityInputs;
            activity.Outputs = activityOutputs;
            activity.TypeProperties = copyActivity;
            //activity.Scheduler = scheduler;

            return (activity);
        }

        //LOAD PROCESS PIPELINE ACTIVITIES
        /**
        * Stored Proc Activity - usp_UpdateControlDetailStatus to PROCESSING
        */
        public Activity create_Activity_LoadProcess_3(string ETlControlID, int i)
        {
            Console.WriteLine("Creating/Executing Stored proc: " + DualLoadConfig.ACTIVITY_LOADPROCESS_3 + "_" + i + " with ETlControlID= " + ETlControlID);

            IDictionary<string, string> sprocParams = new Dictionary<string, string>();

            sprocParams.Add("@ControlProcess", "1");
            sprocParams.Add("@Id", ETlControlID);
            sprocParams.Add("@Status", DualLoadConfig.STATUS_PROCESSING.ToString());

            Activity activity = new Activity();

            List<ActivityOutput> activityOutputs = new List<ActivityOutput>();
            ActivityOutput activityOutput = new ActivityOutput();
            activityOutput.Name = DualLoadConfig.DATASET_LOAD_1_SQLDUMMY + "_" + i;
            activityOutputs.Add(activityOutput);

            SqlServerStoredProcedureActivity sqlserverStoredProcActivity = new SqlServerStoredProcedureActivity();
            sqlserverStoredProcActivity.StoredProcedureName = "usp_UpdateControlDetailStatus";
            sqlserverStoredProcActivity.StoredProcedureParameters = sprocParams;


            activity.Name = DualLoadConfig.ACTIVITY_LOADPROCESS_3 + "_" + i;
            activity.Outputs = activityOutputs;
            activity.TypeProperties = sqlserverStoredProcActivity;

            return (activity);
        }

        /**
        * Stored Proc Activity - usp_UpdateControlDetailStatus to COMPLETE
        */
        public Activity create_Activity_LoadProcess_5(string ETlControlID, int i)
        {
            Console.WriteLine("Creating/Executing Stored proc: " + DualLoadConfig.ACTIVITY_LOADPROCESS_5 + "_" + i + " with ETlControlID= " + ETlControlID);

            IDictionary<string, string> sprocParams = new Dictionary<string, string>();

            sprocParams.Add("@ControlProcess", "1");
            sprocParams.Add("@Id", ETlControlID);
            sprocParams.Add("@Status", DualLoadConfig.STATUS_COMPLETE.ToString());

            Activity activity = new Activity();

            List<ActivityOutput> activityOutputs = new List<ActivityOutput>();
            ActivityOutput activityOutput = new ActivityOutput();
            activityOutput.Name = DualLoadConfig.DATASET_LOAD_2_SQLDUMMY + "_" + i;
            activityOutputs.Add(activityOutput);

            SqlServerStoredProcedureActivity sqlserverStoredProcActivity = new SqlServerStoredProcedureActivity();
            sqlserverStoredProcActivity.StoredProcedureName = "usp_UpdateControlDetailStatus";
            sqlserverStoredProcActivity.StoredProcedureParameters = sprocParams;


            activity.Name = DualLoadConfig.ACTIVITY_LOADPROCESS_5 + "_" + i;
            activity.Outputs = activityOutputs;
            activity.TypeProperties = sqlserverStoredProcActivity;

            return (activity);
        }


        //ARCHIVE PIPELINE ACTIVITIES
        /**
        * CopyActivity from Blob source to Blob sink from ToBeProcessed Folder to Archived Folder
        */
        public Activity create_Activity_Archive_2(String toBeProcessedCompleteFolderDataset, String archiveFolderDataset, int i)
        {
            Console.WriteLine("Creating " + DualLoadConfig.ACTIVITY_ARCHIVE_2 + "_" + i);

            Activity activity = new Activity();

            List<ActivityInput> activityInputs = new List<ActivityInput>();
            ActivityInput activityInput = new ActivityInput();
            activityInput.Name = toBeProcessedCompleteFolderDataset;
            activityInputs.Add(activityInput);

            List<ActivityOutput> activityOutputs = new List<ActivityOutput>();
            ActivityOutput activityOutput = new ActivityOutput();
            activityOutput.Name = archiveFolderDataset + "_" + i;
            activityOutputs.Add(activityOutput);

            CopyActivity copyActivity = new CopyActivity();
            BlobSource blobSource = new BlobSource();
            copyActivity.Source = blobSource;
            //blobSource.Recursive = true;

            BlobSink sink = new BlobSink();
            sink.CopyBehavior = "PreserveHierarchy";

            //sink.WriteBatchSize = 10000;
            //sink.WriteBatchTimeout = TimeSpan.FromMinutes(10);
            copyActivity.Sink = sink;

            //Scheduler scheduler = new Scheduler();
            //scheduler.Frequency = SchedulePeriod.Hour;
            //scheduler.Interval = 1;


            activity.Name = DualLoadConfig.ACTIVITY_ARCHIVE_2 + "_" + i;
            activity.Inputs = activityInputs;
            activity.Outputs = activityOutputs;
            activity.TypeProperties = copyActivity;
            //activity.Scheduler = scheduler;

            return (activity);
        }

        /**
        * Stored Proc Activity - usp_UpdateToArchiveStatus to ARCHIVED
        */
        public Activity create_Activity_Archive_3(int controlDetailID, String fileName, int i)
        {
            Console.WriteLine("Creating/Executing Stored proc: " + DualLoadConfig.ACTIVITY_ARCHIVE_3 + "_" + i + " with controlDetailID= " + controlDetailID);
            IDictionary<string, string> sprocParams = new Dictionary<string, string>();
            sprocParams.Add("@FileName", fileName);
            sprocParams.Add("@ControlDetailId", controlDetailID.ToString());


            Activity activity = new Activity();

            List<ActivityOutput> activityOutputs = new List<ActivityOutput>();
            ActivityOutput activityOutput = new ActivityOutput();
            activityOutput.Name = DualLoadConfig.DATASET_ARCHIVE_1_SQLDUMMY + "_" + i;
            activityOutputs.Add(activityOutput);

            SqlServerStoredProcedureActivity sqlserverStoredProcActivity = new SqlServerStoredProcedureActivity();
            sqlserverStoredProcActivity.StoredProcedureName = "usp_UpdateToArchiveStatus";
            sqlserverStoredProcActivity.StoredProcedureParameters = sprocParams;


            activity.Name = DualLoadConfig.ACTIVITY_ARCHIVE_3 + "_" + i;
            activity.Outputs = activityOutputs;
            activity.TypeProperties = sqlserverStoredProcActivity;

            return (activity);
        }
    }
}