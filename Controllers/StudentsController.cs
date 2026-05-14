using DAL;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.ModelBinding;
using System.Web.Mvc;
using static Controllers.AccessControl;

namespace ContactsDB.Controllers
{
    public class StudentsController : Controller
    {
        public ActionResult ToggleSearch()
        {
            if (Session["ShowSearch"] == null) Session["ShowSearch"] = false;
            Session["ShowSearch"] = !(bool)Session["ShowSearch"];
            return RedirectToAction("List", new { fromToggle = true });
        }

        public ActionResult List(string search = "", bool fromToggle = false)
        {
            if (!fromToggle)
            {
                Session["ShowSearch"] = false;
            }

            ViewBag.ShowSearch = Session["ShowSearch"] as bool? ?? false;
            ViewBag.Search = search;

            var students = DB.Students.ToList().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                students = students.Where(s => s.LastName.Contains(search) ||
                                               s.FirstName.Contains(search) ||
                                               s.Year.ToString().Contains(search));

            return View(students.ToList());
        }
        public ActionResult GetStudent(string search = "")
        {
            ViewBag.Search = search;
            var students = DB.Students.ToList();
            return PartialView("GetStudent", students);
        }



        public ActionResult GetStudentDetails(int id, bool fromToggle = false)
        {
            if (!fromToggle)
            {
                Session["ShowSearch"] = false;
            }
            var student = DB.Students.ToList().FirstOrDefault(s => s.Id == id);
            return View(student);
        }

        [UserAccess(Access.Write)]
        public ActionResult StudentEdit(int id)
        {
            var student = DB.Students.Get(id);
            return View(student);
        }

        [HttpPost]
        [UserAccess(Access.Write)]
        public ActionResult TeachersEdit(Student student)
        {
            DB.Students.Update(student);
            return RedirectToAction("GetStudentDetails", new { id = student.Id });
        }

        [UserAccess(Access.Write)]
        public ActionResult SessionCourante()
        {
            ViewBag.ReturnUrl = Request.UrlReferrer?.ToString();
            var model = new SessionCourante
            {
                Annee = Session["CurrentYear"] != null ? (int)Session["CurrentYear"] : NextSession.Year,
                Session = Session["CurrentSession"] != null ? Session["CurrentSession"].ToString() :
                           (NextSession.ValidSessions.Contains(1) ? "Automne" : "Hiver")
            };

            return View(model);
        }

        [HttpPost]
        [UserAccess(Access.Write)]
        public ActionResult SessionCourante(SessionCourante model)
        {
            Session["CurrentYear"] = model.Annee;
            Session["CurrentSession"] = model.Session;

            return RedirectToAction("List");
        }

        public ActionResult GetTeachers()
        {
            var profs = DB.Teachers.ToList();
            return View(profs);
        }

        [UserAccess(Access.Write)]
        public ActionResult Create()
        {
            ViewBag.ReturnUrl = Request.UrlReferrer?.ToString();
            Student student = new Student();
            student.BirthDate = DateTime.Now;

            int year = Session["CurrentYear"] != null
                ? (int)Session["CurrentYear"]
                : NextSession.Year;

            string randomDigits = new Random().Next(100000, 999999).ToString();

            student.Code = $"{year}{randomDigits}";

            return View(student);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAccess(Access.Write)]
        public ActionResult Create(Student student)
        {
            if (ModelState.IsValid)
            {
                int year = Session["CurrentYear"] != null
                    ? (int)Session["CurrentYear"]
                    : NextSession.Year; 

                string randomDigits = new Random().Next(0, 7).ToString();

                student.Code = $"{year}{randomDigits}";

                DB.Students.Add(student);

                return RedirectToAction("GetStudentDetails", new { id = student.Id });
            }
            return View(student);
        }

        [HttpPost]
        [UserAccess(Access.Write)]
        public ActionResult Edit(Student student, string SelectedCourseIds)
        {
            DB.Students.Update(student);

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

            var registrations = DB.Registrations.ToList()
                .Where(r => r.StudentId == student.Id && r.Year == currentYear)
                .ToList();

            foreach (var registration in registrations)
            {
                DB.Registrations.Delete(registration.Id);
            }

            foreach (int courseId in selectedIds)
            {
                DB.Registrations.Add(new Registration
                {
                    StudentId = student.Id,
                    CourseId = courseId,
                    Year = currentYear
                });
            }

            return RedirectToAction("GetStudentDetails", new { id = student.Id });
        }
    }
}