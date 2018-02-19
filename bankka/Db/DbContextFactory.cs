using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace bankka.Db
{
    public interface IDbContextFactory
    {
        BankkaContext Create();
    }

    public class DbContextFactory : IDbContextFactory
    {
        private readonly DbContextOptions<BankkaContext> _options;

        public DbContextFactory( DbContextOptions<BankkaContext> options)
        {
            _options = options;
        }

        public BankkaContext Create()
        {
            return new BankkaContext(_options);
        }
    }
}
