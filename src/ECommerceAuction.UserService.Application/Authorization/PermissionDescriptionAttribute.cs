using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Authorization;

/// <summary>
/// Assign a description to a permission constant, seed it to the database, and display it in the permission matrix UI
/// Optional — permissions without attributes are still valid (Description = null)
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class PermissionDescriptionAttribute : Attribute
{
    public PermissionDescriptionAttribute(string description) 
    {
        Description = description;
    }
    public string Description { get; } 
}
