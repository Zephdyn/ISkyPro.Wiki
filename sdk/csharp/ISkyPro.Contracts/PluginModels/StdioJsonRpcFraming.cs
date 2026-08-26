using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace ISkyPro.Contracts.PluginModels;

public static class StdioJsonRpcFraming
{
    public const int MaxHeaderLength = PluginSdkV2Protocol.MaxHeaderLength;
    public const int MaxPayloadLength = PluginSdkV2Protocol.MaxPayloadLength;

    private static readonly byte[] HeaderTerminator = "\r\n\r\n"u8.ToArray();

    public static async ValueTask WriteRawAsync(
        Stream stream,
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken)
    {
        using var validatedPayload = ParseAndValidatePayload(payload.Span);
        var header = Encoding.ASCII.GetBytes($"Content-Length: {payload.Length}\r\n\r\n");
        await stream.WriteAsync(header, cancellationToken);
        await stream.WriteAsync(payload, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    public static async ValueTask<JsonDocument?> ReadAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        var payloadLength = await ReadHeaderAsync(stream, cancellationToken);
        if (payloadLength is null)
        {
            return null;
        }

        var rented = ArrayPool<byte>.Shared.Rent(payloadLength.Value);
        try
        {
            var payload = rented.AsMemory(0, payloadLength.Value);
            await ReadExactOrEndAsync(stream, payload, cancellationToken);
            // JsonDocument.Parse(ReadOnlyMemory<byte>) retains the supplied memory. The read buffer
            // comes from ArrayPool and is returned below, so the document must own a copy first.
            return ParseAndValidatePayload(payload.Span);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private static async ValueTask<int?> ReadHeaderAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        var bytes = new List<byte>(128);
        var oneByte = new byte[1];

        while (bytes.Count < MaxHeaderLength)
        {
            var read = await stream.ReadAsync(oneByte.AsMemory(0, oneByte.Length), cancellationToken);
            if (read == 0)
            {
                if (bytes.Count == 0)
                {
                    return null;
                }

                throw new EndOfStreamException("stdio-jsonrpc stream ended in the middle of a header.");
            }

            bytes.Add(oneByte[0]);
            if (EndsWithHeaderTerminator(bytes))
            {
                return ParseContentLength(bytes);
            }
        }

        throw new InvalidDataException($"stdio-jsonrpc header exceeds {MaxHeaderLength} bytes.");
    }

    private static int ParseContentLength(IReadOnlyList<byte> headerBytes)
    {
        var headerText = Encoding.ASCII.GetString(headerBytes.Take(headerBytes.Count - HeaderTerminator.Length).ToArray());
        int? contentLength = null;
        foreach (var line in headerText.Split("\r\n", StringSplitOptions.None))
        {
            if (line.Length == 0)
            {
                continue;
            }

            const string prefix = "Content-Length:";
            if (!line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("stdio-jsonrpc stdout contains non-protocol data before a frame.");
            }

            if (contentLength is not null)
            {
                throw new InvalidDataException("stdio-jsonrpc Content-Length header must appear exactly once.");
            }

            var value = line[prefix.Length..].Trim();
            if (!int.TryParse(value, CultureInfo.InvariantCulture, out var parsedLength))
            {
                throw new InvalidDataException("stdio-jsonrpc Content-Length header is invalid.");
            }

            contentLength = parsedLength;
        }

        return contentLength ?? throw new InvalidDataException("stdio-jsonrpc Content-Length header is missing.");
    }

    private static JsonDocument ParseAndValidatePayload(ReadOnlySpan<byte> payload)
    {
        if (payload.Length <= 0 || payload.Length > MaxPayloadLength)
        {
            throw new InvalidDataException($"Invalid JSON-RPC payload length: {payload.Length}.");
        }

        var document = JsonDocument.Parse(payload.ToArray());
        try
        {
            ValidatePayload(document);
            return document;
        }
        catch
        {
            document.Dispose();
            throw;
        }
    }

    /// <summary>
    /// 对已解析的 JSON-RPC 帧做结构校验（jsonrpc 2.0、method 或 result/error+id 二选一）。
    /// 供 Main 侧缓冲帧读取器复用，避免两套校验逻辑漂移。
    /// </summary>
    public static void ValidatePayload(JsonDocument document)
    {
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidDataException("JSON-RPC payload must be an object.");
        }

        if (!root.TryGetProperty("jsonrpc", out var jsonRpc)
            || jsonRpc.ValueKind != JsonValueKind.String
            || !string.Equals(jsonRpc.GetString(), PluginSdkV2Protocol.JsonRpcVersion, StringComparison.Ordinal))
        {
            throw new InvalidDataException("JSON-RPC payload must declare jsonrpc 2.0.");
        }

        var hasMethod = root.TryGetProperty("method", out var method);
        var hasResult = root.TryGetProperty("result", out _);
        var hasError = root.TryGetProperty("error", out _);
        var hasId = root.TryGetProperty("id", out _);

        if (hasMethod)
        {
            if (method.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(method.GetString()))
            {
                throw new InvalidDataException("JSON-RPC method must be a non-empty string.");
            }

            return;
        }

        if (hasResult == hasError)
        {
            throw new InvalidDataException("JSON-RPC response must contain exactly one of result or error.");
        }

        if (!hasId)
        {
            throw new InvalidDataException("JSON-RPC response must contain id.");
        }
    }

    private static async ValueTask ReadExactOrEndAsync(
        Stream stream,
        Memory<byte> buffer,
        CancellationToken cancellationToken)
    {
        var totalRead = 0;
        while (totalRead < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer[totalRead..], cancellationToken);
            if (read == 0)
            {
                throw new EndOfStreamException("stdio-jsonrpc stream ended in the middle of a frame.");
            }

            totalRead += read;
        }
    }

    private static bool EndsWithHeaderTerminator(IReadOnlyList<byte> bytes)
    {
        if (bytes.Count < HeaderTerminator.Length)
        {
            return false;
        }

        for (var i = 0; i < HeaderTerminator.Length; i++)
        {
            if (bytes[bytes.Count - HeaderTerminator.Length + i] != HeaderTerminator[i])
            {
                return false;
            }
        }

        return true;
    }
}
