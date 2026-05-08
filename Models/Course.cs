using DAL;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Models
{
    public class Course : Record
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public int Session { get; set; }

        [JsonIgnore]
        public string FullCourseName => Code + " " + Title;

        [JsonIgnore]
        public List<Registration> Registrations =>
            DB.Registrations.ToList().Where(r => r.CourseId == Id).ToList();

        [JsonIgnore]
        public List<Student> Students
        {
            get
            {
                List<Student> students = new List<Student>();

                foreach (Registration registration in Registrations)
                {
                    Student student = DB.Students.Get(registration.StudentId);
                    if (student != null)
                        students.Add(student);
                }

                return students;
            }
        }

        [JsonIgnore]
        public List<Allocation> Allocations =>
            DB.Allocations.ToList().Where(a => a.CourseId == Id).ToList();

        [JsonIgnore]
        public List<Teacher> Teachers
        {
            get
            {
                List<Teacher> teachers = new List<Teacher>();

                foreach (Allocation allocation in Allocations)
                {
                    Teacher teacher = DB.Teachers.Get(allocation.TeacherId);
                    if (teacher != null)
                        teachers.Add(teacher);
                }

                return teachers;
            }
        }

    }
}