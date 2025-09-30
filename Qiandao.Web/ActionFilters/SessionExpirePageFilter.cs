using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Qiandao.Web.ActionFilters
{
    public class SessionExpirePageFilter : IPageFilter
    {
        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            var tenantId = context.HttpContext.Session.GetString("TenantId");

            var page = context.ActionDescriptor.ViewEnginePath;

            if (string.IsNullOrEmpty(tenantId) && !page.Contains("/Authentication/Login"))
            {
                context.Result = new RedirectToPageResult("/Authentication/Login", new { sessionExpired = true });
            }
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context) { }
        public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }
    }

}
