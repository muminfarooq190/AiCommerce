using System.Reflection;

namespace Sheared;

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

    public static class Cart
    {
        public const string CanGetCart = $"{nameof(Cart)}.{nameof(CanGetCart)}";
        public const string CanRemoveCart = $"{nameof(Cart)}.{nameof(CanRemoveCart)}";
        public const string CanAddCart = $"{nameof(Cart)}.{nameof(CanAddCart)}";
    }

    public static class Category
    {
        public const string CanGetCategory = $"{nameof(Category)}.{nameof(CanGetCategory)}";
        public const string CanRemoveCategory = $"{nameof(Category)}.{nameof(CanRemoveCategory)}";
        public const string CanAddCategory = $"{nameof(Category)}.{nameof(CanAddCategory)}";
    }

    public static class Collection
    {
        public const string CanGetCollection = $"{nameof(Collection)}.{nameof(CanGetCollection)}";
        public const string CanRemoveCollection = $"{nameof(Collection)}.{nameof(CanRemoveCollection)}";
        public const string CanAddCollection = $"{nameof(Collection)}.{nameof(CanAddCollection)}";
    }

    public static class Discount
    {
        public const string CanGetDiscount = $"{nameof(Discount)}.{nameof(CanGetDiscount)}";
        public const string CanRemoveDiscount = $"{nameof(Discount)}.{nameof(CanRemoveDiscount)}";
        public const string CanAddDiscount = $"{nameof(Discount)}.{nameof(CanAddDiscount)}";
    }

    public static class Order
    {
        public const string CanGetOrder = $"{nameof(Order)}.{nameof(CanGetOrder)}";
        public const string CanRemoveOrder = $"{nameof(Order)}.{nameof(CanRemoveOrder)}";
        public const string CanAddOrder = $"{nameof(Order)}.{nameof(CanAddOrder)}";
    }

    public static class ProductAttributeValue
    {
        public const string CanGetProductAttributeValue = $"{nameof(ProductAttributeValue)}.{nameof(CanGetProductAttributeValue)}";
        public const string CanRemoveroductAttributeValue = $"{nameof(ProductAttributeValue)}.{nameof(CanRemoveroductAttributeValue)}";
        public const string CanAddroductAttributeValue = $"{nameof(ProductAttributeValue)}.{nameof(CanAddroductAttributeValue)}";
    }

    public static class Product
    {
        public const string CanGetProduct = $"{nameof(Product)}.{nameof(CanGetProduct)}";
        public const string CanRemoveProduct = $"{nameof(Product)}.{nameof(CanRemoveProduct)}";
        public const string CanAddProduct = $"{nameof(Product)}.{nameof(CanAddProduct)}";
    }

    public static class ProductReview
    {
        public const string CanGetProductReview = $"{nameof(ProductReview)}.{nameof(CanGetProductReview)}";
        public const string CanRemoveProductReview = $"{nameof(ProductReview)}.{nameof(CanRemoveProductReview)}";
        public const string CanAddProductReview = $"{nameof(ProductReview)}.{nameof(CanAddProductReview)}";
    }

    public static class Wishlist
    {
        public const string CanGetWishlist = $"{nameof(Wishlist)}.{nameof(CanGetWishlist)}";
        public const string CanRemoveWishlist = $"{nameof(Wishlist)}.{nameof(CanRemoveWishlist)}";
        public const string CanAddWishlist = $"{nameof(Wishlist)}.{nameof(CanAddWishlist)}";
    }

    public static class Media
    {
        public const string CanAddMedia = $"{nameof(Media)}.{nameof(CanAddMedia)}";
        public const string CanGetMedia = $"{nameof(Media)}.{nameof(CanGetMedia)}";
       
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
