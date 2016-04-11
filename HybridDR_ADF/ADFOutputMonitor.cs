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

using Microsoft.Azure.Management.DataFactories;
using Microsoft.Azure.Management.DataFactories.Models;
using Microsoft.Azure.Management.DataFactories.Common.Models;

namespace HybridDR_ADF
{
    /**
     * Monitoring ADF output dataset slices
     */
    class ADFOutputMonitor
    {
        private DataFactoryManagementClient client;
        public ADFOutputMonitor(DataFactoryManagementClient client)
        {
            this.client = client;
        }
        public void showInteractiveOutput(DateTime PipelineActivePeriodStartTime, DateTime PipelineActivePeriodEndTime, String outputDataSetName)
        {
            // Pulling status within a timeout threshold
            DateTime start = DateTime.Now;
            bool done = false;

            while (DateTime.Now - start < TimeSpan.FromMinutes(5) && !done)
            {
                Console.WriteLine("Pulling the slice status");
                // wait before the next status check
                Thread.Sleep(1000 * 12);

                var datalistResponse = client.DataSlices.List(DualLoadConfig.RESOURCEGROUP_Name, DualLoadConfig.DATAFACTORY_Name, outputDataSetName,
                new DataSliceListParameters()
                {
                    DataSliceRangeStartTime = PipelineActivePeriodStartTime.ConvertToISO8601DateTimeString(),
                    DataSliceRangeEndTime = PipelineActivePeriodEndTime.ConvertToISO8601DateTimeString()
                });

                foreach (DataSlice slice in datalistResponse.DataSlices)
                {
                    if (slice.State == DataSliceState.Failed || slice.State == DataSliceState.Ready)
                    {
                        Console.WriteLine("Slice execution is done with status: {0}", slice.State);
                        done = true;
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Slice status is: {0}", slice.State);
                    }
                }
            }
            Console.WriteLine("Getting run details of a data slice");

            // give it a few minutes for the output slice to be ready
            Console.WriteLine("\nGive it a few minutes for the output slice to be ready and press any key.");
            Console.ReadKey();

            var datasliceRunListResponse = client.DataSliceRuns.List(
                    DualLoadConfig.RESOURCEGROUP_Name,
                    DualLoadConfig.DATAFACTORY_Name,
                    outputDataSetName,
                    new DataSliceRunListParameters()
                    {
                        DataSliceStartTime = PipelineActivePeriodStartTime.ConvertToISO8601DateTimeString()
                    }
                );

            foreach (DataSliceRun run in datasliceRunListResponse.DataSliceRuns)
            {
                Console.WriteLine("Status: \t\t{0}", run.Status);
                Console.WriteLine("DataSliceStart: \t{0}", run.DataSliceStart);
                Console.WriteLine("DataSliceEnd: \t\t{0}", run.DataSliceEnd);
                Console.WriteLine("ActivityId: \t\t{0}", run.ActivityName);
                Console.WriteLine("ProcessingStartTime: \t{0}", run.ProcessingStartTime);
                Console.WriteLine("ProcessingEndTime: \t{0}", run.ProcessingEndTime);
                Console.WriteLine("ErrorMessage: \t{0}", run.ErrorMessage);
            }
        }

    }
}
