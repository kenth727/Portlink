namespace PortlinkApp.Core.Exceptions;

/// <summary>
/// Base class for all business exceptions
/// </summary>
public abstract class BusinessException : Exception
{
    public int StatusCode { get; }

    protected BusinessException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

/// <summary>
/// Thrown when a requested entity is not found
/// </summary>
public class EntityNotFoundException : BusinessException
{
    public EntityNotFoundException(string entityType, object id)
        : base($"{entityType} with ID {id} was not found.", 404)
    {
    }
}

/// <summary>
/// Thrown when a berth is not available for booking
/// </summary>
public class BerthNotAvailableException : BusinessException
{
    public BerthNotAvailableException(string berthCode, string? reason = null)
        : base($"Berth {berthCode} is not available.{(reason != null ? " " + reason : "")}", 409)
    {
    }
}

/// <summary>
/// Thrown when a vessel exceeds berth capacity constraints
/// </summary>
public class BerthCapacityExceededException : BusinessException
{
    public BerthCapacityExceededException(string berthCode, string constraint, double vesselValue, double berthMaxValue)
        : base($"Berth {berthCode} capacity exceeded: Vessel {constraint} {vesselValue:F1}m exceeds berth maximum {berthMaxValue:F1}m.", 400)
    {
    }
}

/// <summary>
/// Thrown when a port call overlaps with an existing booking
/// </summary>
public class OverlappingPortCallException : BusinessException
{
    public OverlappingPortCallException(string berthCode, string vesselName, DateTime eta, DateTime etd)
        : base(
            $"Berth {berthCode} is already occupied by vessel '{vesselName}' during the requested period " +
            $"(ETA (local): {eta.ToLocalTime():g}, ETD (local): {etd.ToLocalTime():g}).",
            409)
    {
    }
}

/// <summary>
/// Thrown when business validation fails
/// </summary>
public class ValidationException : BusinessException
{
    public ValidationException(string message) : base(message, 400)
    {
    }
}
