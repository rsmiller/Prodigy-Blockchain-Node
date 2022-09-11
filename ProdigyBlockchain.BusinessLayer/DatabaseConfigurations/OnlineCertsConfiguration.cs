using Prodigy.BusinessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Prodigy.BusinessLayer.DatabaseConfigurations
{
    internal class OnlineCertsConfiguration : IEntityTypeConfiguration<Certificate>
    {
        public void Configure(EntityTypeBuilder<Certificate> builder)
        {
            builder.ToTable("online_certs");
            builder.HasKey(x => x.id);
        }
    }
}
