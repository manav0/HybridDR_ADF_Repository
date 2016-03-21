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
    class DualLoadActivities
    {

        public Activity create_QuerySQL_ETLControl_Activity()
        {
            Console.WriteLine("Creating " + DualLoadConfig.ACTIVITY_QuerySQL_ETLControl);

            Activity activity = new Activity();

            List<ActivityInput> activityInputs = new List<ActivityInput>();
            ActivityInput activityInput = new ActivityInput();
            activityInput.Name = DualLoadConfig.DATASET_ETL_Control;
            activityInputs.Add(activityInput);
            SqlSource source = new SqlSource();
            source.SqlReaderQuery = "$$Text.Format('select id, LastRunDate, FileNameLike, FilePath, ToBeProcessedPath, ArchivePath from [dbo].[ETLControl]')";



            List<ActivityOutput> activityOutputs = new List<ActivityOutput>();
            ActivityOutput activityOutput = new ActivityOutput();
            activityOutput.Name = DualLoadConfig.DATASET_SQLOUTPUT;
            activityOutputs.Add(activityOutput);
            SqlSink sink = new SqlSink();

            CopyActivity copyActivity = new CopyActivity();
            copyActivity.Source = source;
            copyActivity.Sink = sink;

            activity.Name = DualLoadConfig.ACTIVITY_QuerySQL_ETLControl;
            activity.Inputs = activityInputs;
            activity.Outputs = activityOutputs;
            activity.TypeProperties = copyActivity;

            return (activity);
        }

        public Activity create_Record_SProc_Activity()
        {
            Console.WriteLine("Creating " + DualLoadConfig.ACTIVITY_SP_RECORD);
            IDictionary<string, string> sprocParams = new Dictionary<string, string>();
            sprocParams.Add("@ETLControlID", "1");
            sprocParams.Add("@FileName", "Z:\\DimEmployee\\DimEmployee1.csv");

            Activity activity = new Activity();

            List<ActivityOutput> activityOutputs = new List<ActivityOutput>();
            ActivityOutput activityOutput = new ActivityOutput();
            activityOutput.Name = DualLoadConfig.DATASET_SQLOUTPUT;
            activityOutputs.Add(activityOutput);

            SqlServerStoredProcedureActivity sqlserverStoredProcActivity = new SqlServerStoredProcedureActivity();
            sqlserverStoredProcActivity.StoredProcedureName = "dbo.usp_RecordFilesToBeProcessed";
            sqlserverStoredProcActivity.StoredProcedureParameters = sprocParams;


            activity.Name = DualLoadConfig.ACTIVITY_SP_RECORD;
            activity.Outputs = activityOutputs;
            activity.TypeProperties = sqlserverStoredProcActivity;

            return (activity);
        }

        public Activity create_MoveFiles_Activity()
        {
            Console.WriteLine("Creating " + DualLoadConfig.ACTIVITY_MOVE_FILES);

            Activity activity = new Activity();

            List<ActivityInput> activityInputs = new List<ActivityInput>();
            ActivityInput activityInput = new ActivityInput();
            activityInput.Name = DualLoadConfig.DATASET_ROOT;
            activityInputs.Add(activityInput);

            List<ActivityOutput> activityOutputs = new List<ActivityOutput>();
            ActivityOutput activityOutput = new ActivityOutput();
            activityOutput.Name = DualLoadConfig.DATASET_ToBeProcessedPath;
            activityOutputs.Add(activityOutput);

            CopyActivity copyActivity = new CopyActivity();
            copyActivity.Source = new BlobSource();

            BlobSink sink = new BlobSink();
            //sink.WriteBatchSize = 10000;
            //sink.WriteBatchTimeout = TimeSpan.FromMinutes(10);
            copyActivity.Sink = sink;

            //Scheduler scheduler = new Scheduler();
            //scheduler.Frequency = SchedulePeriod.Hour;
            //scheduler.Interval = 1;


            activity.Name = DualLoadConfig.ACTIVITY_MOVE_FILES;
            activity.Inputs = activityInputs;
            activity.Outputs = activityOutputs;
            activity.TypeProperties = copyActivity;
            //activity.Scheduler = scheduler;

            return (activity);
        }

    }
}