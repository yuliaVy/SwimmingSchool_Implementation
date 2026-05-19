using Microsoft.AspNet.Identity;
using SwimmingSchool_Implementation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace SwimmingSchool_Implementation.Controllers
{
    public class LearnersController : Controller
    {
        //instance of the database context class to interact
        private SwimSchoolDbContext db = new SwimSchoolDbContext();

        // GET: Learners
        public ActionResult MyLessons()
        {
            var userId = User.Identity.GetUserId();

            // Fetch all physical session dates linked to this user's bookings
            var sessions = db.BookingSessions
                .Include(s => s.Booking)
                .Include(s => s.Booking.Student)
                .Include(s => s.Booking.LessonsBookings.Select(lb => lb.Lesson))
                .Include(s => s.Booking.LessonsBookings.Select(lb => lb.Lesson.Venue))
                // Assuming your Lesson model links to a Teacher (ApplicationUser)
                .Include(s => s.Booking.LessonsBookings.Select(lb => lb.Lesson.Teacher))
                .Where(s => s.Booking.UserId == userId)
                .ToList();

            // Initialize the model and calculate the totals immediately
            var model = new MyLessonsViewModel
            {
                // The total number of sessions ever created for this user
                TotalClassesBooked = sessions.Count,

                // Count only the sessions where the user clicked cancel
                TotalClassesCancelledByUser = sessions.Count(s => s.Status == SessionStatus.CancelledByUser)
            };
            foreach (var s in sessions)
            {
                // Navigate through your database relationships (Adjust names if your properties differ slightly)
                var lesson = db.LessonsBookings.FirstOrDefault(lb => lb.BookingId == s.BookingId)?.Lesson;
                var student = s.Booking.Student;

                if (lesson == null || student == null) continue;

                var detail = new LessonSessionDetail
                {
                    SessionId = s.Id,
                    StudentName = $"{student.FirstName} {student.SecondName}",
                    ClassName = lesson.Title,
                    SessionDate = s.SessionDate,
                    LessonTime = lesson.StartTime.ToString(@"hh\:mm"),
                    VenueName = lesson.Venue?.Name ?? "TBC",
                    TeacherName = lesson.Teacher != null ? $"{lesson.Teacher.FirstName} {lesson.Teacher.SecondName}" : "TBC",
                    Status = s.Status
                };
                // Sort into the correct bucket based on the date
                // We use DateTime.Today so lessons happening today still show in Upcoming until tomorrow
                if (s.SessionDate.Date >= DateTime.Today)
                {
                    model.UpcomingLessons.Add(detail);
                }
                else
                {
                    model.PastLessons.Add(detail);
                }
            }
            // Order them chronologically for a clean UI
            model.UpcomingLessons = model.UpcomingLessons.OrderBy(x => x.SessionDate).ToList();
            model.PastLessons = model.PastLessons.OrderByDescending(x => x.SessionDate).ToList();

            return View(model);
        }

        // 2. POST: /Dashboard/CancelSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelSession(int sessionId)
        {
            var userId = User.Identity.GetUserId();

            // Find the specific session, ensuring it actually belongs to the logged-in user
            var session = db.BookingSessions
                .Include(s => s.Booking)
                .FirstOrDefault(s => s.Id == sessionId && s.Booking.UserId == userId);

            if (session != null && session.Status == SessionStatus.Scheduled)
            {
                // Change the status to Cancelled
                session.Status = SessionStatus.CancelledByUser;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Lesson successfully cancelled.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to cancel this lesson. It may have already been cancelled.";
            }

            return RedirectToAction("MyLessons");
        }

    }
}