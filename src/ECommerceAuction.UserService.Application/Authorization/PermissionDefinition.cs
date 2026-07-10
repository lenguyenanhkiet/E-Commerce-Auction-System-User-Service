using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Authorization
{
    /// <summary>
    /// Describes a permission published by a service.
    /// Common definition: both the publisher (all services) and the receiver (User Service reference this record, and it is serialized in the PermissionCatalogAnnounced event.
    /// </summary>
    public sealed record PermissionDefinition
    {
        /// <summary>
        /// Permission code, for example "USER.CREATE". This is the business definition key.
        /// </summary>
        public required string Code { get; init; }
        /// <summary>
        /// Function group (taken from the child class name in Permissions), for example "Categories".
        /// </summary>
        public required string Group {  get; init; }
        /// <summary>
        /// The service owns this permission, for example, "user".
        /// </summary>
        public required string ServiceCode { get; init; }
        /// <summary>
        /// Description for the permission matrix UI. Optional.
        /// </summary>
        public string? Description { get; init; }
    }
}
