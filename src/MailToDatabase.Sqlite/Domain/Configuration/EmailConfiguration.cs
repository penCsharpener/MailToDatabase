using MailToDatabase.Sqlite.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MailToDatabase.Sqlite.Domain.Configuration
{
    public class EmailConfiguration : IEntityTypeConfiguration<Email>
    {
        public void Configure(EntityTypeBuilder<Email> builder)
        {
            builder.HasIndex(x => x.Sha256).IsUnique();
        }
    }
}
