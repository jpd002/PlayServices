using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Http;

namespace PlayServices.Server
{
    public class SelfUserRequirement : IAuthorizationRequirement
    {

    };

    public class SelfUserHandler : AuthorizationHandler<SelfUserRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SelfUserHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SelfUserRequirement requirement)
        {
            if(context.Resource is RouteEndpoint endpoint)
            {
                var routeData = _httpContextAccessor.HttpContext.GetRouteData();
                var descriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                var userId = routeData.Values["userId"] as string;
                if(context.User.HasClaim(c => c.Type == ClaimTypes.Name && c.Value == userId))
                {
                    context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }
    }    
}