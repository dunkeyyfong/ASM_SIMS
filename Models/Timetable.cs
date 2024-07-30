namespace SIMS_ASM.Models
{
    public class Timetable
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int LectureId { get; set; }
        public string Room { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string DayOfWeek { get; set; }
    }


}
