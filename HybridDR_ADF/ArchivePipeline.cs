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
    class ArchivePipeline
    {
        private static int CONTROLDETAIL_ID;
        private static String
                        CONTROL_PROCESS = "dimemployee",
                        FILENAME,
                        ARCHIVED_FOLDER_PATH,
                        TOBEPROCESSED_FOLDER_PATH = "ToBeProcessed/";

        private AzureStorageController
                        storageController;
        static void Main(string[] args)
        {
            ArchivePipeline archivePipeline = new ArchivePipeline();
            DualLoadUtil util = new DualLoadUtil();

            DataFactoryManagementClient client = DualLoadUtil.createDataFactoryManagementClient();
            //util.tearDown(client, DualLoadConfig.PIPELINE_ARCHIVE);

            DualLoadDatasets datasets = archivePipeline.createDatasets(client);
            util.setDatasets(datasets);
            archivePipeline.initStorageController();
            archivePipeline.createPipelines(util, DualLoadConfig.PIPELINE_ARCHIVE);

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }

        private void initStorageController()
        {
            storageController = new AzureStorageController();
        }

        private DualLoadDatasets createDatasets(DataFactoryManagementClient client)
        {
            DualLoadDatasets datasets = new DualLoadDatasets(client);
            //;datasets.createDataSet_SqlOutput();
            //datasets.createDataSet_ToBeProcessedPath(CONTROL_PROCESS, TOBEPROCESSED_FOLDER_PATH);
            return (datasets);
        }

        private void createPipelines(DualLoadUtil util, String basePipelineName)
        {
            //Console.WriteLine("Creating " + basePipelineName);
            DateTime PipelineActivePeriodStartTime = new DateTime(2014, 8, 9, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime PipelineActivePeriodEndTime = PipelineActivePeriodStartTime.AddMinutes(60);
            DualLoadActivities dLActivities = new DualLoadActivities();
            int i = 0;
            AzureSQLController sqlController = new AzureSQLController();
            List<Dictionary<string, object>> resultList = sqlController.getResultList(DualLoadConfig.QUERY_ARCHIVE_1.Replace("$ControlProcess", "'dimEmployee'"));


            foreach (Dictionary<string, object> result in resultList)
            {
                foreach (KeyValuePair<string, object> kvp in result)
                {
                    string key = kvp.Key;
                    object value = kvp.Value;
                    //Console.WriteLine("Key: " + key + ", value: " + value);
                    CONTROLDETAIL_ID = ("ETLControlDetailID".Equals(key)) ? (int)value : CONTROLDETAIL_ID;
                    FILENAME = ("FileName".Equals(key)) ? value.ToString() : FILENAME;
                    ARCHIVED_FOLDER_PATH = ("ArchivePath".Equals(key)) ? value.ToString() : ARCHIVED_FOLDER_PATH;
                    Console.WriteLine("CONTROLDETAIL_ID = " + CONTROLDETAIL_ID);
                    Console.WriteLine("FILENAME = " + FILENAME);
                    Console.WriteLine("ARCHIVED_FOLDER_PATH = " + ARCHIVED_FOLDER_PATH);
                }

                //foreach (string file in SOURCE_FOLDER_FILELIST)
                //{
                Console.WriteLine("file being processed: " + FILENAME);
                String pipelineName = basePipelineName + "_" + i;

                util.getDatasets().createDataSet_ToBeProcessedPath(CONTROL_PROCESS, TOBEPROCESSED_FOLDER_PATH, FILENAME, pipelineName, i);
                util.getDatasets().createDataSet_ArchivedFolder(CONTROL_PROCESS, ARCHIVED_FOLDER_PATH, FILENAME, i);
                util.getDatasets().createDataSet_Archive_1_SqlDummy(i);

                Console.WriteLine("Creating Pipeline: " + pipelineName);


                util.getClient().Pipelines.CreateOrUpdate(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name,
                     new PipelineCreateOrUpdateParameters()
                     {
                         Pipeline = new Pipeline()
                         {
                             Name = pipelineName,
                             Properties = new PipelineProperties()
                             {
                                 Description = "Archive Pipeline will pull all files to be processed in archived location",

                                 // Initial value for pipeline's active period. With this, you won't need to set slice status
                                 Start = PipelineActivePeriodStartTime,
                                 End = PipelineActivePeriodEndTime,

                                 Activities = new List<Activity>()
                                 {
                                    dLActivities.create_Activity_Archive_2(DualLoadConfig.DATASET_ToBeProcessedFolder + "_" + pipelineName, DualLoadConfig.DATASET_ArchivedFolder, i),
                                    dLActivities.create_Activity_Archive_3(CONTROLDETAIL_ID, FILENAME, i)
                                 }
                             }
                         }
                     }
                         );
                util.showInteractiveOutput(PipelineActivePeriodStartTime, PipelineActivePeriodEndTime, DualLoadConfig.DATASET_ArchivedFolder + "_" + i);
                i++;

                storageController.deleteBlob(CONTROL_PROCESS, TOBEPROCESSED_FOLDER_PATH, FILENAME);
            }
        }


    }
}
