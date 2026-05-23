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
            var model = new CreateTeacherViewModel
            {
                AvailableVenues = db.Venues.Select(v => new VenueCheckboxItem
                {
                    VenueId = v.VenueId,
                    VenueName = v.Name
                }).ToList()
            };
            return View(model);
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
                    var uploadDir = "~/Content/Images/";
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

                    // Saving the selected venues
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
                        db.SaveChanges();
                    }

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
                CurrentProfileImage = user.ProfileImage,
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

                // Handle Profile Picture Upload on Edit
                if (model.ProfileImageUpload != null && model.ProfileImageUpload.ContentLength > 0)
                {
                    var uploadDir = "~/Content/Images/";
                    var physicalPath = Server.MapPath(uploadDir);

                    if (!Directory.Exists(physicalPath))
                    {
                        Directory.CreateDirectory(physicalPath);
                    }

                    var fileName = Path.GetFileName(model.ProfileImageUpload.FileName);
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
                    var fullImagePath = Path.Combine(physicalPath, uniqueFileName);

                    model.ProfileImageUpload.SaveAs(fullImagePath);
                    user.ProfileImage = uniqueFileName;
                }

                var result = await UserManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // VENUE SAVING LOGIC
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

            // 1. CLEANUP ACTIVE ASSIGNMENTS
            // Remove all Venue associations so they don't show up in venue searches
            var relatedVenues = db.TeacherVenues.Where(tv => tv.TeacherId == user.Id).ToList();
            if (relatedVenues.Any())
            {
                db.TeacherVenues.RemoveRange(relatedVenues);
                db.SaveChanges();
            }

            // 2. THE SOFT DELETE (Role Swap & Lockout)

            // A. Remove their Teacher status (This hides them from your dashboard lists!)
            if (await UserManager.IsInRoleAsync(user.Id, "Teacher"))
            {
                await UserManager.RemoveFromRoleAsync(user.Id, "Teacher");
            }

            // B. Lock the account forever so they can never log in again
            await UserManager.SetLockoutEnabledAsync(user.Id, true);
            await UserManager.SetLockoutEndDateAsync(user.Id, DateTimeOffset.MaxValue);


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
            var today = DateTime.Today;

            // Fetch all students and eager-load their bookings, parents, and classes
            var studentsQuery = db.Students
                .Include(s => s.Bookings.Select(b => b.User))
                .Include(s => s.Bookings.Select(b => b.LessonsBookings.Select(lb => lb.Lesson.Venue)))
                .ToList();

            var model = studentsQuery.Select(s => new StudentDirectoryViewModel
            {
                StudentId = s.Id,
                FullName = s.FirstName + " " + s.SecondName,
                Gender = s.Gender,

                // Inline accurate age calculation
                Age = (today.Year - s.DateOfBirth.Year) - (s.DateOfBirth.Date > today.AddYears(-(today.Year - s.DateOfBirth.Year)) ? 1 : 0),

                // Medical Mapping
                MedicalConditions = s.MedicalConditions,
                Allergies = s.Allergies,
                Medications = s.Medications,
                HasMedicalFlag = !string.IsNullOrWhiteSpace(s.MedicalConditions) ||
                                 !string.IsNullOrWhiteSpace(s.Allergies) ||
                                 !string.IsNullOrWhiteSpace(s.Medications),

                // Find the parent linked to their first booking
                ParentName = s.Bookings.FirstOrDefault()?.User != null
                             ? s.Bookings.First().User.FirstName + " " + s.Bookings.First().User.SecondName
                             : "Unknown",
                ParentEmail = s.Bookings.FirstOrDefault()?.User?.Email ?? "N/A",
                ParentPhone = s.Bookings.FirstOrDefault()?.User?.PhoneNumber ?? "N/A",

                // Map out all the specific classes this student is attending
                EnrolledClasses = s.Bookings.SelectMany(b => b.LessonsBookings.Select(lb => new StudentEnrolledClassViewModel
                {
                    ClassName = lb.Lesson.Title,
                    Schedule = lb.Lesson.DayOfWeek + " at " + lb.Lesson.StartTime.ToString(@"hh\:mm"),
                    VenueName = lb.Lesson.Venue?.Name ?? "Unknown",
                    BookingStatus = b.Status.ToString()
                })).ToList()

            }).OrderBy(s => s.FullName).ToList();

            return View(model);
        }

        // GET: /Manager/AccountHolders
        // ==========================================
        // DIRECTORY MANAGEMENT
        // ==========================================

        // GET: /Manager/AccountHolders
        public ActionResult AccountHolders()
        {
            // Fetch any user who has made at least one booking, and eager-load the related students/lessons
            var parentsQuery = db.Users
                .Include(u => u.Bookings.Select(b => b.Student))
                .Include(u => u.Bookings.Select(b => b.LessonsBookings.Select(lb => lb.Lesson)))
                .Where(u => u.Bookings.Any())
                .ToList();

            var model = parentsQuery.Select(u => new AccountHolderDirectoryViewModel
            {
                UserId = u.Id,
                FullName = u.FirstName + " " + u.SecondName,
                Email = u.Email,
                Phone = u.PhoneNumber ?? "No Phone Provided",
                RegisteredDate = u.DateRegistered.ToString(),
                TotalBookings = u.Bookings.Count,
                TotalSpent = u.Bookings.Sum(b => b.AmountPaid),

                // Map out their specific kids and classes
                Bookings = u.Bookings.OrderByDescending(b => b.BookingDate).Select(b => new AccountHolderBookingViewModel
                {
                    BookingId = b.BookingId,
                    BookingDate = b.BookingDate.ToString("MMM dd, yyyy"),
                    StudentName = b.Student != null ? b.Student.FirstName + " " + b.Student.SecondName : "Unknown Swimmer",
                    ClassName = b.LessonsBookings.FirstOrDefault()?.Lesson?.Title ?? "Unknown Class",
                    AmountPaid = b.AmountPaid,
                    Status = b.Status.ToString()
                }).ToList()

            }).OrderByDescending(a => a.RegisteredDate).ToList();

            return View(model);
        }

        // 3. BOOKINGS & TIMETABLE
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
        // REPORTS & EXPORTS
        // ==========================================

        // GET: /Manager/Reports
        public ActionResult Reports(DateTime? startDate, DateTime? endDate)
        {
            // Default to the current month if no dates are provided
            DateTime start = startDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime end = endDate ?? DateTime.Today;

            // Fetch the bookings within the timeframe
            var bookingsQuery = db.Bookings
                .Include(b => b.User)
                .Include(b => b.Student)
                .Include(b => b.LessonsBookings.Select(lb => lb.Lesson))
                .Where(b => DbFunctions.TruncateTime(b.BookingDate) >= start.Date
                         && DbFunctions.TruncateTime(b.BookingDate) <= end.Date)
                .OrderByDescending(b => b.BookingDate)
                .ToList();

            var model = new ReportDashboardViewModel
            {
                StartDate = start,
                EndDate = end,
                TotalBookings = bookingsQuery.Count,
                TotalRevenue = bookingsQuery.Sum(b => b.AmountPaid),

                // Map the data for the preview table
                PreviewData = bookingsQuery.Select(b => new ReportRowViewModel
                {
                    BookingId = b.BookingId,
                    BookingDate = b.BookingDate.ToString("yyyy-MM-dd"),
                    AccountHolder = b.User != null ? b.User.FirstName + " " + b.User.SecondName : "Unknown",
                    AccountEmail = b.User?.Email ?? "N/A",
                    StudentName = b.Student != null ? b.Student.FirstName + " " + b.Student.SecondName : "Unknown",

                    // Grab the first lesson's title safely
                    ClassDetails = b.LessonsBookings.FirstOrDefault()?.Lesson?.Title ?? "Unknown Class",

                    PaymentStatus = b.Status.ToString(),
                    AmountPaid = b.AmountPaid,
                    TotalAmount = b.TotalAmount
                }).ToList()
            };

            return View(model);
        }

        // GET: /Manager/ExportBookingsCsv
        public ActionResult ExportBookingsCsv(DateTime startDate, DateTime endDate)
        {
            // 1. Fetch the exact same data based on the dates
            var bookings = db.Bookings
                .Include(b => b.User)
                .Include(b => b.Student)
                .Include(b => b.LessonsBookings.Select(lb => lb.Lesson))
                .Where(b => DbFunctions.TruncateTime(b.BookingDate) >= startDate.Date
                         && DbFunctions.TruncateTime(b.BookingDate) <= endDate.Date)
                .OrderByDescending(b => b.BookingDate)
                .ToList();

            // 2. Build the CSV String using StringBuilder
            var builder = new System.Text.StringBuilder();

            // Add the Column Headers
            builder.AppendLine("Booking ID,Booking Date,Account Holder,Email,Student Name,Class,Status,Amount Paid,Total Price");

            // Loop through data and add rows
            foreach (var b in bookings)
            {
                string date = b.BookingDate.ToString("yyyy-MM-dd");
                string account = b.User != null ? $"{b.User.FirstName} {b.User.SecondName}" : "Unknown";
                string email = b.User?.Email ?? "";
                string student = b.Student != null ? $"{b.Student.FirstName} {b.Student.SecondName}" : "Unknown";
                string lesson = b.LessonsBookings.FirstOrDefault()?.Lesson?.Title ?? "Unknown";

                // We wrap strings in quotes so if a name contains a comma, it doesn't break the Excel columns
                builder.AppendLine($"\"{b.BookingId}\",\"{date}\",\"{account}\",\"{email}\",\"{student}\",\"{lesson}\",\"{b.Status}\",\"{b.AmountPaid}\",\"{b.TotalAmount}\"");
            }

            // 3. Return the string as a downloadable file
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
            string fileName = $"BookingsReport_{startDate:yyyyMMdd}_to_{endDate:yyyyMMdd}.csv";

            // "text/csv" tells the browser to treat this as an Excel-compatible spreadsheet
            return File(fileBytes, "text/csv", fileName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        // ==========================================
        // LESSON MANAGEMENT CRUD
        // ==========================================

        // GET: /Manager/Lessons
        public ActionResult Lessons()
        {
            var today = DateTime.Today;

            // Fetch all lessons and include the foreign key data
            var allLessons = db.Lessons
                .Include(l => l.Teacher)
                .Include(l => l.Venue)
                .ToList();

            var model = new LessonManagementViewModel
            {
                // Active: Block starts today or in the future
                ActiveLessons = allLessons
                    .Where(l => l.BlockStartDate >= today)
                    .OrderBy(l => l.BlockStartDate)
                    .ToList(),

                // Past: Block started before today
                PastLessons = allLessons
                    .Where(l => l.BlockStartDate < today)
                    .OrderByDescending(l => l.BlockStartDate)
                    .ToList()
            };

            return View(model);
        }

        // GET: /Manager/CreateLesson
        public ActionResult CreateLesson()
        {
            PopulateLessonDropdowns();
            return View();
        }

        // POST: /Manager/CreateLesson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateLesson(Lesson lesson)
        {
            if (ModelState.IsValid)
            {
                // CRITICAL: Set available places to match the capacity they just typed in
                lesson.AvailablePlaces = lesson.Capacity;

                db.Lessons.Add(lesson);
                db.SaveChanges();
                TempData["SuccessMessage"] = "New lesson successfully created!";
                return RedirectToAction("Lessons");
            }

            PopulateLessonDropdowns(lesson.UserId, lesson.VenueId);
            return View(lesson);
        }

        // GET: /Manager/EditLesson/5
        public ActionResult EditLesson(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Lesson lesson = db.Lessons.Find(id);
            if (lesson == null) return HttpNotFound();

            // Security Check: Prevent editing of past lessons by URL manipulation
            if (lesson.BlockStartDate < DateTime.Today)
            {
                TempData["ErrorMessage"] = "You cannot edit a lesson that has already started.";
                return RedirectToAction("Lessons");
            }

            PopulateLessonDropdowns(lesson.UserId, lesson.VenueId);
            return View(lesson);
        }

        // POST: /Manager/EditLesson/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditLesson(Lesson submittedLesson)
        {
            if (ModelState.IsValid)
            {
                // 1. Fetch the ORIGINAL lesson from the database so we can do the math safely
                var existingLesson = db.Lessons.Find(submittedLesson.Id);

                if (existingLesson == null)
                    return HttpNotFound();

                // 2. THE MATH LOGIC
                // Calculate exactly how many students are currently occupying seats
                int spotsTaken = existingLesson.Capacity - existingLesson.AvailablePlaces;

                // 3. THE SAFETY CHECK
                // Block the manager if they try to shrink the class smaller than the booked kids
                if (submittedLesson.Capacity < spotsTaken)
                {
                    ModelState.AddModelError("Capacity", $"You cannot reduce the maximum capacity to {submittedLesson.Capacity}. There are already {spotsTaken} swimmer(s) booked into this class. Please cancel their bookings first.");

                    PopulateLessonDropdowns(submittedLesson.UserId, submittedLesson.VenueId);
                    return View(submittedLesson);
                }

                // 4. THE LIVE RECALCULATION
                // Automatically adjust the available places based on the new capacity
                existingLesson.AvailablePlaces = submittedLesson.Capacity - spotsTaken;

                // 5. Update the rest of the fields manually to prevent overwriting bugs
                existingLesson.Title = submittedLesson.Title;
                existingLesson.AgeGroup = submittedLesson.AgeGroup;
                existingLesson.LessonType = submittedLesson.LessonType;
                existingLesson.Price = submittedLesson.Price;
                existingLesson.BlockStartDate = submittedLesson.BlockStartDate;
                existingLesson.DayOfWeek = submittedLesson.DayOfWeek;
                existingLesson.StartTime = submittedLesson.StartTime;
                existingLesson.DurationInMinutes = submittedLesson.DurationInMinutes;
                existingLesson.Capacity = submittedLesson.Capacity;
                existingLesson.VenueId = submittedLesson.VenueId;
                existingLesson.UserId = submittedLesson.UserId;

                // 6. Save changes
                db.SaveChanges();
                TempData["SuccessMessage"] = "Lesson capacity and details updated successfully!";
                return RedirectToAction("Lessons");
            }

            // If model state is invalid, return the view
            PopulateLessonDropdowns(submittedLesson.UserId, submittedLesson.VenueId);
            return View(submittedLesson);
        }

        // POST: /Manager/DeleteLesson/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteLesson(int id)
        {
            Lesson lesson = db.Lessons.Find(id);
            if (lesson != null)
            {
                if (lesson.BlockStartDate < DateTime.Today)
                {
                    TempData["ErrorMessage"] = "Cannot delete past lessons. They are kept for historical records.";
                    return RedirectToAction("Lessons");
                }

                try
                {
                    db.Lessons.Remove(lesson);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Lesson successfully deleted.";
                }
                catch (Exception)
                {
                    // This catches the SQL constraint error if they try to delete a class that parents have already booked!
                    TempData["ErrorMessage"] = "Cannot delete this lesson because students are already booked into it. Please cancel the bookings first.";
                }
            }
            return RedirectToAction("Lessons");
        }

        // --- HELPER METHOD FOR DROPDOWNS ---
        private void PopulateLessonDropdowns(string selectedTeacherId = null, int? selectedVenueId = null)
        {
            // 1. Get Venues
            ViewBag.VenueId = new SelectList(db.Venues, "VenueId", "Name", selectedVenueId);

            // 2. Get Teachers (Filter by the exact Teacher Role ID in your database)
            var teacherRole = db.Roles.FirstOrDefault(r => r.Name == "Teacher");
            if (teacherRole != null)
            {
                var teachers = db.Users.Where(u => u.Roles.Any(r => r.RoleId == teacherRole.Id)).ToList();

                // Assuming your User model has FirstName and SecondName
                var teacherSelectList = teachers.Select(t => new SelectListItem
                {
                    Value = t.Id,
                    Text = t.FirstName + " " + t.SecondName,
                    Selected = (t.Id == selectedTeacherId)
                });

                ViewBag.UserId = teacherSelectList;
            }
            else
            {
                ViewBag.UserId = new SelectList(new List<SelectListItem>());
            }
        }
    }

}
