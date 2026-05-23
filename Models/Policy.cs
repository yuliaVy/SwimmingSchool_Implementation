using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SwimmingSchool_Implementation.Models
{
    /// <summary>   
    /// This class holds policies that parents must agree to when booking a lesson. It has a one-to-many relationship with PolicyAgreement, which links it to specific bookings.
    /// </summary>
    public class Policy
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public bool IsRequired { get; set; }

        public virtual ICollection<PolicyAgreement> PolicyAgreements { get; set; }
    }
}