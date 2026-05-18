using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SwimmingSchool_Implementation.Models
{
    /// <summary>
    /// This class represents the agreement of a parent to a specific policy for a specific booking. It has foreign keys 
    /// to both the Policy and the Booking, allowing us to track which policies were agreed to for each booking. 
    /// This is important for ensuring that parents have accepted all required policies before confirming their booking.
    /// </summary>
    public class PolicyAgreement
    {
        public int Id { get; set; }
        public bool Accepted { get; set; }
        public DateTime AgreementDate { get; set; }

        //navigation properties
        [ForeignKey("Policy")]
        public int PolicyId { get; set; }

        public virtual Policy Policy { get; set; }

        [ForeignKey("Booking")]
        public int BookingId { get; set; }

        public virtual Booking Booking { get; set; }
    }
}