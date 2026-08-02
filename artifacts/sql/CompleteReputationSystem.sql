IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    IF SCHEMA_ID(N'user') IS NULL EXEC(N'CREATE SCHEMA [user];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE TABLE [user].[IdentityVerification] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [IdentityNumber] nvarchar(50) NOT NULL,
        [IdentityFrontImageUrl] nvarchar(500) NOT NULL,
        [IdentityBackImageUrl] nvarchar(500) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [RejectionReason] nvarchar(1000) NULL,
        [ConfidenceScore] decimal(5,4) NULL,
        [SubmittedAt] datetime2 NOT NULL,
        [VerifiedAt] datetime2 NULL,
        [VerifiedBy] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_IdentityVerification] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE TABLE [user].[PasswordHistories] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [PasswordHash] nvarchar(255) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PasswordHistories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE TABLE [user].[PasswordResetTokens] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [token] nvarchar(500) NOT NULL,
        [expiry_date] datetime2(3) NOT NULL,
        [is_used] bit NOT NULL DEFAULT CAST(0 AS bit),
        [created_at] datetime2(3) NOT NULL,
        CONSTRAINT [PK_PasswordResetTokens] PRIMARY KEY ([id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE TABLE [user].[Privileges] (
        [id] uniqueidentifier NOT NULL,
        [code] nvarchar(100) NOT NULL,
        [name] nvarchar(150) NOT NULL,
        [description] nvarchar(500) NULL,
        [status] nvarchar(30) NOT NULL DEFAULT N'ACTIVE',
        CONSTRAINT [PK_Privileges] PRIMARY KEY ([id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE TABLE [user].[Roles] (
        [id] uniqueidentifier NOT NULL,
        [code] nvarchar(50) NOT NULL,
        [name] nvarchar(150) NOT NULL,
        [description] nvarchar(500) NULL,
        [is_system_role] bit NOT NULL DEFAULT CAST(0 AS bit),
        [status] nvarchar(30) NOT NULL DEFAULT N'ACTIVE',
        [created_at] datetime2(3) NOT NULL,
        [updated_at] datetime2(3) NOT NULL,
        [deleted_at] datetime2(3) NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE TABLE [user].[SellerProfiles] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [SellerType] nvarchar(20) NOT NULL,
        [BusinessName] nvarchar(255) NOT NULL,
        [TaxCode] nvarchar(50) NOT NULL,
        [BusinessLicenseUrl] nvarchar(500) NOT NULL,
        [Address] nvarchar(500) NOT NULL,
        [BankAccountNumber] nvarchar(50) NOT NULL,
        [BankName] nvarchar(255) NOT NULL,
        [BankAccountHolder] nvarchar(255) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [RejectReason] nvarchar(1000) NULL,
        [SubmittedAt] datetime2 NOT NULL,
        [ReviewedAt] datetime2 NULL,
        [ReviewedBy] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_SellerProfiles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE TABLE [user].[UserAuditLogs] (
        [id] uniqueidentifier NOT NULL,
        [actor_user_id] uniqueidentifier NULL,
        [target_user_id] uniqueidentifier NULL,
        [action] nvarchar(100) NOT NULL,
        [entity_type] nvarchar(100) NOT NULL,
        [entity_id] nvarchar(100) NULL,
        [old_value] nvarchar(max) NULL,
        [new_value] nvarchar(max) NULL,
        [ip_address] nvarchar(50) NULL,
        [user_agent] nvarchar(500) NULL,
        [created_at] datetime2(3) NOT NULL,
        CONSTRAINT [PK_UserAuditLogs] PRIMARY KEY ([id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE TABLE [user].[Users] (
        [id] uniqueidentifier NOT NULL,
        [email] nvarchar(255) NOT NULL,
        [phone_number] nvarchar(30) NULL,
        [full_name] nvarchar(255) NOT NULL,
        [gender] nvarchar(20) NULL,
        [date_of_birth] date NULL,
        [password_hash] nvarchar(255) NOT NULL,
        [must_change_password] bit NOT NULL DEFAULT CAST(0 AS bit),
        [password_changed_at] datetime2(3) NULL,
        [status] nvarchar(30) NOT NULL DEFAULT N'ACTIVE',
        [email_verified] bit NOT NULL DEFAULT CAST(0 AS bit),
        [phone_verified] bit NOT NULL DEFAULT CAST(0 AS bit),
        [failed_login_attempts] int NOT NULL DEFAULT 0,
        [status_expires_at] datetime2 NULL,
        [last_login_at] datetime2(3) NULL,
        [created_at] datetime2(3) NOT NULL,
        [updated_at] datetime2(3) NOT NULL,
        [deleted_at] datetime2(3) NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE TABLE [user].[UserSessions] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [refresh_token_hash] nvarchar(255) NOT NULL,
        [device_id] nvarchar(100) NULL,
        [device_name] nvarchar(255) NULL,
        [ip_address] nvarchar(50) NULL,
        [user_agent] nvarchar(500) NULL,
        [expires_at] datetime2(3) NOT NULL,
        [revoked_at] datetime2(3) NULL,
        [last_used_at] datetime2(3) NULL,
        [created_at] datetime2(3) NOT NULL,
        CONSTRAINT [PK_UserSessions] PRIMARY KEY ([id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE TABLE [user].[RolePrivileges] (
        [id] uniqueidentifier NOT NULL,
        [role_id] uniqueidentifier NOT NULL,
        [privilege_id] uniqueidentifier NOT NULL,
        [assigned_by] uniqueidentifier NULL,
        [assigned_at] datetime2(3) NOT NULL,
        CONSTRAINT [PK_RolePrivileges] PRIMARY KEY ([id]),
        CONSTRAINT [FK_RolePrivileges_Privileges_privilege_id] FOREIGN KEY ([privilege_id]) REFERENCES [user].[Privileges] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RolePrivileges_Roles_role_id] FOREIGN KEY ([role_id]) REFERENCES [user].[Roles] ([id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE TABLE [SellerApplicationHistories] (
        [Id] uniqueidentifier NOT NULL,
        [SellerProfileId] uniqueidentifier NOT NULL,
        [FromStatus] nvarchar(20) NULL,
        [ToStatus] nvarchar(20) NOT NULL,
        [ChangedBy] uniqueidentifier NOT NULL,
        [Note] nvarchar(1000) NULL,
        [ChangedAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_SellerApplicationHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SellerApplicationHistories_SellerProfiles_SellerProfileId] FOREIGN KEY ([SellerProfileId]) REFERENCES [user].[SellerProfiles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE TABLE [user].[Addresses] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [RecipientName] nvarchar(255) NOT NULL,
        [RecipientPhone] nvarchar(12) NOT NULL,
        [Province] nvarchar(50) NOT NULL,
        [Ward] nvarchar(50) NOT NULL,
        [Street] nvarchar(255) NOT NULL,
        [Type] nvarchar(50) NULL,
        [IsDefault] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_Addresses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Addresses_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [user].[Users] ([id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE TABLE [user].[ReputationProfiles] (
        [user_id] uniqueidentifier NOT NULL,
        [score] int NOT NULL,
        [trust_level] nvarchar(50) NOT NULL,
        [total_ratings] int NOT NULL,
        [average_rating] decimal(3,2) NOT NULL,
        [successful_transactions] int NOT NULL,
        [failed_transactions] int NOT NULL,
        [successful_auctions] int NOT NULL,
        [failed_auctions] int NOT NULL,
        [penalty_count] int NOT NULL,
        [updated_at] datetime2(3) NOT NULL,
        CONSTRAINT [PK_ReputationProfiles] PRIMARY KEY ([user_id]),
        CONSTRAINT [FK_ReputationProfiles_Users_user_id] FOREIGN KEY ([user_id]) REFERENCES [user].[Users] ([id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE TABLE [user].[UserExternalLogins] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [provider] nvarchar(50) NOT NULL,
        [provider_user_id] nvarchar(255) NOT NULL,
        [provider_email] nvarchar(255) NOT NULL,
        [provider_display_name] nvarchar(255) NULL,
        [access_token_hash] nvarchar(255) NULL,
        [refresh_token_hash] nvarchar(255) NULL,
        [linked_at] datetime2(3) NOT NULL,
        [last_login_at] datetime2(3) NULL,
        [status] nvarchar(30) NOT NULL DEFAULT N'ACTIVE',
        [created_at] datetime2(3) NOT NULL,
        [updated_at] datetime2(3) NOT NULL,
        [deleted_at] datetime2(3) NULL,
        CONSTRAINT [PK_UserExternalLogins] PRIMARY KEY ([id]),
        CONSTRAINT [FK_UserExternalLogins_Users_user_id] FOREIGN KEY ([user_id]) REFERENCES [user].[Users] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE TABLE [user].[UserRoles] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [role_id] uniqueidentifier NOT NULL,
        [assigned_by] uniqueidentifier NULL,
        [assigned_at] datetime2(3) NOT NULL,
        [revoked_at] datetime2(3) NULL,
        [status] nvarchar(30) NOT NULL DEFAULT N'ACTIVE',
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([id]),
        CONSTRAINT [FK_UserRoles_Roles_role_id] FOREIGN KEY ([role_id]) REFERENCES [user].[Roles] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserRoles_Users_user_id] FOREIGN KEY ([user_id]) REFERENCES [user].[Users] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Addresses_UserId] ON [user].[Addresses] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Addresses_UserId_IsDefault] ON [user].[Addresses] ([UserId], [IsDefault]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_IdentityVerification_UserId] ON [user].[IdentityVerification] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PasswordHistories_UserId_CreatedAt] ON [user].[PasswordHistories] ([UserId], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PasswordResetTokens_token] ON [user].[PasswordResetTokens] ([token]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PasswordResetTokens_user_id] ON [user].[PasswordResetTokens] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Privileges_code] ON [user].[Privileges] ([code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RolePrivileges_privilege_id] ON [user].[RolePrivileges] ([privilege_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RolePrivileges_role_id_privilege_id] ON [user].[RolePrivileges] ([role_id], [privilege_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Roles_code] ON [user].[Roles] ([code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SellerApplicationHistories_SellerProfileId] ON [SellerApplicationHistories] ([SellerProfileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SellerProfiles_Status] ON [user].[SellerProfiles] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SellerProfiles_UserId] ON [user].[SellerProfiles] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserAuditLogs_action] ON [user].[UserAuditLogs] ([action]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserAuditLogs_target_user_id] ON [user].[UserAuditLogs] ([target_user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserExternalLogins_provider_provider_user_id] ON [user].[UserExternalLogins] ([provider], [provider_user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserExternalLogins_user_id] ON [user].[UserExternalLogins] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserRoles_role_id] ON [user].[UserRoles] ([role_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_UserRoles_user_id_role_id] ON [user].[UserRoles] ([user_id], [role_id]) WHERE [revoked_at] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_email] ON [user].[Users] ([email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Users_phone_number] ON [user].[Users] ([phone_number]) WHERE [phone_number] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserSessions_refresh_token_hash] ON [user].[UserSessions] ([refresh_token_hash]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260713193911_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260713193911_InitialCreate', N'10.0.8');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714151225_AddUserAvatarUrl'
)
BEGIN
    IF NOT EXISTS (
                        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_SCHEMA = 'user'
                          AND TABLE_NAME = 'Users'
                          AND COLUMN_NAME = 'avatar_url')
                    BEGIN
                        ALTER TABLE [user].[Users] ADD [avatar_url] nvarchar(500) NULL;
                    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260714151225_AddUserAvatarUrl'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260714151225_AddUserAvatarUrl', N'10.0.8');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715001020_AddUserAvatarKey'
)
BEGIN
    IF NOT EXISTS (
                        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_SCHEMA = 'user'
                          AND TABLE_NAME = 'Users'
                          AND COLUMN_NAME = 'avatar_key')
                    BEGIN
                        ALTER TABLE [user].[Users] ADD [avatar_key] nvarchar(500) NULL;
                    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715001020_AddUserAvatarKey'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260715001020_AddUserAvatarKey', N'10.0.8');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715184455_AddAuthProviderToUser'
)
BEGIN
    ALTER TABLE [user].[Users] ADD [auth_provider] nvarchar(20) NOT NULL DEFAULT N'LOCAL';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260715184455_AddAuthProviderToUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260715184455_AddAuthProviderToUser', N'10.0.8');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716073418_EnhanceIdentityVerificationProcess'
)
BEGIN
    ALTER TABLE [user].[Users] ADD [identity_verified] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716073418_EnhanceIdentityVerificationProcess'
)
BEGIN
    ALTER TABLE [user].[IdentityVerification] ADD [DateOfBirth] date NOT NULL DEFAULT '0001-01-01';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716073418_EnhanceIdentityVerificationProcess'
)
BEGIN
    ALTER TABLE [user].[IdentityVerification] ADD [ExpiryDate] date NOT NULL DEFAULT '0001-01-01';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716073418_EnhanceIdentityVerificationProcess'
)
BEGIN
    ALTER TABLE [user].[IdentityVerification] ADD [FullName] nvarchar(100) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716073418_EnhanceIdentityVerificationProcess'
)
BEGIN
    ALTER TABLE [user].[IdentityVerification] ADD [Gender] nvarchar(20) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716073418_EnhanceIdentityVerificationProcess'
)
BEGIN
    ALTER TABLE [user].[IdentityVerification] ADD [IssueDate] date NOT NULL DEFAULT '0001-01-01';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716073418_EnhanceIdentityVerificationProcess'
)
BEGIN
    ALTER TABLE [user].[IdentityVerification] ADD [IssuePlace] nvarchar(250) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716073418_EnhanceIdentityVerificationProcess'
)
BEGIN
    ALTER TABLE [user].[IdentityVerification] ADD [PermanentAddress] nvarchar(500) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716073418_EnhanceIdentityVerificationProcess'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260716073418_EnhanceIdentityVerificationProcess', N'10.0.8');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717070554_RenameIdentityImageUrlToKey'
)
BEGIN
    EXEC sp_rename N'[user].[IdentityVerification].[IdentityFrontImageUrl]', N'IdentityFrontImageKey', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717070554_RenameIdentityImageUrlToKey'
)
BEGIN
    EXEC sp_rename N'[user].[IdentityVerification].[IdentityBackImageUrl]', N'IdentityBackImageKey', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717070554_RenameIdentityImageUrlToKey'
)
BEGIN
    UPDATE [user].[IdentityVerification]
    SET IdentityFrontImageKey =
            SUBSTRING(IdentityFrontImageKey,
                      CHARINDEX('user/identity/', IdentityFrontImageKey),
                      LEN(IdentityFrontImageKey)),
        IdentityBackImageKey =
            SUBSTRING(IdentityBackImageKey,
                      CHARINDEX('user/identity/', IdentityBackImageKey),
                      LEN(IdentityBackImageKey))
    WHERE CHARINDEX('user/identity/', IdentityFrontImageKey) > 0
       OR CHARINDEX('user/identity/', IdentityBackImageKey) > 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717070554_RenameIdentityImageUrlToKey'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260717070554_RenameIdentityImageUrlToKey', N'10.0.8');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725204939_AddReputationLedgerAndBuyerProfiles'
)
BEGIN
    CREATE TABLE [user].[BuyerReputationProfiles] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [confirmed_score] int NOT NULL,
        [pending_score] int NOT NULL,
        [lifetime_earned_points] bigint NOT NULL,
        [lifetime_penalty_points] bigint NOT NULL,
        [successful_transactions] int NOT NULL,
        [failed_transactions] int NOT NULL,
        [successful_auctions] int NOT NULL,
        [failed_auctions] int NOT NULL,
        [penalty_count] int NOT NULL,
        [trust_level] nvarchar(30) NOT NULL,
        [created_at] datetime2(3) NOT NULL,
        [updated_at] datetime2(3) NULL,
        [deleted_at] datetime2(3) NULL,
        CONSTRAINT [PK_BuyerReputationProfiles] PRIMARY KEY ([id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725204939_AddReputationLedgerAndBuyerProfiles'
)
BEGIN
    CREATE TABLE [user].[BuyerVerificationProfiles] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [is_email_verified] bit NOT NULL,
        [is_phone_verified] bit NOT NULL,
        [is_identity_verified] bit NOT NULL,
        [has_verified_address] bit NOT NULL,
        [has_verified_payment_method] bit NOT NULL,
        [email_verified_at] datetime2(3) NULL,
        [phone_verified_at] datetime2(3) NULL,
        [identity_verified_at] datetime2(3) NULL,
        [address_verified_at] datetime2(3) NULL,
        [payment_method_verified_at] datetime2(3) NULL,
        [created_at] datetime2(3) NOT NULL,
        [updated_at] datetime2(3) NULL,
        [deleted_at] datetime2(3) NULL,
        CONSTRAINT [PK_BuyerVerificationProfiles] PRIMARY KEY ([id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725204939_AddReputationLedgerAndBuyerProfiles'
)
BEGIN
    CREATE TABLE [user].[ReputationLedgerEntries] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [entry_type] nvarchar(50) NOT NULL,
        [reason] nvarchar(60) NOT NULL,
        [status] nvarchar(20) NOT NULL,
        [points] int NOT NULL,
        [source_service] nvarchar(50) NOT NULL,
        [source_type] nvarchar(50) NOT NULL,
        [source_id] nvarchar(200) NOT NULL,
        [idempotency_key] nvarchar(200) NOT NULL,
        [rule_version] nvarchar(50) NOT NULL,
        [reversal_entry_id] uniqueidentifier NULL,
        [confirm_after] datetime2(3) NULL,
        [confirmed_at] datetime2(3) NULL,
        [cancelled_at] datetime2(3) NULL,
        [reversed_at] datetime2(3) NULL,
        [created_at] datetime2(3) NOT NULL,
        [updated_at] datetime2(3) NULL,
        [deleted_at] datetime2(3) NULL,
        CONSTRAINT [PK_ReputationLedgerEntries] PRIMARY KEY ([id]),
        CONSTRAINT [CK_ReputationLedgerEntries_entry_type] CHECK ([entry_type] IN ('PROFILE_VERIFICATION','ECOMMERCE_TRANSACTION','AUCTION_TRANSACTION','AUCTION_BONUS','REVIEW','PENALTY','REVERSAL')),
        CONSTRAINT [CK_ReputationLedgerEntries_points_non_zero] CHECK ([points] <> 0),
        CONSTRAINT [CK_ReputationLedgerEntries_status] CHECK ([status] IN ('PENDING','CONFIRMED','REVERSED','CANCELLED'))
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725204939_AddReputationLedgerAndBuyerProfiles'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BuyerReputationProfiles_user_id] ON [user].[BuyerReputationProfiles] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725204939_AddReputationLedgerAndBuyerProfiles'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BuyerVerificationProfiles_user_id] ON [user].[BuyerVerificationProfiles] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725204939_AddReputationLedgerAndBuyerProfiles'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ReputationLedgerEntries_idempotency_key] ON [user].[ReputationLedgerEntries] ([idempotency_key]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725204939_AddReputationLedgerAndBuyerProfiles'
)
BEGIN
    CREATE INDEX [IX_ReputationLedgerEntries_source_service_source_type_source_id] ON [user].[ReputationLedgerEntries] ([source_service], [source_type], [source_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725204939_AddReputationLedgerAndBuyerProfiles'
)
BEGIN
    CREATE INDEX [IX_ReputationLedgerEntries_user_id_status_created_at] ON [user].[ReputationLedgerEntries] ([user_id], [status], [created_at]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725204939_AddReputationLedgerAndBuyerProfiles'
)
BEGIN
    DECLARE @now datetime2(3) = SYSUTCDATETIME();

    -- 1. A buyer verification profile (email verified) for every already-email-confirmed user.
    INSERT INTO [user].[BuyerVerificationProfiles]
        (id, user_id, is_email_verified, is_phone_verified, is_identity_verified,
         has_verified_address, has_verified_payment_method,
         email_verified_at, created_at, updated_at)
    SELECT NEWID(), u.[id], 1, 0, 0, 0, 0, u.[created_at], u.[created_at], @now
    FROM [user].[Users] u
    WHERE u.[email_verified] = 1
      AND u.[deleted_at] IS NULL
      AND NOT EXISTS (
          SELECT 1 FROM [user].[BuyerVerificationProfiles] b WHERE b.[user_id] = u.[id]);

    -- 2. One confirmed EMAIL_VERIFIED ledger entry per such user (history only). The
    --    idempotency key matches the application: user:{userId-lowercase}:email-verified:v1.
    INSERT INTO [user].[ReputationLedgerEntries]
        (id, user_id, entry_type, reason, status, points,
         source_service, source_type, source_id,
         idempotency_key, rule_version, confirmed_at, created_at, updated_at)
    SELECT NEWID(), u.[id], 'PROFILE_VERIFICATION', 'EMAIL_VERIFIED', 'CONFIRMED', 1,
           'user-service', 'USER_EMAIL', LOWER(CONVERT(nvarchar(36), u.[id])),
           'user:' + LOWER(CONVERT(nvarchar(36), u.[id])) + ':email-verified:v1',
           'REPUTATION_V1', u.[created_at], u.[created_at], @now
    FROM [user].[Users] u
    WHERE u.[email_verified] = 1
      AND u.[deleted_at] IS NULL
      AND NOT EXISTS (
          SELECT 1 FROM [user].[ReputationLedgerEntries] l
          WHERE l.[idempotency_key] =
                'user:' + LOWER(CONVERT(nvarchar(36), u.[id])) + ':email-verified:v1');

    -- 3. Seed the new buyer reputation summary from the legacy score (do NOT re-add the
    --    email point: the legacy score already includes it).
    INSERT INTO [user].[BuyerReputationProfiles]
        (id, user_id, confirmed_score, pending_score,
         lifetime_earned_points, lifetime_penalty_points,
         successful_transactions, failed_transactions,
         successful_auctions, failed_auctions, penalty_count,
         trust_level, created_at, updated_at)
    SELECT NEWID(), r.[user_id], r.[score], 0,
           CASE WHEN r.[score] > 0 THEN r.[score] ELSE 0 END, 0,
           r.[successful_transactions], r.[failed_transactions],
           r.[successful_auctions], r.[failed_auctions], r.[penalty_count],
           CASE
               WHEN r.[score] < 0    THEN 'RESTRICTED'
               WHEN r.[score] < 50   THEN 'BASIC'
               WHEN r.[score] < 200  THEN 'TRUSTED'
               WHEN r.[score] < 500  THEN 'RELIABLE'
               WHEN r.[score] < 1000 THEN 'PREMIUM'
               ELSE 'ELITE'
           END,
           @now, @now
    FROM [user].[ReputationProfiles] r
    WHERE NOT EXISTS (
        SELECT 1 FROM [user].[BuyerReputationProfiles] b WHERE b.[user_id] = r.[user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725204939_AddReputationLedgerAndBuyerProfiles'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260725204939_AddReputationLedgerAndBuyerProfiles', N'10.0.8');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DROP INDEX [IX_Addresses_UserId_IsDefault] ON [user].[Addresses];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DROP INDEX [IX_PasswordHistories_UserId_CreatedAt] ON [user].[PasswordHistories];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DROP INDEX [IX_ReputationLedgerEntries_user_id_status_created_at] ON [user].[ReputationLedgerEntries];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DROP INDEX [IX_UserRoles_user_id_role_id] ON [user].[UserRoles];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[UserSessions]') AND [c].[name] = N'revoked_at');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [user].[UserSessions] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [user].[UserSessions] ALTER COLUMN [revoked_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[UserSessions]') AND [c].[name] = N'last_used_at');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [user].[UserSessions] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [user].[UserSessions] ALTER COLUMN [last_used_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[UserSessions]') AND [c].[name] = N'expires_at');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [user].[UserSessions] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [user].[UserSessions] ALTER COLUMN [expires_at] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[UserSessions]') AND [c].[name] = N'created_at');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [user].[UserSessions] DROP CONSTRAINT ' + @var3 + ';');
    ALTER TABLE [user].[UserSessions] ALTER COLUMN [created_at] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var4 nvarchar(max);
    SELECT @var4 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[Users]') AND [c].[name] = N'updated_at');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [user].[Users] DROP CONSTRAINT ' + @var4 + ';');
    ALTER TABLE [user].[Users] ALTER COLUMN [updated_at] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var5 nvarchar(max);
    SELECT @var5 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[Users]') AND [c].[name] = N'status_expires_at');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [user].[Users] DROP CONSTRAINT ' + @var5 + ';');
    ALTER TABLE [user].[Users] ALTER COLUMN [status_expires_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var6 nvarchar(max);
    SELECT @var6 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[Users]') AND [c].[name] = N'password_changed_at');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [user].[Users] DROP CONSTRAINT ' + @var6 + ';');
    ALTER TABLE [user].[Users] ALTER COLUMN [password_changed_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var7 nvarchar(max);
    SELECT @var7 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[Users]') AND [c].[name] = N'last_login_at');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [user].[Users] DROP CONSTRAINT ' + @var7 + ';');
    ALTER TABLE [user].[Users] ALTER COLUMN [last_login_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var8 nvarchar(max);
    SELECT @var8 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[Users]') AND [c].[name] = N'deleted_at');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [user].[Users] DROP CONSTRAINT ' + @var8 + ';');
    ALTER TABLE [user].[Users] ALTER COLUMN [deleted_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var9 nvarchar(max);
    SELECT @var9 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[Users]') AND [c].[name] = N'created_at');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [user].[Users] DROP CONSTRAINT ' + @var9 + ';');
    ALTER TABLE [user].[Users] ALTER COLUMN [created_at] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var10 nvarchar(max);
    SELECT @var10 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[UserRoles]') AND [c].[name] = N'revoked_at');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [user].[UserRoles] DROP CONSTRAINT ' + @var10 + ';');
    ALTER TABLE [user].[UserRoles] ALTER COLUMN [revoked_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var11 nvarchar(max);
    SELECT @var11 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[UserRoles]') AND [c].[name] = N'assigned_at');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [user].[UserRoles] DROP CONSTRAINT ' + @var11 + ';');
    ALTER TABLE [user].[UserRoles] ALTER COLUMN [assigned_at] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var12 nvarchar(max);
    SELECT @var12 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[UserExternalLogins]') AND [c].[name] = N'updated_at');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [user].[UserExternalLogins] DROP CONSTRAINT ' + @var12 + ';');
    ALTER TABLE [user].[UserExternalLogins] ALTER COLUMN [updated_at] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var13 nvarchar(max);
    SELECT @var13 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[UserExternalLogins]') AND [c].[name] = N'linked_at');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [user].[UserExternalLogins] DROP CONSTRAINT ' + @var13 + ';');
    ALTER TABLE [user].[UserExternalLogins] ALTER COLUMN [linked_at] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var14 nvarchar(max);
    SELECT @var14 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[UserExternalLogins]') AND [c].[name] = N'last_login_at');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [user].[UserExternalLogins] DROP CONSTRAINT ' + @var14 + ';');
    ALTER TABLE [user].[UserExternalLogins] ALTER COLUMN [last_login_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var15 nvarchar(max);
    SELECT @var15 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[UserExternalLogins]') AND [c].[name] = N'deleted_at');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [user].[UserExternalLogins] DROP CONSTRAINT ' + @var15 + ';');
    ALTER TABLE [user].[UserExternalLogins] ALTER COLUMN [deleted_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var16 nvarchar(max);
    SELECT @var16 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[UserExternalLogins]') AND [c].[name] = N'created_at');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [user].[UserExternalLogins] DROP CONSTRAINT ' + @var16 + ';');
    ALTER TABLE [user].[UserExternalLogins] ALTER COLUMN [created_at] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var17 nvarchar(max);
    SELECT @var17 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[UserAuditLogs]') AND [c].[name] = N'created_at');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [user].[UserAuditLogs] DROP CONSTRAINT ' + @var17 + ';');
    ALTER TABLE [user].[UserAuditLogs] ALTER COLUMN [created_at] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var18 nvarchar(max);
    SELECT @var18 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[SellerProfiles]') AND [c].[name] = N'UpdatedAt');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [user].[SellerProfiles] DROP CONSTRAINT ' + @var18 + ';');
    ALTER TABLE [user].[SellerProfiles] ALTER COLUMN [UpdatedAt] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var19 nvarchar(max);
    SELECT @var19 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[SellerProfiles]') AND [c].[name] = N'SubmittedAt');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [user].[SellerProfiles] DROP CONSTRAINT ' + @var19 + ';');
    ALTER TABLE [user].[SellerProfiles] ALTER COLUMN [SubmittedAt] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var20 nvarchar(max);
    SELECT @var20 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[SellerProfiles]') AND [c].[name] = N'ReviewedAt');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [user].[SellerProfiles] DROP CONSTRAINT ' + @var20 + ';');
    ALTER TABLE [user].[SellerProfiles] ALTER COLUMN [ReviewedAt] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var21 nvarchar(max);
    SELECT @var21 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[SellerProfiles]') AND [c].[name] = N'DeletedAt');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [user].[SellerProfiles] DROP CONSTRAINT ' + @var21 + ';');
    ALTER TABLE [user].[SellerProfiles] ALTER COLUMN [DeletedAt] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var22 nvarchar(max);
    SELECT @var22 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[SellerProfiles]') AND [c].[name] = N'CreatedAt');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [user].[SellerProfiles] DROP CONSTRAINT ' + @var22 + ';');
    ALTER TABLE [user].[SellerProfiles] ALTER COLUMN [CreatedAt] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var23 nvarchar(max);
    SELECT @var23 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SellerApplicationHistories]') AND [c].[name] = N'UpdatedAt');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [SellerApplicationHistories] DROP CONSTRAINT ' + @var23 + ';');
    ALTER TABLE [SellerApplicationHistories] ALTER COLUMN [UpdatedAt] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var24 nvarchar(max);
    SELECT @var24 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SellerApplicationHistories]') AND [c].[name] = N'DeletedAt');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [SellerApplicationHistories] DROP CONSTRAINT ' + @var24 + ';');
    ALTER TABLE [SellerApplicationHistories] ALTER COLUMN [DeletedAt] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var25 nvarchar(max);
    SELECT @var25 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SellerApplicationHistories]') AND [c].[name] = N'CreatedAt');
    IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [SellerApplicationHistories] DROP CONSTRAINT ' + @var25 + ';');
    ALTER TABLE [SellerApplicationHistories] ALTER COLUMN [CreatedAt] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var26 nvarchar(max);
    SELECT @var26 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SellerApplicationHistories]') AND [c].[name] = N'ChangedAt');
    IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [SellerApplicationHistories] DROP CONSTRAINT ' + @var26 + ';');
    ALTER TABLE [SellerApplicationHistories] ALTER COLUMN [ChangedAt] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var27 nvarchar(max);
    SELECT @var27 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[Roles]') AND [c].[name] = N'updated_at');
    IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [user].[Roles] DROP CONSTRAINT ' + @var27 + ';');
    ALTER TABLE [user].[Roles] ALTER COLUMN [updated_at] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var28 nvarchar(max);
    SELECT @var28 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[Roles]') AND [c].[name] = N'deleted_at');
    IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [user].[Roles] DROP CONSTRAINT ' + @var28 + ';');
    ALTER TABLE [user].[Roles] ALTER COLUMN [deleted_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var29 nvarchar(max);
    SELECT @var29 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[Roles]') AND [c].[name] = N'created_at');
    IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [user].[Roles] DROP CONSTRAINT ' + @var29 + ';');
    ALTER TABLE [user].[Roles] ALTER COLUMN [created_at] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var30 nvarchar(max);
    SELECT @var30 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[RolePrivileges]') AND [c].[name] = N'assigned_at');
    IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [user].[RolePrivileges] DROP CONSTRAINT ' + @var30 + ';');
    ALTER TABLE [user].[RolePrivileges] ALTER COLUMN [assigned_at] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var31 nvarchar(max);
    SELECT @var31 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationProfiles]') AND [c].[name] = N'updated_at');
    IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationProfiles] DROP CONSTRAINT ' + @var31 + ';');
    ALTER TABLE [user].[ReputationProfiles] ALTER COLUMN [updated_at] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var32 nvarchar(max);
    SELECT @var32 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationLedgerEntries]') AND [c].[name] = N'updated_at');
    IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT ' + @var32 + ';');
    ALTER TABLE [user].[ReputationLedgerEntries] ALTER COLUMN [updated_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var33 nvarchar(max);
    SELECT @var33 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationLedgerEntries]') AND [c].[name] = N'reversed_at');
    IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT ' + @var33 + ';');
    ALTER TABLE [user].[ReputationLedgerEntries] ALTER COLUMN [reversed_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var34 nvarchar(max);
    SELECT @var34 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationLedgerEntries]') AND [c].[name] = N'deleted_at');
    IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT ' + @var34 + ';');
    ALTER TABLE [user].[ReputationLedgerEntries] ALTER COLUMN [deleted_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var35 nvarchar(max);
    SELECT @var35 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationLedgerEntries]') AND [c].[name] = N'created_at');
    IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT ' + @var35 + ';');
    ALTER TABLE [user].[ReputationLedgerEntries] ALTER COLUMN [created_at] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var36 nvarchar(max);
    SELECT @var36 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationLedgerEntries]') AND [c].[name] = N'confirmed_at');
    IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT ' + @var36 + ';');
    ALTER TABLE [user].[ReputationLedgerEntries] ALTER COLUMN [confirmed_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var37 nvarchar(max);
    SELECT @var37 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationLedgerEntries]') AND [c].[name] = N'confirm_after');
    IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT ' + @var37 + ';');
    ALTER TABLE [user].[ReputationLedgerEntries] ALTER COLUMN [confirm_after] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var38 nvarchar(max);
    SELECT @var38 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationLedgerEntries]') AND [c].[name] = N'cancelled_at');
    IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT ' + @var38 + ';');
    ALTER TABLE [user].[ReputationLedgerEntries] ALTER COLUMN [cancelled_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var39 nvarchar(max);
    SELECT @var39 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[PasswordResetTokens]') AND [c].[name] = N'expiry_date');
    IF @var39 IS NOT NULL EXEC(N'ALTER TABLE [user].[PasswordResetTokens] DROP CONSTRAINT ' + @var39 + ';');
    ALTER TABLE [user].[PasswordResetTokens] ALTER COLUMN [expiry_date] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var40 nvarchar(max);
    SELECT @var40 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[PasswordResetTokens]') AND [c].[name] = N'created_at');
    IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [user].[PasswordResetTokens] DROP CONSTRAINT ' + @var40 + ';');
    ALTER TABLE [user].[PasswordResetTokens] ALTER COLUMN [created_at] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var41 nvarchar(max);
    SELECT @var41 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[PasswordHistories]') AND [c].[name] = N'CreatedAt');
    IF @var41 IS NOT NULL EXEC(N'ALTER TABLE [user].[PasswordHistories] DROP CONSTRAINT ' + @var41 + ';');
    ALTER TABLE [user].[PasswordHistories] ALTER COLUMN [CreatedAt] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var42 nvarchar(max);
    SELECT @var42 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[IdentityVerification]') AND [c].[name] = N'VerifiedAt');
    IF @var42 IS NOT NULL EXEC(N'ALTER TABLE [user].[IdentityVerification] DROP CONSTRAINT ' + @var42 + ';');
    ALTER TABLE [user].[IdentityVerification] ALTER COLUMN [VerifiedAt] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var43 nvarchar(max);
    SELECT @var43 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[IdentityVerification]') AND [c].[name] = N'UpdatedAt');
    IF @var43 IS NOT NULL EXEC(N'ALTER TABLE [user].[IdentityVerification] DROP CONSTRAINT ' + @var43 + ';');
    ALTER TABLE [user].[IdentityVerification] ALTER COLUMN [UpdatedAt] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var44 nvarchar(max);
    SELECT @var44 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[IdentityVerification]') AND [c].[name] = N'SubmittedAt');
    IF @var44 IS NOT NULL EXEC(N'ALTER TABLE [user].[IdentityVerification] DROP CONSTRAINT ' + @var44 + ';');
    ALTER TABLE [user].[IdentityVerification] ALTER COLUMN [SubmittedAt] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var45 nvarchar(max);
    SELECT @var45 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[IdentityVerification]') AND [c].[name] = N'DeletedAt');
    IF @var45 IS NOT NULL EXEC(N'ALTER TABLE [user].[IdentityVerification] DROP CONSTRAINT ' + @var45 + ';');
    ALTER TABLE [user].[IdentityVerification] ALTER COLUMN [DeletedAt] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var46 nvarchar(max);
    SELECT @var46 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[IdentityVerification]') AND [c].[name] = N'CreatedAt');
    IF @var46 IS NOT NULL EXEC(N'ALTER TABLE [user].[IdentityVerification] DROP CONSTRAINT ' + @var46 + ';');
    ALTER TABLE [user].[IdentityVerification] ALTER COLUMN [CreatedAt] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var47 nvarchar(max);
    SELECT @var47 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[BuyerVerificationProfiles]') AND [c].[name] = N'updated_at');
    IF @var47 IS NOT NULL EXEC(N'ALTER TABLE [user].[BuyerVerificationProfiles] DROP CONSTRAINT ' + @var47 + ';');
    ALTER TABLE [user].[BuyerVerificationProfiles] ALTER COLUMN [updated_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var48 nvarchar(max);
    SELECT @var48 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[BuyerVerificationProfiles]') AND [c].[name] = N'phone_verified_at');
    IF @var48 IS NOT NULL EXEC(N'ALTER TABLE [user].[BuyerVerificationProfiles] DROP CONSTRAINT ' + @var48 + ';');
    ALTER TABLE [user].[BuyerVerificationProfiles] ALTER COLUMN [phone_verified_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var49 nvarchar(max);
    SELECT @var49 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[BuyerVerificationProfiles]') AND [c].[name] = N'payment_method_verified_at');
    IF @var49 IS NOT NULL EXEC(N'ALTER TABLE [user].[BuyerVerificationProfiles] DROP CONSTRAINT ' + @var49 + ';');
    ALTER TABLE [user].[BuyerVerificationProfiles] ALTER COLUMN [payment_method_verified_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var50 nvarchar(max);
    SELECT @var50 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[BuyerVerificationProfiles]') AND [c].[name] = N'identity_verified_at');
    IF @var50 IS NOT NULL EXEC(N'ALTER TABLE [user].[BuyerVerificationProfiles] DROP CONSTRAINT ' + @var50 + ';');
    ALTER TABLE [user].[BuyerVerificationProfiles] ALTER COLUMN [identity_verified_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var51 nvarchar(max);
    SELECT @var51 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[BuyerVerificationProfiles]') AND [c].[name] = N'email_verified_at');
    IF @var51 IS NOT NULL EXEC(N'ALTER TABLE [user].[BuyerVerificationProfiles] DROP CONSTRAINT ' + @var51 + ';');
    ALTER TABLE [user].[BuyerVerificationProfiles] ALTER COLUMN [email_verified_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var52 nvarchar(max);
    SELECT @var52 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[BuyerVerificationProfiles]') AND [c].[name] = N'deleted_at');
    IF @var52 IS NOT NULL EXEC(N'ALTER TABLE [user].[BuyerVerificationProfiles] DROP CONSTRAINT ' + @var52 + ';');
    ALTER TABLE [user].[BuyerVerificationProfiles] ALTER COLUMN [deleted_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var53 nvarchar(max);
    SELECT @var53 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[BuyerVerificationProfiles]') AND [c].[name] = N'created_at');
    IF @var53 IS NOT NULL EXEC(N'ALTER TABLE [user].[BuyerVerificationProfiles] DROP CONSTRAINT ' + @var53 + ';');
    ALTER TABLE [user].[BuyerVerificationProfiles] ALTER COLUMN [created_at] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var54 nvarchar(max);
    SELECT @var54 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[BuyerVerificationProfiles]') AND [c].[name] = N'address_verified_at');
    IF @var54 IS NOT NULL EXEC(N'ALTER TABLE [user].[BuyerVerificationProfiles] DROP CONSTRAINT ' + @var54 + ';');
    ALTER TABLE [user].[BuyerVerificationProfiles] ALTER COLUMN [address_verified_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var55 nvarchar(max);
    SELECT @var55 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[BuyerReputationProfiles]') AND [c].[name] = N'updated_at');
    IF @var55 IS NOT NULL EXEC(N'ALTER TABLE [user].[BuyerReputationProfiles] DROP CONSTRAINT ' + @var55 + ';');
    ALTER TABLE [user].[BuyerReputationProfiles] ALTER COLUMN [updated_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var56 nvarchar(max);
    SELECT @var56 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[BuyerReputationProfiles]') AND [c].[name] = N'deleted_at');
    IF @var56 IS NOT NULL EXEC(N'ALTER TABLE [user].[BuyerReputationProfiles] DROP CONSTRAINT ' + @var56 + ';');
    ALTER TABLE [user].[BuyerReputationProfiles] ALTER COLUMN [deleted_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var57 nvarchar(max);
    SELECT @var57 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[BuyerReputationProfiles]') AND [c].[name] = N'created_at');
    IF @var57 IS NOT NULL EXEC(N'ALTER TABLE [user].[BuyerReputationProfiles] DROP CONSTRAINT ' + @var57 + ';');
    ALTER TABLE [user].[BuyerReputationProfiles] ALTER COLUMN [created_at] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var58 nvarchar(max);
    SELECT @var58 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[Addresses]') AND [c].[name] = N'UpdatedAt');
    IF @var58 IS NOT NULL EXEC(N'ALTER TABLE [user].[Addresses] DROP CONSTRAINT ' + @var58 + ';');
    ALTER TABLE [user].[Addresses] ALTER COLUMN [UpdatedAt] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var59 nvarchar(max);
    SELECT @var59 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[Addresses]') AND [c].[name] = N'DeletedAt');
    IF @var59 IS NOT NULL EXEC(N'ALTER TABLE [user].[Addresses] DROP CONSTRAINT ' + @var59 + ';');
    ALTER TABLE [user].[Addresses] ALTER COLUMN [DeletedAt] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    DECLARE @var60 nvarchar(max);
    SELECT @var60 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[Addresses]') AND [c].[name] = N'CreatedAt');
    IF @var60 IS NOT NULL EXEC(N'ALTER TABLE [user].[Addresses] DROP CONSTRAINT ' + @var60 + ';');
    ALTER TABLE [user].[Addresses] ALTER COLUMN [CreatedAt] datetimeoffset(3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Addresses_UserId_IsDefault] ON [user].[Addresses] ([UserId], [IsDefault]) WHERE [DeletedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    CREATE INDEX [IX_PasswordHistories_UserId_CreatedAt] ON [user].[PasswordHistories] ([UserId], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    CREATE INDEX [IX_ReputationLedgerEntries_user_id_status_created_at] ON [user].[ReputationLedgerEntries] ([user_id], [status], [created_at]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_UserRoles_user_id_role_id] ON [user].[UserRoles] ([user_id], [role_id]) WHERE [revoked_at] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260725214339_UseUtcDateTimeOffsetAndFinalizeReputation', N'10.0.8');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725224348_RemoveLegacyReputationProfile'
)
BEGIN
    DROP TABLE [user].[ReputationProfiles];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725224348_RemoveLegacyReputationProfile'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260725224348_RemoveLegacyReputationProfile', N'10.0.8');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725225719_RemoveDuplicateAccountVerificationFromBuyerProfile'
)
BEGIN
    DECLARE @var61 nvarchar(max);
    SELECT @var61 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[BuyerVerificationProfiles]') AND [c].[name] = N'email_verified_at');
    IF @var61 IS NOT NULL EXEC(N'ALTER TABLE [user].[BuyerVerificationProfiles] DROP CONSTRAINT ' + @var61 + ';');
    ALTER TABLE [user].[BuyerVerificationProfiles] DROP COLUMN [email_verified_at];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725225719_RemoveDuplicateAccountVerificationFromBuyerProfile'
)
BEGIN
    DECLARE @var62 nvarchar(max);
    SELECT @var62 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[BuyerVerificationProfiles]') AND [c].[name] = N'is_email_verified');
    IF @var62 IS NOT NULL EXEC(N'ALTER TABLE [user].[BuyerVerificationProfiles] DROP CONSTRAINT ' + @var62 + ';');
    ALTER TABLE [user].[BuyerVerificationProfiles] DROP COLUMN [is_email_verified];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725225719_RemoveDuplicateAccountVerificationFromBuyerProfile'
)
BEGIN
    DECLARE @var63 nvarchar(max);
    SELECT @var63 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[BuyerVerificationProfiles]') AND [c].[name] = N'is_phone_verified');
    IF @var63 IS NOT NULL EXEC(N'ALTER TABLE [user].[BuyerVerificationProfiles] DROP CONSTRAINT ' + @var63 + ';');
    ALTER TABLE [user].[BuyerVerificationProfiles] DROP COLUMN [is_phone_verified];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725225719_RemoveDuplicateAccountVerificationFromBuyerProfile'
)
BEGIN
    DECLARE @var64 nvarchar(max);
    SELECT @var64 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[BuyerVerificationProfiles]') AND [c].[name] = N'phone_verified_at');
    IF @var64 IS NOT NULL EXEC(N'ALTER TABLE [user].[BuyerVerificationProfiles] DROP CONSTRAINT ' + @var64 + ';');
    ALTER TABLE [user].[BuyerVerificationProfiles] DROP COLUMN [phone_verified_at];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725225719_RemoveDuplicateAccountVerificationFromBuyerProfile'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260725225719_RemoveDuplicateAccountVerificationFromBuyerProfile', N'10.0.8');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725230322_RemoveDuplicateIdentityVerificationFromBuyerProfile'
)
BEGIN
    DECLARE @var65 nvarchar(max);
    SELECT @var65 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[BuyerVerificationProfiles]') AND [c].[name] = N'identity_verified_at');
    IF @var65 IS NOT NULL EXEC(N'ALTER TABLE [user].[BuyerVerificationProfiles] DROP CONSTRAINT ' + @var65 + ';');
    ALTER TABLE [user].[BuyerVerificationProfiles] DROP COLUMN [identity_verified_at];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725230322_RemoveDuplicateIdentityVerificationFromBuyerProfile'
)
BEGIN
    DECLARE @var66 nvarchar(max);
    SELECT @var66 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[BuyerVerificationProfiles]') AND [c].[name] = N'is_identity_verified');
    IF @var66 IS NOT NULL EXEC(N'ALTER TABLE [user].[BuyerVerificationProfiles] DROP CONSTRAINT ' + @var66 + ';');
    ALTER TABLE [user].[BuyerVerificationProfiles] DROP COLUMN [is_identity_verified];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725230322_RemoveDuplicateIdentityVerificationFromBuyerProfile'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260725230322_RemoveDuplicateIdentityVerificationFromBuyerProfile', N'10.0.8');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726083836_RestoreFullBuyerVerificationAndAddBankVerification'
)
BEGIN
    ALTER TABLE [user].[BuyerVerificationProfiles] ADD [email_verified_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726083836_RestoreFullBuyerVerificationAndAddBankVerification'
)
BEGIN
    ALTER TABLE [user].[BuyerVerificationProfiles] ADD [identity_verified_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726083836_RestoreFullBuyerVerificationAndAddBankVerification'
)
BEGIN
    ALTER TABLE [user].[BuyerVerificationProfiles] ADD [is_email_verified] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726083836_RestoreFullBuyerVerificationAndAddBankVerification'
)
BEGIN
    ALTER TABLE [user].[BuyerVerificationProfiles] ADD [is_identity_verified] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726083836_RestoreFullBuyerVerificationAndAddBankVerification'
)
BEGIN
    ALTER TABLE [user].[BuyerVerificationProfiles] ADD [is_phone_verified] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726083836_RestoreFullBuyerVerificationAndAddBankVerification'
)
BEGIN
    ALTER TABLE [user].[BuyerVerificationProfiles] ADD [phone_verified_at] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726083836_RestoreFullBuyerVerificationAndAddBankVerification'
)
BEGIN
    INSERT INTO [user].[BuyerVerificationProfiles]
        ([id], [user_id], [is_email_verified], [is_phone_verified],
         [is_identity_verified], [has_verified_address],
         [has_verified_payment_method], [email_verified_at],
         [phone_verified_at], [identity_verified_at],
         [address_verified_at], [payment_method_verified_at],
         [created_at], [updated_at], [deleted_at])
    SELECT
        NEWID(), u.[id], u.[email_verified], u.[phone_verified],
        u.[identity_verified], 0, 0,
        CASE WHEN u.[email_verified] = 1 THEN u.[updated_at] END,
        CASE WHEN u.[phone_verified] = 1 THEN u.[updated_at] END,
        CASE WHEN u.[identity_verified] = 1 THEN u.[updated_at] END,
        NULL, NULL, u.[created_at], u.[updated_at], NULL
    FROM [user].[Users] u
    WHERE NOT EXISTS (
        SELECT 1
        FROM [user].[BuyerVerificationProfiles] b
        WHERE b.[user_id] = u.[id]);

    UPDATE b
    SET
        b.[is_email_verified] = u.[email_verified],
        b.[is_phone_verified] = u.[phone_verified],
        b.[is_identity_verified] = u.[identity_verified],
        b.[email_verified_at] =
            CASE WHEN u.[email_verified] = 1 THEN u.[updated_at] END,
        b.[phone_verified_at] =
            CASE WHEN u.[phone_verified] = 1 THEN u.[updated_at] END,
        b.[identity_verified_at] =
            CASE WHEN u.[identity_verified] = 1 THEN u.[updated_at] END
    FROM [user].[BuyerVerificationProfiles] b
    INNER JOIN [user].[Users] u ON u.[id] = b.[user_id];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726083836_RestoreFullBuyerVerificationAndAddBankVerification'
)
BEGIN
    CREATE TABLE [user].[BankAccountVerifications] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [provider] nvarchar(50) NOT NULL,
        [provider_reference] nvarchar(200) NOT NULL,
        [bank_code] nvarchar(30) NOT NULL,
        [masked_account_number] nvarchar(64) NOT NULL,
        [account_fingerprint] nvarchar(128) NOT NULL,
        [expected_account_holder_name] nvarchar(200) NOT NULL,
        [verified_account_holder_name] nvarchar(200) NULL,
        [status] nvarchar(30) NOT NULL,
        [failure_code] nvarchar(100) NULL,
        [failure_reason] nvarchar(500) NULL,
        [expires_at] datetimeoffset(3) NULL,
        [verified_at] datetimeoffset(3) NULL,
        [rejected_at] datetimeoffset(3) NULL,
        [revoked_at] datetimeoffset(3) NULL,
        [created_at] datetimeoffset(3) NOT NULL,
        [updated_at] datetimeoffset(3) NULL,
        [deleted_at] datetimeoffset(3) NULL,
        CONSTRAINT [PK_BankAccountVerifications] PRIMARY KEY ([id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726083836_RestoreFullBuyerVerificationAndAddBankVerification'
)
BEGIN
    CREATE INDEX [IX_BankAccountVerifications_account_fingerprint] ON [user].[BankAccountVerifications] ([account_fingerprint]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726083836_RestoreFullBuyerVerificationAndAddBankVerification'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BankAccountVerifications_provider_provider_reference] ON [user].[BankAccountVerifications] ([provider], [provider_reference]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726083836_RestoreFullBuyerVerificationAndAddBankVerification'
)
BEGIN
    CREATE INDEX [IX_BankAccountVerifications_user_id] ON [user].[BankAccountVerifications] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260726083836_RestoreFullBuyerVerificationAndAddBankVerification'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260726083836_RestoreFullBuyerVerificationAndAddBankVerification', N'10.0.8');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727185232_AddMassTransitTransactionalOutbox'
)
BEGIN
    ALTER TABLE [user].[SellerProfiles] ADD [ContactPhoneNumber] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727185232_AddMassTransitTransactionalOutbox'
)
BEGIN
    CREATE TABLE [InboxState] (
        [Id] bigint NOT NULL IDENTITY,
        [MessageId] uniqueidentifier NOT NULL,
        [ConsumerId] uniqueidentifier NOT NULL,
        [LockId] uniqueidentifier NOT NULL,
        [RowVersion] rowversion NULL,
        [Received] datetime2 NOT NULL,
        [ReceiveCount] int NOT NULL,
        [ExpirationTime] datetime2 NULL,
        [Consumed] datetime2 NULL,
        [Delivered] datetime2 NULL,
        [LastSequenceNumber] bigint NULL,
        CONSTRAINT [PK_InboxState] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_InboxState_MessageId_ConsumerId] UNIQUE ([MessageId], [ConsumerId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727185232_AddMassTransitTransactionalOutbox'
)
BEGIN
    CREATE TABLE [OutboxState] (
        [OutboxId] uniqueidentifier NOT NULL,
        [LockId] uniqueidentifier NOT NULL,
        [RowVersion] rowversion NULL,
        [Created] datetime2 NOT NULL,
        [Delivered] datetime2 NULL,
        [LastSequenceNumber] bigint NULL,
        CONSTRAINT [PK_OutboxState] PRIMARY KEY ([OutboxId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727185232_AddMassTransitTransactionalOutbox'
)
BEGIN
    CREATE TABLE [OutboxMessage] (
        [SequenceNumber] bigint NOT NULL IDENTITY,
        [EnqueueTime] datetime2 NULL,
        [SentTime] datetime2 NOT NULL,
        [Headers] nvarchar(max) NULL,
        [Properties] nvarchar(max) NULL,
        [InboxMessageId] uniqueidentifier NULL,
        [InboxConsumerId] uniqueidentifier NULL,
        [OutboxId] uniqueidentifier NULL,
        [MessageId] uniqueidentifier NOT NULL,
        [ContentType] nvarchar(256) NOT NULL,
        [MessageType] nvarchar(max) NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        [ConversationId] uniqueidentifier NULL,
        [CorrelationId] uniqueidentifier NULL,
        [InitiatorId] uniqueidentifier NULL,
        [RequestId] uniqueidentifier NULL,
        [SourceAddress] nvarchar(256) NULL,
        [DestinationAddress] nvarchar(256) NULL,
        [ResponseAddress] nvarchar(256) NULL,
        [FaultAddress] nvarchar(256) NULL,
        [ExpirationTime] datetime2 NULL,
        CONSTRAINT [PK_OutboxMessage] PRIMARY KEY ([SequenceNumber]),
        CONSTRAINT [FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId] FOREIGN KEY ([InboxMessageId], [InboxConsumerId]) REFERENCES [InboxState] ([MessageId], [ConsumerId]),
        CONSTRAINT [FK_OutboxMessage_OutboxState_OutboxId] FOREIGN KEY ([OutboxId]) REFERENCES [OutboxState] ([OutboxId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727185232_AddMassTransitTransactionalOutbox'
)
BEGIN
    CREATE INDEX [IX_InboxState_Delivered] ON [InboxState] ([Delivered]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727185232_AddMassTransitTransactionalOutbox'
)
BEGIN
    CREATE INDEX [IX_OutboxMessage_EnqueueTime] ON [OutboxMessage] ([EnqueueTime]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727185232_AddMassTransitTransactionalOutbox'
)
BEGIN
    CREATE INDEX [IX_OutboxMessage_ExpirationTime] ON [OutboxMessage] ([ExpirationTime]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727185232_AddMassTransitTransactionalOutbox'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber] ON [OutboxMessage] ([InboxMessageId], [InboxConsumerId], [SequenceNumber]) WHERE [InboxMessageId] IS NOT NULL AND [InboxConsumerId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727185232_AddMassTransitTransactionalOutbox'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_OutboxMessage_OutboxId_SequenceNumber] ON [OutboxMessage] ([OutboxId], [SequenceNumber]) WHERE [OutboxId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727185232_AddMassTransitTransactionalOutbox'
)
BEGIN
    CREATE INDEX [IX_OutboxState_Created] ON [OutboxState] ([Created]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727185232_AddMassTransitTransactionalOutbox'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727185232_AddMassTransitTransactionalOutbox', N'10.0.8');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    IF OBJECT_ID(N'[user].[LegacyReputationLedgerEntries]', N'U') IS NULL
    BEGIN
        SELECT * INTO [user].[LegacyReputationLedgerEntries]
        FROM [user].[ReputationLedgerEntries];
    END;

    DELETE FROM [user].[ReputationLedgerEntries]
    WHERE [status] <> 'CONFIRMED';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    DROP INDEX [IX_ReputationLedgerEntries_source_service_source_type_source_id] ON [user].[ReputationLedgerEntries];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    DROP INDEX [IX_ReputationLedgerEntries_user_id_status_created_at] ON [user].[ReputationLedgerEntries];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT [CK_ReputationLedgerEntries_entry_type];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT [CK_ReputationLedgerEntries_points_non_zero];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT [CK_ReputationLedgerEntries_status];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    DECLARE @var67 nvarchar(max);
    SELECT @var67 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationLedgerEntries]') AND [c].[name] = N'cancelled_at');
    IF @var67 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT ' + @var67 + ';');
    ALTER TABLE [user].[ReputationLedgerEntries] DROP COLUMN [cancelled_at];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    DECLARE @var68 nvarchar(max);
    SELECT @var68 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationLedgerEntries]') AND [c].[name] = N'confirm_after');
    IF @var68 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT ' + @var68 + ';');
    ALTER TABLE [user].[ReputationLedgerEntries] DROP COLUMN [confirm_after];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    DECLARE @var69 nvarchar(max);
    SELECT @var69 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationLedgerEntries]') AND [c].[name] = N'confirmed_at');
    IF @var69 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT ' + @var69 + ';');
    ALTER TABLE [user].[ReputationLedgerEntries] DROP COLUMN [confirmed_at];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    DECLARE @var70 nvarchar(max);
    SELECT @var70 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationLedgerEntries]') AND [c].[name] = N'entry_type');
    IF @var70 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT ' + @var70 + ';');
    ALTER TABLE [user].[ReputationLedgerEntries] DROP COLUMN [entry_type];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    DECLARE @var71 nvarchar(max);
    SELECT @var71 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationLedgerEntries]') AND [c].[name] = N'reason');
    IF @var71 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT ' + @var71 + ';');
    ALTER TABLE [user].[ReputationLedgerEntries] DROP COLUMN [reason];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    DECLARE @var72 nvarchar(max);
    SELECT @var72 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationLedgerEntries]') AND [c].[name] = N'reversed_at');
    IF @var72 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT ' + @var72 + ';');
    ALTER TABLE [user].[ReputationLedgerEntries] DROP COLUMN [reversed_at];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    DECLARE @var73 nvarchar(max);
    SELECT @var73 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationLedgerEntries]') AND [c].[name] = N'status');
    IF @var73 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT ' + @var73 + ';');
    ALTER TABLE [user].[ReputationLedgerEntries] DROP COLUMN [status];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    DECLARE @var74 nvarchar(max);
    SELECT @var74 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[BuyerReputationProfiles]') AND [c].[name] = N'trust_level');
    IF @var74 IS NOT NULL EXEC(N'ALTER TABLE [user].[BuyerReputationProfiles] DROP CONSTRAINT ' + @var74 + ';');
    ALTER TABLE [user].[BuyerReputationProfiles] DROP COLUMN [trust_level];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    EXEC sp_rename N'[user].[ReputationLedgerEntries].[reversal_entry_id]', N'reverses_entry_id', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    EXEC sp_rename N'[user].[ReputationLedgerEntries].[points]', N'score_delta', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    DECLARE @var75 nvarchar(max);
    SELECT @var75 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationLedgerEntries]') AND [c].[name] = N'source_type');
    IF @var75 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT ' + @var75 + ';');
    ALTER TABLE [user].[ReputationLedgerEntries] ALTER COLUMN [source_type] varchar(80) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    DECLARE @var76 nvarchar(max);
    SELECT @var76 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationLedgerEntries]') AND [c].[name] = N'source_service');
    IF @var76 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT ' + @var76 + ';');
    ALTER TABLE [user].[ReputationLedgerEntries] ALTER COLUMN [source_service] varchar(80) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    DECLARE @var77 nvarchar(max);
    SELECT @var77 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationLedgerEntries]') AND [c].[name] = N'source_id');
    IF @var77 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT ' + @var77 + ';');
    ALTER TABLE [user].[ReputationLedgerEntries] ALTER COLUMN [source_id] varchar(200) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    DECLARE @var78 nvarchar(max);
    SELECT @var78 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationLedgerEntries]') AND [c].[name] = N'rule_version');
    IF @var78 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT ' + @var78 + ';');
    ALTER TABLE [user].[ReputationLedgerEntries] ALTER COLUMN [rule_version] varchar(50) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    DROP INDEX [IX_ReputationLedgerEntries_idempotency_key] ON [user].[ReputationLedgerEntries];
    DECLARE @var79 nvarchar(max);
    SELECT @var79 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user].[ReputationLedgerEntries]') AND [c].[name] = N'idempotency_key');
    IF @var79 IS NOT NULL EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] DROP CONSTRAINT ' + @var79 + ';');
    ALTER TABLE [user].[ReputationLedgerEntries] ALTER COLUMN [idempotency_key] varchar(450) NOT NULL;
    CREATE UNIQUE INDEX [IX_ReputationLedgerEntries_idempotency_key] ON [user].[ReputationLedgerEntries] ([idempotency_key]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    ALTER TABLE [user].[ReputationLedgerEntries] ADD [correlation_id] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    ALTER TABLE [user].[ReputationLedgerEntries] ADD [evidence_reference] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    ALTER TABLE [user].[ReputationLedgerEntries] ADD [message_id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    ALTER TABLE [user].[ReputationLedgerEntries] ADD [occurred_at] datetimeoffset(3) NOT NULL DEFAULT '0001-01-01T00:00:00.000+00:00';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    ALTER TABLE [user].[ReputationLedgerEntries] ADD [reason_code] varchar(150) NOT NULL DEFAULT '';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    ALTER TABLE [user].[ReputationLedgerEntries] ADD [role] varchar(20) NOT NULL DEFAULT '';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    ALTER TABLE [user].[ReputationLedgerEntries] ADD [score_after] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    ALTER TABLE [user].[ReputationLedgerEntries] ADD [score_before] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    ALTER TABLE [user].[BuyerReputationProfiles] ADD [auction_restriction_status] varchar(30) NOT NULL DEFAULT 'NONE';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    ALTER TABLE [user].[BuyerReputationProfiles] ADD [blocking_violation_code] varchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    ALTER TABLE [user].[BuyerReputationProfiles] ADD [requires_manual_review] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    ALTER TABLE [user].[BuyerReputationProfiles] ADD [restricted_until] datetimeoffset(3) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    ;WITH OrderedLedger AS
    (
        SELECT
            [id],
            COALESCE(
                SUM([score_delta]) OVER (
                    PARTITION BY [user_id]
                    ORDER BY [created_at], [id]
                    ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING),
                0) AS [calculated_before]
        FROM [user].[ReputationLedgerEntries]
    )
    UPDATE target
    SET
        [role] = 'BUYER',
        [reason_code] = CONCAT(
            'buyer.legacy.',
            LOWER(REPLACE(REPLACE(archive.[reason], ' ', '-'), '_', '-'))),
        [score_before] = ordered.[calculated_before],
        [score_after] = ordered.[calculated_before] + target.[score_delta],
        [message_id] = target.[id],
        [occurred_at] = COALESCE(
            archive.[confirmed_at],
            archive.[created_at])
    FROM [user].[ReputationLedgerEntries] target
    INNER JOIN OrderedLedger ordered ON ordered.[id] = target.[id]
    INNER JOIN [user].[LegacyReputationLedgerEntries] archive
        ON archive.[id] = target.[id];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    CREATE TABLE [user].[SellerReputationProfiles] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [confirmed_score] int NOT NULL,
        [selling_restriction_status] varchar(30) NOT NULL,
        [auction_restriction_status] varchar(30) NOT NULL,
        [restricted_until] datetimeoffset(3) NULL,
        [requires_manual_review] bit NOT NULL,
        [blocking_violation_code] varchar(100) NULL,
        [created_at] datetimeoffset(3) NOT NULL,
        [updated_at] datetimeoffset(3) NULL,
        [deleted_at] datetimeoffset(3) NULL,
        CONSTRAINT [PK_SellerReputationProfiles] PRIMARY KEY ([id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    CREATE TABLE [user].[TransactionRatingEligibilities] (
        [id] uniqueidentifier NOT NULL,
        [transaction_type] varchar(20) NOT NULL,
        [transaction_id] uniqueidentifier NOT NULL,
        [rater_user_id] uniqueidentifier NOT NULL,
        [target_user_id] uniqueidentifier NOT NULL,
        [opens_at] datetimeoffset(3) NOT NULL,
        [expires_at] datetimeoffset(3) NOT NULL,
        [revoked_at] datetimeoffset(3) NULL,
        [submitted_rating_id] uniqueidentifier NULL,
        [created_at] datetimeoffset(3) NOT NULL,
        [updated_at] datetimeoffset(3) NOT NULL,
        CONSTRAINT [PK_TransactionRatingEligibilities] PRIMARY KEY ([id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    CREATE TABLE [user].[UserRatings] (
        [id] uniqueidentifier NOT NULL,
        [transaction_type] varchar(20) NOT NULL,
        [transaction_id] uniqueidentifier NOT NULL,
        [rater_user_id] uniqueidentifier NOT NULL,
        [target_user_id] uniqueidentifier NOT NULL,
        [score] int NOT NULL,
        [comment] nvarchar(1000) NULL,
        [created_at] datetimeoffset(3) NOT NULL,
        CONSTRAINT [PK_UserRatings] PRIMARY KEY ([id]),
        CONSTRAINT [CK_UserRatings_score] CHECK ([score] >= 1 AND [score] <= 5)
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    CREATE INDEX [IX_ReputationLedgerEntries_message_id] ON [user].[ReputationLedgerEntries] ([message_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ReputationLedgerEntries_reverses_entry_id] ON [user].[ReputationLedgerEntries] ([reverses_entry_id]) WHERE [reverses_entry_id] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    CREATE INDEX [IX_ReputationLedgerEntries_user_id_role_occurred_at] ON [user].[ReputationLedgerEntries] ([user_id], [role], [occurred_at]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    EXEC(N'ALTER TABLE [user].[ReputationLedgerEntries] ADD CONSTRAINT [CK_ReputationLedgerEntries_score_delta_non_zero] CHECK ([score_delta] <> 0)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SellerReputationProfiles_user_id] ON [user].[SellerReputationProfiles] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TransactionRatingEligibilities_transaction_type_transaction_id_rater_user_id_target_user_id] ON [user].[TransactionRatingEligibilities] ([transaction_type], [transaction_id], [rater_user_id], [target_user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    CREATE INDEX [IX_UserRatings_rater_user_id_created_at] ON [user].[UserRatings] ([rater_user_id], [created_at]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    CREATE INDEX [IX_UserRatings_target_user_id_created_at] ON [user].[UserRatings] ([target_user_id], [created_at]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserRatings_transaction_type_transaction_id_rater_user_id_target_user_id] ON [user].[UserRatings] ([transaction_type], [transaction_id], [rater_user_id], [target_user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801050350_CompleteReputationSystem'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260801050350_CompleteReputationSystem', N'10.0.8');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801073807_DropLegacyReputationLedgerEntries'
)
BEGIN
    IF OBJECT_ID(N'[user].[LegacyReputationLedgerEntries]', N'U') IS NOT NULL
    BEGIN
        DROP TABLE [user].[LegacyReputationLedgerEntries];
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801073807_DropLegacyReputationLedgerEntries'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260801073807_DropLegacyReputationLedgerEntries', N'10.0.8');
END;

COMMIT;
GO

