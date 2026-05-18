using System;
using System.Collections.Generic;
using SwimmingSchool_Implementation.Models;
using System.Linq;
using System.Web;

namespace SwimmingSchool_Implementation.Models
{
    public class BookingPageViewModel
    {
        public List<Lesson> Lessons { get; set; }

        public List<User> Teachers { get; set; }

        public List<Venue> Venues { get; set; }

        public int? SelectedVenueId { get; set; }

        public LessonType? SelectedLessonType { get; set; }

        public Venue SelectedVenue { get; set; }
    }
}
