using System;
using System.Collections.Generic;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace KafkaCreateTopics
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true);

            var config = builder.Build();

            var topicConfig = new ProducerConfig
            {
                BootstrapServers = config["Settings:KafkaServer"],
                ClientId = Dns.GetHostName(),

            };
            var topics = new List<String>();
            topics.Add("logging");
            topics.Add("user-add");
            topics.Add("role-add");
            topics.Add("user-update");
            topics.Add("user-update-password");
            topics.Add("user-banned");
            topics.Add("user-role-add");
            topics.Add("twittor-content-add");
            topics.Add("twittor-content-delete");
            topics.Add("comment-content-add");

            foreach (var topic in topics)
            {
                using (var adminClient = new AdminClientBuilder(topicConfig).Build())
                {
                    Console.WriteLine("Creating a topic....");
                    try
                    {
                        await adminClient.CreateTopicsAsync(new List<TopicSpecification> {
                        new TopicSpecification { Name = topic, NumPartitions = 1, ReplicationFactor = 1 } });
                    }
                    catch (CreateTopicsException e)
                    {
                        if (e.Results[0].Error.Code != ErrorCode.TopicAlreadyExists)
                        {
                            Console.WriteLine($"An error occured creating topic {topic}: {e.Results[0].Error.Reason}");
                        }
                        else
                        {
                            Console.WriteLine("Topic already exists");
                        }
                    }
                }
            }

            return 0;

        }
    }
}
