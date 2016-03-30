using Microsoft.Azure.Management.DataFactories;
using System;

namespace HybridDR_ADF
{
    class DualLoadMain
    {
        static void Main(string[] args)
        {
            DualLoadMain main = new DualLoadMain();
            DualLoadUtil util = new DualLoadUtil();
            //main.tearDown(util);
            main.initialize(util);
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
            DataFactoryManagementClient client = DualLoadUtil.createDataFactoryManagementClient();
            client.DataFactories.Delete(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name);
        }
    }
}
