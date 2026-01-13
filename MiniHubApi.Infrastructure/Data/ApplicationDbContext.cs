using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiniHubApi.Domain.Entities;

namespace MiniHubApi.Infrastructure.Data
{
    public class ApplicationDbContext: IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }
        
        public DbSet<Item> Items { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }

        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            
            base.OnModelCreating(builder);
            
            builder.Entity<Item>(entity =>
            {
                entity.ToTable("Items");
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Nome).HasColumnType("VARCHAR(200)").IsRequired();
                entity.Property(i => i.Descricao).HasColumnType("VARCHAR(500)");
                entity.Property(i => i.Preco).HasColumnType("DECIMAL(10,2)").IsRequired();
                entity.Property(i => i.Ativo).HasColumnType("TINYINT(1)").IsRequired().HasDefaultValue(true);
                
                entity.Property(i => i.CategoryId)
                    .IsRequired(false);
                
                entity.HasOne(i => i.Categoria)
                    .WithOne(c => c.Item)
                    .HasForeignKey<Item>(i => i.CategoryId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
                
                
                entity.HasMany(i => i.Tags)
                    .WithMany(t => t.Items)
                    .UsingEntity<Dictionary<string, object>>(
                        "ItemTags",
                        j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId"),
                        j => j.HasOne<Item>().WithMany().HasForeignKey("ItemId"),
                        j => 
                        {
                            j.HasKey("ItemId", "TagId");
                            j.ToTable("ItemTags");
                        }
                    );
                
            });

            builder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).HasColumnType("VARCHAR(200)").IsRequired();
                entity.HasIndex(c => c.Id)
                    .IsUnique();

            });
            
            builder.Entity<Tag>(entity =>
            {
                entity.ToTable("Tags");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Name).HasColumnType("VARCHAR(200)").IsRequired();
                entity.HasIndex(t => t.Name)
                    .IsUnique()
                    .HasDatabaseName("IX_Tags_Nome");

            });
            
        }
    }
}
