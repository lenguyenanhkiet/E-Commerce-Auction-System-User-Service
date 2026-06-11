namespace ECommerceAuction.UserService.Domain.Entities.Users;

public class UserRole
{
	public Guid Id { get; set; }

	public Guid UserId { get; set; }

	public Guid RoleId { get; set; }

	public Guid? AssignedBy { get; set; }

	public DateTime AssignedAt { get; set; }

	public DateTime? RevokedAt { get; set; }

	public string Status { get; set; } = "ACTIVE";

	public User User { get; set; } = null!;

	public Role Role { get; set; } = null!;
}