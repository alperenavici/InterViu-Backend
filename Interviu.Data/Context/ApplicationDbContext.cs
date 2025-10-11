using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Interviu.Entity.Entities;


namespace Interviu.Data.Context;

public class ApplicationDbContext:IdentityDbContext<ApplicationUser,IdentityRole,string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
    { }
    
    public DbSet<CV>CVs  { get; set; }
    public DbSet<Interview> Interviews { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<InterviewQuestion> InterviewQuestions { get; set; }

    protected override void OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<InterviewQuestion>()
            .HasIndex(iq => new { iq.InterviewId, iq.QuestionId })
            .IsUnique();
        
        modelBuilder.Entity<Interview>()
            .HasOne(i =>i.Cv)
            .WithMany(c =>c.Interviews)
            .HasForeignKey(i =>i.CvId)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.Cvs)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.Interviews)
            .WithOne(i => i.User)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<Interview>()
            .Property(i => i.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Question>()
            .Property(q => q.Difficulty)
            .HasConversion<string>();
    }
    
}