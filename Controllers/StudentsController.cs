using DAL;
using Models;
using System;
using System.Linq;
using System.Web.Mvc;

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

        public ActionResult StudentEdit(int id)
        {
            var student = DB.Students.Get(id);
            return View(student);
        }

        [HttpPost]
        public ActionResult TeachersEdit(Student student)
        {
            DB.Students.Update(student);
            return RedirectToAction("GetStudentDetails", new { id = student.Id });
        }

        public ActionResult SessionCourante()
        {
            var model = new SessionCourante
            {
                Annee = Session["CurrentYear"] != null ? (int)Session["CurrentYear"] : NextSession.Year,
                Session = Session["CurrentSession"] != null ? Session["CurrentSession"].ToString() :
                           (NextSession.ValidSessions.Contains(1) ? "Automne" : "Hiver")
            };

            return View(model);
        }

        [HttpPost]
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

        public ActionResult Create()
        {
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
    }
}