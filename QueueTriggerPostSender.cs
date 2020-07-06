using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Company.Function
{
    public static class QueueTriggerPostSender
    {
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("QueueTriggerPostSender")]
        public static void Run([QueueTrigger("incoming-orders", Connection = "AzureWebJobsStorage")] string myQueueItem, ILogger log)
        {
            /*
                Correlation ID creation
            */
            var CustomCorrelationId = Guid.NewGuid();
            /*
                Defined on the local.settings.json - Need to be configured on Azure Function via Portal or AZ CLI
                local.settings.json - Local Test Environment
                Definitions on Azure Functions at Configuration
            */
            var apimUrl = Environment.GetEnvironmentVariable("apimUrlConf");
            var apimProductId = Environment.GetEnvironmentVariable("apimprodIdConf");

            // Add authorization token
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apimProductId);

            // POST - OK 200 - To clean CustomCorrelationId variable
            if (httpClient.DefaultRequestHeaders.Contains("CustomCorrelationId"))
            {
                httpClient.DefaultRequestHeaders.Remove("CustomCorrelationId");
            }
            // POST - OK 200 - Adding CustomCorrelationId to the HTTP Header
            httpClient.DefaultRequestHeaders.Add("CustomCorrelationId", CustomCorrelationId.ToString());

            var responseMessage = httpClient.PostAsync(apimUrl, new StringContent(myQueueItem, Encoding.UTF8, "application/json"));
            log.LogInformation($"Queue Content: {myQueueItem}");
            log.LogInformation($"### CustomCorrelationId generated: { CustomCorrelationId }");
            log.LogInformation($"Response do Post II: {responseMessage.Result}");
        }
    }
}

