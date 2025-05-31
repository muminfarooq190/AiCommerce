namespace Sheared;

public static class Endpoints
{
    public static class Authentication
    {
        private const string Base = "api/Authentication";
        public const string RegisterTenant = Base + "/tenant/register";        
        public const string GetTenentId = Base + "/get_tenent_id";
        public const string ResendLink = Base + "/resend_link";
        public const string Verify = Base + "/verify";
        public const string Login = Base + "/login";
    }
    public static class User
    {
        private const string Base = "api/users";
        public const string GetUsers = Base + "/api/users";
        public const string CreateUser = "/api/user";
    }
    public static class Permisstion
    {
        private const string Base = "api/Permisstion";
        public const string GetAllPermissionsAsJson = Base + "/get_all_permissions_as_json";
        public const string GetAllPermissions = Base + "/get_all_permissions";
        public const string RemovePermission = Base + "/remove_permission";
        public const string RemovePermissionByName = Base + "/remove_permission_by_name";
        public const string GivePermission = Base + "/give_permission";
        public const string GetPermissions = Base + "/get_permissions";
    }

    public static class Categories
    {
        private const string Base = "api/categories";
        public const string GetAll = Base;
        public const string GetById = Base + "/{id:guid}";
        public const string Create = Base;                   
        public const string Update = Base + "/{id:guid}";                  
        public const string Delete = Base + "/{id:guid}";                  
        public const string FeaturedImageUpload = Base + "/{id:guid}" + "/featured-image";  
        public const string FeaturedImageRemove = Base + "/{id:guid}" + "/featured-image";   
    }

    public static class Products
    {
        private const string Base = "api/products";
        public const string GetAll = Base;
        public const string GetById = Base + "/{id:guid}";
        public const string Create = Base;
        public const string Update = Base + "/{id:guid}";
        public const string Delete = Base + "/{id:guid}";

        public const string ImageUpload = Base + "/{id:guid}" + "/images";         
        public const string ImageRemove = Base + "/{id:guid}" + "/images/{mediaId:guid}";
    }

    public static class Orders
    {
        private const string Base = "api/orders";
        public const string GetAll = Base;
        public const string GetById = Base + "/{id:guid}";
        public const string Create = Base;
        public const string UpdateStatus = Base + "/{id:guid}" + "/status";
        public const string AddPayment = Base + "/{id:guid}" + "/payments";
        public const string AddShipment = Base + "/{id:guid}" + "/shipments";
        public const string ShipUpdate = Base + "/{id:guid}" + "/shipments/{shipId:guid}";
    }

    public static class Cart
    {
        private const string Base = "api/cart";                 
        public const string GetItem = Base + "/items/{itemId:guid}";
        public const string AddItem = Base + "/items";
        public const string UpdateItemQty = Base;
        public const string RemoveItem = Base;
        public const string Clear = Base;
    }

    public static class Wishlist
    {
        private const string Base = "api/wishlist"; 
        public const string GetList = Base;
        public const string AddItem = Base + "/{productId:guid}";
        public const string RemoveItem = Base + "/{productId:guid}";
    }

    public static class Reviews
    {
        private const string Base = "api/products";
        public const string GetReviews = Base + "/{productId:guid}/reviews";
        public const string AddReview = Base + "/{productId:guid}/reviews";
    }

    public static class ProductAttributes
    {
        private const string Base = "api/products/{productId:guid}/attributes"; 
        public const string GetList = Base;
        public const string ByAttr = Base + "/{attributeId:guid}";
    }

    public static class Collections
    {
        private const string Base = "api/collections";
        public const string GetAllCollection = Base ;
        public const string AddCollection = Base ;
        public const string GetCollectionById = Base + "/{id:guid}";
        public const string UpdateCollection = Base + "/{id:guid}";
        public const string DeleteCollection = Base + "/{id:guid}";
        public const string GetCollectionIds = Base + "/{id:guid}";
        public const string AddProduct = Base + "/{id:guid}";
        public const string RemoveProduct = Base + "/{id:guid}/products/{prodId:guid}";
    }

    public static class Discounts
    {
        private const string Base = "api/discounts";
        public const string GetAll = Base;
        public const string Add = Base;
        public const string GetById = Base + "/{id:guid}";
        public const string Update = Base + "/{id:guid}";
        public const string ApplyToOrder = "api/orders/{orderId:guid}/discounts"; 
        public const string RemoveFromOrder = ApplyToOrder + "/{code}";
    }
}
