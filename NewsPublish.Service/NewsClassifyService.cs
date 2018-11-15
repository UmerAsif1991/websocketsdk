using NewsPublish.Model.Request;
using NewsPublish.Model.Response;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NewsPublish.Model.Entity;
using System.Linq.Expressions;

namespace NewsPublish.Service
{
    public class NewsClassifyService
    {
        private Db _db;
        public NewsClassifyService(Db db)
        {
            this._db = db;
        }

        /// <summary>
        /// 添加一个新闻类别
        /// </summary>
        /// <returns></returns>
        public ResponseModel AddNewsClassify(AddNewsClassify newsClassify)
        {
            var exit = _db.NewsClassify.FirstOrDefault(c => c.Name == newsClassify.Name) != null;
            if (exit)
                return new ResponseModel { Code = 0, Result = "该类已经存在" };
            var classify = new NewsClassify() { Name = newsClassify.Name, Sort = newsClassify.Sort, Remark = newsClassify.Remark };
            _db.NewsClassify.Add(classify);
            int i = _db.SaveChanges();
            if (i > 0)
                return new ResponseModel { Code = 200, Result = "Classify 添加成功" };
            return new ResponseModel { Code = 0, Result = "Classify 添加失败" };

        }

        /// <summary>
        /// 获取一个新闻类别
        /// </summary>
        public ResponseModel GetOneNewsClassify(int id)
        {
            var newsClassify = _db.NewsClassify.Find(id);
            if(newsClassify == null)
                return new ResponseModel(){ Code=0,Result="该新闻类别不存在" };

            return new ResponseModel() { Code = 200,Result="新闻获取成功", Data = new NewsClassifyModel() {
                Id = newsClassify.Id,
                Name= newsClassify.Name,
                Sort = newsClassify.Sort,
                Remark = newsClassify.Remark  } };
        }

        /// <summary>
        /// 获取一个新闻类别
        /// </summary>
        public ResponseModel GetOneNewsClassify(Expression<Func<NewsClassify,bool>> where)
        {
            var newsClassify = _db.NewsClassify.Where(where).FirstOrDefault();
            if (newsClassify != null)
                return new ResponseModel() { Code = 0, Result = "该新闻类别不存在" };

            return new ResponseModel()
            {
                Code = 200,
                Result = "新闻获取成功",
                Data = new NewsClassifyModel()
                {
                    Id = newsClassify.Id,
                    Name = newsClassify.Name,
                    Sort = newsClassify.Sort,
                    Remark = newsClassify.Remark
                }
            };
        }

        /// <summary>
        /// 修改新闻类别
        /// </summary>
        public ResponseModel EditNewsClassify(EditNewsClassify editNewsClassify)
        {
            var updateNewsClassify = _db.NewsClassify.Find(editNewsClassify.Id);
            if (updateNewsClassify == null)
                return new ResponseModel() { Code = 0, Result = "该类别不存在" };
            updateNewsClassify.Name = editNewsClassify.Name;
            updateNewsClassify.Sort = editNewsClassify.Sort;
            updateNewsClassify.Remark = editNewsClassify.Remark;
            int i = _db.SaveChanges();
            if (i > 0)
                return new ResponseModel { Code = 200, Result = "Classify 修改成功" };
            return new ResponseModel { Code = 0, Result = "Classify 修改失败" };
        }

        /// <summary>
        /// 获取新闻类别集合
        /// </summary>
        public ResponseModel GetNewsClassigy()
        {
            var newsClassify = _db.NewsClassify.ToList().OrderBy(c => c.Sort);
            var response = new ResponseModel();
            response.Code = 200;
            response.Result = "类别集合获取成功";
            response.Data = new List<NewsClassifyModel>();
            foreach(var item in newsClassify)
            {
                response.Data.Add(new NewsClassifyModel() {
                    Id =item.Id,
                    Name=item.Name,
                    Sort =item.Sort,
                    Remark =item.Remark
                });
            }
            return response;
        }
    }
}
