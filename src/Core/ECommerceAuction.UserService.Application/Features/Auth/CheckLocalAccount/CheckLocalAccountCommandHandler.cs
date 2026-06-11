using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;

namespace ECommerceAuction.UserService.Application.Features.Auth.CheckLocalAccount;

public class CheckLocalAccountCommandHandler
	: ICommandHandler<CheckLocalAccountCommand, CheckLocalAccountResponse>
{
	private readonly IUserRepository _userRepository;
	private readonly IPasswordHasher _passwordHasher;
	private readonly IJwtTokenService _jwtTokenService;
	private readonly IUnitOfWork _unitOfWork;

	public CheckLocalAccountCommandHandler(
		IUserRepository userRepository,
		IPasswordHasher passwordHasher,
		IJwtTokenService jwtTokenService,
		IUnitOfWork unitOfWork)
	{
		_userRepository = userRepository;
		_passwordHasher = passwordHasher;
		_jwtTokenService = jwtTokenService;
		_unitOfWork = unitOfWork;
	}

	public async Task<CheckLocalAccountResponse> Handle(
		CheckLocalAccountCommand request,
		CancellationToken cancellationToken)
	{
		var user = await _userRepository.GetByEmailOrPhoneAsync(
			request.EmailOrPhone,
			cancellationToken);

		if (user is null)
		{
			throw new Exception("Thông tin đăng nhập không chính xác");
		}

		if (user.Status != "ACTIVE")
		{
			throw new Exception("Thông tin đăng nhập không chính xác");
		}

		if (user.LockedUntil is not null && user.LockedUntil > DateTime.UtcNow)
		{
			throw new Exception("Thông tin đăng nhập không chính xác");
		}

		var isPasswordValid = _passwordHasher.VerifyPassword(
			request.Password,
			user.PasswordHash);

		if (!isPasswordValid)
		{
			user.FailedLoginAttempts += 1;
			user.UpdatedAt = DateTime.UtcNow;

			if (user.FailedLoginAttempts >= 5)
			{
				user.Status = "LOCKED";
				user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
			}

			await _unitOfWork.SaveChangesAsync(cancellationToken);

			throw new Exception("Thông tin đăng nhập không chính xác");
		}

		user.FailedLoginAttempts = 0;
		user.LastLoginAt = DateTime.UtcNow;
		user.UpdatedAt = DateTime.UtcNow;

		await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);

        var accessToken = _jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email,
            roles);


        return new CheckLocalAccountResponse(
			user.Id,
			user.Email,
			user.PhoneNumber,
			user.FullName,
			user.Status,
			accessToken);
	}
}