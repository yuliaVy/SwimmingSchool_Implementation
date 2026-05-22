using Microsoft.AspNet.Identity;
using PayPalCheckoutSdk.Orders;
using SwimmingSchool_Implementation.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SwimmingSchool_Implementation.Controllers
{
    // Secure this controller for both staff roles
    [Authorize(Roles = "Manager, Teacher")]
    public class TimetableController : Controller
    {
        private SwimSchoolDbContext db = new SwimSchoolDbContext();

        public ActionResult Index(DateTime? selectedDate, int? selectedVenueId)
        {
            // Enforce Date Rules (Default to today, max 90 days out)
            DateTime filterDate = selectedDate ?? DateTime.Today;
            DateTime maxDate = DateTime.Today.AddDays(90);

            if (filterDate > maxDate) filterDate = maxDate;
            if (filterDate < DateTime.Today.AddDays(-30)) filterDate = DateTime.Today; // Optional: restrict looking too far in the past

            // Base Query: Get all active sessions for this specific date
            var sessionsQuery = db.BookingSessions
                .Include(s => s.Booking.LessonsBookings.Select(lb => lb.Lesson.Teacher))
                .Include(s => s.Booking.LessonsBookings.Select(lb => lb.Lesson.Venue))
                .Include(s => s.Booking.Student)
                .Include(s => s.Booking.User)
                .Where(s => DbFunctions.TruncateTime(s.SessionDate) == filterDate.Date
                         && s.Status != SessionStatus.CancelledByUser);

            // Filter by Venue (if selected)
            if (selectedVenueId.HasValue)
            {
                // Do ANY of the lessons in this booking belong to this venue?
                sessionsQuery = sessionsQuery.Where(s =>
                    s.Booking.LessonsBookings.Any(lb => lb.Lesson.VenueId == selectedVenueId.Value)
                );
            }
            // SECURITY BRANCH: If they are a teacher, strictly limit to their own lessons
            if (User.IsInRole("Teacher") && !User.IsInRole("Manager"))
            {
                var currentUserId = User.Identity.GetUserId();
                // Do ANY of the lessons in this booking belong to this teacher?
                sessionsQuery = sessionsQuery.Where(s =>
                    s.Booking.LessonsBookings.Any(lb => lb.Lesson.UserId == currentUserId)
                );
            }

            var rawSessions = sessionsQuery.ToList();

            // Group the raw database rows into our clean ViewModel structure
            // Group the raw database rows into our clean ViewModel structure
            var groupedSessions = rawSessions
                // FIX 1: Safely navigate through the lists using ?.
                .GroupBy(s => s.Booking?.LessonsBookings?.FirstOrDefault()?.Lesson)
                .Where(g => g.Key != null)
                .Select(g => new TimetableSessionViewModel
                {
                    LessonId = g.Key.Id,
                    LessonType = g.Key.LessonType.ToString(),
                    StartTime = g.Key.StartTime.ToString(@"hh\:mm"),
                    VenueName = g.Key.Venue?.Name ?? "Unknown Venue",
                    TeacherName = g.Key.Teacher != null ? g.Key.Teacher.FirstName + " " + g.Key.Teacher.SecondName : "Instructor TBC",

                    // FIX 2: Safely map students, handling missing data without crashing
                    Students = g.Select(s => new StudentRosterViewModel
                    {
                        SessionId = s.Id,
                        UniqueModalId = Guid.NewGuid().ToString("N"),

                        FullName = (s.Booking?.Student?.FirstName ?? "Unknown") + " " + (s.Booking?.Student?.SecondName ?? ""),

                        // Only calculate age if the DateOfBirth exists
                        Age = s.Booking?.Student != null ? CalculateAge(s.Booking.Student.DateOfBirth) : 0,
                        Gender = s.Booking?.Student?.Gender ?? "N/A",

                        MedicalConditions = s.Booking?.Student?.MedicalConditions,
                        Allergies = s.Booking?.Student?.Allergies,
                        Medications = s.Booking?.Student?.Medications,
                        AquaticGoals = s.Booking?.Student?.AquaticGoals,
                        SwimExperience = s.Booking?.Student?.SwimExperience,

                        HasMedicalFlag = !string.IsNullOrWhiteSpace(s.Booking?.Student?.MedicalConditions) ||
                                         !string.IsNullOrWhiteSpace(s.Booking?.Student?.Allergies) ||
                                         !string.IsNullOrWhiteSpace(s.Booking?.Student?.Medications),

                        SessionStatus = s.Status.ToString(),
                        PaymentStatus = s.Booking.AmountPaid < s.Booking.TotalAmount ? "Deposit Only" : "Paid in Full",

                        ParentName = s.Booking?.User != null ? s.Booking.User.FirstName + " " + s.Booking.User.SecondName : "Unknown",
                        ParentEmail = s.Booking?.User?.Email,
                        ParentPhone = s.Booking?.User?.PhoneNumber
                    }).ToList()
                })
                .OrderBy(s => s.StartTime)
                .ToList();

            // 6. Build the final ViewModel
            var model = new TimetableIndexViewModel
            {
                SelectedDate = filterDate,
                SelectedVenueId = selectedVenueId,
                Venues = new SelectList(db.Venues.ToList(), "VenueId", "Name", selectedVenueId),
                Sessions = groupedSessions
            };

            return View(model);
        }

        // POST: /Timetable/UpdateSessionStatus
        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateSessionStatus(int sessionId, SessionStatus newStatus, DateTime filterDate, int? selectedVenueId)
        {
            // Find the specific session in the database
            var session = db.BookingSessions.Find(sessionId);

            if (session != null)
            {
                // Update the status and save
                session.Status = newStatus;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Session status updated successfully.";
            }

            // Redirect back to the Timetable, remembering the exact date and venue they were looking at!
            return RedirectToAction("Index", new { selectedDate = filterDate.ToString("yyyy-MM-dd"), selectedVenueId = selectedVenueId });
        }

        // Helper method to calculate accurate age
        private int CalculateAge(DateTime dob)
        {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age)) age--;
            return age;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }    
        
    }
}