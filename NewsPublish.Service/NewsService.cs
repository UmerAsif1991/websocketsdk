using NewsPublish.Model.Entity;
using NewsPublish.Model.Request;
using NewsPublish.Model.Response;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using AutoMapper;

namespace NewsPublish.Service
{
    public class NewsService
    {
        private Db _db;
        private readonly NewsClassifyService _newsClassifyService;
        private readonly IMapper _mapper;

        public NewsService(Db db, NewsClassifyService newsClassifyService,IMapper mapper)
        {
            this._db = db;
            this._newsClassifyService = newsClassifyService;
            this._mapper = mapper;
        }

        /// <summary>
        /// 添加新闻
        /// </summary>
        public ResponseModel AddNews(AddNews addNews)
        {
            var classify = _newsClassifyService.GetOneNewsClassify(c => c.Id == addNews.NewsClassifyId);
            if (classify == null)
                return new ResponseModel { Code = 0, Result = "该类别不存在" };
            var n = _mapper.Map<News>(addNews);
            n.PublishDate = DateTime.Now;
            _db.News.Add(n);
            int i = _db.SaveChanges();
            if (i > 0)
                return new ResponseModel { Code = 200, Result = "新闻添加成功" };
            return new ResponseModel { Code = 0, Result = "新闻添加失败" };
        }

        //获取一条新闻
        public ResponseModel GetOneNews(int id)
        {
            var news = _db.News.Include("NewsClassify").Include("NewsComment").FirstOrDefault(n => n.Id == id);
            if (news == null)
                return new ResponseModel { Code = 0, Result = "该新闻不存在" };
            return new ResponseModel
            {
                Code = 200,
                Result = "新闻获取成功",
                Data =  new NewsModel
                {
                    Id = news.Id,
                    ClassifyName = news.NewsClassify.Name,
                    Title = news.Title,
                    Image = news.Image,
                    Contents = news.Contents,
                    PublishDate = news.PublishDate.ToString("yyyy-MM-dd"),
                    CommentCount = news.NewsComment.Count(),
                    Remark = news.Remark
                }
            };
        }

        //删除一条新闻
        public ResponseModel DelNews(int id)
        {
            var news = _db.News.Find(id);
            if (news == null)
                return new ResponseModel { Code = 0, Result = "该新闻不存在" };
            _db.News.Remove(news);
            int i = _db.SaveChanges();
            if (i > 0)
                return new ResponseModel { Code = 200, Result = "删除成功！" };
            return new ResponseModel { Code = 0, Result = "删除失败！" };
        }

        //分页获取新闻
        public ResponseModel GetNewsPageQuery(int pageSize, int pageIndex, out int total, List<Expression<Func<News, bool>>> where)
        {
            var list = _db.News.Include("NewsClassify").Include("NewsComment");
            //各个查询条件添加
            foreach (var item in where)
            {
                list = list.Where(item);
            }
            total = list.Count();
            var pageData = list.OrderByDescending(n => n.PublishDate).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            var response = new ResponseModel { Code = 200, Result = "分页新闻获取成功" };
            response.Data = new List<NewsModel>();

            foreach (var news in pageData)
            {
                response.Data.Add(new NewsModel
                {
                    Id = news.Id,
                    ClassifyName = news.NewsClassify.Name,
                    Title = news.Title,
                    Image = news.Image,
                    Contents = news.Contents.Length > 50 ? news.Contents.Substring(0, 50) + "..." : news.Contents,
                    PublishDate = news.PublishDate.ToString("yyyy-MM-dd"),
                    CommentCount = news.NewsComment.Count(),
                    Remark = news.Remark
                });
            }
            return response;
        }

        //获取新闻最新几条
        public ResponseModel GetNewsTop(Expression<Func<News, bool>> where, int topCount)
        {
            var list = _db.News.Include("NewsClassify").Include("NewsComment").Where(where).OrderByDescending(n => n.PublishDate).Take(topCount);
            var response = new ResponseModel
            {
                Code = 200,
                Result = "新闻列表获取成功"
            };
            response.Data = new List<NewsModel>();
            foreach (var news in list)
            {
                response.Data.Add(new NewsModel
                {
                    Id = news.Id,
                    ClassifyName = news.NewsClassify.Name,
                    Title = news.Title,
                    Image = news.Image,
                    Contents = news.Contents.Length > 50 ? news.Contents.Substring(0, 50) + "..." : news.Contents,
                    PublishDate = news.PublishDate.ToString("yyyy-MM-dd"),
                    CommentCount = news.NewsComment.Count(),
                    Remark = news.Remark
                });
            }
            return response;
        }

        //获取最新评论的新闻集合
        public ResponseModel GetNewCommentNewsList(Expression<Func<News, bool>> where, int topCount)
        {
            //获取最新评论对应的新闻Id
            var newsIds = _db.NewsComment.OrderByDescending(c => c.AddTime).GroupBy(c => c.NewsId).Select(c => c.Key).Take(topCount);
            var list = _db.News.Include("NewsClassify").Include("NewsComment").Where(n => newsIds.Contains(n.Id)).Where(where
                ).OrderByDescending(n => n.PublishDate);
            var response = new ResponseModel { Code = 200, Result = "最新评论新闻获取成功" };
            response.Data = new List<NewsModel>();

            foreach (var news in list)
            {
                response.Data.Add(new NewsModel
                {
                    Id = news.Id,
                    ClassifyName = news.NewsClassify.Name,
                    Title = news.Title,
                    Image = news.Image,
                    Contents = news.Contents.Length > 50 ? news.Contents.Substring(0, 50) + "..." : news.Contents,
                    PublishDate = news.PublishDate.ToString("yyyy-MM-dd"),
                    CommentCount = news.NewsComment.Count(),
                    Remark = news.Remark
                });
            }
            return response;
        }
        
        //搜索一条新闻
        public ResponseModel GetSearchOneNews(Expression<Func<News, bool>> where)
        {
            var news = _db.News.Where(where).FirstOrDefault();
            if (news == null)
                return new ResponseModel { Code = 0, Result = "没有获得新闻" };
            return new ResponseModel { Code = 200, Result = "获得该条新闻的Id", Data = news.Id };
        }

        //获取新闻数量
        public ResponseModel GetNewsCount(Expression<Func<News, bool>> where)
        {
            var count = _db.News.Where(where).Count();
            return new ResponseModel { Code = 200, Result = "新闻数量获取成功", Data = count };
        }

        // 获取相关新闻列表
        public ResponseModel GetRecommentNewsList(int newsId)
        {
            var news = _db.News.FirstOrDefault(c => c.Id == newsId);
            if (news == null)
                return new ResponseModel { Code = 0, Result = "新闻不存在" };

            var newsList = _db.News.Include("NewsComment").Where(n => n.NewsClassifyId == news.NewsClassifyId&& n.Id != news.Id)
            .OrderByDescending(c => c.PublishDate).OrderByDescending(n => n.NewsComment.Count()).Take(6).ToList();

            var response = new ResponseModel { Code = 200, Result = "相关新闻新闻获取成功" };
            response.Data = new List<NewsModel>();
            foreach (var item in newsList)
            {
                response.Data.Add(new NewsModel
                {
                    Id = item.Id,
                    ClassifyName = item.NewsClassify.Name,
                    Title = item.Title,
                    Image = item.Image,
                    Contents = item.Contents.Length > 50 ? news.Contents.Substring(0, 50) + "..." : news.Contents,
                    PublishDate = item.PublishDate.ToString("yyyy-MM-dd"),
                    CommentCount = item.NewsComment.Count(),
                    Remark = item.Remark
                });
            }
            return response;
        }
    }
}
