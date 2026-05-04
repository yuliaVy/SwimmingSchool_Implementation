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
                    UserName = "theBestMangerEver@aqualife.com",
                    Email = "theBestMangerEver@aqualife.com",
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

                if (userManager.FindByName("theBestMangerEver@aqualife.com") == null)
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
                    userManager.Create(manager, "manager123");

                    //add the user to the role Admin
                    userManager.AddToRole(manager.Id, "Manger");
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
                    DateRegistered = new DateTime(2018, 9, 8)
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



            }// end of if statement checking if there are any users in the database
        }//end of Seed method
    }//end of the class
}//end of namespace