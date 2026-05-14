using DAL;
using System;
using System.Linq;
using System.Web.Helpers;

namespace Models
{
    public class StudentsRepository : Repository<Student>
    {
        public override int Add(Student student)
        {
            return base.Add(student);
        }

        public override bool Update(Student student)
        {
            Student stored = Get(student.Id);
            if (stored == null)
                return false;

            if (ToList().Any(s =>
                s.Id != student.Id &&
                !string.IsNullOrWhiteSpace(s.Email) &&
                s.Email.Equals(student.Email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("Ce courriel existe déjà.");
            }

            if (ToList().Any(s =>
                s.Id != student.Id &&
                !string.IsNullOrWhiteSpace(s.Code) &&
                s.Code.Equals(student.Code, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("Ce code étudiant existe déjà.");
            }

            stored.FirstName = student.FirstName;
            stored.LastName = student.LastName;
            stored.Email = student.Email;
            stored.Phone = student.Phone;
            stored.BirthDate = student.BirthDate;

            return base.Update(stored);
        }


        public override bool Delete(int studentId)
        {
            try
            {
                Student student = Get(studentId);
                if (student == null)
                    return false;

                BeginTransaction();

                foreach (var reg in student.Registrations)
                    DB.Registrations.Delete(reg.Id);

                bool result = base.Delete(studentId);

                EndTransaction();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Student delete failed: {ex.Message}");
                EndTransaction();
                return false;
            }
        }
    }
}
