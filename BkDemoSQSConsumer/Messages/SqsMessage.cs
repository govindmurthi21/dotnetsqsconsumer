using System.Text.Json.Serialization;

namespace BkDemoSQSConsumer.Messages;

public class SqsMessage
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    [JsonPropertyName("message")] public string Message { get; init; }
    [JsonPropertyName("insertDate")]
    public DateTime InsertDate { get; init; }
    [JsonPropertyName("spamOrNot")]
    public string SpamOrNot { get; init; }
    [JsonPropertyName("updateDate")]
    public DateTime UpdateDate { get; init; }
}