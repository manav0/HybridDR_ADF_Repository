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
    class LoadProcessPipeline
    {
        private static char PdwId = '1';
        private static String ControlProcess = "DimEmployee";

        static void Main(string[] args)
        {
            LoadProcessPipeline loadProcessPipeline = new LoadProcessPipeline();
            DualLoadUtil util = new DualLoadUtil();

            DataFactoryManagementClient client = DualLoadUtil.createDataFactoryManagementClient();
            util.tearDown(client, DualLoadConfig.PIPELINE_LOADPROCESS);
            loadProcessPipeline.createDatasets(client);
            loadProcessPipeline.createPipeline(util, DualLoadConfig.PIPELINE_LOADPROCESS);

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }


        private void createDatasets(DataFactoryManagementClient client)
        {
            DualLoadDatasets datasets = new DualLoadDatasets(client);
            //datasets.createDataSet_ETLControl();
            datasets.createDataSet_ETLControlDetail();
            datasets.createDataSet_SqlOutput();
            datasets.createDataSet_SqlDummy();
            //datasets.createDataSet_root();
            //datasets.createDataSet_ToBeProcessedPath();
        }

        private void createPipeline(DualLoadUtil util, String pipelineName)
        {
            Console.WriteLine("Creating " + pipelineName);
            DateTime PipelineActivePeriodStartTime = new DateTime(2014, 8, 9, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime PipelineActivePeriodEndTime = PipelineActivePeriodStartTime.AddMinutes(60);
            DualLoadActivities dLActivities = new DualLoadActivities();

            DBQuery dbQuery = new DBQuery();
            //List<Dictionary<string, object>> resultList = dbQuery.getResultList(DualLoadConfig.QUERY_LOADPROCESS_1.Replace('?', PdwId));

            List<Dictionary<string, object>> resultList = dbQuery.getResultList(DualLoadConfig.QUERY_LOADPROCESS_2.Replace("$PdwId", "1").Replace("$ControlProcess", "'DimEmployee'"));

            List<Object> controlIdList = new List<Object>();

            foreach (Dictionary<string, object> result in resultList)
            {
                foreach (KeyValuePair<string, object> kvp in result)
                {
                    string key = kvp.Key;
                    object value = kvp.Value;
                    Console.WriteLine("Key: " + key + ", value: " + value);
                    if ("ETLControlDetailID".Equals(key))
                    {
                        controlIdList.Add(value);
                    }
                }
            }

            for (int i = 0; i < controlIdList.Count; i++)
            {
                string controlId = controlIdList.ElementAt(i).ToString();
                Console.WriteLine("controlId " + controlId);

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
                                dLActivities.create_Activity_LoadProcess_3(controlId),
                                dLActivities.create_Activity_LoadProcess_5(controlId)
                }
                                 }
                             }
                         }
                             );
            }


            util.showInteractiveOutput(PipelineActivePeriodStartTime, PipelineActivePeriodEndTime, DualLoadConfig.DATASET_SQLOUTPUT);
        }

    }
}