namespace Sheared;

public static class Endpoints
{
    public static class Authentication
    {
        public const string RegisterTenant = "api/tenant/register";        
        public const string GetTenentId = "api/get_tenent_id";
        public const string ResendLink = "api/resend_link";
        public const string Verify = "api/verify";
        public const string Login = "api/login";
    }
    public static class User
    {
        public const string GetUsers = "api/users";
        public const string CreateUser = "api/user";
        public const string GetUserById = "api/users/{id}";
        public const string UpdateUser = "api/users/{id}";
        public const string DeleteUser = "api/users/{id}";
    }
}
