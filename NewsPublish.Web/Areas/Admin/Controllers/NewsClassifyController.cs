using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NewsPublish.Model.Request;
using NewsPublish.Service;

namespace NewsPublish.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NewsClassifyController : Controller
    {
        private readonly NewsClassifyService _newsClassifyService;

        public NewsClassifyController(NewsClassifyService newsClassifyService)
        {
            this._newsClassifyService = newsClassifyService;
        }
        /// <summary>
        /// 查看新闻类别列表
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var newsClassify = _newsClassifyService.GetNewsClassigy();
            return View(newsClassify);
        }

        public IActionResult NewsClassifyAdd()
        {
            return View();
        }

        public JsonResult AddNewsClassify(AddNewsClassify addNewsClassify)
        {
            var newsClassify = _newsClassifyService.AddNewsClassify(addNewsClassify);
            return Json(newsClassify);
        }

        public IActionResult NewsClassifyEdit(int id)
        {
            var newsClassify = _newsClassifyService.GetOneNewsClassify(id);
            return View(newsClassify);
        }

        public JsonResult EditNewsClassify(EditNewsClassify editNewsClassify)
        {
            var newsClassify = _newsClassifyService.EditNewsClassify(editNewsClassify);
            return Json(newsClassify);
        }
    }
}