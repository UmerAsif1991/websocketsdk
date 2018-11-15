using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NewsPublish.Service;

namespace NewsPublish.Web.Controllers
{
    public class NewsClassifyController : Controller
    {
        private readonly NewsClassifyService _newsClassifyService;

        public NewsClassifyController(NewsClassifyService newsClassifyService)
        {
            this._newsClassifyService = newsClassifyService;
        }
        public JsonResult GetNewsClassify()
        {
            var newsClassify = _newsClassifyService.GetNewsClassigy();
            return Json(newsClassify);
        }
    }
}