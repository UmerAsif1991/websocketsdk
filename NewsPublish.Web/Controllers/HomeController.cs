using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NewsPublish.Web.Models;
using NewsPublish.Service;

namespace NewsPublish.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly BannerService _bannerService;
        private readonly NewsService _newsService;

        public HomeController(BannerService bannerService,NewsService newsService)
        {
            this._bannerService = bannerService;
            this._newsService = newsService;
        }

        public IActionResult Index()
        {
            return View();
        }

        //首页banner
        public JsonResult GetBanners()
        {
            var banners = _bannerService.GetBannerList();
            return Json(banners);
        }

        public JsonResult GetTopNews()
        {
            int top = 5;
            var news = _newsService.GetNewsTop(n => true, top);
            return Json(news);
        }

        public JsonResult GetTotalNewsCount()
        {
            var totalNews = _newsService.GetNewsCount(n=>true);
            return Json(totalNews);
        }

        public JsonResult GetNewCommentNewsList()
        {
            var news = _newsService.GetNewCommentNewsList(n => true, 4);
            return Json(news);
        }


    }
}
