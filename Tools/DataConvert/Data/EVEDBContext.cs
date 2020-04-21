using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace DataConvert.Data
{
    public class EVEDBContext : DbContext
    {
        public DbSet<PropData> Props { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=eve_static.db");
        }
    }
}
