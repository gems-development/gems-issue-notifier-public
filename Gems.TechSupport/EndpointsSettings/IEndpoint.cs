using Microsoft.AspNetCore.Routing;

namespace Gems.TechSupport.EndpointsSettings;

public interface IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app);
}
