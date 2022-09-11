using Prodigy.BusinessLayer.DatabaseConfigurations;
using Prodigy.BusinessLayer.Models;
using Prodigy.BusinessLayer.Models.Database;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer
{
    public interface IBlockchainContext
    {
        DbSet<BlockRecord> Blocks { get; set; }
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
    }

    public class BlockchainContext: DbContext, IBlockchainContext
    {
        public IDatabaseConnectionSettings ConnectionSettings { get; set; }

        public BlockchainContext(IDatabaseConnectionSettings configuration)
        {
            this.ConnectionSettings = configuration;
        }

        public BlockchainContext(DbContextOptions<BlockchainContext> options, IDatabaseConnectionSettings configuration)
            : base(options)
        {
            this.ConnectionSettings = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured == false)
            {
                optionsBuilder.UseSqlite(this.ConnectionSettings.ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BlocksConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<BlockRecord> Blocks { get; set; }
    }
}
