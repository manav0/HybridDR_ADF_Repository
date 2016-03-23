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

        //INIT ACTIVITIES
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
            activityOutput.Name = DualLoadConfig.DATASET_SQLOUTPUT;
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

        public Activity create_Activity_Init_3()
        {
            Console.WriteLine("Creating " + DualLoadConfig.ACTIVITY_INIT_3);
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


            activity.Name = DualLoadConfig.ACTIVITY_INIT_3;
            activity.Outputs = activityOutputs;
            activity.TypeProperties = sqlserverStoredProcActivity;

            return (activity);
        }


        public Activity create_Activity_Init_4()
        {
            Console.WriteLine("Creating " + DualLoadConfig.ACTIVITY_INIT_4);

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


            activity.Name = DualLoadConfig.ACTIVITY_INIT_4;
            activity.Inputs = activityInputs;
            activity.Outputs = activityOutputs;
            activity.TypeProperties = copyActivity;
            //activity.Scheduler = scheduler;

            return (activity);
        }

        //LOAD PROCESS ACTIVITIES
        public Activity create_Activity_LoadProcess_3()
        {
            Console.WriteLine("Creating " + DualLoadConfig.ACTIVITY_LOADPROCESS_3);

            Activity activity = new Activity();

            List<ActivityInput> activityInputs = new List<ActivityInput>();
            ActivityInput activityInput = new ActivityInput();
            activityInput.Name = DualLoadConfig.DATASET_ETL_ControlDetail;
            activityInputs.Add(activityInput);
            SqlSource source = new SqlSource();
            source.SqlReaderQuery = (DualLoadConfig.QUERY_LOADPROCESS_3).Replace('?', '3');

            // source.SqlReaderQuery = "Update [dbo].[ETLControlDetail] Set PrimaryAPSStatus = 2 Where Id = 3";
            Console.WriteLine(" source.SqlReaderQuery= " + source.SqlReaderQuery);


            List<ActivityOutput> activityOutputs = new List<ActivityOutput>();
            ActivityOutput activityOutput = new ActivityOutput();
            activityOutput.Name = DualLoadConfig.DATASET_SQLOUTPUT;
            activityOutputs.Add(activityOutput);
            SqlSink sink = new SqlSink();

            CopyActivity copyActivity = new CopyActivity();
            copyActivity.Source = source;
            copyActivity.Sink = sink;

            activity.Name = DualLoadConfig.ACTIVITY_LOADPROCESS_3;
            activity.Inputs = activityInputs;
            activity.Outputs = activityOutputs;
            activity.TypeProperties = copyActivity;

            return (activity);
        }

        public Activity create_Activity_LoadProcess_5()
        {
            Console.WriteLine("Creating " + DualLoadConfig.ACTIVITY_LOADPROCESS_5);

            Activity activity = new Activity();

            List<ActivityInput> activityInputs = new List<ActivityInput>();
            ActivityInput activityInput = new ActivityInput();
            activityInput.Name = DualLoadConfig.DATASET_ETL_Control;
            activityInputs.Add(activityInput);
            SqlSource source = new SqlSource();
            source.SqlReaderQuery = DualLoadConfig.QUERY_LOADPROCESS_5.Replace('?', '3');

            List<ActivityOutput> activityOutputs = new List<ActivityOutput>();
            ActivityOutput activityOutput = new ActivityOutput();
            activityOutput.Name = DualLoadConfig.DATASET_SQLDUMMY;
            activityOutputs.Add(activityOutput);
            SqlSink sink = new SqlSink();

            CopyActivity copyActivity = new CopyActivity();
            copyActivity.Source = source;
            copyActivity.Sink = sink;

            activity.Name = DualLoadConfig.ACTIVITY_LOADPROCESS_5;
            activity.Inputs = activityInputs;
            activity.Outputs = activityOutputs;
            activity.TypeProperties = copyActivity;

            return (activity);
        }

    }
}