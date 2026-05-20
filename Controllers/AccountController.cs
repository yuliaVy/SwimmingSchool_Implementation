using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using Stripe;
using Stripe.Checkout;
using SwimmingSchool_Implementation.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SwimmingSchool_Implementation.Controllers
{
    [Authorize]

    public class AccountController : Controller
    {
        //create a database context to interact with the database 
        private SwimSchoolDbContext db = new SwimSchoolDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

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

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        /// <summary>
        /// The user clicks the register button on the booking page, which sends a list of selected lesson IDs to this action.
        /// There's no option for user to register without booking a lesson, so we can assume that the list of lesson IDs will always be provided when the user clicks register.
        /// </summary>
        [AllowAnonymous]
        public ActionResult Register(int lessonId)
        {
            var selectedLesson = db.Lessons.FirstOrDefault(p => p.Id == lessonId);

            var model = new RegisterViewModel
            {
                Students = new List<StudentViewModel>
        {
            new StudentViewModel
            {
                SelectedLessonId = lessonId,
                SelectedLessonTitle = selectedLesson != null ? $"{selectedLesson.Title} ({selectedLesson.DayOfWeek})" : "",
                LessonPrice = selectedLesson?.Price ?? 0
            }
        }
            };

            ViewBag.Policies = db.Policies.Where(p => p.IsRequired).ToList();

            // Pass ALL available lessons to the view so the Modal can display them
            ViewBag.AllLessons = db.Lessons.Include(l => l.Venue).Where(l => l.AvailablePlaces > 0).ToList();

            //fetch venues from the database and pass them to the view
            ViewBag.Venues = db.Venues.ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            // Re-populate view drop-downs if validation fails
            ViewBag.Policies = db.Policies.Where(p => p.IsRequired).ToList();
            ViewBag.AllLessons = db.Lessons.Include(l => l.Venue).Where(l => l.AvailablePlaces > 0).ToList();
            ViewBag.Venues = db.Venues.ToList();

            if (!ModelState.IsValid)
                return View(model);

            // PRE-CHECK: Ensure spots are still open before sending them to pay
            var requestedSpots = model.Students.GroupBy(s => s.SelectedLessonId).ToDictionary(g => g.Key, g => g.Count());
            foreach (var request in requestedSpots)
            {
                var lesson = db.Lessons.Find(request.Key);
                if (lesson == null || lesson.AvailablePlaces < request.Value)
                {
                    ModelState.AddModelError("", $"Sorry, '{lesson?.Title}' just filled up. Please select a different class.");
                    return View(model);
                }
            }

            // Calculate the Grand Total for all students
            decimal grandTotal = 0;
            foreach (var studentVm in model.Students)
            {
                var lesson = db.Lessons.Find(studentVm.SelectedLessonId);
                if (lesson != null) grandTotal += lesson.Price;
            }
            // Calculate what they actually owe TODAY based on the Deposit checkbox
            decimal amountToPayToday = model.PayDepositOnly ? (grandTotal * 0.20m) : grandTotal;

            // We are putting the whole 'model' in a safe box called "PendingRegistration"
            Session["PendingRegistration"] = model;

            // Redirect based on chosen button path
            if (model.PaymentMethod == "Stripe")
            {
                return RedirectToStripeCheckout(amountToPayToday, model);
            }
            else if (model.PaymentMethod == "PayPal")
            {
                return await RedirectToPayPalCheckout(amountToPayToday, model);
            }

            ModelState.AddModelError("", "Invalid payment method selected.");
            return View(model);

            
        }

        // =================================================================
        // Stripe Checkout Logic - This is where we create the Stripe Session and redirect the user to the Stripe-hosted payment page
        // =================================================================
        private ActionResult RedirectToStripeCheckout(decimal amount, RegisterViewModel model)
        {
            var domain = Request.Url.GetLeftPart(UriPartial.Authority);

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                // Stripe expects amounts in pence/cents (e.g., £10.00 = 1000)
                                UnitAmount = (long)(amount * 100),
                                Currency = "gbp",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = model.PayDepositOnly ? "Aqua Life - 20% Deposit" : "Aqua Life - Full Payment",
                                    Description = $"Booking for {model.Students.Count} swimmer(s)."
                                },
                            },
                            Quantity = 1,
                        },
                    },
                Mode = "payment",
                // Pass the generated booking ID so the Success page knows which booking to mark as Paid!
                SuccessUrl = domain + "/Account/Success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = domain + "/Account/Cancel",
            };

            var service = new SessionService();
            Session session = service.Create(options);

            // Redirect the user to Stripe's secure hosted checkout page
            Response.Headers.Add("Location", session.Url);
            return new HttpStatusCodeResult(303);

        }

        private async Task<ActionResult> RedirectToPayPalCheckout(decimal amount, RegisterViewModel model)
        {
            // Pull the keys from your Web.config
            string clientId = ConfigurationManager.AppSettings["PayPalClientId"];
            string secret = ConfigurationManager.AppSettings["PayPalSecret"];

            // Use SandboxEnvironment for testing. When you go live, change this to LiveEnvironment()
            var environment = new SandboxEnvironment(clientId, secret);
            var client = new PayPalHttpClient(environment);

            var domain = Request.Url.GetLeftPart(UriPartial.Authority);

            // 2. CREATE THE ORDER BLUEPRINT
            var order = new OrderRequest()
            {
                // "CAPTURE" tells PayPal you want to charge the user immediately 
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new System.Collections.Generic.List<PurchaseUnitRequest>()
                {
                    new PurchaseUnitRequest()
                    {
                        AmountWithBreakdown = new AmountWithBreakdown()
                        {
                            CurrencyCode = "GBP",
                            // PayPal requires the amount as a string with exactly 2 decimal places
                            Value = amount.ToString("0.00")
                        },
                        Description = model.PayDepositOnly ? "Aqua Life - 20% Deposit" : "Aqua Life - Full Payment"
                    }
                },
                    ApplicationContext = new ApplicationContext()
                    {
                        BrandName = "Aqua Life Swim School",
                        // "PAY_NOW" changes the PayPal button from "Continue" to "Pay Now"
                        UserAction = "PAY_NOW",
                        // PayPal will automatically attach "?token=EC-12345..." to this URL when it redirects back!
                        ReturnUrl = domain + "/Account/Success",
                        CancelUrl = domain + "/Account/RegisterCancel"
                    }
            };
            // 3. SEND THE REQUEST TO PAYPAL
            var request = new OrdersCreateRequest();
            request.Prefer("return=representation");
            request.RequestBody(order);

            try
            {
                // Execute the API call
                var response = await client.Execute(request);
                var result = response.Result<Order>();

                // 4. FIND THE REDIRECT LINK
                // PayPal returns a list of links. We need to find the one labeled "approve", 
                // which is the hosted checkout page where the user types in their password.
                var approveLink = result.Links.FirstOrDefault(link => link.Rel == "approve");

                if (approveLink != null)
                {
                    // Redirect the user's browser to the PayPal checkout page
                    return Redirect(approveLink.Href);
                }
                else
                {
                    // Fallback if PayPal's API behaves unexpectedly
                    ModelState.AddModelError("", "Unable to generate PayPal checkout link.");
                    return View("Register", model);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PayPal Checkout Error: " + ex.Message);
                ModelState.AddModelError("", "Error communicating with PayPal. Please try again.");
                return View("Register", model);
            }

        }

        [AllowAnonymous]
        // Add token and PayerID as optional parameters to catch PayPal's redirect data
        public async Task<ActionResult> Success(string session_id = null, string token = null, string PayerID = null)
        {
            // 1. GET THE DATA BACK OUT OF THE SAFE BOX FIRST
            // We must do this first so we know WHICH payment method they selected!
            var model = Session["PendingRegistration"] as RegisterViewModel;

            if (model == null)
            {
                // Safety net: Session expired or they refreshed the page
                return RedirectToAction("Index", "Home");
            }

            // 2. VERIFY THE PAYMENT BASED ON THE CHOSEN GATEWAY
            bool isPaymentValid = false;
            decimal totalCollected = 0;

            try
            {
                if (model.PaymentMethod == "Stripe")
                {
                    // STRIPE VERIFICATION
                    if (string.IsNullOrEmpty(session_id)) return RedirectToAction("PaymentFailed");

                    var service = new SessionService();
                    Stripe.Checkout.Session stripeSession = service.Get(session_id);

                    if (stripeSession != null && stripeSession.PaymentStatus == "paid")
                    {
                        isPaymentValid = true;
                        totalCollected = (decimal)(stripeSession.AmountTotal / 100.00);
                    }
                }
                else if (model.PaymentMethod == "PayPal")
                {
                    // 1. Set up the PayPal Environment
                    string clientId = System.Configuration.ConfigurationManager.AppSettings["PayPalClientId"];
                    string secret = System.Configuration.ConfigurationManager.AppSettings["PayPalSecret"];

                    // Change to LiveEnvironment() when you launch your site for real
                    var environment = new PayPalCheckoutSdk.Core.SandboxEnvironment(clientId, secret);
                    var client = new PayPalCheckoutSdk.Core.PayPalHttpClient(environment);

                    // 2. Create the Capture Request using the Token (which is the Order ID)
                    var request = new PayPalCheckoutSdk.Orders.OrdersCaptureRequest(token);
                    request.RequestBody(new PayPalCheckoutSdk.Orders.OrderActionRequest());

                    // 3. Execute the Request to officially capture the funds
                    var response = await client.Execute(request);
                    var resultt = response.Result<PayPalCheckoutSdk.Orders.Order>();

                    // 4. Verify the Status
                    if (resultt.Status == "COMPLETED")
                    {
                        isPaymentValid = true;

                        // 5. Extract the exact amount PayPal actually charged them
                        var captureAmountStr = resultt.PurchaseUnits[0].Payments.Captures[0].Amount.Value;

                        if (decimal.TryParse(captureAmountStr, out decimal parsedAmount))
                        {
                            totalCollected = parsedAmount;
                        }
                        else
                        {
                            // Fallback calculation just in case the string parsing fails
                            decimal subtotall = model.Students.Sum(s => db.Lessons.Find(s.SelectedLessonId)?.Price ?? 0);
                            totalCollected = model.PayDepositOnly ? (subtotall * 0.20m) : subtotall;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"PayPal Capture Failed. Status: {resultt.Status}");
                        return RedirectToAction("PaymentFailed");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the API error securely
                System.Diagnostics.Debug.WriteLine($"Payment Gateway Error: {ex.Message}");
                return RedirectToAction("PaymentFailed");
            }

            // 3. IF PAYMENT IS INVALID, KICK THEM OUT
            if (!isPaymentValid)
            {
                return RedirectToAction("PaymentFailed");
            }

            // =========================================================
            // SAFE TO WRITE TO THE DATABASE
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                SecondName = model.SecondName,
                PhoneNumber = model.PhoneNumber,
                DateRegistered = DateTime.Now
            };

            // Group the students by the lesson they selected to see how many total spots this family needs per class
            var requestedSpotsPerLesson = model.Students
                .GroupBy(s => s.SelectedLessonId)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var request in requestedSpotsPerLesson)
            {
                var lessonId = request.Key;
                var spotsNeeded = request.Value;

                var dbLesson = db.Lessons.Find(lessonId);

                // If the lesson doesn't exist, or they are trying to book 2 kids into a class with 1 spot left
                if (dbLesson == null || dbLesson.AvailablePlaces < spotsNeeded)
                {
                    ModelState.AddModelError("", $"We're sorry! Another customer just booked spots in '{dbLesson?.Title}'. There are only {dbLesson?.AvailablePlaces} openings left.");
                    return View(model); // Kick them back to the form with the error message
                }
            }

            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var result = await userManager.CreateAsync(user, model.Password);
            //assign the user to the member role
            await UserManager.AddToRoleAsync(user.Id, "Learner");

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error);
                return View(model);
            }


            // 2. LOOP THROUGH EACH STUDENT AND CREATE THEIR BOOKINGS
            foreach (var studentVm in model.Students)
            {

                var lesson = db.Lessons.Find(studentVm.SelectedLessonId);
                if (lesson == null) continue; // Skip if invalid lesson
                //check the amount paid for a lesson
                decimal amountPaidForThisLesson = model.PayDepositOnly ? (lesson.Price * 0.20m) : lesson.Price;

                // Create Student
                var student = new Student
                {
                    FirstName = studentVm.FirstName,
                    SecondName = studentVm.LastName,
                    Gender = studentVm.Gender,
                    DateOfBirth = studentVm.BirthDate,
                    MedicalConditions = studentVm.MedicalConditions,
                    Allergies = studentVm.Allergies,
                    Medications = studentVm.Medications,
                    AquaticGoals = studentVm.AquaticGoals,
                    SwimExperience = studentVm.SwimExperience
                };
                db.Students.Add(student);
                db.SaveChanges(); // Generates student.Id

                // Create Booking
                var booking = new Booking
                {
                    UserId = user.Id,
                    StudentId = student.Id,
                    BookingDate = DateTime.Now,
                    TotalAmount = lesson.Price,
                    AmountPaid = amountPaidForThisLesson,
                    AdminNotes = "Standard Registration",
                    Status = BookingStatus.Completed
                };
                db.Bookings.Add(booking);
                db.SaveChanges(); // Generates booking.BookingId

                // Attach Lesson to Booking
                db.LessonsBookings.Add(new LessonBooking
                {
                    BookingId = booking.BookingId,
                    LessonId = studentVm.SelectedLessonId
                });
                db.SaveChanges();
                // NEW: GENERATE THE 4 CHILD BOOKING SESSIONS (4 Weeks)

                // Find the date of their very first class
                DateTime firstClassDate = GetNextOccurrenceOfDay(lesson.DayOfWeek.ToString());

                // If you have a specific time (e.g., 14:30), you can combine the date and time here:
                // firstClassDate = firstClassDate.Add(lesson.StartTime);

                // Loop 4 times to create 4 weeks of sessions
                for (int i = 0; i < 4; i++)
                {
                    var session = new BookingSession
                    {
                        BookingId = booking.BookingId,
                        // Add 7 days for every week (Week 0 = +0 days, Week 1 = +7 days, etc.)
                        SessionDate = firstClassDate.AddDays(i * 7),
                        Status = SessionStatus.Scheduled
                    };

                    db.BookingSessions.Add(session);
                }

                // Save the 4 generated sessions to the database
                db.SaveChanges();

                // THE DEDUCTION (Secure the spot!)
                lesson.AvailablePlaces -= 1;
                db.Entry(lesson).State = System.Data.Entity.EntityState.Modified;

                // Add Policies Specific to this student
                if (studentVm.AcceptedPolicies != null)
                {
                    foreach (var policyId in studentVm.AcceptedPolicies)
                    {
                        db.PolicyAgreements.Add(new PolicyAgreement
                        {
                            BookingId = booking.BookingId,
                            PolicyId = policyId,
                            Accepted = true,
                            AgreementDate = DateTime.Now
                        });
                    }
                }

                // Add Payment record
                db.Payments.Add(new Payment
                {
                    BookingId = booking.BookingId,
                    Amount = amountPaidForThisLesson,
                    PaymentDate = DateTime.Now,
                    Success = true
                });

                db.SaveChanges();
            }

            SendConfirmationEmail(model.Email, model.FirstName);
            decimal subtotal = model.Students.Sum(s => db.Lessons.Find(s.SelectedLessonId)?.Price ?? 0);
            decimal balanceRemaining = model.PayDepositOnly ? (subtotal - totalCollected) : 0;

            // 5. BUILD THE SUCCESS VIEW MODEL FOR THE RECEIPT SCREEN
            var successModel = new BookingSuccessViewModel
            {
                AccountHolderName = $"{model.FirstName} {model.SecondName}",
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                PaymentMethod = model.PaymentMethod,
                TotalPaidToday = totalCollected, // From the Stripe/PayPal verification
                PaymentType = model.PayDepositOnly ? "20% Deposit" : "Paid in Full",
                TransactionDate = DateTime.Now,
                OutstandingBalance = balanceRemaining
            };

            foreach (var student in model.Students)
            {
                // Pull the lesson from the DB to get the Venue and Day names
                var lesson = db.Lessons.Include(l => l.Venue).FirstOrDefault(l => l.Id == student.SelectedLessonId);

                if (lesson != null)
                {
                    var studentDetail = new SuccessStudentDetail
                    {
                        StudentName = $"{student.FirstName} {student.LastName}",
                        ClassName = lesson.Title,
                        DayOfWeek = lesson.DayOfWeek.ToString(),
                        LessonTime = lesson.StartTime.ToString(@"hh\:mm"),
                        Venue = lesson.Venue?.Name ?? "TBC",
                        ClassPrice = lesson.Price

                    };

                    // Calculate the 4 specific dates for the receipt
                    DateTime firstDate = GetNextOccurrenceOfDay(lesson.DayOfWeek.ToString());
                    for (int i = 0; i < 4; i++)
                    {
                        studentDetail.SessionDates.Add(firstDate.AddDays(i * 7));
                    }
                    successModel.Students.Add(studentDetail);
                }
            }

            // 6. Clear the session memory cache container so they can't refresh and duplicate
            Session["PendingRegistration"] = null;

            // Pass the specialized model to the view!
            return View(successModel);
        }


        private void SendConfirmationEmail(string customerEmail, string customerName)
        {
           
            try
            {
                string senderEmail = ConfigurationManager.AppSettings["SenderEmail"];
                string senderPassword = ConfigurationManager.AppSettings["SenderPassword"];
                var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(senderEmail, "AquaLife Swimming School");
                mailMessage.To.Add(customerEmail);
                mailMessage.Subject = "Aqua Life - Booking Confirmation";
                mailMessage.Body = $"Hi {customerName},\n\n" +
                                    $"Thank you for registering with Aqua Life! We have successfully received your payment.\n\n" +
                                    $"We look forward to seeing you in the pool!\n\n" +
                                    $"- The Aqua Life Team";

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
                // Log the error, but don't crash the application if an email fails to send!
                System.Diagnostics.Debug.WriteLine($"Email failed to send: {ex.Message}");
            }
        }

        private DateTime GetNextOccurrenceOfDay(string dayOfWeek)
        {
            // Convert the string (e.g., "Tuesday") into a C# DayOfWeek enum
            if (!Enum.TryParse(dayOfWeek, true, out DayOfWeek targetDay))
            {
                targetDay = DayOfWeek.Monday; // Fallback just in case
            }

            DateTime today = DateTime.Today;
            // Calculate how many days until the next target day
            int daysUntil = ((int)targetDay - (int)today.DayOfWeek + 7) % 7;

            // If they book ON a Tuesday for a Tuesday class, let's assume they start next week 
            // (You can change this to 0 if you want them to start the same day)
            if (daysUntil == 0)
            {
                daysUntil = 7;
            }

            return today.AddDays(daysUntil);
        }


        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new User { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
    {
        public ChallengeResult(string provider, string redirectUri)
            : this(provider, redirectUri, null)
        {
        }

        public ChallengeResult(string provider, string redirectUri, string userId)
        {
            LoginProvider = provider;
            RedirectUri = redirectUri;
            UserId = userId;
        }

        public string LoginProvider { get; set; }
        public string RedirectUri { get; set; }
        public string UserId { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
            if (UserId != null)
            {
                properties.Dictionary[XsrfKey] = UserId;
            }
            context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
        }
    }
    #endregion
    }   
}
