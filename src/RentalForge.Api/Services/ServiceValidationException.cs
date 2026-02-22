namespace RentalForge.Api.Services;

/// <summary>
/// Thrown when a service-level validation rule fails (e.g., FK reference doesn't exist).
/// </summary>
public class ServiceValidationException(string propertyName, string message)
    : Exception(message)
{
    public string PropertyName { get; } = propertyName;
}
