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
        public DbSet<RefreshTokenModel> RefreshTokens { get; set; } = null!;
        public DbSet<InstructorModel> Instructors { get; set; } = null!;
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
        public DbSet<ActivityLogModel> ActivityLogs { get; set; } = null!;

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
                .OnDelete(DeleteBehavior.Restrict);

            // Configure other relationships to prevent cascade conflicts
            modelBuilder.Entity<EnrollmentModel>()
                .HasOne(e => e.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AttemptModel>()
                .HasOne(a => a.User)
                .WithMany(u => u.Attempts)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NotificationModel>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure one-to-one relationships
            modelBuilder.Entity<InstructorModel>()
                .HasOne(t => t.User)
                .WithOne(u => u.Instructor)
                .HasForeignKey<InstructorModel>(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentModel>()
                .HasOne(s => s.User)
                .WithOne(u => u.Student)
                .HasForeignKey<StudentModel>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure other relationships to prevent cascade conflicts
            modelBuilder.Entity<ExportImportLogModel>()
                .HasOne(e => e.User)
                .WithMany(u => u.ExportImportLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityLogModel>()
                .HasOne(a => a.User)
                .WithMany(u => u.ActivityLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Course-Teacher relationship
            modelBuilder.Entity<CourseModel>()
                .HasOne(c => c.Instructor)
                .WithMany(t => t.Courses)
                .HasForeignKey(c => c.InstructorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Course-Creator relationship
            modelBuilder.Entity<CourseModel>()
                .HasOne(c => c.Creator)
                .WithMany()
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Enrollment-Course relationship
            modelBuilder.Entity<EnrollmentModel>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Quiz relationships
            modelBuilder.Entity<QuizModel>()
                .HasOne(q => q.Course)
                .WithMany(c => c.Quizzes)
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Question relationships
            modelBuilder.Entity<QuestionModel>()
                .HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Choice relationships
            modelBuilder.Entity<ChoiceModel>()
                .HasOne(c => c.Question)
                .WithMany(q => q.Choices)
                .HasForeignKey(c => c.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Attempt relationships
            modelBuilder.Entity<AttemptModel>()
                .HasOne(a => a.Quiz)
                .WithMany(q => q.Attempts)
                .HasForeignKey(a => a.QuizId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure AttemptAnswer relationships
            modelBuilder.Entity<AttemptAnswerModel>()
                .HasOne(aa => aa.Attempt)
                .WithMany(a => a.AttemptAnswers)
                .HasForeignKey(aa => aa.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AttemptAnswerModel>()
                .HasOne(aa => aa.Question)
                .WithMany(q => q.AttemptAnswers)
                .HasForeignKey(aa => aa.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AttemptAnswerModel>()
                .HasOne(aa => aa.Choice)
                .WithMany(c => c.AttemptAnswers)
                .HasForeignKey(aa => aa.ChoiceId)
                .OnDelete(DeleteBehavior.Restrict);

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
                new RoleModel { RoleId = 2, Name = "Instructor" },
                new RoleModel { RoleId = 3, Name = "Student" }
            );

            // Add check constraints using the new syntax
            modelBuilder.Entity<StudentModel>()
                .ToTable(t => t.HasCheckConstraint("CK_Students_EnrollmentStatus", 
                    "EnrollmentStatus IN ('Active','OnLeave','Graduated','Withdrawn','Suspended')"));

            modelBuilder.Entity<EnrollmentModel>()
                .ToTable(t => t.HasCheckConstraint("CK_Enrollments_Status", 
                    "Status IN ('Active','Dropped','Completed','Withdrawn')"));

            modelBuilder.Entity<AttemptModel>()
                .ToTable(t => t.HasCheckConstraint("CK_Attempts_Status", 
                    "Status IN ('InProgress','Submitted','Abandoned','Graded')"));

            // Add indexes for performance
            modelBuilder.Entity<UserModel>()
                .HasIndex(u => u.Status);
            
            modelBuilder.Entity<UserModel>()
                .HasIndex(u => u.LastLoginAt);
            
            modelBuilder.Entity<UserModel>()
                .HasIndex(u => u.IsDeleted);

            modelBuilder.Entity<InstructorModel>()
                .HasIndex(i => i.Department);

            modelBuilder.Entity<StudentModel>()
                .HasIndex(s => s.EnrollmentStatus);
            
            modelBuilder.Entity<StudentModel>()
                .HasIndex(s => s.Program);
            
            modelBuilder.Entity<StudentModel>()
                .HasIndex(s => s.YearLevel);

            modelBuilder.Entity<CourseModel>()
                .HasIndex(c => c.Status);
            
            modelBuilder.Entity<CourseModel>()
                .HasIndex(c => c.Semester);
            
            modelBuilder.Entity<CourseModel>()
                .HasIndex(c => c.AcademicYear);
            
            modelBuilder.Entity<CourseModel>()
                .HasIndex(c => c.IsPublished);
            
            modelBuilder.Entity<CourseModel>()
                .HasIndex(c => c.IsDeleted);
            
            modelBuilder.Entity<CourseModel>()
                .HasIndex(c => new { c.StartDate, c.EndDate });

            modelBuilder.Entity<EnrollmentModel>()
                .HasIndex(e => e.Status);
            
            modelBuilder.Entity<EnrollmentModel>()
                .HasIndex(e => e.EnrolledAt);

            modelBuilder.Entity<QuizModel>()
                .HasIndex(q => q.IsPublished);
            
            modelBuilder.Entity<QuizModel>()
                .HasIndex(q => q.DueAt);
            
            modelBuilder.Entity<QuizModel>()
                .HasIndex(q => new { q.AvailableFrom, q.AvailableUntil });
            
            modelBuilder.Entity<QuizModel>()
                .HasIndex(q => q.IsDeleted);

            modelBuilder.Entity<QuestionModel>()
                .HasIndex(q => q.Type);
            
            modelBuilder.Entity<QuestionModel>()
                .HasIndex(q => new { q.QuizId, q.SortOrder });

            modelBuilder.Entity<AttemptModel>()
                .HasIndex(a => a.Status);
            
            modelBuilder.Entity<AttemptModel>()
                .HasIndex(a => a.SubmittedAt);
            
            modelBuilder.Entity<AttemptModel>()
                .HasIndex(a => a.FlaggedForReview);

            modelBuilder.Entity<NotificationModel>()
                .HasIndex(n => n.Priority);
            
            modelBuilder.Entity<NotificationModel>()
                .HasIndex(n => n.IsArchived);
            
            modelBuilder.Entity<NotificationModel>()
                .HasIndex(n => n.ExpiresAt);

            modelBuilder.Entity<ActivityLogModel>()
                .HasIndex(a => new { a.Action, a.Entity });
            
            modelBuilder.Entity<ActivityLogModel>()
                .HasIndex(a => a.CreatedAt);
            
            modelBuilder.Entity<ActivityLogModel>()
                .HasIndex(a => a.Severity);

            modelBuilder.Entity<RefreshTokenModel>()
                .HasIndex(r => r.ExpiresAt);
            
            modelBuilder.Entity<RefreshTokenModel>()
                .HasIndex(r => r.RevokedAt);

            modelBuilder.Entity<ExportImportLogModel>()
                .HasIndex(e => e.Status);
        }
    }
}