using Microsoft.EntityFrameworkCore;
using NewsPublish.Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NewsPublish.Service
{
    public class Db:DbContext
    {
        public Db(DbContextOptions options):base(options)
        { }

        public virtual DbSet<Banner> Banner { get; set; }
        public virtual DbSet<News> News { get; set; }
        public virtual DbSet<NewsClassify> NewsClassify { get; set; }
        public virtual DbSet<NewsComment> NewsComment { get; set; }
    }
}
