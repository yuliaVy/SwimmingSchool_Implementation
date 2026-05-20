using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SwimmingSchool_Implementation.Models
{
    public enum SessionStatus
    {
        Scheduled,
        Attended,
        NoShow,
        CancelledByUser,
        CancelledBySchool
    }

    public class BookingSession
    {
        [Key]
        public int Id { get; set; }

        // Links back to the main payment/booking
        [ForeignKey("Booking")]
        public int BookingId { get; set; }
        public virtual Booking Booking { get; set; }

        // The specific date and time this physical lesson takes place
        public DateTime SessionDate { get; set; }

        public SessionStatus Status { get; set; }

        // Optional: If a teacher leaves a note for a specific week (e.g., "Struggled with breathing")
        public string TeacherNotes { get; set; }
    }
}