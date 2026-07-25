using System.Text.Json;

namespace ISkyPro.Contracts.BotModels;

public sealed record BotMessageEnvelope(
    string Id,
    BotMessageKind Kind,
    DateTimeOffset Timestamp,
    JsonElement Payload)
{
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id)
            && Kind != BotMessageKind.Unknown
            && Payload.ValueKind is not JsonValueKind.Undefined and not JsonValueKind.Null;
    }
}
