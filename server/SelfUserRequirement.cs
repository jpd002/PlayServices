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

        string GetUserIdOrMeFromRouteData(RouteData routeData)
        {
            var userId = routeData.Values["userId"] as string;
            var userIdOrMe = routeData.Values["userIdOrMe"] as string;
            if(!string.IsNullOrEmpty(userId)) return userId;
            return userIdOrMe;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SelfUserRequirement requirement)
        {
            if(context.Resource is RouteEndpoint endpoint)
            {
                var routeData = _httpContextAccessor.HttpContext.GetRouteData();
                var descriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                var userIdOrMe = GetUserIdOrMeFromRouteData(routeData);
                if(userIdOrMe == "me" && context.User.HasClaim(c => c.Type == ClaimTypes.Name))
                {
                    context.Succeed(requirement);
                }
                else if(context.User.HasClaim(c => c.Type == ClaimTypes.Name && c.Value == userIdOrMe))
                {
                    context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }
    }    
}