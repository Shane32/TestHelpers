using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using GraphQL;
using GraphQL.Execution;
using GraphQL.Types;
using Shouldly;
using Xunit;

namespace Tests;

public class ShouldlyExtensionsTests
{
    // ----------------------------
    // Tests for ShouldBeSuccessful
    // ----------------------------

    [Fact]
    public void ShouldBeSuccessful_ExecutionResponse_Successful()
    {
        // Arrange
        var response = new ExecutionResponse {
            Data = JsonDocument.Parse("{\"hello\": \"world\"}").RootElement,
            StatusCode = HttpStatusCode.OK
        };

        // Act & Assert
        response.ShouldBeSuccessful();
    }

    [Fact]
    public void ShouldBeSuccessful_ExecutionResponse_Unsuccessful()
    {
        // Arrange
        var response = new ExecutionResponse {
            Errors = JsonDocument.Parse("[{\"message\": \"Something went wrong.\"}]").RootElement,
            StatusCode = HttpStatusCode.InternalServerError
        };

        // Act & Assert
        Should.Throw<ShouldAssertException>(() => response.ShouldBeSuccessful());
    }

    [Fact]
    public void ShouldBeSuccessful_ExecutionResult_Successful()
    {
        // Arrange
        var result = new ExecutionResult {
            Data = "dummy"
        };

        // Act & Assert
        result.ShouldBeSuccessful();
    }

    [Fact]
    public void ShouldBeSuccessful_ExecutionResult_Unsuccessful()
    {
        // Arrange
        var result = new ExecutionResult {
            Errors = new ExecutionErrors { new ExecutionError("Error message") },
        };

        // Act & Assert
        Should.Throw<ShouldAssertException>(() => result.ShouldBeSuccessful());
    }

    [Fact]
    public void ShouldBeSimilarTo_Passing()
    {
        // Arrange
        var actual = new { Name = "Alice", Age = 30 };
        var expected = new { Name = "Alice", Age = 30 };

        // Act & Assert
        actual.ShouldBeSimilarTo(expected);
    }

    [Fact]
    public void ShouldBeSimilarTo_Failing()
    {
        // Arrange
        var actual = new { Name = "Alice", Age = 30 };
        var expected = new { Name = "Bob", Age = 25 };

        // Act & Assert
        Should.Throw<ShouldAssertException>(() => actual.ShouldBeSimilarTo(expected));
    }

    [Fact]
    public void ShouldBeSimilarToJson_Passing()
    {
        // Arrange
        var actual = new { Name = "Alice", Age = 30 };
        string expectedJson = """{"Age":30,"Name":"Alice"}""";

        // Act & Assert
        actual.ShouldBeSimilarToJson(expectedJson);
    }

    [Fact]
    public void ShouldBeSimilarToJson_Failing()
    {
        // Arrange
        var actual = new { Name = "Alice", Age = 30 };
        string expectedJson = """{"Age":25,"Name":"Bob"}""";

        // Act & Assert
        Should.Throw<ShouldAssertException>(() => actual.ShouldBeSimilarToJson(expectedJson));
    }

    [Fact]
    public async Task ShouldHaveData_ExecutionResult_Successful()
    {
        // Arrange
        var expectedData = new { greeting = "Hello" };
        var queryType = new ObjectGraphType() { Name = "Query" };
        queryType.Field<StringGraphType>("greeting").Resolve(_ => "Hello");
        var schema = new Schema() { Query = queryType };
        var result = await new DocumentExecuter().ExecuteAsync(o => {
            o.Schema = schema;
            o.Query = "{ greeting }";
        });

        // Act & Assert
        result.ShouldHaveData(expectedData);
    }

    [Fact]
    public async Task ShouldHaveData_ExecutionResult_Failing()
    {
        // Arrange
        var expectedData = new { greeting = "Hello" };
        var queryType = new ObjectGraphType() { Name = "Query" };
        queryType.Field<StringGraphType>("greeting").Resolve(_ => "Hi");
        var schema = new Schema() { Query = queryType };
        var result = await new DocumentExecuter().ExecuteAsync(o => {
            o.Schema = schema;
            o.Query = "{ greeting }";
        });

        // Act & Assert
        Should.Throw<ShouldAssertException>(() => result.ShouldHaveData(expectedData));
    }

    [Fact]
    public void ShouldHaveData_ExecutionResponse_Successful()
    {
        // Arrange
        var expectedData = new { message = "Success" };
        var response = new ExecutionResponse {
            Data = JsonDocument.Parse("""{"message": "Success"}""").RootElement,
            StatusCode = HttpStatusCode.OK
        };

        // Act & Assert
        response.ShouldHaveData(expectedData);
    }

    [Fact]
    public void ShouldHaveData_ExecutionResponse_Failing()
    {
        // Arrange
        var expectedData = new { message = "Success" };
        var response = new ExecutionResponse {
            Data = JsonDocument.Parse("""{"message": "Failure"}""").RootElement,
            StatusCode = HttpStatusCode.OK
        };

        // Act & Assert
        Should.Throw<ShouldAssertException>(() => response.ShouldHaveData(expectedData));
    }

    [Fact]
    public void ShouldHaveError_Exception_Successful()
    {
        // Arrange
        var exception = new InvalidOperationException("Invalid operation.");
        var result = new ExecutionResult {
            Errors = new ExecutionErrors
            {
                new ExecutionError("Error", exception)
            }
        };

        // Act
        var ex = result.ShouldHaveError<InvalidOperationException>();

        // Assert
        ex.ShouldBe(exception);
    }

    [Fact]
    public void ShouldHaveError_Exception_Failing()
    {
        // Arrange
        var result = new ExecutionResult {
            Errors = new ExecutionErrors
            {
                new ExecutionError("Error", new Exception("General error"))
            }
        };

        // Act & Assert
        Should.Throw<ShouldAssertException>(() => result.ShouldHaveError<InvalidOperationException>());
    }

    [Fact]
    public void ShouldMatchApproved_String_Successful()
    {
        // Arrange
        string value = "Approved content";
        string fileExtension = ".txt";
        string discriminator = "test";

        // Act & Assert
        Should.NotThrow(() => value.ShouldMatchApproved(fileExtension, discriminator));
    }

    [Fact]
    public void ShouldMatchApproved_String_Failing()
    {
        // Arrange
        string value = "Changed content";
        string fileExtension = ".txt";
        string discriminator = "test";

        // Act & Assert
        Should.Throw<ShouldAssertException>(() => value.ShouldMatchApproved(fileExtension, discriminator));
    }

    [Fact]
    public void ShouldMatchApproved_Generic_Successful()
    {
        // Arrange
        var obj = new { Name = "Alice", Age = 30 };
        string fileExtension = ".txt";
        string discriminator = "genericTest";

        // Act & Assert
        Should.NotThrow(() => obj.ShouldMatchApproved(fileExtension, discriminator));
    }

    [Fact]
    public void ShouldMatchApproved_Generic_Failing()
    {
        // Arrange
        var obj = new { Name = "Bob", Age = 25 };
        string fileExtension = ".txt";
        string discriminator = "genericTest";

        // Act & Assert
        Should.Throw<ShouldAssertException>(() => obj.ShouldMatchApproved(fileExtension, discriminator));
    }
}
