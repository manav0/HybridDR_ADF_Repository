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

        /**
        * Stored Proc Activity - usp_RecordFilesToBeProcessed
        */
        public Activity create_Activity_Init_3(String controlID, String filePath)
        {
            Console.WriteLine("Creating " + DualLoadConfig.ACTIVITY_INIT_3);
            IDictionary<string, string> sprocParams = new Dictionary<string, string>();
            //sprocParams.Add("@ETLControlID", "1");
            //sprocParams.Add("@FileName", "Z:\\DimEmployee\\DimEmployee1.csv");

            sprocParams.Add("@ETLControlID", controlID);
            sprocParams.Add("@FileName", filePath);

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

        /**
        * CopyActivity from source to destination dataset
        */
        public Activity create_Activity_Init_4(String sourceDataset, String destinationDataset)
        {
            Console.WriteLine("Creating " + DualLoadConfig.ACTIVITY_INIT_4);

            Activity activity = new Activity();

            List<ActivityInput> activityInputs = new List<ActivityInput>();
            ActivityInput activityInput = new ActivityInput();
            activityInput.Name = sourceDataset;
            activityInputs.Add(activityInput);

            List<ActivityOutput> activityOutputs = new List<ActivityOutput>();
            ActivityOutput activityOutput = new ActivityOutput();
            activityOutput.Name = destinationDataset;
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
        public Activity create_Activity_LoadProcess_3(string ETlControlID)
        {
            Console.WriteLine("Creating " + DualLoadConfig.ACTIVITY_LOADPROCESS_3);

            IDictionary<string, string> sprocParams = new Dictionary<string, string>();

            sprocParams.Add("@ControlProcess", "1");
            sprocParams.Add("@Id", ETlControlID);
            sprocParams.Add("@Status", "2");

            Activity activity = new Activity();

            List<ActivityOutput> activityOutputs = new List<ActivityOutput>();
            ActivityOutput activityOutput = new ActivityOutput();
            activityOutput.Name = DualLoadConfig.DATASET_SQLOUTPUT;
            activityOutputs.Add(activityOutput);

            SqlServerStoredProcedureActivity sqlserverStoredProcActivity = new SqlServerStoredProcedureActivity();
            sqlserverStoredProcActivity.StoredProcedureName = "usp_UpdateControlDetailStatus";
            sqlserverStoredProcActivity.StoredProcedureParameters = sprocParams;


            activity.Name = DualLoadConfig.ACTIVITY_LOADPROCESS_3;
            activity.Outputs = activityOutputs;
            activity.TypeProperties = sqlserverStoredProcActivity;

            return (activity);
        }

        public Activity create_Activity_LoadProcess_5(string ETlControlID)
        {
            Console.WriteLine("Creating " + DualLoadConfig.ACTIVITY_LOADPROCESS_5);

            IDictionary<string, string> sprocParams = new Dictionary<string, string>();

            sprocParams.Add("@ControlProcess", "1");
            sprocParams.Add("@Id", ETlControlID);
            sprocParams.Add("@Status", "3");

            Activity activity = new Activity();

            List<ActivityOutput> activityOutputs = new List<ActivityOutput>();
            ActivityOutput activityOutput = new ActivityOutput();
            activityOutput.Name = DualLoadConfig.DATASET_SQLDUMMY;
            activityOutputs.Add(activityOutput);

            SqlServerStoredProcedureActivity sqlserverStoredProcActivity = new SqlServerStoredProcedureActivity();
            sqlserverStoredProcActivity.StoredProcedureName = "usp_UpdateControlDetailStatus";
            sqlserverStoredProcActivity.StoredProcedureParameters = sprocParams;


            activity.Name = DualLoadConfig.ACTIVITY_LOADPROCESS_5;
            activity.Outputs = activityOutputs;
            activity.TypeProperties = sqlserverStoredProcActivity;

            return (activity);
        }

        //public activity create_activity_loadprocess_5()
        //{
        //    console.writeline("creating " + dualloadconfig.activity_loadprocess_5);

        //    activity activity = new activity();

        //    list<activityinput> activityinputs = new list<activityinput>();
        //    activityinput activityinput = new activityinput();
        //    activityinput.name = dualloadconfig.dataset_etl_control;
        //    activityinputs.add(activityinput);
        //    sqlsource source = new sqlsource();
        //    source.sqlreaderquery = dualloadconfig.query_loadprocess_5.replace('?', '3');

        //    list<activityoutput> activityoutputs = new list<activityoutput>();
        //    activityoutput activityoutput = new activityoutput();
        //    activityoutput.name = dualloadconfig.dataset_sqldummy;
        //    activityoutputs.add(activityoutput);
        //    sqlsink sink = new sqlsink();

        //    copyactivity copyactivity = new copyactivity();
        //    copyactivity.source = source;
        //    copyactivity.sink = sink;

        //    activity.name = dualloadconfig.activity_loadprocess_5;
        //    activity.inputs = activityinputs;
        //    activity.outputs = activityoutputs;
        //    activity.typeproperties = copyactivity;

        //    return (activity);
        //}

    }
}
