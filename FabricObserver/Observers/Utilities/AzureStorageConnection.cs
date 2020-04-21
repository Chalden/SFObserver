using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FabricObserver.Observers.Utilities
{
    class AzureStorageConnection
    {
        public static CloudQueue queueConnection(string queueName)
        {
            //Azure storage account connection (key retrieved from App.config)
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnection"]);

            //Create queue client
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            //Retrieve queue reference
            CloudQueue queue = queueClient.GetQueueReference(queueName);

            //Create queue if not exists
            queue.CreateIfNotExists();

            return queue;
        }
    }
}
