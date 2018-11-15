using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NewsPublish.Model.Entity;
using NewsPublish.Model.Request;
using NewsPublish.Model.Response;
using System.Linq;

namespace NewsPublish.Service
{
    public class CommentService
    {
        private readonly Db _db;
        private readonly NewsService _newsService;
        public CommentService(Db db, NewsService newsService)
        {
            this._db = db;
            this._newsService = newsService;
        }

        //添加新闻
        public ResponseModel AddComment(AddComment comment)
        {
            var news = _newsService.GetOneNews(comment.NewsId);
            if (news.Code == 0)
                return new ResponseModel { Code = 0, Result = "新闻不存在" };
            var com = new NewsComment
            {
                AddTime = DateTime.Now,
                NewsId = comment.NewsId,
                Contents = comment.Contents
            };
            _db.NewsComment.Add(com);
            int i = _db.SaveChanges();
            if (i > 0)
            {
                return new ResponseModel
                {
                    Code = 200,
                    Result = "新闻评论添加成功",
                    Data = new
                    {
                        contents = comment.Contents,
                        floor = "#" + (news.Data.CommentCount + 1),
                        addTime = DateTime.Now.ToString("yyyy-MM-dd")
                    }
                };
            }
            return new ResponseModel { Code = 0, Result = "新闻评论添加失败" };
        }

        //删除新闻
        public ResponseModel DelComment(int id)
        {
            var comment = _db.NewsComment.Find(id);
            if (comment == null)
                return new ResponseModel { Code = 0, Result = "该条评论不存在" };
            _db.NewsComment.Remove(comment);
            int i = _db.SaveChanges();
            if (i > 0)
            {
                return new ResponseModel
                {
                    Code = 200,
                    Result = "新闻评论删除成功"
                };
            }
            return new ResponseModel { Code = 0, Result = "新闻评论删除失败" };
        }

        //获取评论集合
        public ResponseModel GetCommentList(Expression<Func<NewsComment, bool>> where)
        {
            var comments = _db.NewsComment.Include("News").Where(where).OrderBy(c => c.AddTime).ToList();
            var response = new ResponseModel();
            response.Code = 200;
            response.Result = "评论获取成功！";
            response.Data = new List<CommentModel>();
            int floor = 1;
            foreach (var comment in comments)
            {
                response.Data.Add(new CommentModel
                {
                    Id = comment.Id,
                    NewsName = comment.News.Title,
                    Content = comment.Contents,
                    AddTime = comment.AddTime,
                    Remark = comment.Remark,
                    Floor = "#" + floor
                });
                floor++;
            }

            response.Data.Reverse();
            return response;
        }
    }
}
