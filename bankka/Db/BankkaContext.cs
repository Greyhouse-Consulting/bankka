using System;
using System.Collections.Generic;
using System.Text;
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

    public class Customer
    {
        public long Id { get; set; }

        public string Name { get; set; }
        public IList<Account> Accounts { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class Account
    {

        public Account()
        {
            Transactions = new List<Transaction>();
        }
        public long Id { get; set; }

        public decimal Balance { get; set; }

        public IList<Transaction> Transactions { get; set; }
        public Customer Customer { get; set; }
        public string Name { get; set; }
    }

    public class Transaction
    {
        public  long Id { get; set; }

        public decimal Amount { get; set; }

        public DateTime DateTime { get; set; }
    }
}
