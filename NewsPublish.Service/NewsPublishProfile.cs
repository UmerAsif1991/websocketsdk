using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using NewsPublish.Model.Entity;
using NewsPublish.Model.Response;
using NewsPublish.Model.Request;

namespace NewsPublish.Service
{
    public class NewsPublishProfile:Profile
    {
        public NewsPublishProfile()
        {
            #region Entity to Response

            CreateMap<Banner,BannerModel>();
            CreateMap<News, NewsModel>();
            CreateMap<NewsClassify, NewsClassifyModel>();
            CreateMap<NewsComment, CommentModel>();

            #endregion Entity to Response

            #region require to model

            CreateMap<AddBanner, Banner>();
            CreateMap<AddComment, NewsComment>();
            CreateMap<AddNews, News>();
            CreateMap<AddNewsClassify, NewsClassify>();
            CreateMap<EditNewsClassify, NewsClassify>();
            #endregion require to model
        }
    }
}
