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
            
            
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FirstName).HasMaxLength(100);
                entity.Property(u => u.LastName).HasMaxLength(100);
                entity.Property(u => u.CreatedAt).IsRequired();
                entity.Property(u => u.IsActive).HasDefaultValue(true);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.IsActive);
                entity.HasIndex(u => u.CreatedAt);
            });
            
        }
        
        
         public async Task SeedBasicDataAsync()
        {
            if (!Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category 
                    { 
                        Id = 1,
                        Name = "Eletrônicos", 
                        ExternalId = "CAT-ELET-001" 
                    },
                    new Category 
                    { 
                        Id = 2,
                        Name = "Informática", 
                        ExternalId = "CAT-INFO-002" 
                    },
                    new Category 
                    { 
                        Id = 3,
                        Name = "Livros", 
                        ExternalId = "CAT-LIVR-003" 
                    },
                    new Category { 
                        Id = 4,
                        Name = "Roupas", 
                        ExternalId = "CAT-ROUP-004" 
                    },
                    new Category  { 
                        Id = 5,
                        Name = "Casa & Cozinha", 
                        ExternalId = "CAT-CASA-005" 
                    }
                };

                await Categories.AddRangeAsync(categories);
                await SaveChangesAsync();
            }
            
            if (!Tags.Any())
            {
                var tags = new List<Tag>
                {
                    new Tag 
                    { 
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                        Name = "Promoção" 
                    },
                    new Tag 
                    { 
                        Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                        Name = "Lançamento" 
                    },
                    new Tag 
                    { 
                        Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                        Name = "Mais Vendido" 
                    },
                    new Tag 
                    { 
                        Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                        Name = "Eco-friendly" 
                    }
                };

                await Tags.AddRangeAsync(tags);
                await SaveChangesAsync();
            }
            
            if (!Items.Any())
            {
                var eletronicaCategory = await Categories
                    .FirstOrDefaultAsync(c => c.ExternalId == "CAT-ELET-001");
                    
                var informaticaCategory = await Categories
                    .FirstOrDefaultAsync(c => c.ExternalId == "CAT-INFO-002");
                    
                var promocaoTag = await Tags
                    .FirstOrDefaultAsync(t => t.Name == "Promoção");
                    
                var lancamentoTag = await Tags
                    .FirstOrDefaultAsync(t => t.Name == "Lançamento");
                    
                var maisVendidoTag = await Tags
                    .FirstOrDefaultAsync(t => t.Name == "Mais Vendido");

                var items = new List<Item>
                {
                    new Item
                    {
                        Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                        Nome = "Smartphone Android 128GB",
                        Descricao = "Smartphone com câmera de 48MP e bateria de 5000mAh",
                        Preco = 1499.99m,
                        Ativo = true,
                        Estoque = 25,
                        ExternalId = "ITEM-SMARTPHONE-001",
                        CategoryId = eletronicaCategory?.Id,
                        CategoryExternalId = "CAT-ELET-001",
                    },
                    new Item
                    {
                        Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                        Nome = "Notebook Core i5 16GB",
                        Descricao = "Notebook para trabalho e estudos com SSD 512GB",
                        Preco = 3599.90m,
                        Ativo = true,
                        Estoque = 12,
                        ExternalId = "ITEM-NOTEBOOK-002",
                        CategoryId = informaticaCategory?.Id,
                        CategoryExternalId = "CAT-INFO-002",
                    },
                    new Item
                    {
                        Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                        Nome = "Livro Clean Code",
                        Descricao = "Livro sobre boas práticas de programação",
                        Preco = 89.90m,
                        Ativo = true,
                        Estoque = 50,
                        ExternalId = "ITEM-LIVRO-003",
                        CategoryId = (await Categories.FirstOrDefaultAsync(c => c.ExternalId == "CAT-LIVR-003"))?.Id,
                        CategoryExternalId = "CAT-LIVR-003",
                    }
                };
                
                if (promocaoTag != null && lancamentoTag != null)
                {
                    items[0].Tags = new List<Tag> { promocaoTag, lancamentoTag };
                }
                
                if (maisVendidoTag != null)
                {
                    items[1].Tags = new List<Tag> { maisVendidoTag };
                }

                await Items.AddRangeAsync(items);
                await SaveChangesAsync();
            }
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
