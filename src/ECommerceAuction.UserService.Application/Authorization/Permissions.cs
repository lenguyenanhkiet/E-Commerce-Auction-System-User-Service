namespace ECommerceAuction.UserService.Application.Authorization;

public static class Permissions
{
    public const string ServiceCode = "user";

    public static class Auth
    {
        public const string Login = "AUTH.LOGIN";
        public const string Logout = "AUTH.LOGOUT";
    }
    public static class Profile
    {
        public const string View = "PROFILE.VIEW";
        public const string Update = "PROFILE.UPDATE";
        public const string ChangePassword = "PROFILE.CHANGE_PASSWORD";
        public const string ResetPassword = "PROFILE.RESET_PASSWORD";
    }
    public static class Users
    {
        public const string Create = "USER.CREATE";
        public const string Update = "USER.UPDATE";
        public const string Delete = "USER.DELETE";
        public const string View = "USER.VIEW";
        public const string List = "USER.LIST";
        public const string ChangePassword = "USER.CHANGE_PASSWORD";
    }
    public static class UserReputation
    {
        public const string View = "USER.REPUTATION.VIEW";
        public const string Adjust = "USER.REPUTATION.ADJUST";
    }
    public static class Roles
    {
        public const string Create = "ROLE.CREATE";
        public const string Update = "ROLE.UPDATE";
        public const string Delete = "ROLE.DELETE";
        public const string View = "ROLE.VIEW";
        public const string List = "ROLE.LIST";
    }
    public static class Reputation
    {
        public const string View = "REPUTATION.VIEW";
        public const string Rate = "REPUTATION.RATE";
    }
    public static class ReputationAdmin
    {
        public const string View = "REPUTATION.ADMIN.VIEW";
        public const string Adjust = "REPUTATION.ADMIN.ADJUST";
        public const string Reverse = "REPUTATION.ADMIN.REVERSE";
        public const string ClearRestriction = "REPUTATION.RESTRICTION.CLEAR";
    }
    public static class ReputationPenalty
    {
        public const string View = "REPUTATION.PENALTY.VIEW";
        public const string Apply = "REPUTATION.PENALTY.APPLY";
    }
    public static class AuditLogs
    {
        public const string View = "AUDIT_LOG.VIEW";
    }
    public static class System
    {
        public const string ViewConfig = "SYSTEM.CONFIG.VIEW";
        public const string UpdateConfig = "SYSTEM.CONFIG.UPDATE";
    }
    public static class SystemHealth
    {
        public const string View = "SYSTEM.HEALTH.VIEW";
    }

    public static class Sellers
    {
        [PermissionDescription("View the list of seller applications.")]
        public const string ListApplications = "SELLER.APPLICATION.LIST";

        [PermissionDescription("View seller application details")]
        public const string ViewApplication = "SELLER.APPLICATION.VIEW";

        [PermissionDescription("Approved seller application")]
        public const string ApproveApplication = "SELLER.APPLICATION.APPROVE";

        [PermissionDescription("Rejecting seller application")]
        public const string RejectApplication = "SELLER.APPLICATION.REJECT";
    }
}
