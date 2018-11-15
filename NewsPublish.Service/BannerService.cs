using NewsPublish.Model.Entity;
using NewsPublish.Model.Request;
using NewsPublish.Model.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewsPublish.Service
{
    /// <summary>
    /// Banner服务
    /// </summary>
    public class BannerService
    {
        private Db _db;
        public BannerService(Db db)
        {
            this._db = db;
        }

        /// <summary>
        /// 添加Banner
        /// </summary>
        public ResponseModel AddBanner(AddBanner addBanner)
        {
            var banner = new Banner() { Image = addBanner.Image, Remark = addBanner.Remark, AddTime = DateTime.Now, Url = addBanner.Url };
            _db.Banner.Add(banner);
            int i = _db.SaveChanges();
            if (i > 0)
            {
                return new ResponseModel() { Code = 200, Result = "Banner添加成功" };
            }
            else
            {
                return new ResponseModel() { Code = 0, Result = "Banner添加失败" };
            }
        }

        /// <summary>
        /// Banner集合获取
        /// </summary>
        public ResponseModel GetBannerList()
        {
            var banners = _db.Banner.ToList().OrderByDescending(c => c.AddTime);
            var response = new ResponseModel();
            response.Code = 200;
            response.Result = "Banner集合获取成功";
            response.Data = new List<BannerModel>();
            foreach (var item in banners)
            {
                response.Data.Add(new BannerModel()
                {
                    Id = item.Id,
                    Image = item.Image,
                    Url = item.Url,
                    Remark = item.Remark
                });
            }
            return response;
        }

        public ResponseModel DeletaBanner(int bannerID)
        {
            var banner = _db.Banner.Find(bannerID);
            if (banner == null)
                return new ResponseModel() { Code = 0, Result = "Banner不存在" };
            _db.Banner.Remove(banner);
            int i = _db.SaveChanges();
            if (i > 0)
                return new ResponseModel { Code = 200, Result = "Banner删除成功" };

            return new ResponseModel { Code = 0, Result = "Banner删除失败" };
        }
    }
}
