using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace projetCannabis
{
    public class CannabisContext : DbContext
    {
        public DbSet<Plantule> Plantules { get; set; }

        
        public DbSet<HistoriqueModifications> HistoriqueModifications { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;");
        }


      

    }
}
