namespace FluentEmailer.Core.Models;

public sealed class Address
{
    public string? Name { get; }
    public string EmailAddress { get; } = string.Empty;

    public Address() { }

    public Address(string emailAddress, string? name = null)
    {
        EmailAddress = emailAddress;
        Name = name;
    }

    public override string ToString() 
        => string.IsNullOrWhiteSpace(Name) ? EmailAddress : $"{Name} <{EmailAddress}>";

    private bool Equals(Address other) 
        => Name == other.Name && EmailAddress == other.EmailAddress;

    public override bool Equals(object? obj) 
        => ReferenceEquals(this, obj) || obj is Address other && Equals(other);

    public override int GetHashCode() 
        => HashCode.Combine(Name, EmailAddress);
}
