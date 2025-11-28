using Microsoft.EntityFrameworkCore;
using EcoRecyclersGreenTech.Data.Orders;
using EcoRecyclersGreenTech.Data.Settings;
using EcoRecyclersGreenTech.Data.Stores;
using EcoRecyclersGreenTech.Data.Users;
using EcoRecyclersGreenTech.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EcoRecyclersGreenTech.Models
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {

        }

        // ======= Users & Types =======
        public DbSet<User> Users { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<Factory> Factories { get; set; }
        public DbSet<Individual> Individuals { get; set; }
        public DbSet<Craftsman> Craftsmen { get; set; }
        public DbSet<Admin> Admins { get; set; }

        // ======= Stores =======
        public DbSet<RentalStore> RentalStores { get; set; }
        public DbSet<AuctionStore> AuctionStores { get; set; }
        public DbSet<MaterialStore> MaterialStores { get; set; }
        public DbSet<JobStore> JobStores { get; set; }
        public DbSet<MachineStore> MachineStores { get; set; }

        // ======= Orders =======
        public DbSet<MaterialOrder> MaterialOrders { get; set; }
        public DbSet<MachineOrder> MachineOrders { get; set; }
        public DbSet<RentalOrder> RentalOrders { get; set; }
        public DbSet<AuctionOrder> AuctionOrders { get; set; }
        public DbSet<JobOrder> JobOrders { get; set; }

        // ======= Chat & Feedback =======
        public DbSet<ChatUser> ChatUsers { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Complaint> Complaints { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== UserType 1 - M Users =====
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserType)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.UserTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserType>()
                .Property(u => u.RealTypeID)
                .IsRequired(false);

            // ===== RentalStore -> Owner(User) =====
            modelBuilder.Entity<RentalStore>()
                .HasOne(r => r.Owner)
                .WithMany()
                .HasForeignKey(r => r.OwnerID)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== AuctionStore -> Seller(User) =====
            modelBuilder.Entity<AuctionStore>()
                .HasOne(a => a.Seller)
                .WithMany()
                .HasForeignKey(a => a.SellerID)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== MaterialStore -> Seller(User) =====
            modelBuilder.Entity<MaterialStore>()
                .HasOne(m => m.Seller)
                .WithMany()
                .HasForeignKey(m => m.SellerID)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== MachineStore -> Seller(User) =====
            modelBuilder.Entity<MachineStore>()
                .HasOne(m => m.Seller)
                .WithMany()
                .HasForeignKey(m => m.SellerID)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== JobStore -> PostedBy(User) =====
            modelBuilder.Entity<JobStore>()
                .HasOne(j => j.User)
                .WithMany()
                .HasForeignKey(j => j.PostedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== Orders Relations =====

            // MaterialOrder -> MaterialStore + Buyer(User)
            modelBuilder.Entity<MaterialOrder>()
                .HasOne(o => o.MaterialStore)
                .WithMany()
                .HasForeignKey(o => o.MaterialStoreID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaterialOrder>()
                .HasOne(o => o.Buyer)
                .WithMany()
                .HasForeignKey(o => o.BuyerID)
                .OnDelete(DeleteBehavior.Restrict);

            // MachineOrder -> MachineStore + Buyer(User)
            modelBuilder.Entity<MachineOrder>()
                .HasOne(o => o.MachineStore)
                .WithMany()
                .HasForeignKey(o => o.MachineStoreID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MachineOrder>()
                .HasOne(o => o.Buyer)
                .WithMany()
                .HasForeignKey(o => o.BuyerID)
                .OnDelete(DeleteBehavior.Restrict);

            // RentalOrder -> RentalStore + Buyer(User)
            modelBuilder.Entity<RentalOrder>()
                .HasOne(o => o.RentalStore)
                .WithMany()
                .HasForeignKey(o => o.RentalStoreID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RentalOrder>()
                .HasOne(o => o.Buyer)
                .WithMany()
                .HasForeignKey(o => o.BuyerID)
                .OnDelete(DeleteBehavior.Restrict);

            // AuctionOrder -> AuctionStore + Winner(User)
            modelBuilder.Entity<AuctionOrder>()
                .HasOne(o => o.AuctionStore)
                .WithMany()
                .HasForeignKey(o => o.AuctionStoreID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AuctionOrder>()
                .HasOne(o => o.Winner)
                .WithMany()
                .HasForeignKey(o => o.WinnerID)
                .OnDelete(DeleteBehavior.Restrict);

            // JobOrder -> JobStore + User
            modelBuilder.Entity<JobOrder>()
                .HasOne(o => o.JobStore)
                .WithMany()
                .HasForeignKey(o => o.JobStoreID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JobOrder>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== ChatUser (From, To) =====
            modelBuilder.Entity<ChatUser>()
                .HasOne(c => c.FromUser)
                .WithMany()
                .HasForeignKey(c => c.From)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatUser>()
                .HasOne(c => c.ToUser)
                .WithMany()
                .HasForeignKey(c => c.To)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== Feedback (From, To) =====
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.FromUser)
                .WithMany()
                .HasForeignKey(f => f.From)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.ToUser)
                .WithMany()
                .HasForeignKey(f => f.To)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== Complaint -> From User =====
            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.FromUser)
                .WithMany()
                .HasForeignKey(c => c.From)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}