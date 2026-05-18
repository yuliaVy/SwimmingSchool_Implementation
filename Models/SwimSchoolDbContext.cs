using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace SwimmingSchool_Implementation.Models
{
    public class SwimSchoolDbContext : IdentityDbContext<User>
    {
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<LessonBooking> LessonsBookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<PolicyAgreement> PolicyAgreements { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<TeacherVenue> TeacherVenues{ get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // call the base method first so ASP.NET Identity doesn't break
            base.OnModelCreating(modelBuilder);

            // Tell EF to turn OFF Cascade Delete for the Teacher -> Lesson relationship
            modelBuilder.Entity<Lesson>()
                .HasRequired(l => l.Teacher)
                .WithMany(u => u.TaughtLessons) // Make sure this matches the list name in your User class!
                .HasForeignKey(l => l.UserId)
                .WillCascadeOnDelete(false);
        }

        public SwimSchoolDbContext() : base("SwimSchoolConnection", throwIfV1Schema: false)
        {
            Database.SetInitializer(new DatabaseInitializer());
        }

        public static SwimSchoolDbContext Create()
        {
            return new SwimSchoolDbContext();
        }
    }
}