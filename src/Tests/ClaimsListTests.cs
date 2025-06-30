namespace Tests;

public class ClaimsListTests
{
    // Tests for the Set method

    [Fact]
    public void Set_Should_AddOrUpdateClaim_WhenTypeExists()
    {
        // Arrange
        var claimsList = new ClaimsList {
            { "role", "admin" }
        };

        // Act
        claimsList.Set("role", "user");

        // Assert
        claimsList.Count.ShouldBe(1);
        claimsList[0].Type.ShouldBe("role");
        claimsList[0].Value.ShouldBe("user");
    }

    [Fact]
    public void Set_Should_RemoveExistingClaimsAndAddNewOne()
    {
        // Arrange
        var claimsList = new ClaimsList {
            { "role", "admin" },
            { "role", "superadmin" }
        };

        // Act
        claimsList.Set("role", "user");

        // Assert
        claimsList.Count.ShouldBe(1);
        claimsList[0].Type.ShouldBe("role");
        claimsList[0].Value.ShouldBe("user");
    }

    // Tests for the Add method

    [Fact]
    public void Add_Should_AddNewClaim()
    {
        // Arrange
        var claimsList = new ClaimsList {
            // Act
            { "email", "test@example.com" }
        };

        // Assert
        claimsList.Count.ShouldBe(1);
        claimsList[0].Type.ShouldBe("email");
        claimsList[0].Value.ShouldBe("test@example.com");
    }

    [Fact]
    public void Add_Should_NotReplaceExistingClaim()
    {
        // Arrange
        var claimsList = new ClaimsList {
            { "email", "test@example.com" },

            // Act
            { "email", "another@example.com" }
        };

        // Assert
        claimsList.Count.ShouldBe(2);
        claimsList.Any(c => c.Value == "test@example.com").ShouldBeTrue();
        claimsList.Any(c => c.Value == "another@example.com").ShouldBeTrue();
    }

    // Tests for the Remove method

    [Fact]
    public void Remove_Should_RemoveAllClaimsOfType()
    {
        // Arrange
        var claimsList = new ClaimsList {
            { "role", "admin" },
            { "role", "user" },
            { "email", "test@example.com" }
        };

        // Act
        claimsList.Remove("role");

        // Assert
        claimsList.Count.ShouldBe(1);
        claimsList[0].Type.ShouldBe("email");
    }

    [Fact]
    public void Remove_Should_NotRemoveClaimsOfOtherTypes()
    {
        // Arrange
        var claimsList = new ClaimsList {
            { "role", "admin" },
            { "email", "test@example.com" }
        };

        // Act
        claimsList.Remove("role");

        // Assert
        claimsList.Count.ShouldBe(1);
        claimsList[0].Type.ShouldBe("email");
        claimsList[0].Value.ShouldBe("test@example.com");
    }

    // Tests for the FindAll method

    [Fact]
    public void FindAll_Should_ReturnAllClaimsOfSpecifiedType()
    {
        // Arrange
        var claimsList = new ClaimsList {
            { "role", "admin" },
            { "role", "user" },
            { "email", "test@example.com" }
        };

        // Act
        var result = claimsList.FindAll("role");

        // Assert
        result.Count().ShouldBe(2);
        result.ShouldContain(c => c.Value == "admin");
        result.ShouldContain(c => c.Value == "user");
    }

    [Fact]
    public void FindAll_Should_ReturnEmpty_WhenNoClaimsOfType()
    {
        // Arrange
        var claimsList = new ClaimsList {
            { "email", "test@example.com" }
        };

        // Act
        var result = claimsList.FindAll("role");

        // Assert
        result.ShouldBeEmpty();
    }

    // Tests for the FindFirst method

    [Fact]
    public void FindFirst_Should_ReturnFirstClaimOfSpecifiedType()
    {
        // Arrange
        var claimsList = new ClaimsList {
            { "role", "admin" },
            { "role", "user" }
        };

        // Act
        var claim = claimsList.FindFirst("role");

        // Assert
        claim.ShouldNotBeNull();
        claim!.Value.ShouldBe("admin");
    }

    [Fact]
    public void FindFirst_Should_ReturnNull_WhenNoClaimOfType()
    {
        // Arrange
        var claimsList = new ClaimsList {
            { "email", "test@example.com" }
        };

        // Act
        var claim = claimsList.FindFirst("role");

        // Assert
        claim.ShouldBeNull();
    }

    // Tests for the FindFirstValue method

    [Fact]
    public void FindFirstValue_Should_ReturnValueOfFirstClaimOfSpecifiedType()
    {
        // Arrange
        var claimsList = new ClaimsList {
            { "role", "admin" },
            { "role", "user" }
        };

        // Act
        var value = claimsList.FindFirstValue("role");

        // Assert
        value.ShouldBe("admin");
    }

    [Fact]
    public void FindFirstValue_Should_ReturnNull_WhenNoClaimOfType()
    {
        // Arrange
        var claimsList = new ClaimsList {
            { "email", "test@example.com" }
        };

        // Act
        var value = claimsList.FindFirstValue("role");

        // Assert
        value.ShouldBeNull();
    }
}
