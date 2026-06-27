using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAuction.UserService.Persistence.Migrations
{
    /// <summary>
    /// Seeds the complete SRS privilege catalog and the default privilege matrix
    /// for ADMIN, SELLER, BUYER, and SUPPORT_STAFF system roles.
    /// </summary>
    public partial class SeedSrsPrivilegeCatalog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Keep the privilege catalog and system-role matrix aligned with SRS section 2.7.
            migrationBuilder.Sql(
                """
                DECLARE @now datetime2(3) = SYSUTCDATETIME();

                DECLARE @Privileges TABLE
                (
                    [code] nvarchar(100) NOT NULL PRIMARY KEY,
                    [name] nvarchar(150) NOT NULL,
                    [description] nvarchar(500) NULL
                );

                INSERT INTO @Privileges ([code], [name], [description])
                VALUES
                    (N'AUTH.LOGIN', N'Login', N'Login to the system.'),
                    (N'AUTH.LOGOUT', N'Logout', N'Logout from the system.'),
                    (N'PROFILE.VIEW', N'View Own Profile', N'View the authenticated user''s profile.'),
                    (N'PROFILE.UPDATE', N'Update Own Profile', N'Update the authenticated user''s profile.'),
                    (N'PROFILE.CHANGE_PASSWORD', N'Change Own Password', N'Change the authenticated user''s password.'),
                    (N'PROFILE.RESET_PASSWORD', N'Reset Own Password', N'Reset the authenticated user''s password.'),
                    (N'USER.CREATE', N'Create User', N'Create a user account.'),
                    (N'USER.UPDATE', N'Update User', N'Update another user''s account.'),
                    (N'USER.DELETE', N'Delete User', N'Delete a user account.'),
                    (N'USER.VIEW', N'View User', N'View user details.'),
                    (N'USER.LIST', N'List Users', N'List and search users.'),
                    (N'USER.CHANGE_PASSWORD', N'Change User Password', N'Change another user''s password.'),
                    (N'USER.REPUTATION.VIEW', N'View User Reputation', N'View detailed user reputation information.'),
                    (N'USER.REPUTATION.ADJUST', N'Adjust User Reputation', N'Manually adjust a user''s reputation.'),
                    (N'ROLE.CREATE', N'Create Role', N'Create a custom RBAC role.'),
                    (N'ROLE.UPDATE', N'Update Role', N'Update a custom RBAC role.'),
                    (N'ROLE.DELETE', N'Delete Role', N'Delete an unused custom RBAC role.'),
                    (N'ROLE.VIEW', N'View Role', N'View role details and assigned privileges.'),
                    (N'ROLE.LIST', N'List Roles', N'List and search RBAC roles.'),
                    (N'PRODUCT.CREATE', N'Create Product', N'Create a product.'),
                    (N'PRODUCT.UPDATE', N'Update Product', N'Update a product.'),
                    (N'PRODUCT.DELETE', N'Delete Product', N'Delete or deactivate a product.'),
                    (N'PRODUCT.VIEW', N'View Product', N'View product details.'),
                    (N'PRODUCT.LIST', N'List Products', N'List products.'),
                    (N'PRODUCT.SEARCH', N'Search Products', N'Search products.'),
                    (N'CATEGORY.CREATE', N'Create Category', N'Create a product category.'),
                    (N'CATEGORY.UPDATE', N'Update Category', N'Update a product category.'),
                    (N'CATEGORY.DELETE', N'Delete Category', N'Delete or deactivate a product category.'),
                    (N'CATEGORY.VIEW', N'View Category', N'View category details.'),
                    (N'CATEGORY.LIST', N'List Categories', N'List product categories.'),
                    (N'CART.CREATE', N'Create Cart', N'Create a shopping cart.'),
                    (N'CART.UPDATE', N'Update Cart', N'Update shopping cart items.'),
                    (N'CART.REMOVE_ITEM', N'Remove Cart Item', N'Remove an item from the shopping cart.'),
                    (N'CART.VIEW', N'View Cart', N'View the current shopping cart.'),
                    (N'CHECKOUT.START', N'Start Checkout', N'Start the checkout process.'),
                    (N'ORDER.CREATE', N'Create Order', N'Create an order.'),
                    (N'ORDER.CANCEL', N'Cancel Order', N'Cancel an eligible order.'),
                    (N'ORDER.VIEW', N'View Order', N'View order details.'),
                    (N'ORDER.LIST', N'List Orders', N'List orders visible to the actor.'),
                    (N'ORDER.REFUND', N'Refund Order', N'Process an eligible order refund.'),
                    (N'AUCTION.CREATE', N'Create Auction', N'Create an auction.'),
                    (N'AUCTION.UPDATE', N'Update Auction', N'Update an eligible auction.'),
                    (N'AUCTION.CANCEL', N'Cancel Auction', N'Cancel an eligible owned auction.'),
                    (N'AUCTION.ADMIN_CANCEL', N'Administratively Cancel Auction', N'Force-cancel an auction as an administrator.'),
                    (N'AUCTION.VIEW', N'View Auction', N'View auction details.'),
                    (N'AUCTION.LIST', N'List Auctions', N'List auctions.'),
                    (N'AUCTION.BID', N'Place Bid', N'Place a bid on an active auction.'),
                    (N'AUCTION.VIEW_BID_HISTORY', N'View Bid History', N'View auction bid history.'),
                    (N'SHIPPING.QUOTE', N'Get Shipping Quote', N'Get a shipping quote.'),
                    (N'SHIPPING.CREATE_SHIPMENT', N'Create Shipment', N'Create a shipment for an eligible order.'),
                    (N'SHIPPING.VIEW_SHIPMENT', N'View Shipment', N'View shipment details.'),
                    (N'SHIPPING.UPDATE_STATUS', N'Update Shipment Status', N'Update shipment status.'),
                    (N'SHIPPING.MANUAL_OVERRIDE', N'Override Shipment', N'Manually override shipment processing.'),
                    (N'REPUTATION.VIEW', N'View Reputation', N'View reputation summary.'),
                    (N'REPUTATION.RATE', N'Rate Transaction', N'Rate a completed transaction.'),
                    (N'REPUTATION.PENALTY.VIEW', N'View Reputation Penalties', N'View reputation penalty records.'),
                    (N'REPUTATION.PENALTY.APPLY', N'Apply Reputation Penalty', N'Apply a reputation penalty.'),
                    (N'NOTIFICATION.VIEW', N'View Notifications', N'View notifications.'),
                    (N'NOTIFICATION.MANAGE_PREFERENCE', N'Manage Notification Preferences', N'Manage notification preferences.'),
                    (N'EVENT_LOG.VIEW', N'View Event Logs', N'View integration event logs.'),
                    (N'EVENT_LOG.VIEW_DETAIL', N'View Event Log Detail', N'View integration event payload details.'),
                    (N'AUDIT_LOG.VIEW', N'View Audit Logs', N'View security and administrative audit logs.'),
                    (N'SYSTEM.CONFIG.VIEW', N'View System Configuration', N'View system configuration.'),
                    (N'SYSTEM.CONFIG.UPDATE', N'Update System Configuration', N'Update system configuration.'),
                    (N'SYSTEM.HEALTH.VIEW', N'View System Health', N'View system health information.');

                UPDATE target
                SET target.[name] = source.[name],
                    target.[description] = source.[description],
                    target.[status] = N'ACTIVE'
                FROM [user].[Privileges] target
                INNER JOIN @Privileges source ON source.[code] = target.[code];

                INSERT INTO [user].[Privileges] ([id], [code], [name], [description], [status])
                SELECT NEWID(), source.[code], source.[name], source.[description], N'ACTIVE'
                FROM @Privileges source
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM [user].[Privileges] target
                    WHERE target.[code] = source.[code]
                );

                -- PRIVILEGE.LIST was an early implementation-only permission.
                -- The SRS has no Privilege Management use case, so the privilege catalog now uses ROLE.VIEW.
                DELETE rolePrivilege
                FROM [user].[RolePrivileges] rolePrivilege
                INNER JOIN [user].[Privileges] privilege
                    ON privilege.[id] = rolePrivilege.[privilege_id]
                WHERE privilege.[code] = N'PRIVILEGE.LIST';

                UPDATE [user].[Privileges]
                SET [status] = N'INACTIVE'
                WHERE [code] = N'PRIVILEGE.LIST';

                UPDATE [user].[Roles]
                SET [is_system_role] = 1,
                    [status] = N'ACTIVE'
                WHERE [code] IN (N'ADMIN', N'SELLER', N'BUYER', N'SUPPORT_STAFF');

                DECLARE @Assignments TABLE
                (
                    [role_code] nvarchar(50) NOT NULL,
                    [privilege_code] nvarchar(100) NOT NULL,
                    PRIMARY KEY ([role_code], [privilege_code])
                );

                INSERT INTO @Assignments ([role_code], [privilege_code])
                VALUES
                    (N'ADMIN', N'AUTH.LOGIN'),
                    (N'SELLER', N'AUTH.LOGIN'),
                    (N'BUYER', N'AUTH.LOGIN'),
                    (N'SUPPORT_STAFF', N'AUTH.LOGIN'),
                    (N'ADMIN', N'AUTH.LOGOUT'),
                    (N'SELLER', N'AUTH.LOGOUT'),
                    (N'BUYER', N'AUTH.LOGOUT'),
                    (N'SUPPORT_STAFF', N'AUTH.LOGOUT'),
                    (N'ADMIN', N'PROFILE.VIEW'),
                    (N'SELLER', N'PROFILE.VIEW'),
                    (N'BUYER', N'PROFILE.VIEW'),
                    (N'SUPPORT_STAFF', N'PROFILE.VIEW'),
                    (N'ADMIN', N'PROFILE.UPDATE'),
                    (N'SELLER', N'PROFILE.UPDATE'),
                    (N'BUYER', N'PROFILE.UPDATE'),
                    (N'SUPPORT_STAFF', N'PROFILE.UPDATE'),
                    (N'ADMIN', N'PROFILE.CHANGE_PASSWORD'),
                    (N'SELLER', N'PROFILE.CHANGE_PASSWORD'),
                    (N'BUYER', N'PROFILE.CHANGE_PASSWORD'),
                    (N'SUPPORT_STAFF', N'PROFILE.CHANGE_PASSWORD'),
                    (N'ADMIN', N'PROFILE.RESET_PASSWORD'),
                    (N'SELLER', N'PROFILE.RESET_PASSWORD'),
                    (N'BUYER', N'PROFILE.RESET_PASSWORD'),
                    (N'SUPPORT_STAFF', N'PROFILE.RESET_PASSWORD'),
                    (N'ADMIN', N'USER.CREATE'),
                    (N'ADMIN', N'USER.UPDATE'),
                    (N'SUPPORT_STAFF', N'USER.UPDATE'),
                    (N'ADMIN', N'USER.DELETE'),
                    (N'ADMIN', N'USER.VIEW'),
                    (N'SUPPORT_STAFF', N'USER.VIEW'),
                    (N'ADMIN', N'USER.LIST'),
                    (N'SUPPORT_STAFF', N'USER.LIST'),
                    (N'ADMIN', N'USER.CHANGE_PASSWORD'),
                    (N'ADMIN', N'USER.REPUTATION.VIEW'),
                    (N'SUPPORT_STAFF', N'USER.REPUTATION.VIEW'),
                    (N'ADMIN', N'USER.REPUTATION.ADJUST'),
                    (N'ADMIN', N'ROLE.CREATE'),
                    (N'ADMIN', N'ROLE.UPDATE'),
                    (N'ADMIN', N'ROLE.DELETE'),
                    (N'ADMIN', N'ROLE.VIEW'),
                    (N'SUPPORT_STAFF', N'ROLE.VIEW'),
                    (N'ADMIN', N'ROLE.LIST'),
                    (N'SUPPORT_STAFF', N'ROLE.LIST'),
                    (N'ADMIN', N'PRODUCT.CREATE'),
                    (N'SELLER', N'PRODUCT.CREATE'),
                    (N'ADMIN', N'PRODUCT.UPDATE'),
                    (N'SELLER', N'PRODUCT.UPDATE'),
                    (N'ADMIN', N'PRODUCT.DELETE'),
                    (N'SELLER', N'PRODUCT.DELETE'),
                    (N'ADMIN', N'PRODUCT.VIEW'),
                    (N'SELLER', N'PRODUCT.VIEW'),
                    (N'BUYER', N'PRODUCT.VIEW'),
                    (N'SUPPORT_STAFF', N'PRODUCT.VIEW'),
                    (N'ADMIN', N'PRODUCT.LIST'),
                    (N'SELLER', N'PRODUCT.LIST'),
                    (N'BUYER', N'PRODUCT.LIST'),
                    (N'SUPPORT_STAFF', N'PRODUCT.LIST'),
                    (N'ADMIN', N'PRODUCT.SEARCH'),
                    (N'SELLER', N'PRODUCT.SEARCH'),
                    (N'BUYER', N'PRODUCT.SEARCH'),
                    (N'SUPPORT_STAFF', N'PRODUCT.SEARCH'),
                    (N'ADMIN', N'CATEGORY.CREATE'),
                    (N'ADMIN', N'CATEGORY.UPDATE'),
                    (N'ADMIN', N'CATEGORY.DELETE'),
                    (N'ADMIN', N'CATEGORY.VIEW'),
                    (N'SELLER', N'CATEGORY.VIEW'),
                    (N'BUYER', N'CATEGORY.VIEW'),
                    (N'SUPPORT_STAFF', N'CATEGORY.VIEW'),
                    (N'ADMIN', N'CATEGORY.LIST'),
                    (N'SELLER', N'CATEGORY.LIST'),
                    (N'BUYER', N'CATEGORY.LIST'),
                    (N'SUPPORT_STAFF', N'CATEGORY.LIST'),
                    (N'BUYER', N'CART.CREATE'),
                    (N'BUYER', N'CART.UPDATE'),
                    (N'BUYER', N'CART.REMOVE_ITEM'),
                    (N'BUYER', N'CART.VIEW'),
                    (N'BUYER', N'CHECKOUT.START'),
                    (N'BUYER', N'ORDER.CREATE'),
                    (N'ADMIN', N'ORDER.CANCEL'),
                    (N'BUYER', N'ORDER.CANCEL'),
                    (N'SUPPORT_STAFF', N'ORDER.CANCEL'),
                    (N'ADMIN', N'ORDER.VIEW'),
                    (N'SELLER', N'ORDER.VIEW'),
                    (N'BUYER', N'ORDER.VIEW'),
                    (N'SUPPORT_STAFF', N'ORDER.VIEW'),
                    (N'ADMIN', N'ORDER.LIST'),
                    (N'SELLER', N'ORDER.LIST'),
                    (N'BUYER', N'ORDER.LIST'),
                    (N'SUPPORT_STAFF', N'ORDER.LIST'),
                    (N'ADMIN', N'ORDER.REFUND'),
                    (N'SUPPORT_STAFF', N'ORDER.REFUND'),
                    (N'ADMIN', N'AUCTION.CREATE'),
                    (N'SELLER', N'AUCTION.CREATE'),
                    (N'ADMIN', N'AUCTION.UPDATE'),
                    (N'SELLER', N'AUCTION.UPDATE'),
                    (N'ADMIN', N'AUCTION.CANCEL'),
                    (N'SELLER', N'AUCTION.CANCEL'),
                    (N'ADMIN', N'AUCTION.ADMIN_CANCEL'),
                    (N'ADMIN', N'AUCTION.VIEW'),
                    (N'SELLER', N'AUCTION.VIEW'),
                    (N'BUYER', N'AUCTION.VIEW'),
                    (N'SUPPORT_STAFF', N'AUCTION.VIEW'),
                    (N'ADMIN', N'AUCTION.LIST'),
                    (N'SELLER', N'AUCTION.LIST'),
                    (N'BUYER', N'AUCTION.LIST'),
                    (N'SUPPORT_STAFF', N'AUCTION.LIST'),
                    (N'BUYER', N'AUCTION.BID'),
                    (N'ADMIN', N'AUCTION.VIEW_BID_HISTORY'),
                    (N'SELLER', N'AUCTION.VIEW_BID_HISTORY'),
                    (N'BUYER', N'AUCTION.VIEW_BID_HISTORY'),
                    (N'SUPPORT_STAFF', N'AUCTION.VIEW_BID_HISTORY'),
                    (N'ADMIN', N'SHIPPING.QUOTE'),
                    (N'SELLER', N'SHIPPING.QUOTE'),
                    (N'BUYER', N'SHIPPING.QUOTE'),
                    (N'SUPPORT_STAFF', N'SHIPPING.QUOTE'),
                    (N'ADMIN', N'SHIPPING.CREATE_SHIPMENT'),
                    (N'SUPPORT_STAFF', N'SHIPPING.CREATE_SHIPMENT'),
                    (N'ADMIN', N'SHIPPING.VIEW_SHIPMENT'),
                    (N'SELLER', N'SHIPPING.VIEW_SHIPMENT'),
                    (N'BUYER', N'SHIPPING.VIEW_SHIPMENT'),
                    (N'SUPPORT_STAFF', N'SHIPPING.VIEW_SHIPMENT'),
                    (N'ADMIN', N'SHIPPING.UPDATE_STATUS'),
                    (N'SUPPORT_STAFF', N'SHIPPING.UPDATE_STATUS'),
                    (N'ADMIN', N'SHIPPING.MANUAL_OVERRIDE'),
                    (N'SUPPORT_STAFF', N'SHIPPING.MANUAL_OVERRIDE'),
                    (N'ADMIN', N'REPUTATION.VIEW'),
                    (N'SELLER', N'REPUTATION.VIEW'),
                    (N'BUYER', N'REPUTATION.VIEW'),
                    (N'SUPPORT_STAFF', N'REPUTATION.VIEW'),
                    (N'SELLER', N'REPUTATION.RATE'),
                    (N'BUYER', N'REPUTATION.RATE'),
                    (N'ADMIN', N'REPUTATION.PENALTY.VIEW'),
                    (N'SUPPORT_STAFF', N'REPUTATION.PENALTY.VIEW'),
                    (N'ADMIN', N'REPUTATION.PENALTY.APPLY'),
                    (N'ADMIN', N'NOTIFICATION.VIEW'),
                    (N'SELLER', N'NOTIFICATION.VIEW'),
                    (N'BUYER', N'NOTIFICATION.VIEW'),
                    (N'SUPPORT_STAFF', N'NOTIFICATION.VIEW'),
                    (N'SELLER', N'NOTIFICATION.MANAGE_PREFERENCE'),
                    (N'BUYER', N'NOTIFICATION.MANAGE_PREFERENCE'),
                    (N'ADMIN', N'EVENT_LOG.VIEW'),
                    (N'SUPPORT_STAFF', N'EVENT_LOG.VIEW'),
                    (N'ADMIN', N'EVENT_LOG.VIEW_DETAIL'),
                    (N'SUPPORT_STAFF', N'EVENT_LOG.VIEW_DETAIL'),
                    (N'ADMIN', N'AUDIT_LOG.VIEW'),
                    (N'ADMIN', N'SYSTEM.CONFIG.VIEW'),
                    (N'ADMIN', N'SYSTEM.CONFIG.UPDATE'),
                    (N'ADMIN', N'SYSTEM.HEALTH.VIEW'),
                    (N'SUPPORT_STAFF', N'SYSTEM.HEALTH.VIEW');

                -- System roles are fixed application contracts. Remove assignments that are outside the SRS matrix.
                DELETE rolePrivilege
                FROM [user].[RolePrivileges] rolePrivilege
                INNER JOIN [user].[Roles] role ON role.[id] = rolePrivilege.[role_id]
                INNER JOIN [user].[Privileges] privilege ON privilege.[id] = rolePrivilege.[privilege_id]
                INNER JOIN @Privileges catalog ON catalog.[code] = privilege.[code]
                WHERE role.[code] IN (N'ADMIN', N'SELLER', N'BUYER', N'SUPPORT_STAFF')
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM @Assignments expected
                      WHERE expected.[role_code] = role.[code]
                        AND expected.[privilege_code] = privilege.[code]
                  );

                INSERT INTO [user].[RolePrivileges]
                    ([id], [role_id], [privilege_id], [assigned_by], [assigned_at])
                SELECT NEWID(), role.[id], privilege.[id], NULL, @now
                FROM @Assignments assignment
                INNER JOIN [user].[Roles] role ON role.[code] = assignment.[role_code]
                INNER JOIN [user].[Privileges] privilege ON privilege.[code] = assignment.[privilege_code]
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM [user].[RolePrivileges] existing
                    WHERE existing.[role_id] = role.[id]
                      AND existing.[privilege_id] = privilege.[id]
                );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the small Role Management seed that existed before this migration.
            migrationBuilder.Sql(
                """
                DECLARE @now datetime2(3) = SYSUTCDATETIME();

                DECLARE @Catalog TABLE ([code] nvarchar(100) NOT NULL PRIMARY KEY);
                INSERT INTO @Catalog ([code])
                VALUES
                    (N'AUTH.LOGIN'),
                    (N'AUTH.LOGOUT'),
                    (N'PROFILE.VIEW'),
                    (N'PROFILE.UPDATE'),
                    (N'PROFILE.CHANGE_PASSWORD'),
                    (N'PROFILE.RESET_PASSWORD'),
                    (N'USER.CREATE'),
                    (N'USER.UPDATE'),
                    (N'USER.DELETE'),
                    (N'USER.VIEW'),
                    (N'USER.LIST'),
                    (N'USER.CHANGE_PASSWORD'),
                    (N'USER.REPUTATION.VIEW'),
                    (N'USER.REPUTATION.ADJUST'),
                    (N'ROLE.CREATE'),
                    (N'ROLE.UPDATE'),
                    (N'ROLE.DELETE'),
                    (N'ROLE.VIEW'),
                    (N'ROLE.LIST'),
                    (N'PRODUCT.CREATE'),
                    (N'PRODUCT.UPDATE'),
                    (N'PRODUCT.DELETE'),
                    (N'PRODUCT.VIEW'),
                    (N'PRODUCT.LIST'),
                    (N'PRODUCT.SEARCH'),
                    (N'CATEGORY.CREATE'),
                    (N'CATEGORY.UPDATE'),
                    (N'CATEGORY.DELETE'),
                    (N'CATEGORY.VIEW'),
                    (N'CATEGORY.LIST'),
                    (N'CART.CREATE'),
                    (N'CART.UPDATE'),
                    (N'CART.REMOVE_ITEM'),
                    (N'CART.VIEW'),
                    (N'CHECKOUT.START'),
                    (N'ORDER.CREATE'),
                    (N'ORDER.CANCEL'),
                    (N'ORDER.VIEW'),
                    (N'ORDER.LIST'),
                    (N'ORDER.REFUND'),
                    (N'AUCTION.CREATE'),
                    (N'AUCTION.UPDATE'),
                    (N'AUCTION.CANCEL'),
                    (N'AUCTION.ADMIN_CANCEL'),
                    (N'AUCTION.VIEW'),
                    (N'AUCTION.LIST'),
                    (N'AUCTION.BID'),
                    (N'AUCTION.VIEW_BID_HISTORY'),
                    (N'SHIPPING.QUOTE'),
                    (N'SHIPPING.CREATE_SHIPMENT'),
                    (N'SHIPPING.VIEW_SHIPMENT'),
                    (N'SHIPPING.UPDATE_STATUS'),
                    (N'SHIPPING.MANUAL_OVERRIDE'),
                    (N'REPUTATION.VIEW'),
                    (N'REPUTATION.RATE'),
                    (N'REPUTATION.PENALTY.VIEW'),
                    (N'REPUTATION.PENALTY.APPLY'),
                    (N'NOTIFICATION.VIEW'),
                    (N'NOTIFICATION.MANAGE_PREFERENCE'),
                    (N'EVENT_LOG.VIEW'),
                    (N'EVENT_LOG.VIEW_DETAIL'),
                    (N'AUDIT_LOG.VIEW'),
                    (N'SYSTEM.CONFIG.VIEW'),
                    (N'SYSTEM.CONFIG.UPDATE'),
                    (N'SYSTEM.HEALTH.VIEW');

                DELETE rolePrivilege
                FROM [user].[RolePrivileges] rolePrivilege
                INNER JOIN [user].[Roles] role ON role.[id] = rolePrivilege.[role_id]
                INNER JOIN [user].[Privileges] privilege ON privilege.[id] = rolePrivilege.[privilege_id]
                INNER JOIN @Catalog catalog ON catalog.[code] = privilege.[code]
                WHERE role.[code] IN (N'ADMIN', N'SELLER', N'BUYER', N'SUPPORT_STAFF');

                UPDATE privilege
                SET privilege.[status] = N'INACTIVE'
                FROM [user].[Privileges] privilege
                INNER JOIN @Catalog catalog ON catalog.[code] = privilege.[code]
                WHERE privilege.[code] NOT IN
                    (N'ROLE.CREATE', N'ROLE.UPDATE', N'ROLE.DELETE', N'ROLE.VIEW', N'ROLE.LIST');

                UPDATE [user].[Privileges]
                SET [status] = N'ACTIVE'
                WHERE [code] IN
                    (N'ROLE.CREATE', N'ROLE.UPDATE', N'ROLE.DELETE', N'ROLE.VIEW', N'ROLE.LIST', N'PRIVILEGE.LIST');

                DECLARE @adminRoleId uniqueidentifier =
                    (SELECT TOP (1) [id] FROM [user].[Roles] WHERE [code] = N'ADMIN');

                INSERT INTO [user].[RolePrivileges]
                    ([id], [role_id], [privilege_id], [assigned_by], [assigned_at])
                SELECT NEWID(), @adminRoleId, privilege.[id], NULL, @now
                FROM [user].[Privileges] privilege
                WHERE privilege.[code] IN
                    (N'ROLE.CREATE', N'ROLE.UPDATE', N'ROLE.DELETE', N'ROLE.VIEW', N'ROLE.LIST', N'PRIVILEGE.LIST')
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM [user].[RolePrivileges] existing
                      WHERE existing.[role_id] = @adminRoleId
                        AND existing.[privilege_id] = privilege.[id]
                  );
                """);
        }
    }
}
