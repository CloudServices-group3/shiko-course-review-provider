using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Shiko.CourseReviewProvider.Api.OpenApi;

internal sealed class AuthOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var endpointMetadata = context.Description.ActionDescriptor.EndpointMetadata;

        var hasAllowAnonymous = endpointMetadata
            .OfType<IAllowAnonymous>()
            .Any();

        if (hasAllowAnonymous)
        {
            return Task.CompletedTask;
        }

        var hasAuthorize = endpointMetadata
            .OfType<IAuthorizeData>()
            .Any();

        if (!hasAuthorize)
        {
            return Task.CompletedTask;
        }

        if (context.Document is null)
        {
            return Task.CompletedTask;
        }

        operation.Security ??= [];

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                JwtBearerDefaults.AuthenticationScheme,
                context.Document)] = []
        });

        return Task.CompletedTask;
    }
}