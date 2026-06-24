//Define namespace for this class
namespace ECommerceAuction.UserService.Domain.Common;

/// <summary>
///BaseEntity class - base class for all entities in the domain
///This class provides common properties such as Id to all other entities
///Use an abstract class so that it cannot be instantiated directly, only inheriting from other classes
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    ///Id property - unique identifier of the entity
    ///Guid type (Globally Unique Identifier) ​​to ensure uniqueness
    ///The default value is set using Guid.NewGuid() when the object is created
    ///protected set only allows changes from this class and derived classes
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();
}

