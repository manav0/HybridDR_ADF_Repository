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
        static void Main(string[] args)
        {
            InitPipeline initPipeline = new InitPipeline();
            DualLoadUtil util = new DualLoadUtil();

            DataFactoryManagementClient client = DualLoadUtil.createDataFactoryManagementClient();
            util.tearDown(client, DualLoadConfig.PIPELINE_INIT);
            //return;
            //util.createDataFactory();
            //util.createLinkedService_ControlDB();
            //util.createLinkedService_BlobStorage();
            initPipeline.createDatasets(client);
            initPipeline.createPipeline(util, DualLoadConfig.PIPELINE_INIT);

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }


        private void createDatasets(DataFactoryManagementClient client)
        {
            DualLoadDatasets datasets = new DualLoadDatasets(client);

            //dualLoadInit.createDataSet_PDW(client);
            //dualLoadInit.createOutputDataSet(client);
            //util.createDataSet_BlobStore(client);
            datasets.createDataSet_ETLControl();
            datasets.createDataSet_SqlOutput();
            datasets.createDataSet_root();
            datasets.createDataSet_ToBeProcessedPath();
        }

        private void createPipeline(DualLoadUtil util, String pipelineName)
        {
            Console.WriteLine("Creating " + pipelineName);
            DualLoadActivities dualLoad = new DualLoadActivities();
            DateTime PipelineActivePeriodStartTime = new DateTime(2014, 8, 9, 0, 0, 0, 0, DateTimeKind.Utc);
            DateTime PipelineActivePeriodEndTime = PipelineActivePeriodStartTime.AddMinutes(60);


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
                                 dualLoad.create_Activity_Init_1()
                                 //dualLoad.create_Record_SProc_Activity()
                                 //dualLoad.create_MoveFiles_Activity()
        }
                         }
                     }
                 }
                     );

            util.showInteractiveOutput(PipelineActivePeriodStartTime, PipelineActivePeriodEndTime, "@Todo-OUTPUT DATASET");
        }

    }
}