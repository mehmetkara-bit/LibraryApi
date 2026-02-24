using LibraryApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();
    public DbSet<BookCopy> BookCopies => Set<BookCopy>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<Fine> Fines => Set<Fine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Users
        modelBuilder.Entity<User>(b =>
        {
            b.HasIndex(x => x.Email).IsUnique();
            b.Property(x => x.Email).IsRequired();
            b.Property(x => x.PasswordHash).IsRequired();
        });

        // Member: shared PK (Id is PK + FK to Users.Id)
        modelBuilder.Entity<Member>(b =>
        {
            b.HasKey(x => x.Id);

            b.HasOne(x => x.User)
             .WithOne(x => x.Member)
             .HasForeignKey<Member>(x => x.Id)
             .OnDelete(DeleteBehavior.Cascade);

            b.Property(x => x.FirstName).IsRequired();
            b.Property(x => x.LastName).IsRequired();
        });

        // Staff: shared PK
        modelBuilder.Entity<Staff>(b =>
        {
            b.HasKey(x => x.Id);

            b.HasOne(x => x.User)
             .WithOne(x => x.Staff)
             .HasForeignKey<Staff>(x => x.Id)
             .OnDelete(DeleteBehavior.Cascade);

            b.Property(x => x.FirstName).IsRequired();
            b.Property(x => x.LastName).IsRequired();
        });

        // Categories
        modelBuilder.Entity<Category>(b =>
        {
            b.HasIndex(x => x.Name).IsUnique();
            b.Property(x => x.Name).IsRequired();
        });

        // Books
        modelBuilder.Entity<Book>(b =>
        {
            b.Property(x => x.Title).IsRequired();
            b.HasIndex(x => x.ISBN).IsUnique();

            b.HasOne(x => x.Category)
             .WithMany(x => x.Books)
             .HasForeignKey(x => x.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // BookAuthors: composite PK
        modelBuilder.Entity<BookAuthor>(b =>
        {
            b.HasKey(x => new { x.BookId, x.AuthorId });

            b.HasOne(x => x.Book)
             .WithMany(x => x.BookAuthors)
             .HasForeignKey(x => x.BookId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Author)
             .WithMany(x => x.BookAuthors)
             .HasForeignKey(x => x.AuthorId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // BookCopies
        modelBuilder.Entity<BookCopy>(b =>
        {
            b.Property(x => x.Barcode).IsRequired();
            b.HasIndex(x => x.Barcode).IsUnique();
            b.HasIndex(x => x.BookId);

            b.HasOne(x => x.Book)
             .WithMany(x => x.Copies)
             .HasForeignKey(x => x.BookId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Loans
        modelBuilder.Entity<Loan>(b =>
        {
            b.HasIndex(x => new { x.MemberId, x.LoanDate });
            b.HasIndex(x => x.BookCopyId);
            b.HasIndex(x => x.StaffId);

            // SQLite aktif-loan kuralı:
            // Aynı BookCopyId için ActiveMarker=1 yalnızca 1 kayıt olabilir.
            // ActiveMarker NULL olunca UNIQUE çakışmaz.
            b.HasIndex(x => new { x.BookCopyId, x.ActiveMarker }).IsUnique();

            // ActiveMarker: sadece NULL veya 1 olsun (CHECK)
            b.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Loans_ActiveMarker", "ActiveMarker IS NULL OR ActiveMarker = 1");
                t.HasCheckConstraint("CK_Loans_DueDate", "DueDate >= LoanDate");
            });

            b.HasOne(x => x.Member)
             .WithMany(x => x.Loans)
             .HasForeignKey(x => x.MemberId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.BookCopy)
             .WithMany(x => x.Loans)
             .HasForeignKey(x => x.BookCopyId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Staff)
             .WithMany(x => x.Loans)
             .HasForeignKey(x => x.StaffId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Fines (1 loan -> 0..1 fine)
        modelBuilder.Entity<Fine>(b =>
        {
            b.HasIndex(x => x.LoanId).IsUnique();

            b.HasOne(x => x.Loan)
             .WithOne(x => x.Fine)
             .HasForeignKey<Fine>(x => x.LoanId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}