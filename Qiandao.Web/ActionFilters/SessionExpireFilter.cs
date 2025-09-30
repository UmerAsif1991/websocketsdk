using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Qiandao.Web.ActionFilters
{
    public class SessionExpireFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Skip if decorated with [AllowAnonymousSession]
            var hasAttr = context.ActionDescriptor.EndpointMetadata
                .OfType<AllowAnonymousSessionAttribute>().Any();

            if (hasAttr) return;

            var tenantId = context.HttpContext.Session.GetString("TenantId");
            if (string.IsNullOrEmpty(tenantId))
            {
                context.Result = new RedirectToActionResult(
                    "Login",
                    "Authentication",
                    new { sessionExpired = true });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AllowAnonymousSessionAttribute : Attribute { }

}
