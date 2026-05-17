using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SwimmingSchool_Implementation.Models
{
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Address { get; set; }

        public string PoolInfo { get; set; }

        public string Amenities { get; set; }

        public string ImageName { get; set; }

        // Navigation

        public virtual ICollection<Lesson> Lessons { get; set; }

        public virtual ICollection<TeacherVenue> TeacherVenues { get; set; }
    }
}