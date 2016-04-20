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
    * Init workflow begins the dual load process by retrieving a list of flat files stored in a directory and recording the location and filename in the control db.
    */
    class InitWorkflow
    {
        private static int CONTROL_ID;
        private static DateTime LASTRUN_DATE;
        private static String
                        CONTROL_PROCESS,
                        FILENAME_LIKE,
                        SOURCE_FOLDER_PATH,
                        TOBEPROCESSED_FOLDER_PATH,
                        ARCHIVE_FOLDER_PATH;
        private static List<String>
                        SOURCE_FOLDER_FILELIST;

        private AzureStorageController
                        storageController;

        static void Main(string[] args)
        {
            InitWorkflow initWorkflow = new InitWorkflow();
            DualLoadUtil util = new DualLoadUtil();

            DataFactoryManagementClient client = ADFLoginController.createDataFactoryManagementClient();
            //util.tearDown(client, DualLoadConfig.PIPELINE_INIT);

            util.setADFMonitor(new ADFOutputMonitor(client));

            initWorkflow.executeDBQuery_Step1();
            initWorkflow.executeStorageQuery_Step2();

            DualLoadDatasets datasets = initWorkflow.createDatasets(client);
            util.setDatasets(datasets);
            initWorkflow.createPipelines(util, DualLoadConfig.PIPELINE_INIT);

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }

        private void executeDBQuery_Step1()
        {
            AzureSQLController sqlController = new AzureSQLController();
            List<Dictionary<string, object>> resultList = sqlController.executeSQLQuery(DualLoadConfig.CONNECTION_STRING_ControlDB, DualLoadConfig.QUERY_INIT_1);

            List<Object> controlIdList = new List<Object>();
            foreach (Dictionary<string, object> result in resultList)
            {
                foreach (KeyValuePair<string, object> kvp in result)
                {
                    string key = kvp.Key;
                    object value = kvp.Value;
                    //Console.WriteLine("Key: " + key + ", value: " + value);
                    CONTROL_ID = ("id".Equals(key)) ? (int)value : CONTROL_ID;
                    LASTRUN_DATE = ("LastRunDate".Equals(key)) ? (DateTime)value : LASTRUN_DATE;
                    FILENAME_LIKE = ("FileNameLike".Equals(key)) ? value.ToString() : FILENAME_LIKE;
                    CONTROL_PROCESS = ("ControlProcess".Equals(key)) ? value.ToString() : CONTROL_PROCESS;
                    SOURCE_FOLDER_PATH = ("FilePath".Equals(key)) ? value.ToString() : SOURCE_FOLDER_PATH;
                    TOBEPROCESSED_FOLDER_PATH = ("ToBeProcessedPath".Equals(key)) ? value.ToString() : TOBEPROCESSED_FOLDER_PATH;
                    ARCHIVE_FOLDER_PATH = ("ArchivePath".Equals(key)) ? value.ToString() : ARCHIVE_FOLDER_PATH;
                }
            }

            Console.WriteLine("CONTROL_ID = " + CONTROL_ID);
            Console.WriteLine("CONTROL_PROCESS = " + CONTROL_PROCESS);
            Console.WriteLine("LASTRUN_DATE = " + LASTRUN_DATE);
            Console.WriteLine("FILENAME_LIKE = " + FILENAME_LIKE);
            Console.WriteLine("SOURCE_FOLDER_PATH = " + SOURCE_FOLDER_PATH);
            Console.WriteLine("TOBEPROCESSED_FOLDER_PATH = " + TOBEPROCESSED_FOLDER_PATH);
            Console.WriteLine("ARCHIVE_FOLDER_PATH = " + ARCHIVE_FOLDER_PATH);
        }

        private void executeStorageQuery_Step2()
        {
            storageController = new AzureStorageController();
            SOURCE_FOLDER_FILELIST = storageController.getFilePathList(CONTROL_PROCESS, SOURCE_FOLDER_PATH);
            Console.WriteLine("SOURCE_FOLDER_FILELIST = " + SOURCE_FOLDER_FILELIST);
        }


        private DualLoadDatasets createDatasets(DataFactoryManagementClient client)
        {
            DualLoadDatasets datasets = new DualLoadDatasets(client);
            //datasets.createDataSet_SqlOutput();
            //datasets.createDataSet_ToBeProcessedPath(CONTROL_PROCESS, TOBEPROCESSED_FOLDER_PATH);
            return (datasets);
        }

        private void createPipelines(DualLoadUtil util, String basePipelineName)
        {
            DualLoadActivities dLActivities = new DualLoadActivities();
            int i = 0; //i represents # of Init pipelines that will get created
            foreach (string file in SOURCE_FOLDER_FILELIST)
            {
                DateTime PipelineActivePeriodStartTime = new DateTime(2014, 8, 9, 1, 0, 0, 0, DateTimeKind.Local).AddMinutes(60);
                DateTime PipelineActivePeriodEndTime = PipelineActivePeriodStartTime.AddMinutes(60);
                Console.WriteLine("file being processed: " + file);
                String pipelineName = basePipelineName + "_" + i;

                util.getDatasets().createDataSet_SourceFolder(CONTROL_PROCESS, SOURCE_FOLDER_PATH, file, i);
                util.getDatasets().createDataSet_ToBeProcessedPath(CONTROL_PROCESS, TOBEPROCESSED_FOLDER_PATH, file, pipelineName, i);
                util.getDatasets().createDataSet_Init_SqlDummy(i);
                Console.WriteLine("Creating Pipeline: " + pipelineName);



                util.getClient().Pipelines.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                                     new PipelineCreateOrUpdateParameters()
                                     {
                                         Pipeline = new Pipeline()
                                         {
                                             Name = pipelineName,
                                             Properties = new PipelineProperties()
                                             {
                                                 Description = "DualLoadInit Pipeline will pull all files to be processed in central location",

                                                 // Initial value for pipeline's active period. With this, you won't need to set slice status

                                                 Start = PipelineActivePeriodStartTime,
                                                 End = PipelineActivePeriodEndTime,


                                                 Activities = new List<Activity>()
                                                 {
                                    dLActivities.create_Activity_Init_3(CONTROL_ID, file, i),
                                    dLActivities.create_Activity_Init_4(DualLoadConfig.DATASET_SOURCEFOLDER + "_" + i, DualLoadConfig.DATASET_ToBeProcessedFolder + "_" + pipelineName, i)
                                                 }
                                             }
                                         }
                                     }
                                         );
                util.getADFMonitor().monitorPipelineOutput(PipelineActivePeriodStartTime, PipelineActivePeriodEndTime, DualLoadConfig.DATASET_ToBeProcessedFolder + "_" + pipelineName);
                i++;
                storageController.deleteBlob(CONTROL_PROCESS, SOURCE_FOLDER_PATH, file);
            }
        }

    }
}