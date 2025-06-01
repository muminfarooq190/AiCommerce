namespace Sheared
{
    public static class Endpoints
    {
        /* ──────────── Authentication ──────────── */
        public static class Authentication
        {
            private const string Base = "api/Authentication";
            public const string RegisterTenant = Base + "/tenant/register";
            public const string GetTenentId = Base + "/get_tenent_id";
            public const string ResendLink = Base + "/resend_link";
            public const string Verify = Base + "/verify";
            public const string Login = Base + "/login";
        }

        /* ──────────── Users ──────────── */
        public static class User
        {
            private const string Base = "api/users";
            public const string GetUsers = Base;            // GET  api/users
            public const string CreateUser = Base;            // POST api/users  ← FIX same path, diff verb (allowed)
        }

        /* ──────────── Permissions ──────────── */
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

        /* ──────────── Categories ──────────── */
        public static class Categories
        {
            private const string Base = "api/categories";
            public const string GetAll = Base + "/get-all";
            public const string GetById = Base + "/{id:guid}";
            public const string Create = Base;
            public const string Update = Base + "/{id:guid}";
            public const string Delete = Base + "/{id:guid}";
           
        }

        /* ──────────── Products ──────────── */
        public static class Products
        {
            private const string Base = "api/products";
            public const string GetAll = Base + "/get-all";
            public const string GetById = Base + "/{id:guid}";
            public const string Create = Base;
            public const string Update = Base + "/{id:guid}";
            public const string Delete = Base + "/{id:guid}";
            public const string ImageUpload = Base + "/{id:guid}/images";
            public const string ImageRemove = Base + "/{id:guid}/images/{mediaId:guid}";
        }

        /* ──────────── Orders ──────────── */
        public static class Orders
        {
            private const string Base = "api/orders";
            public const string GetAll = Base + "/get-all";
            public const string GetById = Base + "/{id:guid}";
            public const string Create = Base;
            public const string UpdateStatus = Base + "/{id:guid}/status";
            public const string AddPayment = Base + "/{id:guid}/payments";
            public const string AddShipment = Base + "/{id:guid}/shipments";
            public const string ShipUpdate = Base + "/{id:guid}/shipments/{shipId:guid}";
        }

        /* ──────────── Cart ──────────── */
        public static class Cart
        {
            private const string Base = "api/cart";
            public const string GetItem = Base + "/items/{itemId:guid}";
            public const string AddItem = Base + "/items";
            public const string UpdateItemQty = Base;                       // PUT / PATCH
            public const string RemoveItem = Base;                       // DELETE
            public const string Clear = Base + "/clear";
        }

        /* ──────────── Wishlist ──────────── */
        public static class Wishlist
        {
            private const string Base = "api/wishlist";
            public const string GetList = Base + "/get-all";
            public const string AddItem = Base + "/{productId:guid}";
            public const string RemoveItem = Base + "/{productId:guid}";
        }

        /* ──────────── Reviews ──────────── */
        public static class Reviews
        {
            private const string Base = "api/products";
            public const string GetReviews = Base + "/{productId:guid}/reviews";
            public const string AddReview = Base + "/{productId:guid}/reviews";
        }

        /* ──────────── Product Attributes ──────────── */
        public static class ProductAttributes
        {
            private const string Base = "api/products/{productId:guid}/attributes";
            public const string GetList = Base + "/get-all";
            public const string ByAttr = Base + "/{attributeId:guid}";
        }

        /* ──────────── Collections ──────────── */
        public static class Collections
        {
            private const string Base = "api/collections";
            public const string GetAllCollection = Base + "/get-all";
            public const string AddCollection = Base + "/add-collection";
            public const string GetCollectionById = Base + "/{id:guid}";
            public const string UpdateCollection = Base + "/{id:guid}";
            public const string DeleteCollection = Base + "/{id:guid}";

            // unique nested paths ↓↓↓
            public const string GetCollectionIds = Base + "/{id:guid}/products";
            public const string AddProduct = Base + "/{id:guid}/products";
            public const string RemoveProduct = Base + "/{id:guid}/products/{prodId:guid}";
        }

        /* ──────────── Discounts ──────────── */
        public static class Discounts
        {
            private const string Base = "api/discounts";
            public const string GetAll = Base + "/get-all";
            public const string Add = Base + "/add-discount";
            public const string GetById = Base + "/{id:guid}";
            public const string Update = Base + "/{id:guid}";
            public const string ApplyToOrder = "api/orders/{orderId:guid}/discounts";
            public const string RemoveFromOrder = ApplyToOrder + "/{code}";
        }

        public static class Media
        {
            private const string Base = "api/media";
            public const string Upload = Base + "/uplaod";
            public const string Get = Base + "/{id:guid}";
        }
    }
}
