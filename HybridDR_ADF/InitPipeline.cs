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
    class InitPipeline
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
            InitPipeline initPipeline = new InitPipeline();
            DualLoadUtil util = new DualLoadUtil();

            DataFactoryManagementClient client = DualLoadUtil.createDataFactoryManagementClient();
            //util.tearDown(client, DualLoadConfig.PIPELINE_INIT);

            initPipeline.executeDBQuery_Step1();
            initPipeline.executeStorageQuery_Step2();

            DualLoadDatasets datasets = initPipeline.createDatasets(client);
            util.setDatasets(datasets);
            initPipeline.createPipelines(util, DualLoadConfig.PIPELINE_INIT);

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }

        private void executeDBQuery_Step1()
        {
            AzureSQLController sqlController = new AzureSQLController();
            List<Dictionary<string, object>> resultList = sqlController.getResultList(DualLoadConfig.QUERY_INIT_1);

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
            //Console.WriteLine("Creating " + basePipelineName);
            //DateTime PipelineActivePeriodStartTime = new DateTime(2014, 8, 9, 0, 0, 0, 0, DateTimeKind.Utc);
            //DateTime PipelineActivePeriodEndTime = PipelineActivePeriodStartTime.AddMinutes(60);
            DualLoadActivities dLActivities = new DualLoadActivities();
            int i = 0;
            foreach (string file in SOURCE_FOLDER_FILELIST)
            {
                DateTime PipelineActivePeriodStartTime = new DateTime(2014, 8, 9, i, 0, 0, 0, DateTimeKind.Local);
                DateTime PipelineActivePeriodEndTime = PipelineActivePeriodStartTime.AddMinutes(60);
                Console.WriteLine("file being processed: " + file);

                util.getDatasets().createDataSet_SourceFolder(CONTROL_PROCESS, SOURCE_FOLDER_PATH, file);
                util.getDatasets().createDataSet_ToBeProcessedPath(CONTROL_PROCESS, TOBEPROCESSED_FOLDER_PATH, file);
                util.getDatasets().createDataSet_Init_SqlDummy(i);
                Console.WriteLine("Creating Pipeline: " + basePipelineName + "_" + i);



                util.getClient().Pipelines.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                                     new PipelineCreateOrUpdateParameters()
                                     {
                                         Pipeline = new Pipeline()
                                         {
                                             Name = basePipelineName + "_" + i,
                                             Properties = new PipelineProperties()
                                             {
                                                 Description = "DualLoadInit Pipeline will pull all files to be processed in central location",

                                                 // Initial value for pipeline's active period. With this, you won't need to set slice status

                                                 Start = PipelineActivePeriodStartTime,
                                                 End = PipelineActivePeriodEndTime,


                                                 Activities = new List<Activity>()
                                                 {
                                    dLActivities.create_Activity_Init_3(CONTROL_ID, file, i),
                                    dLActivities.create_Activity_Init_4(DualLoadConfig.DATASET_SOURCEFOLDER, DualLoadConfig.DATASET_ToBeProcessedFolder)
                                                 }
                                             }
                                         }
                                     }
                                         );
                util.showInteractiveOutput(PipelineActivePeriodStartTime, PipelineActivePeriodEndTime, DualLoadConfig.DATASET_ToBeProcessedFolder);
                i++;
                storageController.deleteBlob(CONTROL_PROCESS, SOURCE_FOLDER_PATH, file);
            }
            //util.showInteractiveOutput(PipelineActivePeriodStartTime, PipelineActivePeriodEndTime, DualLoadConfig.DATASET_ToBeProcessedFolder);
        }

    }
}