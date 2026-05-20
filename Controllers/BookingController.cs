using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SwimmingSchool_Implementation.Models;
using System.Data.Entity;

namespace SwimmingSchool_Implementation.Controllers
{
    public class BookingController : Controller
    {
        //instance of the database context class to interact
        private SwimSchoolDbContext db = new SwimSchoolDbContext();

        // GET: Booking
        public ActionResult Index(int? venueId, LessonType? lessonType)
        {
            // ALL venues
            var venues = db.Venues.ToList();

            // LESSONS QUERY
            var lessonsQuery = db.Lessons
                .Include(l => l.Teacher)
                .Include(l => l.Venue)
                .Include(l => l.LessonsBookings)
                .Where(l => l.BlockStartDate >= DateTime.Today)
                .AsQueryable();

            // FILTER BY VENUE
            if (venueId.HasValue)
            {
                lessonsQuery = lessonsQuery
                    .Where(l => l.VenueId == venueId.Value);
            }

            // FILTER BY TYPE
            if (lessonType.HasValue)
            {
                lessonsQuery = lessonsQuery
                    .Where(l => l.LessonType == lessonType.Value);
            }

            var lessons = lessonsQuery.ToList();

            // GET CURRENT VENUE
            var selectedVenue = venueId.HasValue
                ? db.Venues.FirstOrDefault(v => v.VenueId == venueId.Value)
                : null;

            // FIND TEACHER ROLE
            var teacherRoleId = db.Roles
                .FirstOrDefault(r => r.Name == "Teacher")?.Id;
            // GET TEACHERS
            var teachers = db.Users
                .Where(u =>
                    u.Roles.Any(r => r.RoleId == teacherRoleId)
                    &&
                    (!venueId.HasValue ||
                     u.TeacherVenues.Any(tv => tv.VenueId == venueId.Value))
                    &&
                    (
                        !lessonType.HasValue
                        ||
                        u.TeacherPreference == TeacherPreference.Both
                        ||
                        (lessonType == LessonType.Kids &&
                         u.TeacherPreference == TeacherPreference.Kids)
                        ||
                        (lessonType == LessonType.Adult &&
                         u.TeacherPreference == TeacherPreference.Adults)
                    )
                )
                .ToList();

            // VIEW MODEL
            var model = new BookingPageViewModel
            {
                Lessons = lessons,
                Teachers = teachers,
                Venues = venues,
                SelectedVenueId = venueId,
                SelectedLessonType = lessonType,
                SelectedVenue = selectedVenue
            };

            return View(model);
        }

        // GET: Booking/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Booking/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Booking/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Booking/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Booking/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Booking/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Booking/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
