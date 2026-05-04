using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace SwimmingSchool_Implementation.Models
{
    public enum BookingStatus
    {
        Pending,    // Waiting for payment or admin approval
        Confirmed,  // Fully booked and ready to go
        Cancelled,  // User or Admin cancelled the booking
        Completed   // The lesson has taken place
    }
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        [Display(Name = "Status")]
        public BookingStatus Status { get; set; } = BookingStatus.Pending; // Default to Pending

        [Display(Name = "Deposit Only?")]
        public bool IsDepositOnly { get; set; }

        [Required]
        [Display(Name = "Booking Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime BookingDate { get; set; } = DateTime.Now; // Automatically records exactly when they booked

        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; } 

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Amount Paid")]
        public decimal AmountPaid { get; set; } // If user paid deposit only

        [StringLength(250)]
        public string AdminNotes { get; set; }

        // --- Foreign Key & Navigation properties ---
        [ForeignKey("LearnerId")]
        [Required]
        [Display(Name = "Learner")]
        public string LearnerId { get; set; } 
        public User Learner { get; set; }



        // --- The Lessons List (Many-to-Many Relationship) ---

        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();




        
    }
}