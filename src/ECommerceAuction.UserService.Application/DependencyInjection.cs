using FluentValidation;
using ECommerceAuction.UserService.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Application.Features.Users.VerifyAddress;
using ECommerceAuction.UserService.Application.Features.Auth.VerifyEmail;
using ECommerceAuction.UserService.Application.Features.Identities.VerifyIdentity;
using ECommerceAuction.UserService.Application.Features.Users.VerifyPhone;

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

        // Handlers (e.g. RegisterSeller/ApproveSeller) take an ambient clock so a single
        // occurredAt drives approval, verification, ledger and event timestamps together.
        services.TryAddSingleton(TimeProvider.System);

        // Reputation application services.
        services.AddScoped<
            IReputationAwardService,
            ReputationAwardService>();
        services.AddScoped<CompleteEmailVerificationService>();
        services.AddScoped<CompletePhoneVerificationService>();
        services.AddScoped<CompleteIdentityVerificationService>();
        services.AddScoped<CompleteAddressVerificationService>();

        return services;
    }
}
