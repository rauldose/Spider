using Spider.Core.SharedKernel.Base;

namespace Spider.DataAcquisition.Domain.ValueObjects;

/// <summary>
/// Represents a data address within a device
/// </summary>
public class DataAddress : ValueObject
{
    public string Address { get; }
    public string? Group { get; }
    public int? Register { get; }

    public DataAddress(string address, string? group = null, int? register = null)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be null or empty", nameof(address));

        Address = address;
        Group = group;
        Register = register;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Address;
        yield return Group ?? string.Empty;
        yield return Register ?? -1;
    }

    public override string ToString() => Address;
}