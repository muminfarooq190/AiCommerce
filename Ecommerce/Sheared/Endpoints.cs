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
}
