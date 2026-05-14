using DAL;
using System;
using System.Linq;

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
            try
            {
                BeginTransaction();

                Student stored = Get(student.Id);
                if (stored == null)
                {
                    EndTransaction();
                    return false;
                }

                stored.FirstName = student.FirstName;
                stored.LastName = student.LastName;
                stored.Email = student.Email;
                stored.Phone = student.Phone;
                stored.BirthDate = student.BirthDate;

                bool result = base.Update(stored);

                EndTransaction();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Student update failed: {ex.Message}");
                EndTransaction();
                return false;
            }
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
