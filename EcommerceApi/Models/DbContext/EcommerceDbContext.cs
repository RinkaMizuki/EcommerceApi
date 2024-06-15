using EcommerceApi.Models.Chat;
using MessageModel = EcommerceApi.Models.Chat.Message;
using EcommerceApi.Models.Coupon;
using EcommerceApi.Models.Feedback;
using EcommerceApi.Models.Order;
using EcommerceApi.Models.Payment;
using EcommerceApi.Models.Product;
using EcommerceApi.Models.Segment;
using EcommerceApi.Models.UserAddress;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Models;

public class EcommerceDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Order.Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Contact.Contact> Contacts { get; set; }
    public DbSet<Coupon.Coupon> Coupons { get; set; }
    public DbSet<CouponCondition> CouponConditions { get; set; }
    public DbSet<Condition> Conditions { get; set; }
    public DbSet<Product.Product> Products { get; set; }
    public DbSet<ProductStock> ProductStocks { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<ProductColor> ProductColors { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Rate.Rate> Rates { get; set; }
    public DbSet<FeedbackRate> Feedbacks { get; set; }
    public DbSet<UserSegment> UserSegments { get; set; }
    public DbSet<Segment.Segment> Segments { get; set; }
    public DbSet<Slider.Slider> Sliders { get; set; }
    public DbSet<UserAddress.UserAddress> UserAddresses { get; set; }
    public DbSet<Merchant> Merchants { get; set; }
    public DbSet<Payment.Payment> Payments { get; set; }
    public DbSet<PaymentDestination> PaymentDestinations { get; set; }
    public DbSet<PaymentNotification> PaymentNotifications { get; set; }
    public DbSet<PaymentSignature> PaymentSignatures { get; set; }
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    public DbSet<MessageModel> Messages { get; set; }
    public DbSet<Participation> Participations { get; set; }
    public DbSet<Conversation> Conversations { get; set; }

    public EcommerceDbContext(DbContextOptions<EcommerceDbContext> dbContextOptions) : base(dbContextOptions)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>(entity => { entity.HasIndex(u => u.UserId); });
        modelBuilder.Entity<OrderDetail>().HasKey(od => new { od.OrderId, od.ProductId });
        modelBuilder.Entity<UserSegment>().HasKey(us => new { us.SegmentId, us.UserId });
        modelBuilder.Entity<CouponCondition>().HasKey(cc => new { cc.ConditionId, cc.CouponId });
        modelBuilder.Entity<Participation>().HasKey(pp => new { pp.UserId, pp.ConversationId });
    }
}