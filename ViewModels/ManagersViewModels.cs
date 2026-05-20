using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace SwimmingSchool_Implementation.Models
{
    public class VenueCheckboxItem
    {
        public int VenueId { get; set; }
        public string VenueName { get; set; }
        public bool IsSelected { get; set; }
    }

    // Used for the main List and viewing details
    public class TeacherListViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Preference { get; set; }
        public string ProfileImage { get; set; }
    }

    // Used for Creating a new Teacher
    public class CreateTeacherViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string SecondName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        [Display(Name = "Teaching Preference")]
        public TeacherPreference TeacherPreference { get; set; }

        [Display(Name = "Profile Picture")]
        public HttpPostedFileBase ProfileImageUpload { get; set; }
    }

    // Used for Editing an existing Teacher
    public class EditTeacherViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string SecondName { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Display(Name = "Teaching Preference")]
        public TeacherPreference TeacherPreference { get; set; }

        public string Bio { get; set; }
        public List<VenueCheckboxItem> AvailableVenues { get; set; } = new List<VenueCheckboxItem>();

        // NEW: Used to "catch" the checked boxes when the form is submitted
        public int[] SelectedVenueIds { get; set; }
    }
}