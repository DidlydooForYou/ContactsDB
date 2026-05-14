using DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Models
{
    public class Student : Record
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Code { get; set; }
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        [JsonIgnore]
        public string FullName => FirstName + " " + LastName;

        [JsonIgnore]
        public int Year
        {
            get
            {
                if (string.IsNullOrEmpty(Code) || Code.Length < 4)
                    return 0;

                return int.Parse(Code.Substring(0, 4));
            }
        }

        [JsonIgnore]
        public List<Registration> Registrations =>
            DB.Registrations.ToList().Where(r => r.StudentId == Id).ToList();

        [JsonIgnore]
        public List<Course> Courses
        {
            get
            {
                List<Course> courses = new List<Course>();

                foreach (Registration registration in Registrations)
                {
                    Course course = DB.Courses.Get(registration.CourseId);
                    if (course != null)
                        courses.Add(course);
                }

                return courses;
            }
        }

        public override bool IsValid()
        {
            if (!HasRequiredLength(FirstName, 1)) return false;
            if (!HasRequiredLength(LastName, 1)) return false;
            if (!HasRequiredLength(Code, 10)) return false;
            if (!HasRequiredLength(Email, 1)) return false;
            if (!HasRequiredLength(Phone, 1)) return false;

            if (BirthDate > DateTime.Today) return false;
            if (BirthDate < DateTime.Today.AddYears(-120)) return false;
            if (BirthDate > DateTime.Today.AddYears(-17)) return false;

            if (DB.Students.ToList().Any(s => s.Code == Code && s.Id != Id)) return false;
            if (DB.Students.ToList().Any(s => s.Email == Email && s.Id != Id)) return false;

            return true;
        }
    }
}