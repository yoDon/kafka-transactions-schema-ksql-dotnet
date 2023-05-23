using Newtonsoft.Json;

namespace KafkaSchema;

public class PurchasedItem
{
    [JsonRequired] // optionally use Newtonsoft.Json annotations
    [JsonProperty("name")] // optionally customize property names
    public string Name { get; set; } = string.Empty;

    [JsonRequired]
    [JsonProperty("what_color")]
    public string Color { get; set; } = string.Empty;

    [JsonProperty("how_many")]
    public long Quantity { get; set; }
}
