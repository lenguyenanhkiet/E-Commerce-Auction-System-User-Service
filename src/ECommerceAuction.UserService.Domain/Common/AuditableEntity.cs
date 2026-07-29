//Define namespace for this class
namespace ECommerceAuction.UserService.Domain.Common;

/// <summary>
///AuditableEntity class - base class for entities that need to track change history
///Inherit from BaseEntity to get Id, and add properties for audit trail (historical tracking)
///Used to record entity creation, update, and deletion times
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>
    ///CreatedAt property - the time the entity was created
    ///DateTimeOffset type to store time in UTC
    ///Usually set when the entity is first created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    ///UpdatedAt property - the time the entity was last updated
    ///DateTimeOffset type? (nullable) because the entity may never have been updated
    ///A null value means the entity has never been updated since its creation
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    ///DeletedAt property - the time the entity was deleted (soft delete)
    ///DateTimeOffset type? (nullable) because the entity may never have been deleted
    ///Soft delete means the entity is not physically deleted but is only marked as deleted
    ///A null value means the entity is still active (not deleted).
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }
}

