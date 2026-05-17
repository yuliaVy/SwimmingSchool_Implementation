using SwimmingSchool_Implementation.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace SwimmingSchool_Implementation.Controllers
{
    public class HomeController : Controller
    {
        //create a database context to interact with the database 
        private SwimSchoolDbContext db = new SwimSchoolDbContext();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Team()
        {

            //find out roleId of "teacher"
            var roleId = db.Roles.FirstOrDefault(r => r.Name == "Teacher")?.Id;

            // Filter users who belong to that role ID
            var teachers = db.Users
                .Where(u => u.Roles.Any(r => r.RoleId == roleId))
                .ToList();
            //pass them to the view
            return View(teachers);
        }

        // 1. Loads the Contact Page
        [HttpGet]
        public ActionResult Contact()
        {
            return View();
        }

        // 2. Handles the Form Submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(ContactViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                string senderEmail = ConfigurationManager.AppSettings["SenderEmail"];
                string senderPassword = ConfigurationManager.AppSettings["SenderPassword"];
                string receiverEmail = ConfigurationManager.AppSettings["ReceiverEmail"];

                MailMessage mail = new MailMessage();

                mail.From = new MailAddress(senderEmail, "AquaLife Swimming School");

                mail.To.Add(receiverEmail);

                mail.Subject = $"New Contact Form Message from {model.FirstName} {model.SecondName}";

                mail.Body =
                    $"Name: {model.FirstName} {model.SecondName}\n\n" +
                    $"Email: {model.Email}\n\n" +
                    $"Phone: {model.Phone}\n\n" +
                    $"Message:\n{model.Message}";

                mail.IsBodyHtml = false;
                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;

                    smtp.UseDefaultCredentials = false;

                    smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);

                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                    smtp.Send(mail);
                }

                TempData["SuccessMessage"] =
                    "Thank you! Your message has been sent successfully.";

                return RedirectToAction("Contact");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View(model);
            }

            //    if (ModelState.IsValid)
            //    {
            //        try
            //        {
            //            // Setup the email message
            //            var mailMessage = new MailMessage();
            //            mailMessage.From = new MailAddress("kuzindabarambinda@gmail.com"); // Usually a dedicated system email
            //            mailMessage.To.Add("yuliiaaav@gmail.com"); // The manager's email
            //            mailMessage.Subject = $"New Aqua Life Enquiry from {model.FirstName} {model.SecondName}";
            //            mailMessage.Body = $"Name: {model.FirstName} {model.SecondName}\n" +
            //                               $"Email: {model.Email}\n" +
            //                               $"Phone: {model.Phone}\n\n" +
            //                               $"Message:\n{model.Message}";

            //            // Setup the SMTP Client (This example uses Gmail's SMTP server)
            //            using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
            //            {
            //                smtpClient.Credentials = new NetworkCredential("kuzindabarambinda@gmail.com", "mamisiki");
            //                smtpClient.EnableSsl = true;
            //                smtpClient.Send(mailMessage);
            //            }

            //            // Show a success message to the user
            //            TempData["SuccessMessage"] = "Thank you! Your message has been sent to our team.";
            //            return RedirectToAction("Contact");
            //        }
            //        catch (System.Exception ex)
            //        {
            //            // If something goes wrong with the email, show an error
            //            ModelState.AddModelError("", "Sorry, there was a problem sending your message. Please try calling us instead.");
            //        }
            //    }

            //    // If validation failed, return the view with the user's data so they can fix it
            //    return View(model);
        }
    }
}