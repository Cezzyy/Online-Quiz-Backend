using Microsoft.EntityFrameworkCore;
using OnlineQuiz.Models;

namespace OnlineQuiz.Data
{
    public class OnlineQuizDbContext : DbContext
    {
        public OnlineQuizDbContext(DbContextOptions<OnlineQuizDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<UserModel> Users { get; set; } = null!;
        public DbSet<RoleModel> Roles { get; set; } = null!;
        public DbSet<UserRoleModel> UserRoles { get; set; } = null!;
        public DbSet<TeacherModel> Teachers { get; set; } = null!;
        public DbSet<StudentModel> Students { get; set; } = null!;
        public DbSet<CourseModel> Courses { get; set; } = null!;
        public DbSet<EnrollmentModel> Enrollments { get; set; } = null!;
        public DbSet<QuizModel> Quizzes { get; set; } = null!;
        public DbSet<QuestionModel> Questions { get; set; } = null!;
        public DbSet<ChoiceModel> Choices { get; set; } = null!;
        public DbSet<AttemptModel> Attempts { get; set; } = null!;
        public DbSet<AttemptAnswerModel> AttemptAnswers { get; set; } = null!;
        public DbSet<NotificationModel> Notifications { get; set; } = null!;
        public DbSet<ExportImportLogModel> ExportImportLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite key for UserRole
            modelBuilder.Entity<UserRoleModel>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // Configure relationships
            modelBuilder.Entity<UserRoleModel>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRoleModel>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure unique constraints
            modelBuilder.Entity<UserModel>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<RoleModel>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<StudentModel>()
                .HasIndex(s => s.StudentNumber)
                .IsUnique();

            modelBuilder.Entity<CourseModel>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<EnrollmentModel>()
                .HasIndex(e => new { e.UserId, e.CourseId })
                .IsUnique();

            // Configure decimal precision
            modelBuilder.Entity<QuestionModel>()
                .Property(q => q.Points)
                .HasPrecision(5, 2);

            modelBuilder.Entity<AttemptModel>()
                .Property(a => a.Score)
                .HasPrecision(10, 2);

            // Seed default roles
            modelBuilder.Entity<RoleModel>().HasData(
                new RoleModel { RoleId = 1, Name = "Admin" },
                new RoleModel { RoleId = 2, Name = "Teacher" },
                new RoleModel { RoleId = 3, Name = "Student" }
            );
        }
    }
}