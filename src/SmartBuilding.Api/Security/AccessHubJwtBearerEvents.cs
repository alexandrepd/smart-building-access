using Microsoft.AspNetCore.Authentication.JwtBearer;
using SmartBuilding.Api.Hubs;

namespace SmartBuilding.Api.Security;

internal sealed class AccessHubJwtBearerEvents : JwtBearerEvents
{
    public override Task MessageReceived(MessageReceivedContext context)
    {
        // Browser WebSocket/SSE clients cannot set an Authorization header.
        // Never accept query-string bearer tokens on REST endpoints.
        if (context.Request.Path.StartsWithSegments(AccessHub.Path)
            && context.Request.Query.TryGetValue("access_token", out var token)
            && !string.IsNullOrEmpty(token))
        {
            context.Token = token;
        }

        return Task.CompletedTask;
    }
}
