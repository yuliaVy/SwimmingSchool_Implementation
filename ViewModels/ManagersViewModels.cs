using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

    public class TimetableIndexViewModel
    {
        [DataType(DataType.Date)]
        public DateTime SelectedDate { get; set; }

        public int? SelectedVenueId { get; set; }
        public SelectList Venues { get; set; }

        // The list of classes happening on this date
        public List<TimetableSessionViewModel> Sessions { get; set; } = new List<TimetableSessionViewModel>();
    }

    public class TimetableSessionViewModel
    {
        public int LessonId { get; set; }
        public string LessonType { get; set; }
        public string StartTime { get; set; }
        public string TeacherName { get; set; }
        public string VenueName { get; set; }
        public List<StudentRosterViewModel> Students { get; set; } = new List<StudentRosterViewModel>();
    }

    public class StudentRosterViewModel
    {
        // Add a unique ID so we can trigger the correct pop-up modal
        public string UniqueModalId { get; set; }
        public int SessionId { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string MedicalConditions { get; set; }
        public string Allergies { get; set; }
        public string Medications { get; set; }
        public string AquaticGoals { get; set; }
        public string SwimExperience { get; set; } 
        public bool HasMedicalFlag { get; set; }
        public string SessionStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string ParentName { get; set; }
        public string ParentEmail { get; set; }
        public string ParentPhone { get; set; }
    }
}