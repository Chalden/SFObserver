using System;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace QueueTest
{
    class Program
    {
        static void Main(string[] args)
        {
            bool continueOk = true;

            //Azure storage account connection (retrieved key from App.config)
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnection"));

            //Queue connection
            CloudQueueClient QueueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = QueueClient.GetQueueReference("queuetest");

            //Create queue if not exists
            queue.CreateIfNotExists();

            while (continueOk)
            {
                //Add or remove a message
                Console.WriteLine("\nAdd (a), Remove (b) or Inscrement DequeueCount (c)");
                string choice = Console.ReadLine();

                if (choice.Equals("a"))
                {
                    //Ask user to type a message
                    Console.WriteLine("\nPlease type a message to add to the queue:");
                    string input = Console.ReadLine();

                    //Add message to the queue
                    TimeSpan expTime = new TimeSpan(24, 0, 0);
                    CloudQueueMessage message = new CloudQueueMessage(input);
                    queue.AddMessage(message, expTime, null, null);
                    Console.WriteLine("Message inserted");
                }
                else if (choice.Equals("b"))
                {
                    //Remove message from the queue
                    CloudQueueMessage retrievedMessage = queue.GetMessage();
                    queue.DeleteMessage(retrievedMessage);
                    Console.WriteLine("Message \"" + retrievedMessage.AsString + "\" deleted");
                }
                else if (choice.Equals("c"))
                {
                    //Increment dequeue count (change visibility message during 5sec)
                    CloudQueueMessage retrievedMessage = queue.GetMessage(visibilityTimeout: TimeSpan.FromSeconds(5));
                    Console.WriteLine("DequeueCount incremented");
                }

                Console.WriteLine("\nContinue ?\nYes (a)\nNo (b)");
                string answer = Console.ReadLine();
                if (answer.Equals("b"))
                {
                    continueOk = false;
                }
            }
        }
    }
}
