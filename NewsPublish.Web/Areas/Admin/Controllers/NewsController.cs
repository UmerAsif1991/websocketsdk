using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewsPublish.Model.Entity;
using NewsPublish.Model.Request;
using NewsPublish.Model.Response;
using NewsPublish.Service;

namespace NewsPublish.Web.Areas.Admin.Controllers
{
    [Area("admin")]
    public class NewsController : Controller
    {
        private readonly NewsService _newsService;
        private readonly IHostingEnvironment _host;
        private readonly NewsClassifyService _newsClassifyService;

        public NewsController(NewsService newsService,IHostingEnvironment host,NewsClassifyService newsClassifyService)
        {
            this._newsService = newsService;
            this._host = host;
            this._newsClassifyService = newsClassifyService;
        }

        public ActionResult Index()
        {
            var newClassifys = _newsClassifyService.GetNewsClassigy();
            return View(newClassifys);
        }

        [HttpGet]
        public JsonResult GetNews(int pageIndex,int pageSize,int classifyId,string keyword)
        {
            List<Expression<Func<News, bool>>> wheres = new List<Expression<Func<News, bool>>>();
            if (classifyId > 0)
            {
                wheres.Add(n => n.NewsClassifyId == classifyId);
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                wheres.Add(n => n.Title.ToLower().Contains(keyword.ToLower()));
            }

            int total = 0;
            var news = _newsService.GetNewsPageQuery(pageSize, pageIndex, out total, wheres);
            return Json(new { total = total, data = news.Data });
        }

        [HttpGet]
        public ActionResult NewsAdd()
        {
            var newClassifys = _newsClassifyService.GetNewsClassigy();
            return View(newClassifys);
        }

        [HttpPost]
        [ResponseCache(NoStore=true)]
        public async Task<JsonResult> NewsAdd(AddNews news, IFormCollection collection)
        {
            var files = collection.Files;
            if (files.Count > 0)
            {
                var webRootPath = _host.WebRootPath;
                string relativeDirPath = "\\BannerPic";
                string absolutePath = webRootPath + relativeDirPath;

                string[] fileTypes = new string[] { ".gif", ".jpg", ".jpeg", ".png", ".bmp" };
                string extension = Path.GetExtension(files[0].FileName);
                if (fileTypes.Contains(extension.ToLower()))
                {
                    if (!Directory.Exists(absolutePath))
                    {
                        Directory.CreateDirectory(absolutePath);
                    }
                    string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                    string filePath = absolutePath + "\\" + fileName;
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await files[0].CopyToAsync(stream);
                    }
                    news.Image = "/BannerPic/" + fileName;
                    return Json(_newsService.AddNews(news));
                }
                return Json(new ResponseModel { Code = 0, Result = "图片格式有误" });
            }
            return Json(new ResponseModel { Code = 0, Result = "请上传图片文件" });
        }

        public JsonResult DelNews(int id)
        {
            return Json(_newsService.DelNews(id));
        }
    }
}