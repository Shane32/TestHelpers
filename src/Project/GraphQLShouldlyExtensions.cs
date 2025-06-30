using System.Diagnostics;
using System.Runtime.CompilerServices;
using GraphQL;
using Shane32.TestHelpers;

namespace Shouldly;

/// <summary>
/// Shouldly extensions for GraphQL calls.
/// </summary>
[DebuggerStepThrough]
[ShouldlyMethods]
public static class GraphQLShouldlyExtensions
{
    /// <summary>
    /// Asserts that the <paramref name="executionResponse"/> is successful.
    /// </summary>
    public static ExecutionResponse ShouldBeSuccessful(this ExecutionResponse executionResponse, string? customMessage = null)
    {
        if (executionResponse.Errors != null)
            throw new ShouldAssertException(new SuccessfulShouldlyMessage(executionResponse, customMessage).ToString());
        if (executionResponse.Data == null)
            throw new ShouldAssertException(new SuccessfulShouldlyMessage(executionResponse, customMessage).ToString());
        return executionResponse;
    }

    /// <summary>
    /// Asserts that the <paramref name="executionResult"/> is successful.
    /// </summary>
    public static ExecutionResult ShouldBeSuccessful(this ExecutionResult executionResult, string? customMessage = null)
    {
        if (executionResult.Errors != null)
            throw new ShouldAssertException(new SuccessfulShouldlyMessage(executionResult, customMessage).ToString());
        if (executionResult.Data == null)
            throw new ShouldAssertException(new SuccessfulShouldlyMessage(executionResult, customMessage).ToString());
        return executionResult;
    }

    /// <summary>
    /// Represents a message for a failed assertion.
    /// </summary>
    public class SuccessfulShouldlyMessage : ActualShouldlyMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SuccessfulShouldlyMessage"/> class.
        /// </summary>
        public SuccessfulShouldlyMessage(ExecutionResponse executionResponse, string? customMessage, [CallerMemberName] string shouldlyMethod = null!)
            : base(executionResponse.Serialize(), customMessage, shouldlyMethod)
        {
        }

        /// <inheritdoc cref="SuccessfulShouldlyMessage(ExecutionResponse, string?, string)"/>
        public SuccessfulShouldlyMessage(ExecutionResult executionResult, string? customMessage, [CallerMemberName] string shouldlyMethod = null!)
            : base(executionResult.Serialize(), customMessage, shouldlyMethod)
        {
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return base.ToString().Replace(
                """

                    should be successful
                null
                    but was

                """,
                """

                    should be successful but was

                """);
        }
    }

    /// <summary>
    /// Asserts that the <paramref name="executionResult"/> has the specified <paramref name="data"/>.
    /// </summary>
    public static ExecutionResult ShouldHaveData<T>(this ExecutionResult executionResult, T data, string? customMessage = null)
    {
        executionResult.ShouldBeSimilarTo(new { data }, customMessage);
        return executionResult;
    }

    /// <summary>
    /// Asserts that the <paramref name="executionResponse"/> has the specified <paramref name="data"/>.
    /// </summary>
    public static ExecutionResponse ShouldHaveData<T>(this ExecutionResponse executionResponse, T data, string? customMessage = null)
    {
        executionResponse.ShouldBeSimilarTo(new { data }, customMessage);
        return executionResponse;
    }

    /// <summary>
    /// Asserts that the <paramref name="executionResult"/> has an error of the specified type.
    /// </summary>
    public static T ShouldHaveError<T>(this ExecutionResult executionResult, string? customMessage = null)
        where T : Exception
    {
        if (executionResult.Errors == null)
            throw new ShouldAssertException(new ExpectedActualShouldlyMessage(typeof(T).Name, executionResult.Serialize(), customMessage).ToString());
        if (typeof(T).IsAssignableTo(typeof(ExecutionError))) {
            var match = (T?)(object?)executionResult.Errors.FirstOrDefault(x => x.GetType() == typeof(T))
                ?? throw new ShouldAssertException(new ExpectedActualShouldlyMessage(typeof(T).Name, executionResult.Serialize(), customMessage).ToString());
            return match;
        } else {
            var match = (T?)(object?)executionResult.Errors.FirstOrDefault(x => x.InnerException?.GetType() == typeof(T))?.InnerException
                ?? throw new ShouldAssertException(new ExpectedActualShouldlyMessage(typeof(T).Name, executionResult.Serialize(), customMessage).ToString());
            return match;
        }
    }
}
