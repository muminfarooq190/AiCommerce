using System.Text.RegularExpressions;

namespace EcommerceApi.Utilities;

public class SchemaGenerater
{   
    public static string Generate(string CompanyName)
    {
        if (string.IsNullOrWhiteSpace(CompanyName))
            throw new ArgumentException("Company name cannot be empty.");
        var name = CompanyName.ToLowerInvariant();
        name = Regex.Replace(name, @"[\s\-]+", "_");
        name = Regex.Replace(name, @"[^a-zA-Z0-9_]", string.Empty);
        if (!Regex.IsMatch(name, @"^[a-zA-Z_]"))
        {
            name = "_" + name;
        }
        if (name.Length > 128)
        {
            name = name.Substring(0, 128);
        }
        return name;
    }
}
