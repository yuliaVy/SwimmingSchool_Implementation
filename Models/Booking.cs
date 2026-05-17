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
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; } 

        [DataType(DataType.Currency)]
        [Display(Name = "Amount Paid")]
        public decimal AmountPaid { get; set; } // If user paid deposit only

        [StringLength(250)]
        public string AdminNotes { get; set; }

        // --- Foreign Key & Navigation properties ---
        [ForeignKey("User")]
        [Required]
        public string UserId { get; set; }
        public virtual User User { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public virtual Student Student { get; set; }




        // --- The LessonsBooking table ---
        public ICollection<LessonBooking> LessonsBookings { get; set; }

        //one-to-many connection with policy agreement table
        public virtual ICollection<PolicyAgreement> PolicyAgreements { get; set; }

        //one-to-many connection with payment table
        public virtual ICollection<Payment> Payments { get; set; }



    }
}