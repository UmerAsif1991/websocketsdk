using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewsPublish.Service;
using NewsPublish.Model.Response;

namespace NewsPublish.Web.Components
{
    public class NavigateComponent: ViewComponent
    {
        private readonly NewsClassifyService _newsClassifyService;

        public NavigateComponent(NewsClassifyService newsClassifyService) {
            this._newsClassifyService = newsClassifyService;
        }

        public IViewComponentResult Invoke()
        {
            ResponseModel classify = this._newsClassifyService.GetNewsClassigy();
            return View(classify);
        }
    }
}
