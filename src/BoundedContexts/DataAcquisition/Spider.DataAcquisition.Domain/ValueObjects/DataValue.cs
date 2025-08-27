using Spider.Core.SharedKernel.Base;
using Spider.DataAcquisition.Domain.Enumerations;

namespace Spider.DataAcquisition.Domain.ValueObjects;

/// <summary>
/// Represents a collected data value with quality and timestamp
/// </summary>
public class DataValue : ValueObject
{
    public object Value { get; }
    public DataType DataType { get; }
    public DataQuality Quality { get; }
    public DateTime Timestamp { get; }

    public DataValue(object value, DataType dataType, DataQuality quality, DateTime timestamp)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
        Quality = quality ?? throw new ArgumentNullException(nameof(quality));
        Timestamp = timestamp;
    }

    public static DataValue CreateGood(object value, DataType dataType, DateTime? timestamp = null) =>
        new(value, dataType, DataQuality.Good, timestamp ?? DateTime.UtcNow);

    public static DataValue CreateBad(object value, DataType dataType, DateTime? timestamp = null) =>
        new(value, dataType, DataQuality.Bad, timestamp ?? DateTime.UtcNow);

    public static DataValue CreateError(DataType dataType, DateTime? timestamp = null) =>
        new(DBNull.Value, dataType, DataQuality.Error, timestamp ?? DateTime.UtcNow);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return DataType;
        yield return Quality;
        yield return Timestamp;
    }

    public T GetValue<T>() => (T)Convert.ChangeType(Value, typeof(T));

    public bool IsGood => Quality.Equals(DataQuality.Good);
    public bool IsBad => Quality.Equals(DataQuality.Bad);
    public bool IsError => Quality.Equals(DataQuality.Error);
}