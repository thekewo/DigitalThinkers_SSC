using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalThinkers_SSC.Models
{
    public class MoneyContext : DbContext
    {
        public MoneyContext(DbContextOptions<MoneyContext> options)
            : base(options)
        {
        }

        public DbSet<Money> MoneyItems { get; set; }
    }
}
