using Amazon;
using Amazon.SQS;
using BkDemoSQSConsumer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<SqsConsumerService>();
builder.Services.AddSingleton<IAmazonSQS>(_ => new AmazonSQSClient(RegionEndpoint.USEast1));

var app = builder.Build();

app.Run();