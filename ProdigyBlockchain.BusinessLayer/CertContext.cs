using Prodigy.BusinessLayer.Models;
using Prodigy.BusinessLayer.DatabaseConfigurations;
using Prodigy.BusinessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer
{
    public interface ICertContext
    {
        DbSet<Certificate> OnlineCerts { get; set; }

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
    }

    public class CertContext: DbContext, ICertContext
    {
        public IDatabaseConnectionSettings ConnectionSettings { get; set; }

        public CertContext(IDatabaseConnectionSettings configuration)
        {
            this.ConnectionSettings = configuration;
        }

        public CertContext(DbContextOptions<BlockchainContext> options, IDatabaseConnectionSettings configuration)
            : base(options)
        {
            this.ConnectionSettings = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured == false)
            {
                optionsBuilder.UseMySql(this.ConnectionSettings.ConnectionString, ServerVersion.AutoDetect(this.ConnectionSettings.ConnectionString));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OnlineCertsConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<Certificate> OnlineCerts { get; set; }
    }
}
