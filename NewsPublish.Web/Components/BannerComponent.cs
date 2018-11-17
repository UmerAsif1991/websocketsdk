using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewsPublish.Service;

namespace NewsPublish.Web.Components
{
    public class BannerComponent: ViewComponent
    {
        private readonly BannerService _bannerService;

        public BannerComponent(BannerService bannerService) {
            this._bannerService = bannerService;
        }

        public IViewComponentResult Invoke()
        {
            var banners = this._bannerService.GetBannerList();

            return View(banners);
        }

    }
}
