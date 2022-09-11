using Prodigy.BusinessLayer.Models;
using Prodigy.BusinessLayer.Models.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Prodigy.BusinessLayer.DatabaseConfigurations
{
    internal class BlocksConfiguration : IEntityTypeConfiguration<BlockRecord>
    {
        public void Configure(EntityTypeBuilder<BlockRecord> builder)
        {
            builder.ToTable("BlockRecords");
            builder.HasKey(x => x.id);
            builder.Property(x => x.id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(x => x.block_id).IsRequired();
            builder.Property(x => x.index).IsRequired();
        }
    }
}
