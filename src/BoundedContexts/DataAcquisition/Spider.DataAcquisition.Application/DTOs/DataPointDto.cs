namespace Spider.DataAcquisition.Application.DTOs;

/// <summary>
/// Data transfer object for data points
/// </summary>
public record DataPointDto(
    Guid Id,
    string Name,
    string? Description,
    string Address,
    string DataType,
    Guid DeviceId,
    bool IsEnabled,
    int ScanInterval,
    object? LastValue,
    string? LastValueQuality,
    DateTime? LastScanTime);