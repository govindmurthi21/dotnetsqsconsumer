using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Amazon.SQS;
using Amazon.SQS.Model;
using BkDemoSQSConsumer.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BkDemoSQSConsumer;

public class SqsConsumerService : BackgroundService
{
    private readonly IAmazonSQS _sqs;
    private readonly List<string> _messageAttributeNames = new() { "All" };
    private const string QueueName = "bkdemomicroapponequeue";
    private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        },
        Formatting = Formatting.Indented
    };


    public SqsConsumerService(IAmazonSQS sqs)
    {
        _sqs = sqs;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var queueUrl = await _sqs.GetQueueUrlAsync(QueueName, ct);
        var receiveReq = new ReceiveMessageRequest
        {
            QueueUrl = queueUrl.QueueUrl,
            MessageAttributeNames = _messageAttributeNames,
            AttributeNames = _messageAttributeNames
        };

        while (!ct.IsCancellationRequested)
        {
            var messageResp = await _sqs.ReceiveMessageAsync(receiveReq, ct);

            if (messageResp.HttpStatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("There was error processing messages");
                continue;
            }

            foreach (var message in messageResp.Messages)
            {
                var mess = new SqsMessage()
                {
                    Id = Int32.Parse(message.MessageAttributes["message_id"].StringValue),
                    Message = message.Body,
                    InsertDate = DateTime.Parse(message.MessageAttributes["message_insert_date"].StringValue)
                };
                Console.WriteLine(message);
                Thread.Sleep(5000);
                if (await SendMessageAsync(mess))
                {
                    await _sqs.DeleteMessageAsync(queueUrl.QueueUrl, message.ReceiptHandle, ct);
                }
            }
        }
    }

    private async Task<bool> SendMessageAsync(SqsMessage message)
    {
        Uri uri = new Uri("http://localhost:8000/predict/spamnospam");
        var payload = JsonConvert.SerializeObject(message, jsonSerializerSettings);
        HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var client = new HttpClient();
        var resp = await client.PostAsync(uri, content);
        return resp.IsSuccessStatusCode;
    }
}