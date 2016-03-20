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
    class DualLoadInit
    {
        static void Main(string[] args)
        {
            DualLoadInit dualLoadInit = new DualLoadInit();
            DualLoadUtil util = new DualLoadUtil();

            DataFactoryManagementClient client = util.createDataFactoryManagementClient();
            dualLoadInit.tearDown(client);
            //return;
            util.createDataFactory();
            util.createLinkedService_ControlDB();
            util.createLinkedService_BlobStorage();
            dualLoadInit.createDatasets(client);
            dualLoadInit.createPipeline_Init(util);

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }


        private void createDatasets(DataFactoryManagementClient client)
        {
            DualLoadDatasets datasets = new DualLoadDatasets(client);

            //dualLoadInit.createDataSet_PDW(client);
            //dualLoadInit.createOutputDataSet(client);
            //util.createDataSet_BlobStore(client);
            datasets.createDataSet_SqlOutput();
            datasets.createDataSet_root();
            datasets.createDataSet_ToBeProcessedPath();
        }

        private void tearDown(DataFactoryManagementClient client)
        {
            Console.WriteLine("Tearing down " + DualLoadConfig.DATAFACTORY_Name);
            client.DataFactories.Delete(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name);
        }

        private void createPipeline_Init(DualLoadUtil util)
        {
            Console.WriteLine("Creating " + DualLoadConfig.PIPELINE_INIT_Name);
            DateTime PipelineActivePeriodStartTime = new DateTime(2014, 8, 9, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime PipelineActivePeriodEndTime = PipelineActivePeriodStartTime.AddMinutes(60);


            util.getClient().Pipelines.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                 new PipelineCreateOrUpdateParameters()
                 {
                     Pipeline = new Pipeline()
                     {
                         Name = DualLoadConfig.PIPELINE_INIT_Name,
                         Properties = new PipelineProperties()
                         {
                             Description = "DualLoadInit Pipeline will pull all files to be processed in central location",

                             // Initial value for pipeline's active period. With this, you won't need to set slice status
                             Start = PipelineActivePeriodStartTime,
                             End = PipelineActivePeriodEndTime,

                             Activities = new List<Activity>()
                             {
                                 //create_QuerySQL_ETLControl_Activity()
                                 //create_Record_SProc_Activity()
                                 create_MoveFiles_Activity()
        }
                         }
                     }
                 }
                     );

            util.showInteractiveOutput(PipelineActivePeriodStartTime, PipelineActivePeriodEndTime);
        }


        private Activity create_QuerySQL_ETLControl_Activity()
        {
            Console.WriteLine("Creating " + DualLoadConfig.ACTIVITY_QuerySQL_ETLControl);
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


            activity.Name = DualLoadConfig.ACTIVITY_QuerySQL_ETLControl;
            activity.Outputs = activityOutputs;
            activity.TypeProperties = sqlserverStoredProcActivity;

            return (activity);
        }

        private Activity create_Record_SProc_Activity()
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

        private Activity create_MoveFiles_Activity()
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
