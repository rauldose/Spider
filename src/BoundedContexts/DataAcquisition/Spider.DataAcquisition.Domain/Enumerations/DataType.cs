using Spider.Core.SharedKernel.Base;

namespace Spider.DataAcquisition.Domain.Enumerations;

/// <summary>
/// Represents the data type of a collected value
/// </summary>
public class DataType : Enumeration
{
    public static readonly DataType Boolean = new(1, nameof(Boolean));
    public static readonly DataType Integer = new(2, nameof(Integer));
    public static readonly DataType Double = new(3, nameof(Double));
    public static readonly DataType String = new(4, nameof(String));
    public static readonly DataType DateTime = new(5, nameof(DateTime));
    public static readonly DataType Binary = new(6, nameof(Binary));

    public DataType(int id, string name) : base(id, name) { }

    public static IEnumerable<DataType> GetAll() => GetAll<DataType>();
}