namespace SmartBuilding.Application.Common;

public sealed class ApplicationValidationException(
    string propertyName,
    string message) : Exception(message)
{
    public string PropertyName { get; } = propertyName;
}