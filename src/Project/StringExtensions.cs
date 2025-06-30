using System.Text.Json;
using GraphQL;
using GraphQL.SystemTextJson;

namespace System;

/// <summary>
/// Provides extension methods for converting JSON strings to <see cref="Inputs"/> instances.
/// </summary>
public static class StringExtensions
{
    private static readonly GraphQLSerializer _serializer = new();

    /// <summary>
    /// Converts the specified JSON string to an <see cref="Inputs"/> instance.
    /// </summary>
    public static Inputs ToInputs(this string json)
        => json == null ? Inputs.Empty : _serializer.Deserialize<Inputs>(json) ?? Inputs.Empty;

    /// <summary>
    /// Converts the specified JSON element to an <see cref="Inputs"/> instance.
    /// </summary>
    public static Inputs ToInputs(this JsonElement element)
        => _serializer.ReadNode<Inputs>(element) ?? Inputs.Empty;

    /// <summary>
    /// Converts the specified JSON string to an object of the specified type.
    /// </summary>
    public static T? FromJson<T>(this string json)
        => _serializer.Deserialize<T>(json);

    /// <summary>
    /// Converts the specified JSON stream to an object of the specified type.
    /// </summary>
    public static ValueTask<T?> FromJsonAsync<T>(this Stream stream, CancellationToken cancellationToken = default)
        => _serializer.ReadAsync<T>(stream, cancellationToken);
}
