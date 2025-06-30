using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shane32.TestHelpers;

/// <summary>
/// Represents the response from a GraphQL execution, including the result data, errors, and extensions.
/// </summary>
public class ExecutionResponse
{
    /// <summary>
    /// Gets or sets the result data returned by the GraphQL execution.
    /// This corresponds to the "data" property in the response JSON.
    /// </summary>
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Data { get; set; }

    /// <summary>
    /// Gets or sets the errors encountered during the GraphQL execution.
    /// This corresponds to the "errors" property in the response JSON.
    /// </summary>
    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Errors { get; set; }

    /// <summary>
    /// Gets or sets the extensions provided by the GraphQL execution.
    /// This corresponds to the "extensions" property in the response JSON.
    /// </summary>
    [JsonPropertyName("extensions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Extensions { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code of the GraphQL response.
    /// This property is ignored when serializing the response to JSON.
    /// </summary>
    [JsonIgnore]
    public HttpStatusCode StatusCode { get; set; }
}
