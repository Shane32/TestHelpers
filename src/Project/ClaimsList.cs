using System.Security.Claims;

namespace Shane32.TestHelpers;

/// <summary>
/// Represents a collection of claims.
/// </summary>
public class ClaimsList : List<Claim>
{
    /// <summary>
    /// Adds or updates a claim. If a claim of the same type exists, it will be replaced.
    /// </summary>
    /// <param name="type">The type of the claim.</param>
    /// <param name="value">The value of the claim.</param>
    public void Set(string type, string value)
    {
        // Remove any existing claims of the same type
        Remove(type);
        // Add the new claim
        Add(type, value);
    }

    /// <summary>
    /// Adds a claim to the list.
    /// </summary>
    /// <param name="type">The type of the claim.</param>
    /// <param name="value">The value of the claim.</param>
    public void Add(string type, string value)
    {
        Add(new(type, value));
    }

    /// <summary>
    /// Removes all claims of the specified type.
    /// </summary>
    /// <param name="type">The type of the claims to remove.</param>
    public void Remove(string type)
    {
        RemoveAll(claim => claim.Type == type);
    }

    /// <summary>
    /// Finds all claims of the specified type.
    /// </summary>
    /// <param name="type">The type of the claims to find.</param>
    /// <returns>A collection of claims matching the type.</returns>
    public IEnumerable<Claim> FindAll(string type)
    {
        return this.Where(claim => claim.Type == type);
    }

    /// <summary>
    /// Finds the first claim of the specified type, or null if no claim is found.
    /// </summary>
    /// <param name="type">The type of the claim to find.</param>
    /// <returns>The first claim matching the type, or null.</returns>
    public Claim? FindFirst(string type)
    {
        return this.FirstOrDefault(claim => claim.Type == type);
    }

    /// <summary>
    /// Finds the value of the first claim of the specified type, or null if no claim is found.
    /// </summary>
    /// <param name="type">The type of the claim to find.</param>
    /// <returns>The value of the first claim matching the type, or null.</returns>
    public string? FindFirstValue(string type)
    {
        return FindFirst(type)?.Value;
    }
}
