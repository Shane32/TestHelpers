using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using GraphQL;
using GraphQL.Execution;
using GraphQL.SystemTextJson;

namespace Shouldly;

/// <summary>
/// Shouldly extensions for asserting anonymous objects and/or GraphQL objects.
/// </summary>
[DebuggerStepThrough]
[ShouldlyMethods]
public static class SerializingShouldlyExtensions
{
    /// <summary>
    /// Converts the <paramref name="actual"/> and <paramref name="expected"/> data to JSON and compares the JSON for equality.
    /// </summary>
    public static void ShouldBeSimilarTo(this object? actual, object? expected, string? customMessage = null)
    {
        var actualJson = actual.Serialize();
        var expectedJson = expected.Serialize();
        if (actualJson != expectedJson) {
            throw new ShouldAssertException(new ExpectedActualShouldlyMessage(expectedJson, actualJson, customMessage).ToString());
        }
    }

    /// <summary>
    /// Converts the <paramref name="actual"/> data to JSON and compares it to the expected JSON for equality.
    /// </summary>
    public static void ShouldBeSimilarToJson(this object? actual, string expectedJson, string? customMessage = null)
    {
        var actualJson = actual.Serialize();
        expectedJson = JsonSerializer.Deserialize<JsonElement>(expectedJson).Serialize();
        if (actualJson != expectedJson) {
            throw new ShouldAssertException(new ExpectedActualShouldlyMessage(expectedJson, actualJson, customMessage).ToString());
        }
    }

    private static readonly JsonSerializerOptions _jsonSerializerOptionsIndented;
    static SerializingShouldlyExtensions()
    {
        _jsonSerializerOptionsIndented = new JsonSerializerOptions {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };
        _jsonSerializerOptionsIndented.Converters.Add(new ExecutionResultJsonConverter());
        _jsonSerializerOptionsIndented.Converters.Add(new ExecutionErrorJsonConverter(new ErrorInfoProvider(o => o.ExposeExceptionDetails = true)));
        _jsonSerializerOptionsIndented.Converters.Add(new JsonStringEnumConverter());
        _jsonSerializerOptionsIndented.Converters.Add(new SortedDocumentConverter());
        _jsonSerializerOptionsIndented.Converters.Add(new DecimalConverter());
    }

    /// <summary>
    /// Serializes the <paramref name="objectToSerialize"/> to JSON, sorting objects according to their property names.
    /// </summary>
    public static string Serialize<T>(this T objectToSerialize)
    {
        if (typeof(T) == typeof(ExecutionResult)) {
            //don't sort properties
            return JsonSerializer.Serialize(objectToSerialize, _jsonSerializerOptionsIndented);
        } else {
            //sort properties
            var doc = new SortedDocument(JsonSerializer.SerializeToElement(objectToSerialize, _jsonSerializerOptionsIndented));
            return JsonSerializer.Serialize(doc, _jsonSerializerOptionsIndented);
        }
    }

    private sealed class DecimalConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotImplementedException();

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteRawValue(value.ToString("0.###################", CultureInfo.InvariantCulture));
        }
    }

    private sealed class SortedDocumentConverter : JsonConverter<SortedDocument>
    {
        public override void Write(Utf8JsonWriter writer, SortedDocument value, JsonSerializerOptions options)
        {
            Write(value.Element);

            void Write(JsonElement element)
            {
                switch (element.ValueKind) {
                    case JsonValueKind.Object:
                        var props = element.EnumerateObject()
                            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                            .ThenBy(x => x.Name, StringComparer.Ordinal);

                        writer.WriteStartObject();
                        foreach (var prop in props) {
                            writer.WritePropertyName(prop.Name);
                            Write(prop.Value);
                        }
                        writer.WriteEndObject();
                        break;
                    case JsonValueKind.Array:
                        writer.WriteStartArray();
                        foreach (var elem in element.EnumerateArray())
                            Write(elem);
                        writer.WriteEndArray();
                        break;
                    default:
                        element.WriteTo(writer);
                        break;
                }
            }

        }

        public override SortedDocument? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotImplementedException();
    }

    private sealed record SortedDocument(JsonElement Element);

    /// <summary>
    /// Asserts that the specified <paramref name="value"/> has not changed since the last approval.
    /// The approved file will be saved with an extension of &quot;.txt&quot;.
    /// </summary>
    public static void ShouldMatchApproved(this string value)
        => value.ShouldMatchApproved(".txt", null);

    /// <summary>
    /// Asserts that the specified <paramref name="value"/> has not changed since the last approval.
    /// The approved file will be saved with an extension of <paramref name="fileExtension"/>.
    /// If specified, the <paramref name="discriminator"/> will be appended to the approved file name.
    /// </summary>
    public static void ShouldMatchApproved(this string value, string fileExtension = ".txt", string? discriminator = null)
        => ShouldMatchApprovedTestExtensions.ShouldMatchApproved(value + Environment.NewLine, options => {
            var builder = options.NoDiff().WithFileExtension(fileExtension);
            if (discriminator != null)
                builder.WithDiscriminator(discriminator);
        });

    /// <inheritdoc cref="ShouldMatchApproved(string, string, string?)"/>
    public static void ShouldMatchApproved<T>(this T value, string fileExtension = ".txt", string? discriminator = null)
        => value.Serialize().ShouldMatchApproved(fileExtension, discriminator);
}
