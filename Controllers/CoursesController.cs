using DAL;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ContactsDB.Controllers
{
    public class CoursesController : Controller
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
            return View();
        }

        public ActionResult GetCourses(bool fromToggle = false)
        {
            var courses = DB.Courses.ToList();
            return PartialView(courses);
        }

        public ActionResult CourseDetails(int id)
        {
            Course course = DB.Courses.Get(id);

            List<Registration> registrations = DB.Registrations.ToList();

            List<Student> registeredStudents = DB.Students.ToList()
                .Where(s => registrations.Any(r =>
                    r.CourseId == course.Id &&
                    r.StudentId == s.Id))
                .ToList();

            ViewBag.RegisteredStudents = registeredStudents;

            return View(course);
        }

        public ActionResult CourseEdit(int id)
        {
            var course = DB.Courses.Get(id);
            return View(course);
        }

        [HttpPost]
        public ActionResult CourseEdit(Course course)
        {
            DB.Courses.Update(course);
            return RedirectToAction("CourseDetails", new { id = course.Id });
        }

        public ActionResult Delete(int id)
        {
            DB.Courses.Delete(id);
            return RedirectToAction("List");
        }
    }
}