using System;
using Microsoft.EntityFrameworkCore;
using ProniaTask.Models;
using System.Reflection.Metadata;

namespace ProniaTask.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt)
        {

        }

        public DbSet<Product> Products { get; set; }

    }
}


