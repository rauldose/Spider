using Spider.Core.SharedKernel.Base;

namespace Spider.DataAcquisition.Domain.Enumerations;

/// <summary>
/// Represents the quality status of acquired data
/// </summary>
public class DataQuality : Enumeration
{
    public static readonly DataQuality Good = new(1, nameof(Good));
    public static readonly DataQuality Bad = new(2, nameof(Bad));
    public static readonly DataQuality Uncertain = new(3, nameof(Uncertain));
    public static readonly DataQuality Timeout = new(4, nameof(Timeout));
    public static readonly DataQuality Error = new(5, nameof(Error));

    public DataQuality(int id, string name) : base(id, name) { }

    public static IEnumerable<DataQuality> GetAll() => GetAll<DataQuality>();
}