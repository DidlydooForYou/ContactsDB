using DAL;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static Controllers.AccessControl;

namespace ContactsDB.Controllers
{
    public class TeachersController : Controller
    {
        public ActionResult ToggleSearch()
        {
            if (Session["ShowSearch"] == null) Session["ShowSearch"] = false;
            Session["ShowSearch"] = !(bool)Session["ShowSearch"];
            return RedirectToAction("List", new { fromToggle = true });
        }
        public ActionResult List(bool fromToggle = false)
        {
            if (!fromToggle)
            {
                Session["ShowSearch"] = false;
            }
            ViewBag.ShowSearch = Session["ShowSearch"] as bool? ?? false;
            return View();
        }

        public ActionResult GetTeachers()
        {
            ViewBag.ShowSearch = Session["ShowSearch"] as bool? ?? false;
            var profs = DB.Teachers.ToList();
            return PartialView(profs);
        }

        public ActionResult TeachersDetails(int id)
        {
            Teacher teacher = DB.Teachers.Get(id);

            List<Allocation> allocations = DB.Allocations.ToList()
                .Where(a => a.TeacherId == teacher.Id)
                .ToList();

            ViewBag.Allocations = allocations;
            ViewBag.Courses = DB.Courses.ToList();

            return View(teacher);
        }

        [UserAccess(Access.Write)]
        public ActionResult Delete(int id)
        {
            DB.Teachers.Delete(id);
            return RedirectToAction("List");
        }

        [UserAccess(Access.Write)]
        public ActionResult TeachersEdit(int id)
        {
            var prof = DB.Teachers.Get(id);
            return View(prof);
        }

        [HttpPost]
        [UserAccess(Access.Write)]
        public ActionResult TeachersEdit(Teacher prof)
        {
            DB.Teachers.Update(prof);
            return RedirectToAction("TeachersDetails", new { id = prof.Id });
        }


        [UserAccess(Access.Write)]
        public ActionResult Create()
        {
            ViewBag.ReturnUrl = Request.UrlReferrer?.ToString();
            Teacher teacher = new Teacher();

            teacher.StartDate = DateTime.Now;
            teacher.Avatar = "no_avatar.png";

            string randomDigits = new Random().Next(100000, 999999).ToString();

            teacher.Code = $"CLG-420-{randomDigits}";

            return View(teacher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAccess(Access.Write)]
        public ActionResult Create(Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                string randomDigits = new Random().Next(0, 6).ToString();

                teacher.Code = $"CLG-420-{randomDigits}";

                DB.Teachers.Add(teacher);

                return RedirectToAction("TeachersDetails", new { id = teacher.Id });
            }
            return View(teacher);
        }

        [HttpPost]
        [UserAccess(Access.Write)]
        public ActionResult Edit(Teacher teacher, string SelectedCourseIds)
        {
            DB.Teachers.Update(teacher);

            int currentYear = Session["CurrentYear"] != null
                ? int.Parse(Session["CurrentYear"].ToString())
                : NextSession.Year;

            List<int> selectedIds = new List<int>();

            if (!string.IsNullOrEmpty(SelectedCourseIds))
            {
                selectedIds = SelectedCourseIds
                    .Split(',')
                    .Select(id => int.Parse(id))
                    .ToList();
            }

            var allocations = DB.Allocations.ToList()
                .Where(a => a.TeacherId == teacher.Id &&
                            a.Year == currentYear)
                .ToList();

            foreach (var allocation in allocations)
            {
                DB.Allocations.Delete(allocation.Id);
            }

            foreach (int courseId in selectedIds)
            {
                DB.Allocations.Add(new Allocation
                {
                    TeacherId = teacher.Id,
                    CourseId = courseId,
                    Year = currentYear
                });
            }

            return RedirectToAction("TeachersDetails", new { id = teacher.Id });
        }
    }
}