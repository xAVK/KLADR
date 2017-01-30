using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace CreteElementAndWriteDB
{
    public class DbElementContext : DbContext
    {
        public DbElementContext()
            : base("DbConnection")
        { }
        public DbSet<DbElement> Buldings { get; set; }
    }
}
