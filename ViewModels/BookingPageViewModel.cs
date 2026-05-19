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

    public class MyLessonsViewModel
    {
        // Summary Statistics
        public int TotalClassesBooked { get; set; }
        public int TotalClassesCancelledByUser { get; set; }
        public List<LessonSessionDetail> UpcomingLessons { get; set; } = new List<LessonSessionDetail>();
        public List<LessonSessionDetail> PastLessons { get; set; } = new List<LessonSessionDetail>();
    }

    public class LessonSessionDetail
    {
        public int SessionId { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; }

        public DateTime SessionDate { get; set; }
        public string LessonTime { get; set; }

        public string TeacherName { get; set; }
        public string VenueName { get; set; }

        public SessionStatus Status { get; set; }

        // Calculates if the lesson is less than 48 hours away
        public bool IsLateCancellation
        {
            get
            {
                // Combine Date and Time for an accurate calculation
                if (TimeSpan.TryParse(LessonTime, out TimeSpan time))
                {
                    DateTime exactLessonStart = SessionDate.Date + time;
                    return (exactLessonStart - DateTime.Now).TotalHours < 48;
                }
                // Fallback if time parsing fails
                return (SessionDate - DateTime.Now).TotalDays < 2;
            }
        }
    }
}
