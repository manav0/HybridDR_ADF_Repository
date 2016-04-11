using Microsoft.Azure.Management.DataFactories;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;
using System.Collections.ObjectModel;
using Microsoft.Azure;

namespace HybridDR_ADF
{
    /**
     * Logging into Azure & creating Data Factory Management Client
     */
    class ADFLoginController
    {
        private static DataFactoryManagementClient client;

        static void Main(string[] args)
        {
            ADFLoginController loginController = new ADFLoginController();
        }

        public DataFactoryManagementClient getClient()
        {
            return (client);
        }

        public static DataFactoryManagementClient createDataFactoryManagementClient()
        {
            if (client != null)
            {
                Console.WriteLine("Reusing DataFactoryManagementClient");
                return (client);
            }
            Console.WriteLine("Creating DataFactoryManagementClient");
            TokenCloudCredentials aadTokenCredentials = new TokenCloudCredentials(DualLoadConfig.SUBSCRIPTION_ID, GetAuthorizationHeader(DualLoadConfig.AD_TENANT_ID));

            Uri resourceManagerUri = new Uri(ConfigurationManager.AppSettings["ResourceManagerEndpoint"]);

            // create data factory management client
            return (client = new DataFactoryManagementClient(aadTokenCredentials, resourceManagerUri));
        }


        public static string GetAuthorizationHeader(String AD_TENANT_ID)
        {
            AuthenticationResult result = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var context = new AuthenticationContext(ConfigurationManager.AppSettings["ActiveDirectoryEndpoint"] + AD_TENANT_ID);

                    result = context.AcquireToken(
                        resource: ConfigurationManager.AppSettings["WindowsManagementUri"],
                        clientId: ConfigurationManager.AppSettings["AdfClientId"],
                        redirectUri: new Uri(ConfigurationManager.AppSettings["RedirectUri"]),
                        promptBehavior: PromptBehavior.Always);
                }
                catch (Exception threadEx)
                {
                    Console.WriteLine(threadEx.Message);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Name = "AcquireTokenThread";
            thread.Start();
            thread.Join();

            if (result != null)
            {
                return result.AccessToken;
            }

            throw new InvalidOperationException("Failed to acquire token");
        }
    }
}
