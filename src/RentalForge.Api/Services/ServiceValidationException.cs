namespace RentalForge.Api.Services;

/// <summary>
/// Thrown when one or more service-level validation rules fail (e.g., FK references don't exist).
/// Carries all errors so the caller can return them in a single response.
/// </summary>
public class ServiceValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ServiceValidationException(IDictionary<string, string[]> errors)
        : base("One or more service validation errors occurred.")
    {
        Errors = errors;
    }

    /// <summary>
    /// Convenience constructor for a single validation error.
    /// </summary>
    public ServiceValidationException(string propertyName, string message)
        : this(new Dictionary<string, string[]> { [propertyName] = [message] }) { }
}
