using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiniHubApi.Domain.Entities;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

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
                
                entity.Property(i => i.Estoque)
                    .IsRequired()
                    .HasDefaultValue(0);
            
                entity.Property(i => i.ExternalId)
                    .HasColumnType("VARCHAR(50)")
                    .IsRequired(false);
            
                entity.Property(i => i.CategoryExternalId)
                    .HasColumnType("VARCHAR(50)")
                    .IsRequired(false);
                
                entity.Property(i => i.CategoryId)
                    .IsRequired(false);
                
                entity.HasIndex(i => i.ExternalId)
                    .IsUnique()
                    .HasDatabaseName("IX_Items_ExternalId");
            
                entity.HasIndex(i => i.CategoryExternalId)
                    .HasDatabaseName("IX_Items_CategoryExternalId");
                
                entity.HasOne(i => i.Categoria)
                    .WithMany(c => c.Items)  
                    .HasForeignKey(i => i.CategoryId)
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
                
                entity.Property(c => c.Id)
                    .ValueGeneratedOnAdd();
                
                entity.Property(c => c.Name).HasColumnType("VARCHAR(200)").IsRequired();
                
                entity.Property(c => c.ExternalId)
                    .HasColumnType("VARCHAR(50)")
                    .IsRequired(false);
            
                entity.HasIndex(c => c.ExternalId)
                    .IsUnique()
                    .HasDatabaseName("IX_Categories_ExternalId");
                
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = "Server=localhost;Port=3307;Database=MiniHubApi;Uid=myuser;Pwd=mypassword;";
                  
                optionsBuilder.UseMySql(
                    connectionString,
                    Microsoft.EntityFrameworkCore.ServerVersion.AutoDetect(connectionString),
                    mysqlOptions =>
                    {
                        mysqlOptions.MigrationsAssembly("MiniHubApi.Infrastructure");
                    });
            }
            base.OnConfiguring(optionsBuilder);
        }
    }
}
