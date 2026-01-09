namespace ODK.Core.Emails;

public class EmailAddressee
{
    public EmailAddressee(string address, string name)
    {
        Address = address;
        Name = name;
    }

    public string Address { get; }

    public string Name { get; }

    public override string ToString() => Address + (!string.IsNullOrEmpty(Name) ? $" ({Name})" : string.Empty);
}