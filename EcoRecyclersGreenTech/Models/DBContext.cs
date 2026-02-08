using Microsoft.EntityFrameworkCore;
using EcoRecyclersGreenTech.Data.Orders;
using EcoRecyclersGreenTech.Data.Settings;
using EcoRecyclersGreenTech.Data.Stores;
using EcoRecyclersGreenTech.Data.Users;

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
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }

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
            // UserType 1 - M Users
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserType)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.UserTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            // User (1) -> PaymentCustomer (0..1)
            //modelBuilder.Entity<User>()
            //    .HasOne(u => u.PaymentCustomer)
            //    .WithOne(pc => pc.User)
            //    .HasForeignKey<PaymentCustomer>(pc => pc.UserId)
            //    .OnDelete(DeleteBehavior.Cascade);

            // User (1) -> Wallet (0..1)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Wallet)
                .WithOne(w => w.User)
                .HasForeignKey<Wallet>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique Wallet per User
            modelBuilder.Entity<Wallet>()
                .HasIndex(w => w.UserId)
                .IsUnique();

            modelBuilder.Entity<WalletTransaction>()
                .HasOne(t => t.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<PaymentTransaction>()
                .HasOne(p => p.User)
                .WithMany(u => u.PaymentTransactions)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentTransaction>()
                .HasIndex(p => new { p.Provider, p.ProviderPaymentId })
                .IsUnique();

            modelBuilder.Entity<WalletTransaction>()
            .HasIndex(t => new { t.WalletId, t.IdempotencyKey })
            .IsUnique();

            //modelBuilder.Entity<PaymentCustomer>()
            //    .HasIndex(pc => pc.UserId)
            //    .IsUnique();


            // Individual 1-1
            modelBuilder.Entity<Individual>()
                .HasIndex(x => x.UserID).IsUnique();

            modelBuilder.Entity<Individual>()
                .HasOne(x => x.User)
                .WithOne(u => u.IndividualProfile)
                .HasForeignKey<Individual>(x => x.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Factory 1-1
            modelBuilder.Entity<Factory>()
                .HasIndex(x => x.UserID).IsUnique();

            modelBuilder.Entity<Factory>()
                .HasOne(x => x.User)
                .WithOne(u => u.FactoryProfile)
                .HasForeignKey<Factory>(x => x.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Craftsman 1-1
            modelBuilder.Entity<Craftsman>()
                .HasIndex(x => x.UserID).IsUnique();

            modelBuilder.Entity<Craftsman>()
                .HasOne(x => x.User)
                .WithOne(u => u.CraftsmanProfile)
                .HasForeignKey<Craftsman>(x => x.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Admin 1-1
            modelBuilder.Entity<Admin>()
                .HasIndex(x => x.UserID).IsUnique();

            modelBuilder.Entity<Admin>()
                .HasOne(x => x.User)
                .WithOne(u => u.AdminProfile)
                .HasForeignKey<Admin>(x => x.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserType>().HasData(
                new UserType { TypeID = (int)UserTypeEnum.Individual, TypeName = "Individual" },
                new UserType { TypeID = (int)UserTypeEnum.Factory, TypeName = "Factory" },
                new UserType { TypeID = (int)UserTypeEnum.Craftsman, TypeName = "Craftsman" },
                new UserType { TypeID = (int)UserTypeEnum.Admin, TypeName = "Admin" }
            );

            // ===== RentalStore -> Owner(User) =====
            modelBuilder.Entity<RentalStore>()
                .HasOne(r => r.Owner)
                .WithMany()
                .HasForeignKey(r => r.OwnerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RentalOrder>()
                .HasIndex(x => new { x.PaymentProvider, x.PaymentProviderId })
                .IsUnique();

            // اختيارياً: تمنع أكتر من Confirmed على نفس Rental
            modelBuilder.Entity<RentalStore>()
                .HasIndex(x => x.ReservedForOrderId);

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