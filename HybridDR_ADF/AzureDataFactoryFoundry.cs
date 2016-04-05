using Microsoft.Azure.Management.DataFactories;
using System;

namespace HybridDR_ADF
{
    /**
    * Foundry to create Data Factory, Linked Services
    * also has utility methods to tear down data factory & pipelines
    */
    class AzureDataFactoryFoundry
    {
        static void Main(string[] args)
        {
            AzureDataFactoryFoundry adfFoundry = new AzureDataFactoryFoundry();
            DualLoadUtil util = new DualLoadUtil();
            //main.tearDown(util);
            adfFoundry.initialize(util);
        }

        public void initialize(DualLoadUtil util)
        {
            util.createDataFactory();
            util.createLinkedService_BlobStorage();
            util.createLinkedService_ControlDB();
        }

        public void tearDown(DualLoadUtil util)
        {
            Console.WriteLine("Tearing down " + DualLoadConfig.DATAFACTORY_Name);
            DataFactoryManagementClient client = AzureLoginController.createDataFactoryManagementClient();
            client.DataFactories.Delete(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name);
            //util.teardown(client, dualloadconfig.pipeline_init);
            //util.teardown(client, dualloadconfig.pipeline_loadprocess);
            //util.teardown(client, dualloadconfig.pipeline_archive);
        }
    }
}
