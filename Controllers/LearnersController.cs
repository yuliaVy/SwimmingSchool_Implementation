using Microsoft.AspNet.Identity;
using Stripe;
using SwimmingSchool_Implementation.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
                    Status = s.Status,
                    PaymentStatus = s.Booking.AmountPaid < s.Booking.TotalAmount ? "Deposit Only" : "Paid in Full"
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
        //[HttpPost]
        //[ValidateAntiForgeryToken]
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


        /// summary
        /// Refund through stripe or paypal, not working

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult CancelSession(int sessionId)
        //{
        //    var userId = User.Identity.GetUserId();

        //    // 1. Get the Session AND the Parent Booking so we can access the money and Transaction ID
        //    var session = db.BookingSessions
        //        .Include(s => s.Booking)
        //        .FirstOrDefault(s => s.Id == sessionId && s.Booking.UserId == userId);

        //    if (session != null && session.Status == SessionStatus.Scheduled)
        //    {
        //        // 2. Calculate the 48-hour rule
        //        bool isLateCancel = (session.SessionDate - DateTime.Now).TotalHours < 48;

        //        if (!isLateCancel && session.Booking.AmountPaid >= session.Booking.TotalAmount)
        //        {
        //            // ==========================================
        //            // REFUND LOGIC (Early Cancel & Paid in Full)
        //            // ==========================================

        //            // Calculate: (Total Block Price / 4 classes) * 80%
        //            decimal singleClassValue = session.Booking.TotalAmount / 4;
        //            decimal refundAmount = singleClassValue * 0.80m;

        //            try
        //            {
        //                if (session.Booking.PaymentMethod == "Stripe")
        //                {
        //                    // Call the Stripe Refund API
        //                    var options = new Stripe.RefundCreateOptions
        //                    {
        //                        // You MUST save the Stripe PaymentIntentId to your DB during checkout for this to work!
        //                        PaymentIntent = session.Booking.TransactionId,
        //                        Amount = (long)(refundAmount * 100), // Convert to pence
        //                        Reason = RefundReasons.RequestedByCustomer
        //                    };
        //                    var service = new Stripe.RefundService();
        //                    service.Create(options);
        //                }
        //                else if (session.Booking.PaymentMethod == "PayPal")
        //                {
        //                    // PayPal SDK Refund Logic goes here using the PayPal Capture ID
        //                }

        //                // Log the refund in your database so admins can track it
        //                db.Refunds.Add(new RefundRecord
        //                {
        //                    BookingId = session.BookingId,
        //                    Amount = refundAmount,
        //                    DateProcessed = DateTime.Now
        //                });

        //                TempData["SuccessMessage"] = $"Lesson cancelled successfully. £{refundAmount:0.00} has been refunded to your original payment method.";
        //            }
        //            catch (Exception ex)
        //            {
        //                // If Stripe's API fails, still cancel the class, but alert the admin
        //                TempData["SuccessMessage"] = "Lesson cancelled. Your refund requires manual processing. Support has been notified.";
        //                System.Diagnostics.Debug.WriteLine($"Refund failed: {ex.Message}");
        //            }
        //        }
        //        else
        //        {
        //            // Late cancellation OR they only paid a deposit (deposits are usually non-refundable)
        //            TempData["SuccessMessage"] = "Lesson cancelled successfully. As per policy, no refund is issued for late cancellations or deposit-only bookings.";
        //        }

        //        // 3. Mark the class as cancelled in the database
        //        session.Status = SessionStatus.CancelledByUser;
        //        db.SaveChanges();
        //    }

        //    return RedirectToAction("MyLessons");
        //}

    }
}