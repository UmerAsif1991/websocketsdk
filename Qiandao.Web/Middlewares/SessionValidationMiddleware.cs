namespace Qiandao.Web.Middlewares
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            // Public paths (login, landing page, static files, etc.)
            if (!path.Equals("/", StringComparison.OrdinalIgnoreCase) &&
                !path.Contains("/Authentication/Login", StringComparison.OrdinalIgnoreCase) &&
                !path.StartsWith("/css", StringComparison.OrdinalIgnoreCase) &&
                !path.StartsWith("/js", StringComparison.OrdinalIgnoreCase) &&
                !path.StartsWith("/images", StringComparison.OrdinalIgnoreCase))
            {
                var tenantId = context.Session.GetString("TenantId");
                if (string.IsNullOrEmpty(tenantId))
                {
                    context.Response.Redirect("/Authentication/Login?sessionExpired=true");
                    return;
                }
            }

            await _next(context);
        }

    }

}
