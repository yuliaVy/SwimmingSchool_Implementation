using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace SwimmingSchool_Implementation.Models
{
    public enum LessonType
    {
        Kids = 1,
        Adult = 2
    }
    public class Lesson
    {
        // --- Core Lesson Properties ---

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Lesson Name / Level")]
        public string Title { get; set; } // can be "Beginner Freestyle" or "Toddler Splash"

        [Required(ErrorMessage = "A day of the week  is required.")]
        [Display(Name = "Lesson Day")]
        public DayOfWeek DayOfWeek { get; set; } 

        [Required(ErrorMessage = "A start time is required.")]
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Range(30, 60, ErrorMessage = "Duration must be between 30 and 60 minutes.")]
        [Display(Name = "Duration (Minutes)")]
        public int DurationInMinutes { get; set; }

        [Required(ErrorMessage = "Please set a class capacity.")]
        [Range(1, 6, ErrorMessage = "Capacity must be between 1 and 6.")]
        [Display(Name = "Max Capacity")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public int AvailablePlaces { get; set; }

        public string AgeGroup { get; set; }
        public LessonType LessonType { get; set; }


        // --- Foreign Key & Navigation properties ---
        [ForeignKey("Teacher")]
        [Required(ErrorMessage = "Please assign a teacher.")]
        [Display(Name = "Instructor")]
        public string UserId { get; set; }
        public User Teacher { get; set; }

        [ForeignKey("Venue")]
        public int VenueId { get; set; }

        public virtual Venue Venue { get; set; }


        // Navigation property to track who has booked this specific lesson
        public virtual ICollection<LessonBooking> LessonsBookings { get; set; }


    }
}