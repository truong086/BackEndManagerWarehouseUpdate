using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;

namespace quanlykhoupdate.Models
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options) { }

        #region DBSet
        public DbSet<plan> plan { get; set; }
        public DbSet<product> product { get; set; }
        public DbSet<location_addr> location_addr { get; set; }
        public DbSet<product_location> product_location { get; set; }
        public DbSet<usetokenapp> usetokenapp { get; set; }
        public DbSet<inbound> inbound { get; set; }
        public DbSet<inbound_product> inbound_product { get; set; }
        public DbSet<outbound> outbound { get; set; }
        public DbSet<outbound_product> outbound_product { get; set; }
        public DbSet<update_history> update_history { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<inbound>()
                .HasMany(c => c.inbound_Products)
                .WithOne(p => p.inbounds)
                .HasForeignKey(p => p.inbound_id)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<outbound>()
                .HasMany(c => c.outbound_Products)
                .WithOne(p => p.outbounds)
                .HasForeignKey(p => p.outbound_id)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<product>()
                .HasMany(c => c.outbound_Products)
                .WithOne(p => p.products)
                .HasForeignKey(p => p.product_id)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<product>()
                .HasMany(c => c.inbound_Products)
                .WithOne(p => p.products)
                .HasForeignKey(p => p.product_id)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<location_addr>()
                .HasMany(c => c.product_Locations)
                .WithOne(p => p.location_Addrs)
                .HasForeignKey(p => p.location_addr_id)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<product>()
               .HasMany(c => c.product_Locations)
               .WithOne(p => p.products)
               .HasForeignKey(p => p.product_id)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<plan>()
               .HasOne(m => m.location_Addr_New)
                .WithMany()
                .HasForeignKey(m => m.location_addr_id_new)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<plan>()
               .HasOne(m => m.location_Addr_Old)
                .WithMany()
                .HasForeignKey(m => m.location_addr_id_old)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<update_history>()
                .HasOne(uh => uh.products)
                .WithMany() 
                .HasForeignKey(uh => uh.product_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<update_history>()
                .HasOne(uh => uh.location_Addrs)
                .WithMany() 
                .HasForeignKey(uh => uh.location_addr_id)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
