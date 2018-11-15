using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NewsPublish.Model.Response;
using NewsPublish.Service;

namespace NewsPublish.Web.Areas.Admin.Controllers
{
    [Area("admin")]
    public class CommentController : Controller
    {
        private readonly CommentService _commentService;

        public CommentController(CommentService commentService)
        {
            this._commentService = commentService;
        }

        public IActionResult Index()
        {
            var list = _commentService.GetCommentList(c =>true);
            return View(list);
        }

        public JsonResult DelComment(int id)
        {
            if (id <= 0)
                return Json(new ResponseModel { Code = 0, Result = "参数有误" });
            return Json(_commentService.DelComment(id));
        }
    }
}