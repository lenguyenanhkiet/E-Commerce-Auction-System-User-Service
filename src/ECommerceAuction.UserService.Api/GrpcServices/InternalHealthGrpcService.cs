using ECommerceAuction.UserService.Api.Grpc;
using Grpc.Core;

namespace ECommerceAuction.UserService.Api.GrpcServices;

public sealed class InternalHealthGrpcService : InternalHealth.InternalHealthBase
{
    private readonly IConfiguration _configuration;

    public InternalHealthGrpcService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override Task<InternalHealthReply> Check(
        InternalHealthRequest request,
        ServerCallContext context)
    {
        var serviceName = _configuration["Service:Name"] ?? "ECommerceAuction.UserService.Api";

        return Task.FromResult(new InternalHealthReply
        {
            Service = serviceName,
            Status = "Healthy",
            CheckedAtUtc = DateTimeOffset.UtcNow.ToString("O")
        });
    }
}
