using DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Models
{
    public class Teacher : Record
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Code { get; set; }
        public DateTime StartDate { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Avatar { get; set; }

        [JsonIgnore]
        public string FullName => FirstName + " " + LastName;

        [JsonIgnore]
        public List<Allocation> Allocations =>
            DB.Allocations.ToList().Where(a => a.TeacherId == Id).ToList();

        [JsonIgnore]
        public List<Course> Courses
        {
            get
            {
                List<Course> courses = new List<Course>();

                foreach (Allocation allocation in Allocations)
                {
                    Course course = DB.Courses.Get(allocation.CourseId);
                    if (course != null)
                        courses.Add(course);
                }

                return courses;
            }
        }
    }
}