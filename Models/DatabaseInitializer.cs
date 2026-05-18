using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace SwimmingSchool_Implementation.Models
{
    public class DatabaseInitializer : DropCreateDatabaseAlways<SwimSchoolDbContext>
    {
        protected override void Seed(SwimSchoolDbContext context)
        {
            if (!context.Users.Any())
            {
                //create a few roles and store them in dateabase

                //create a roleManeger object will allow us to create roles and store them in the database
                RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

                //if the Admin role doesn't exist
                if (!roleManager.RoleExists("Manager"))
                {
                    //create an Admin role
                    roleManager.Create(new IdentityRole("Manager"));
                }

                //if the Member role doesn't exist
                if (!roleManager.RoleExists("Learner"))
                {
                    //create a Member role
                    roleManager.Create(new IdentityRole("Learner"));
                }

                //if the Member role doesn't exist
                if (!roleManager.RoleExists("Teacher"))
                {
                    //create a Member role
                    roleManager.Create(new IdentityRole("Teacher"));
                }
                //save new roles to the database
                context.SaveChanges();


                //********************************************************
                //create some users now and assign them to different toles
                //********************************************************

                //the userManager object allows creating users and store them in the database
                UserManager<User> userManager = new UserManager<User>(new UserStore<User>(context));



                //create a user Manager
                var manager = new User()
                {
                    UserName = "manager@aqualife.com",
                    Email = "manager@aqualife.com",
                    FirstName = "Jim",
                    SecondName = "Smith",
                    AddressLine1 = "56 High Street",
                    City = "Glasgow",
                    Postcode = "G1 67AD",
                    Country = "UK",
                    EmailConfirmed = true,
                    DateOfBirth = new DateTime(2000, 5, 15),
                    PhoneNumber = "07799112233",
                    DateRegistered = new DateTime(2018, 9, 8)
                };
                //if users with theBestMangerEver@aqualife.com username doesn't exist then ->)

                if (userManager.FindByName("manager@aqualife.com") == null)
                {
                    //super relaxed password validator
                    userManager.PasswordValidator = new PasswordValidator()
                    {
                        RequireDigit = false,
                        RequiredLength = 1,
                        RequireLowercase = false,
                        RequireNonLetterOrDigit = false,
                        RequireUppercase = false
                    };

                    //add the hashed password to user
                   

                    //check for any errors in the creation process and display them in the output window
                    var result = userManager.Create(manager, "manager123");
                    if (!result.Succeeded)
                    {
                        // SEE REAL ERRORS
                        foreach (var err in result.Errors)
                        {
                            Debug.WriteLine(err);
                        }
                        return;
                    }

                    //add the user to the role Admin
                    userManager.AddToRole(manager.Id, "Manager");
                }

                //create a teacher user
                var teacher = new User()
                {
                    UserName = "teacher@aqualife.com",
                    Email = "teacher@aqualife.com",
                    FirstName = "Paul",
                    SecondName = "Goat",
                    AddressLine1 = "5 Meerry Street",
                    AddressLine2 = "Flat 6",
                    City = "Glasgow",
                    Postcode = "G5 7AD",
                    Country = "UK",
                    EmailConfirmed = true,
                    DateOfBirth = new DateTime(1980, 5, 15),
                    PhoneNumber = "02244772233",
                    ProfileImage = "PaulTeacher.jpg",
                    Bio = "Paul has been teaching swimming for over 15 years and has a passion for helping children learn to swim. He is patient, encouraging, and always creates a fun and supportive learning environment for his students.",
                    DateRegistered = new DateTime(2018, 9, 8),
                    TeacherPreference = TeacherPreference.Both
                };

                if (userManager.FindByName("teacher@aqualife.com") == null)
                {
                    //super relaxed password validator
                    userManager.PasswordValidator = new PasswordValidator()
                    {
                        RequireDigit = false,
                        RequiredLength = 1,
                        RequireLowercase = false,
                        RequireNonLetterOrDigit = false,
                        RequireUppercase = false
                    };
                    //add the hashed password to user
                    userManager.Create(teacher, "password1");
                    //add the user to the role "Staff"
                    userManager.AddToRole(teacher.Id, "Teacher");
                }

                //save changes to the database
                context.SaveChanges();

                //create a learner 
                var learner = new User()
                {
                    UserName = "learner@aqualife.com",
                    Email = "learner@aqualife.com",
                    FirstName = "Luigi",
                    SecondName = "Monk",
                    AddressLine1 = "34 Confused Street",
                    City = "Edinburg",
                    Postcode = "8P9 7Y8",
                    Country = "UK",
                    EmailConfirmed = true,
                    DateOfBirth = new DateTime(2005, 5, 15),
                    PhoneNumber = "02244772233",
                    DateRegistered = new DateTime(2018, 9, 8)
                };
                if (userManager.FindByName("member@aqualife.com") == null)
                {
                    //super relaxed password validator
                    userManager.PasswordValidator = new PasswordValidator()
                    {
                        RequireDigit = false,
                        RequiredLength = 1,
                        RequireLowercase = false,
                        RequireNonLetterOrDigit = false,
                        RequireUppercase = false
                    };

                    
                    userManager.Create(learner, "Password1!");

                    //check for any errors in the creation process and display them in the output window
                    //var result =
                    //if (!result.Succeeded)
                    //{
                    //    // SEE REAL ERRORS
                    //    foreach (var err in result.Errors)
                    //    {
                    //        Debug.WriteLine(err);
                    //    }
                    //    return;
                    //}

                    userManager.AddToRole(learner.Id, "Learner");
                }
                //save changes to the database
                context.SaveChanges();
                //create another learner
                var learner1 = new User()
                {
                    UserName = "learner1@aqualife.com",
                    Email = "learner1@aqualife.com",
                    FirstName = "Monica",
                    SecondName = "Beluchi",
                    AddressLine1 = "3 Halfords Street",
                    City = "Aberdeen",
                    Postcode = "9IH 78Y",
                    Country = "UK",
                    EmailConfirmed = true,
                    DateOfBirth = new DateTime(2003, 5, 15),
                    PhoneNumber = "02244772233",
                    DateRegistered = new DateTime(2018, 9, 8)
                };

                if (userManager.FindByName("learner1@aqualife.com") == null)
                {
                    //super relaxed password validator
                    userManager.PasswordValidator = new PasswordValidator()
                    {
                        RequireDigit = false,
                        RequiredLength = 1,
                        RequireLowercase = false,
                        RequireNonLetterOrDigit = false,
                        RequireUppercase = false
                    };
                    userManager.Create(learner1, "Password1!");

                    //check for any errors in the creation process and display them in the output window
                    //var result =
                    //if (!result.Succeeded)
                    //{
                    //    // SEE REAL ERRORS
                    //    foreach (var err in result.Errors)
                    //    {
                    //        Debug.WriteLine(err);
                    //    }
                    //    return;
                    //}

                    userManager.AddToRole(learner1.Id, "Learner");

                }

                //save changes to the database
                context.SaveChanges();

                //*******************************
                //seeding Students table
                //*******************************
                var student1 = new Student()
                {
                    FirstName = "Charlie",
                    SecondName = "Smith",
                    Gender = "male",
                    DateOfBirth = new DateTime(2010, 5, 15),
                    AquaticGoals = "Be able to float on their own"
                };
                context.Students.Add(student1);

                var student2 = new Student()
                {
                    FirstName = "Sophie",
                    SecondName = "Smith",
                    Gender = "female",
                    DateOfBirth = new DateTime(2012, 5, 15),
                    AquaticGoals = "Be able to swim 10 meters unaided"
                };
                context.Students.Add(student2);

                //save changes to students in the database
                context.SaveChanges();

                //**********************************
                //seeding the Venues table
                //**********************************

                var westEnd = new Venue
                {
                    Name = "West End Baths",
                    Address = "West Glasgow",
                    PoolInfo = "16m heated teaching pool",
                    Amenities = "Changing rooms, Wi-Fi, Showers",
                    ImageName = "WestEndBaths.jpg"
                };

                var southside = new Venue
                {
                    Name = "Southside Hub",
                    Address = "South Glasgow",
                    PoolInfo = "25m competition pool",
                    Amenities = "Cafe, Lockers, Viewing Area",
                    ImageName = "SouthSideHub.jpg"
                };

                var eastEnd = new Venue
                {
                    Name = "East End Village",
                    Address = "East Glasgow",
                    PoolInfo = "Family teaching pool",
                    Amenities = "Changing rooms, Parking",
                    ImageName = "EastEndVillage.jpg"
                };

                context.Venues.Add(westEnd);
                context.Venues.Add(southside);
                context.Venues.Add(eastEnd);

                context.SaveChanges();

                ////**********************************
                //seeding the TeachersVenue table
                //**********************************
                var teacherVenue1 = new TeacherVenue()
                {
                    TeacherId = teacher.Id,
                    VenueId = westEnd.VenueId
                };
                context.TeacherVenues.Add(teacherVenue1);
                var teacherVenue2 = new TeacherVenue()
                {
                    TeacherId = teacher.Id,
                    VenueId = southside.VenueId
                };
                context.TeacherVenues.Add(teacherVenue2);

                //**********************************
                //seeding the Lessons table
                //**********************************

                try
                {
                    var lesson1 = new Lesson()
                    {
                        Title = "Beginner Backstroke",
                        DayOfWeek = DayOfWeek.Tuesday,
                        StartTime = new TimeSpan(9, 0, 0),
                        DurationInMinutes = 30,
                        Capacity = 6,
                        AvailablePlaces = 2,
                        Price = 20.00m,
                        AgeGroup = "8-12",
                        VenueId = westEnd.VenueId,
                        LessonType = LessonType.Kids,
                        UserId = teacher.Id
                    };

                    context.Lessons.Add(lesson1);

                    context.SaveChanges();

                    System.Diagnostics.Debug.WriteLine("LESSON SAVED");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());

                    throw;
                }

                var lesson2 = new Lesson()
                {
                    Title = "Beginner FrontCrawl",
                    DayOfWeek = DayOfWeek.Monday,
                    StartTime = new TimeSpan(10, 0, 0), // 10:00 AM
                    DurationInMinutes = 30,
                    Capacity = 1,
                    AgeGroup = "8-12",
                    AvailablePlaces = 1,
                    VenueId = southside.VenueId,
                    LessonType = LessonType.Adult,
                    Price = 20.00m,
                    UserId = teacher.Id
                };
                context.Lessons.Add(lesson2);
                //save the changes to the database
                context.SaveChanges();

                //*******************************
                //seeding the Policies table
                //*******************************
                var policy1 = new Policy()
                {
                    Title = "Cancellation Policy",
                    IsRequired = true,
                    Description = "Cancellations must be made at least 48 hours in advance to receive a full refund. Cancellations made within 24 hours of the lesson will not be refunded."
                };
                context.Policies.Add(policy1);
                //save the changes to the database
                context.SaveChanges();

                //*******************************
                //seeding the bookings table
                //*******************************

                var booking1 = new Booking()
                {
                    Status = BookingStatus.Confirmed,
                    IsDepositOnly = false,
                    BookingDate = DateTime.Now.AddDays(-1),
                    TotalAmount = lesson2.Price,
                    AmountPaid = lesson2.Price,
                    User = learner,
                    Student = student1,
                };
                context.Bookings.Add(booking1);

                var booking2 = new Booking()
                {
                    User = learner1,
                    Status = BookingStatus.Pending,
                    IsDepositOnly = true,
                    BookingDate = DateTime.Now,
                    TotalAmount = lesson2.Price,
                    AmountPaid = (lesson2.Price * 20) / 100, // 20% deposit
                    AdminNotes = "Waiting on final payment.",
                    Student = student2
                };
                context.Bookings.Add(booking2);

                //save the changes to the database
                context.SaveChanges();

                //*******************************
                //seeding the PolicyAgreemnents table
                //*******************************

                var policyAgreement1 = new PolicyAgreement()
                {
                    Booking = booking1,
                    Policy = policy1,
                    AgreementDate = DateTime.Now,
                    Accepted = true
                };
                context.PolicyAgreements.Add(policyAgreement1);

                var policyAgreement2 = new PolicyAgreement()
                {
                    Booking = booking2,
                    Policy = policy1,
                    AgreementDate = DateTime.Now,
                    Accepted = true
                };


                //*******************************
                //seeding lessonBooking table
                //*******************************
                var lessonBooking1 = new LessonBooking()
                {
                    Booking = booking1,
                    Lesson = lesson2
                };
                context.LessonsBookings.Add(lessonBooking1);

                var lessonBooking2 = new LessonBooking()
                {
                    Booking = booking1,
                    Lesson = lesson2
                };
                context.LessonsBookings.Add(lessonBooking2);

                var lessonBooking3 = new LessonBooking()
                {
                    Booking = booking2,
                    Lesson = lesson2
                };
                context.LessonsBookings.Add(lessonBooking3);

                //save the changes to the database
                context.SaveChanges();

            }// end of if statement checking if there are any users in the database
        }//end of Seed method
    }//end of the class
}//end of namespace