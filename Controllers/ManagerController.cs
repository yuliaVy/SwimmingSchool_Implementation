using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PayPalCheckoutSdk.Orders;
using SwimmingSchool_Implementation.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace SwimmingSchool_Implementation.Controllers
{
    // SECURITY: Only users with the "Manager" role can access ANYTHING in this controller
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private SwimSchoolDbContext db = new SwimSchoolDbContext();

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }



        // GET: Manager
        public ActionResult Index()
        {
            // We can pass summary statistics to the main dashboard later
            ViewBag.TotalStudents = db.Students.Count();
            ViewBag.ActiveBookings = db.Bookings.Count(b => b.Status == BookingStatus.Completed);

            return View();
        }

        // GET: /Manager/Teachers (LIST)
        public ActionResult Teachers()
        {
            var teacherRoleId = db.Roles.FirstOrDefault(r => r.Name == "Teacher")?.Id;

            var teachers = db.Users
                .Where(u => u.Roles.Any(r => r.RoleId == teacherRoleId))
                .Select(u => new TeacherListViewModel
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.SecondName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Preference = u.TeacherPreference.ToString(),
                    ProfileImage = u.ProfileImage
                })
                .OrderBy(t => t.FullName)
                .ToList();

            return View(teachers);
        }

        // GET: /Manager/CreateTeacher
        public ActionResult CreateTeacher()
        {
            return View(new CreateTeacherViewModel());
        }

        // POST: /Manager/CreateTeacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateTeacher(CreateTeacherViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    SecondName = model.SecondName,
                    PhoneNumber = model.PhoneNumber,
                    TeacherPreference = model.TeacherPreference
                };

                // ==========================================
                // 1. HANDLE PROFILE PICTURE UPLOAD
                // ==========================================
                if (model.ProfileImageUpload != null && model.ProfileImageUpload.ContentLength > 0)
                {
                    var uploadDir = "~/Content/Images/Teachers/";
                    var physicalPath = Server.MapPath(uploadDir);

                    if (!Directory.Exists(physicalPath))
                    {
                        Directory.CreateDirectory(physicalPath);
                    }

                    var fileName = Path.GetFileName(model.ProfileImageUpload.FileName);
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
                    var imagePath = Path.Combine(physicalPath, uniqueFileName);

                    model.ProfileImageUpload.SaveAs(imagePath);
                    user.ProfileImage = uniqueFileName;
                }

                // ==========================================
                // 2. CREATE THE USER & ASSIGN ROLE
                // ==========================================
                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, "Teacher");

                    // ==========================================
                    // 3. SEND AUTOMATED WELCOME EMAIL
                    // ==========================================
                    try
                    {
                        string senderEmail = ConfigurationManager.AppSettings["SenderEmail"];
                        string senderPassword = ConfigurationManager.AppSettings["SenderPassword"];
                        var mailMessage = new MailMessage();
                        mailMessage.From = new MailAddress(senderEmail, "AquaLife Swimming School");
                        mailMessage.To.Add(model.Email);
                        string loginUrl = Url.Action("Login", "Account", null, protocol: Request.Url.Scheme);
                        string subject = "Welcome to the Team! Your Instructor Account";
                        string body = $@"
                        <h3>Welcome aboard, {user.FirstName}!</h3>
                        <p>Your instructor account has been successfully created by the management team.</p>
                        <p><strong>Your Login Credentials:</strong></p>
                        <ul>
                            <li><strong>Email:</strong> {user.Email}</li>
                            <li><strong>Temporary Password:</strong> {model.Password}</li>
                        </ul>
                        <p style='color: red; font-weight: bold;'>For security purposes, please log in immediately and navigate to your Dashboard to change your password.</p>
                        <p><a href='{loginUrl}' style='padding: 10px 15px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px;'>Log In Now</a></p>
                        ";
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = true;

                        using (SmtpClient smtp = new SmtpClient())
                        {
                            smtp.Host = "smtp.gmail.com";
                            smtp.Port = 587;
                            smtp.EnableSsl = true;
                            smtp.UseDefaultCredentials = false;
                            smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                            smtp.Send(mailMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        // If the email fails (e.g. SMTP not configured yet), still create the user but warn the admin
                        System.Diagnostics.Debug.WriteLine($"Failed to send email: {ex.Message}");
                        TempData["SuccessMessage"] = "Teacher created successfully, BUT the welcome email failed to send. Please provide them their password manually.";
                        return RedirectToAction("Teachers");
                    }

                    TempData["SuccessMessage"] = "Teacher created successfully and welcome email sent!";
                    return RedirectToAction("Teachers");
                }
                AddErrors(result);
            }
            return View(model);
        }

        // GET: /Manager/EditTeacher/5
        public async Task<ActionResult> EditTeacher(string id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var user = await UserManager.FindByIdAsync(id);
            if (user == null) return HttpNotFound();

            // Get all venues in the database
            var allVenues = db.Venues.ToList();

            // Get the venues this teacher is currently assigned to
            var currentVenueIds = db.TeacherVenues
                                    .Where(tv => tv.TeacherId == user.Id)
                                    .Select(tv => tv.VenueId)
                                    .ToList();

            var model = new EditTeacherViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                SecondName = user.SecondName,
                PhoneNumber = user.PhoneNumber,
                TeacherPreference = user.TeacherPreference,
                Bio = user.Bio,
                // 3. Build the Checkbox list
                AvailableVenues = allVenues.Select(v => new VenueCheckboxItem
                {
                    VenueId = v.VenueId, // Or v.Id depending on your Venue model
                    VenueName = v.Name,
                    IsSelected = currentVenueIds.Contains(v.VenueId)
                }).ToList()
            };

            return View(model);
        }

        // POST: /Manager/EditTeacher/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditTeacher(EditTeacherViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(model.Id);
                if (user == null) return HttpNotFound();

                user.FirstName = model.FirstName;
                user.SecondName = model.SecondName;
                user.PhoneNumber = model.PhoneNumber;
                user.TeacherPreference = model.TeacherPreference;
                user.Bio = model.Bio;

                var result = await UserManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // VENUE SAVING LOGIC
                    // ==========================================

                    // 1. Wipe the old venue links to start fresh
                    var oldVenues = db.TeacherVenues.Where(tv => tv.TeacherId == user.Id).ToList();
                    db.TeacherVenues.RemoveRange(oldVenues);

                    // 2. Add the newly checked venues
                    if (model.SelectedVenueIds != null && model.SelectedVenueIds.Length > 0)
                    {
                        foreach (var venueId in model.SelectedVenueIds)
                        {
                            db.TeacherVenues.Add(new TeacherVenue
                            {
                                TeacherId = user.Id,
                                VenueId = venueId
                            });
                        }
                    }

                    // 3. Save the database!
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Teacher updated successfully.";
                    return RedirectToAction("Teachers");
                }
                AddErrors(result);
            }
            model.AvailableVenues = db.Venues.Select(v => new VenueCheckboxItem { VenueId = v.VenueId, VenueName = v.Name }).ToList();
            return View(model);
        }

        // GET: /Manager/DeleteTeacher/5
        public async Task<ActionResult> DeleteTeacher(string id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var user = await UserManager.FindByIdAsync(id);
            if (user == null) return HttpNotFound();

            // We can reuse the TeacherListViewModel just to display their info on the warning page
            var model = new TeacherListViewModel
            {
                Id = user.Id,
                FullName = user.FirstName + " " + user.SecondName,
                Email = user.Email,
                ProfileImage = user.ProfileImage
            };

            return View(model);
        }

        [HttpPost, ActionName("DeleteTeacher")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteTeacherConfirmed(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user == null) return HttpNotFound();

            // ==========================================
            // 1. CLEANUP ACTIVE ASSIGNMENTS
            // ==========================================
            // Remove all Venue associations so they don't show up in venue searches
            var relatedVenues = db.TeacherVenues.Where(tv => tv.TeacherId == user.Id).ToList();
            if (relatedVenues.Any())
            {
                db.TeacherVenues.RemoveRange(relatedVenues);
                db.SaveChanges();
            }

            // ==========================================
            // 2. THE SOFT DELETE (Role Swap & Lockout)
            // ==========================================

            // A. Remove their Teacher status (This hides them from your dashboard lists!)
            if (await UserManager.IsInRoleAsync(user.Id, "Teacher"))
            {
                await UserManager.RemoveFromRoleAsync(user.Id, "Teacher");
            }

            // B. Lock the account forever so they can never log in again
            await UserManager.SetLockoutEnabledAsync(user.Id, true);
            await UserManager.SetLockoutEndDateAsync(user.Id, DateTimeOffset.MaxValue);

            // ==========================================

            TempData["SuccessMessage"] = "Teacher successfully archived! Their login has been disabled and they have been removed from the directory, but past lesson records are safely preserved.";

            return RedirectToAction("Teachers");
        }

        // Helper method for Identity Errors
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        // GET: /Manager/Students
        public ActionResult Students()
        {
            // Includes the parent/account holder data so managers can see who to call
            var students = db.Students.Include(s => s.Bookings.Select(b => b.User)).ToList();
            return View(students);
        }

        // GET: /Manager/AccountHolders
        public ActionResult AccountHolders()
        {
            // Gets all registered users who have made at least one booking
            var accountHolders = db.Users.Where(u => u.Bookings.Any()).ToList();
            return View(accountHolders);
        }

        // 3. BOOKINGS & TIMETABLE
        // ==========================================

        // GET: /Manager/Bookings
        public ActionResult Bookings()
        {
            var bookings = db.Bookings
                .Include(b => b.Student)
                .Include(b => b.User) // The account holder who paid
                .Include(b => b.LessonsBookings.Select(lb => lb.Lesson))
                .OrderByDescending(b => b.BookingDate)
                .ToList();

            return View(bookings);
        }

        // GET: /Manager/Timetable
        public ActionResult Timetable(DateTime? weekStart)
        {
            // Default to the current week's Monday if no date is provided
            DateTime startDate = weekStart ?? DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            DateTime endDate = startDate.AddDays(7);

            // Fetch all individual sessions happening in this 7-day window
            var weeklySessions = db.BookingSessions
                .Include(s => s.Booking.LessonsBookings.Select(lb => lb.Lesson))
                .Include(s => s.Booking.LessonsBookings.Select(lb => lb.Lesson.Teacher))
                .Include(s => s.Booking.LessonsBookings.Select(lb => lb.Lesson.Venue))
                .Where(s => s.SessionDate >= startDate && s.SessionDate < endDate)
                .ToList();

            ViewBag.WeekStart = startDate;

            return View(weeklySessions);
        }

        // GET: /Manager/ClassRoster
        // Allows the manager to click a specific class on the timetable and see who is swimming
        public ActionResult ClassRoster(int lessonId, DateTime sessionDate)
        {
            var roster = db.BookingSessions
                .Include(s => s.Booking.Student)
                .Include(s => s.Booking.User)
                .Where(s => s.Booking.LessonsBookings.Any(lb => lb.LessonId == lessonId)
                 && DbFunctions.TruncateTime(s.SessionDate) == sessionDate.Date
                 && s.Status != SessionStatus.CancelledByUser)
                .ToList();

            return View(roster);
        }

        // ==========================================
        // 4. REPORTS & EXPORTS
        // ==========================================

        // GET: /Manager/Reports
        public ActionResult Reports()
        {
            return View();
        }

        // (We will add the PDF/Excel generation methods here later)
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

}
