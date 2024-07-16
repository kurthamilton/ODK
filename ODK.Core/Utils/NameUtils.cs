namespace ODK.Core.Utils;
public static class NameUtils
{
    public static string FullName(string firstName, string lastName) 
        => $"{firstName.Trim()} {lastName.Trim()}".Trim();
}
