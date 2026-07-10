using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ECommerceAuction.UserService.Application.Authorization;

public static class PermissionRegistry
{
    /// <summary>
    // Scan all permissions that this service possesses
    // Each child class of <see cref="Permissions"/> is a group
    // Each "public const string" constant inside is a permission
    /// </summary>
    public static IReadOnlyList<PermissionDefinition> GetAll()
    {
        var serviceCode = Permissions.ServiceCode;
        var definitions = new List<PermissionDefinition>();

        var groups = typeof(Permissions).GetNestedTypes(BindingFlags.Public);

        foreach (var group in groups)
        {
            foreach (var field in GetConstStringFields(group))
            {
                // GetRawConstantValue()
                var code = (string)field.GetRawConstantValue()!;
                var description = field.GetCustomAttribute<PermissionDescriptionAttribute>()?.Description;

                definitions.Add(new PermissionDefinition
                {
                    Code = code,
                    Group = group.Name,
                    ServiceCode = serviceCode,
                    Description = description
                });
            }
        }
        return definitions;
    }
    /// <summary>
    // Filter out fields that are "const strings
    // Key: const has IsLiteral = true and IsInitOnly = false
    // while "static readonly" is the opposite (IsLiteral = false, IsInitOnly = true) — it is excluded
    // </summary>
    private static IEnumerable<FieldInfo> GetConstStringFields(Type type)
    {
        return type
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral
                        && !f.IsInitOnly
                        && f.FieldType == typeof(string));
    }
}
