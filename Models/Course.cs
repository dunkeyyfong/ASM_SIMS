using System;
using System.Collections.Generic;

namespace SIMS_ASM.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TotalNumberOfStudents { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int AttendanceSessions { get; set; }

        public List<int> UserIds { get; set; } = new List<int>();


    }
}
