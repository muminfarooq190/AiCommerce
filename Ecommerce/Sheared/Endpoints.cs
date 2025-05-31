namespace Sheared;

public static class Endpoints
{
    public static class AuthenticationEndpoints
    {
        public const string RegisterTenant = "api/tenant/register";
        public const string CreateUser = "api/createuser";
        public const string GetTenentId = "api/get_tenent_id";
        public const string ResendLink = "api/resend_link";
        public const string Verify = "api/verify";
        public const string Login = "api/login";
    }
    public static class UserEndpoints
    {
        public const string GetUsers = "api/users";
        public const string GetUserById = "api/users/{id}";
        public const string UpdateUser = "api/users/{id}";
        public const string DeleteUser = "api/users/{id}";
    }

    public static class Categories
    {
        private const string Base = "api/categories";
        public const string ById = Base + "/{id:guid}";
        public const string Create = Base;                   
        public const string Update = ById;                  
        public const string Delete = ById;                  
        public const string FeaturedImageUpload = ById + "/featured-image";  
        public const string FeaturedImageRemove = ById + "/featured-image";   
    }

    public static class Products
    {
        private const string Base = "api/products";
        public const string ById = Base + "/{id:guid}";
        public const string Create = Base;
        public const string Update = ById;
        public const string Delete = ById;

        public const string ImageUpload = ById + "/images";         
        public const string ImageRemove = ById + "/images/{mediaId:guid}";
    }

    public static class Orders
    {
        private const string Base = "api/orders";
        public const string ById = Base + "/{id:guid}";
        public const string Create = Base;

        public const string UpdateStatus = ById + "/status";        // POST
        public const string AddPayment = ById + "/payments";      // POST
        public const string AddShipment = ById + "/shipments";     // POST
        public const string ShipUpdate = ById + "/shipments/{shipId:guid}"; // PUT
    }

    public static class Cart
    {
        private const string Base = "api/cart";                 
        public const string Item = Base + "/items/{itemId:guid}";
        public const string Items = Base + "/items";              // POST, PUT
        public const string Clear = Base;                         // DELETE
    }

    public static class Wishlist
    {
        private const string Base = "api/wishlist";               // current shopper
        public const string Item = Base + "/{productId:guid}";   // POST/DELETE
    }

    public static class Reviews
    {
        public const string ByProduct = "api/products/{productId:guid}/reviews";
    }

    public static class ProductAttributes
    {
        public const string Base =
            "api/products/{productId:guid}/attributes";           // GET

        public const string ByAttr =
            Base + "/{attributeId:guid}";                         // PUT
    }

    public static class Collections
    {
        public const string Base = "api/collections";
        public const string ById = Base + "/{id:guid}";
        public const string Products = ById + "/products";                // GET / POST bulk
        public const string Product = ById + "/products/{prodId:guid}";  // DELETE
    }

    public static class Discounts
    {
        public const string Base = "api/discounts";
        public const string ById = Base + "/{id:guid}";
        public const string ApplyToOrder = "api/orders/{orderId:guid}/discounts"; // POST remove via DELETE
        public const string RemoveFromOrder = ApplyToOrder + "/{code}";
    }
}
