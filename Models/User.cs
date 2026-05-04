using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SwimmingSchool_Implementation.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class User : IdentityUser
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Use valid symbols in your First Name")]
        [MaxLength(30, ErrorMessage = "Second name can't be less then 2 and more then 30 characters"), MinLength(2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Use valid symbols in your Second Name")]
        [MaxLength(40, ErrorMessage = "Second name can't be less then 2 and more then 40 characters"), MinLength(2)]
        [Display(Name = "Second Name")]
        public string SecondName { get; set; }

        [Display(Name = "Address Lane 1")]

        public string AddressLine1 { get; set; }

        [Display(Name = "Address Lane 2")]
        public string AddressLine2 { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "Post Code")]
        public string Postcode { get; set; }

        [Display(Name = "Country")] 
        public string Country { get; set; }


        [Required(ErrorMessage = "Please enter your Date of Birth.")]
        [DataType(DataType.Date)]
        [ValidDate]
        [Display(Name = "Date of Birth")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "You must provide a mobile phone number")]
        [Display(Name = "Mobile Number")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{4})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid mobile number")]
        public override string PhoneNumber { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Registered At")]
        public DateTime DateRegistered { get; set; } = DateTime.Now;


        
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        //navigation properties to link to the Bookings and Lessons tables
        // --- Teacher Connection ---
        // A teacher can teach many lessons
        public ICollection<Lesson> TaughtLessons { get; set; }

        // --- Learner Connection ---
        // A learner can make many bookings
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}