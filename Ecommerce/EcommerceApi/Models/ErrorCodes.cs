namespace EcommerceApi.Models;

public class ErrorCodes
{
    public const string Unauthenticated = "UN_AUTHENTICATED";
    public const string TanentIdMissing = "TANENT_ID_MISSING";
    public const string TenantIdMissingOrInvalid = "TANENT_ID_MISSING_OR_INVALID";
    public const string InvalidEmailOrPassword = "INVAID_EMAIL_OR_PASSWORD";
    public const string UnverifiedEmail = "UNVERIFIED_EMAIL";
    public const string AccountLocked = "ACCOUNT_LOCKED";
    public const string EmailAlreadyExist = "EMAIL_ALREADY_EXIST";
    public const string CompanyAlreadyExist = "COMPANY_ALREADY_EXIST";
    public const string CompanyNotExist = "COMPANY_NOT_EXIST";
    public const string PhoneAlreadyExist = "PHONE_ALREADY_EXIST";
    public const string AccountNotPrimary = "ACCOUNT_NOT_PRIMARY";
    public const string AlreadyVerifiedEmail = "ALREADY_VERIFIED_EMAIL";
    public const string ExpiredOrInvalidLink = "EXPIRED_OR_INVALID";
    public const string InvalidPermission = "INVALID_PERMISSION";
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string PermissionAlreadyExists = "PERMISSION_ALREADY_EXIST";
    public const string PermissionNotExist = "PERMISSION_NOT_EXIST";
    public const string InsufficientPermissions = "Insufficient_Permissions";
    public const string CantRemoveAccessToPrimary = "CANT_REMOVE_ACCESS_TO_PRIMARY";
    public const string InvalidPaginationParameters = "INVALID_PAGINATION_PARAMETERS";
}
