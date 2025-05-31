using System.Reflection;

namespace EcommerceApi.Models;

public static class FeatureFactory
{

    public static class Permission
    {
        public const string CanGivePermisston = $"{nameof(Permission)}.{nameof(CanGivePermisston)}";
        public const string CanRemovePermisston = $"{nameof(Permission)}.{nameof(CanRemovePermisston)}";
    }

    public static class User
    {
        public const string CanGetUsers = $"{nameof(User)}.{nameof(CanGetUsers)}";
        public const string CanGetUserById = $"{nameof(User)}.{nameof(CanGetUserById)}";
        public const string CanCreateUser = $"{nameof(User)}.{nameof(CanCreateUser)}";
        public const string CanUpdateUser = $"{nameof(User)}.{nameof(CanUpdateUser)}";
        public const string CanDeleteUser = $"{nameof(User)}.{nameof(CanDeleteUser)}";
    }

    public static Dictionary<string, Dictionary<string, string>> GetJsonRepresentation()
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

        return result;
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
                var value = constant.GetRawConstantValue()?.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    flatList.Add(value);
                }
            }
        }

        return flatList;
    }

}
