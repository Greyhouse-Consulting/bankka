using System.Text;
using bankka.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace bankka.Db
{
    public class BankkaContext : DbContext
    {
        public BankkaContext(DbContextOptions<BankkaContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>().HasKey(k => k.Id);
            modelBuilder.Entity<Account>().HasMany(p => p.Transactions);

            modelBuilder.Entity<Customer>().HasKey(k => k.Id);
            modelBuilder.Entity<Customer>().HasMany(p => p.Accounts).WithOne(a => a.Customer);
        }
    }
}
