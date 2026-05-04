using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace SwimmingSchool_Implementation.Models
{
    public class Lesson
    {
        // --- Core Lesson Properties ---

        [Key]
        public int LessonId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Lesson Name / Level")]
        public string Title { get; set; } // can be "Beginner Freestyle" or "Toddler Splash"

        [Required(ErrorMessage = "A date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Lesson Date")]
        public DateTime LessonDate { get; set; } 

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
        [Column(TypeName = "decimal(18, 2)")] 
        public decimal Price { get; set; }

        // --- Foreign Key & Navigation properties ---
        [ForeignKey("TeacherId")]
        [Required(ErrorMessage = "Please assign a teacher.")]
        [Display(Name = "Instructor")]
        public string TeacherId { get; set; }
        public User Teacher { get; set; }


        // Navigation property to track who has booked this specific lesson
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}