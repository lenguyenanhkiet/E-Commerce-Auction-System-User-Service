using FluentValidation;
using ECommerceAuction.UserService.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceAuction.UserService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
        });

        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Reputation application services.
        services.AddScoped<
            Reputation.Services.IReputationAwardService,
            Reputation.Services.ReputationAwardService>();
        services.AddScoped<IdentityVerifications.VerifyEmail.CompleteEmailVerificationService>();
        services.AddScoped<IdentityVerifications.VerifyPhone.CompletePhoneVerificationService>();
        services.AddScoped<IdentityVerifications.VerifyIdentity.CompleteIdentityVerificationService>();
        services.AddScoped<IdentityVerifications.VerifyAddress.CompleteAddressVerificationService>();

        return services;
    }
}
