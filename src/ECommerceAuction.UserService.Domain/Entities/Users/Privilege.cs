namespace ECommerceAuction.UserService.Domain.Entities.Users;

/// <summary>
/// Represents a fine-grained action that can be granted to an RBAC role.
/// </summary>
public sealed class Privilege
{
    private Privilege()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Status { get; private set; } = PrivilegeStatuses.Active;

    /// <summary>
    /// Creates a system-defined privilege used by RBAC seed data and authorization policies.
    /// </summary>
    public static Privilege CreateSystem(string code, string name, string? description)
    {
        return new Privilege
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Status = PrivilegeStatuses.Active
        };
    }
}

/// <summary>
/// Defines privilege lifecycle statuses.
/// </summary>
public static class PrivilegeStatuses
{
    public const string Active = "ACTIVE";
    public const string Inactive = "INACTIVE";
}

/// <summary>
/// Centralizes the privilege codes defined by section 2.7 of the SRS.
/// These strings are stable authorization contracts shared by seed data, JWT claims, and API policies.
/// </summary>
public static class PrivilegeCodes
{
    public const string AuthLogin = "AUTH.LOGIN";
    public const string AuthLogout = "AUTH.LOGOUT";

    public const string ProfileView = "PROFILE.VIEW";
    public const string ProfileUpdate = "PROFILE.UPDATE";
    public const string ProfileChangePassword = "PROFILE.CHANGE_PASSWORD";
    public const string ProfileResetPassword = "PROFILE.RESET_PASSWORD";

    public const string UserCreate = "USER.CREATE";
    public const string UserUpdate = "USER.UPDATE";
    public const string UserDelete = "USER.DELETE";
    public const string UserView = "USER.VIEW";
    public const string UserList = "USER.LIST";
    public const string UserChangePassword = "USER.CHANGE_PASSWORD";
    public const string UserReputationView = "USER.REPUTATION.VIEW";
    public const string UserReputationAdjust = "USER.REPUTATION.ADJUST";

    public const string RoleCreate = "ROLE.CREATE";
    public const string RoleUpdate = "ROLE.UPDATE";
    public const string RoleDelete = "ROLE.DELETE";
    public const string RoleView = "ROLE.VIEW";
    public const string RoleList = "ROLE.LIST";

    public const string ProductCreate = "PRODUCT.CREATE";
    public const string ProductUpdate = "PRODUCT.UPDATE";
    public const string ProductDelete = "PRODUCT.DELETE";
    public const string ProductView = "PRODUCT.VIEW";
    public const string ProductList = "PRODUCT.LIST";
    public const string ProductSearch = "PRODUCT.SEARCH";

    public const string CategoryCreate = "CATEGORY.CREATE";
    public const string CategoryUpdate = "CATEGORY.UPDATE";
    public const string CategoryDelete = "CATEGORY.DELETE";
    public const string CategoryView = "CATEGORY.VIEW";
    public const string CategoryList = "CATEGORY.LIST";

    public const string CartCreate = "CART.CREATE";
    public const string CartUpdate = "CART.UPDATE";
    public const string CartRemoveItem = "CART.REMOVE_ITEM";
    public const string CartView = "CART.VIEW";
    public const string CheckoutStart = "CHECKOUT.START";

    public const string OrderCreate = "ORDER.CREATE";
    public const string OrderCancel = "ORDER.CANCEL";
    public const string OrderView = "ORDER.VIEW";
    public const string OrderList = "ORDER.LIST";
    public const string OrderRefund = "ORDER.REFUND";

    public const string AuctionCreate = "AUCTION.CREATE";
    public const string AuctionUpdate = "AUCTION.UPDATE";
    public const string AuctionCancel = "AUCTION.CANCEL";
    public const string AuctionAdminCancel = "AUCTION.ADMIN_CANCEL";
    public const string AuctionView = "AUCTION.VIEW";
    public const string AuctionList = "AUCTION.LIST";
    public const string AuctionBid = "AUCTION.BID";
    public const string AuctionViewBidHistory = "AUCTION.VIEW_BID_HISTORY";

    public const string ShippingQuote = "SHIPPING.QUOTE";
    public const string ShippingCreateShipment = "SHIPPING.CREATE_SHIPMENT";
    public const string ShippingViewShipment = "SHIPPING.VIEW_SHIPMENT";
    public const string ShippingUpdateStatus = "SHIPPING.UPDATE_STATUS";
    public const string ShippingManualOverride = "SHIPPING.MANUAL_OVERRIDE";

    public const string ReputationView = "REPUTATION.VIEW";
    public const string ReputationRate = "REPUTATION.RATE";
    public const string ReputationPenaltyView = "REPUTATION.PENALTY.VIEW";
    public const string ReputationPenaltyApply = "REPUTATION.PENALTY.APPLY";

    public const string NotificationView = "NOTIFICATION.VIEW";
    public const string NotificationManagePreference = "NOTIFICATION.MANAGE_PREFERENCE";

    public const string EventLogView = "EVENT_LOG.VIEW";
    public const string EventLogViewDetail = "EVENT_LOG.VIEW_DETAIL";
    public const string AuditLogView = "AUDIT_LOG.VIEW";

    public const string SystemConfigView = "SYSTEM.CONFIG.VIEW";
    public const string SystemConfigUpdate = "SYSTEM.CONFIG.UPDATE";
    public const string SystemHealthView = "SYSTEM.HEALTH.VIEW";
}
