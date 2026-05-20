namespace SwimmingSchool_Implementation.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Bookings",
                c => new
                    {
                        BookingId = c.Int(nullable: false, identity: true),
                        Status = c.Int(nullable: false),
                        IsDepositOnly = c.Boolean(nullable: false),
                        BookingDate = c.DateTime(nullable: false),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AmountPaid = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AdminNotes = c.String(maxLength: 250),
                        UserId = c.String(nullable: false, maxLength: 128),
                        StudentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.BookingId)
                .ForeignKey("dbo.Students", t => t.StudentId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.StudentId);
            
            CreateTable(
                "dbo.LessonBookings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BookingId = c.Int(nullable: false),
                        LessonId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Bookings", t => t.BookingId, cascadeDelete: true)
                .ForeignKey("dbo.Lessons", t => t.LessonId, cascadeDelete: true)
                .Index(t => t.BookingId)
                .Index(t => t.LessonId);
            
            CreateTable(
                "dbo.Lessons",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 100),
                        DayOfWeek = c.Int(nullable: false),
                        StartTime = c.Time(nullable: false, precision: 7),
                        DurationInMinutes = c.Int(nullable: false),
                        Capacity = c.Int(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AvailablePlaces = c.Int(nullable: false),
                        AgeGroup = c.String(),
                        LessonType = c.Int(nullable: false),
                        BlockStartDate = c.DateTime(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        VenueId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .ForeignKey("dbo.Venues", t => t.VenueId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.VenueId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(nullable: false, maxLength: 30),
                        SecondName = c.String(nullable: false, maxLength: 40),
                        AddressLine1 = c.String(),
                        AddressLine2 = c.String(),
                        City = c.String(),
                        Postcode = c.String(),
                        Country = c.String(),
                        DateOfBirth = c.DateTime(),
                        PhoneNumber = c.String(nullable: false),
                        DateRegistered = c.DateTime(),
                        ProfileImage = c.String(),
                        Bio = c.String(),
                        TeacherPreference = c.Int(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.TeacherVenues",
                c => new
                    {
                        TeacherVenueId = c.Int(nullable: false, identity: true),
                        TeacherId = c.String(nullable: false, maxLength: 128),
                        VenueId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TeacherVenueId)
                .ForeignKey("dbo.AspNetUsers", t => t.TeacherId, cascadeDelete: true)
                .ForeignKey("dbo.Venues", t => t.VenueId, cascadeDelete: true)
                .Index(t => t.TeacherId)
                .Index(t => t.VenueId);
            
            CreateTable(
                "dbo.Venues",
                c => new
                    {
                        VenueId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Address = c.String(),
                        PoolInfo = c.String(),
                        Amenities = c.String(),
                        ImageName = c.String(),
                    })
                .PrimaryKey(t => t.VenueId);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentMethod = c.String(),
                        TransactionId = c.String(),
                        PaymentDate = c.DateTime(nullable: false),
                        Success = c.Boolean(nullable: false),
                        BookingId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Bookings", t => t.BookingId, cascadeDelete: true)
                .Index(t => t.BookingId);
            
            CreateTable(
                "dbo.PolicyAgreements",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Accepted = c.Boolean(nullable: false),
                        AgreementDate = c.DateTime(nullable: false),
                        PolicyId = c.Int(nullable: false),
                        BookingId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Bookings", t => t.BookingId, cascadeDelete: true)
                .ForeignKey("dbo.Policies", t => t.PolicyId, cascadeDelete: true)
                .Index(t => t.PolicyId)
                .Index(t => t.BookingId);
            
            CreateTable(
                "dbo.Policies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                        Description = c.String(nullable: false),
                        IsRequired = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Students",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, maxLength: 30),
                        SecondName = c.String(nullable: false, maxLength: 40),
                        Gender = c.String(nullable: false),
                        DateOfBirth = c.DateTime(nullable: false),
                        MedicalConditions = c.String(),
                        Allergies = c.String(),
                        Medications = c.String(),
                        AquaticGoals = c.String(),
                        SwimExperience = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BookingSessions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BookingId = c.Int(nullable: false),
                        SessionDate = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        TeacherNotes = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Bookings", t => t.BookingId, cascadeDelete: true)
                .Index(t => t.BookingId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.BookingSessions", "BookingId", "dbo.Bookings");
            DropForeignKey("dbo.Bookings", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Bookings", "StudentId", "dbo.Students");
            DropForeignKey("dbo.PolicyAgreements", "PolicyId", "dbo.Policies");
            DropForeignKey("dbo.PolicyAgreements", "BookingId", "dbo.Bookings");
            DropForeignKey("dbo.Payments", "BookingId", "dbo.Bookings");
            DropForeignKey("dbo.LessonBookings", "LessonId", "dbo.Lessons");
            DropForeignKey("dbo.Lessons", "VenueId", "dbo.Venues");
            DropForeignKey("dbo.Lessons", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.TeacherVenues", "VenueId", "dbo.Venues");
            DropForeignKey("dbo.TeacherVenues", "TeacherId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.LessonBookings", "BookingId", "dbo.Bookings");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.BookingSessions", new[] { "BookingId" });
            DropIndex("dbo.PolicyAgreements", new[] { "BookingId" });
            DropIndex("dbo.PolicyAgreements", new[] { "PolicyId" });
            DropIndex("dbo.Payments", new[] { "BookingId" });
            DropIndex("dbo.TeacherVenues", new[] { "VenueId" });
            DropIndex("dbo.TeacherVenues", new[] { "TeacherId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Lessons", new[] { "VenueId" });
            DropIndex("dbo.Lessons", new[] { "UserId" });
            DropIndex("dbo.LessonBookings", new[] { "LessonId" });
            DropIndex("dbo.LessonBookings", new[] { "BookingId" });
            DropIndex("dbo.Bookings", new[] { "StudentId" });
            DropIndex("dbo.Bookings", new[] { "UserId" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.BookingSessions");
            DropTable("dbo.Students");
            DropTable("dbo.Policies");
            DropTable("dbo.PolicyAgreements");
            DropTable("dbo.Payments");
            DropTable("dbo.Venues");
            DropTable("dbo.TeacherVenues");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Lessons");
            DropTable("dbo.LessonBookings");
            DropTable("dbo.Bookings");
        }
    }
}
