using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SwimmingSchool_Implementation.Models
{
    /// <summary>
    /// this class represents a payment made for a booking. 
    /// It has a foreign key to the Booking class, allowing us to link each payment to a specific booking. 
    /// This is important for tracking payments and ensuring that bookings are properly paid for before they are confirmed.
    /// </summary>
    public class Payment
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; }

        public string TransactionId { get; set; }

        public DateTime PaymentDate { get; set; }

        public bool Success { get; set; }

        [ForeignKey("Booking")]
        public int BookingId { get; set; }

        public virtual Booking Booking { get; set; }
    }
}