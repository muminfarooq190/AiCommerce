using System.Reflection;
using System.Text.Json;

namespace EcommerceApi.Models;

public static class FeatureFactory
{
    public static class Authentication
    {
        public const string CanLogin = nameof(CanLogin);
        public const string CanCreateUser = nameof(CanCreateUser);
        public const string CanLogout = nameof(CanLogout);
    }
    public static class Permission
    {
        public const string CanGivePermisston = nameof(CanGivePermisston);
    }
    public static class Category
    {
        public const string CanCreateCategory = nameof(CanCreateCategory);
        public const string CanUpdateCategory = nameof(CanUpdateCategory);
        public const string CanDeleteCategory = nameof(CanDeleteCategory);
    }
    public static string GetJsonRepresentation()
    {
        var result = new Dictionary<string, Dictionary<string, string>>();

        Type outerType = typeof(FeatureFactory);
        var nestedTypes = outerType.GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

        foreach (var nestedType in nestedTypes)
        {
            var constants = nestedType
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                .ToDictionary(fi => fi.Name, fi => fi.GetRawConstantValue()?.ToString() ?? "");

            result[nestedType.Name] = constants;
        }

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }
    public static List<string> GetFlattenedPermissionList()
    {
        var flatList = new List<string>();
        Type outerType = typeof(FeatureFactory);
        var nestedTypes = outerType.GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

        foreach (var nestedType in nestedTypes)
        {
            var constants = nestedType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly);

            foreach (var constant in constants)
            {
                flatList.Add($"{nestedType.Name}.{constant.Name}");
            }
        }

        return flatList;
    }
}
