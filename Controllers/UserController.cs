using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
    



namespace SIMS_ASM.Controllers
{
    public class UserController : Controller
    {


        private readonly string _userDataFilePath = "data_user.json";
        private readonly string _courseDataFilePath = "course_data.json";
        private readonly string _timetableFilePath = "timetable_data.json";


        // Action để hiển thị thông tin người dùng dựa trên Id
        public IActionResult Profile(int id)
        {
            var user = GetUserById(id);
            if (user == null)
            {
                // Trả về trang lỗi nếu không tìm thấy người dùng
                return RedirectToAction("Error", "Home");
            }

            return View(user);
        }

        // Phương thức để lấy thông tin người dùng từ tệp user_data.json
        private User GetUserById(int userId)
        {
            var allUsers = GetAllUsers();
            return allUsers.FirstOrDefault(u => u.Id == userId);
        }

        // Phương thức để đọc dữ liệu từ tệp user_data.json
        private List<User> GetAllUsers()
        {
            if (System.IO.File.Exists(_userDataFilePath))
            {
                string json = System.IO.File.ReadAllText(_userDataFilePath);
                return JsonConvert.DeserializeObject<List<User>>(json);
            }
            return new List<User>();
        }

        private List<Course> GetAllCourses()
        {
            if (System.IO.File.Exists(_courseDataFilePath))
            {
                string json = System.IO.File.ReadAllText(_courseDataFilePath);
                return JsonConvert.DeserializeObject<List<Course>>(json);
            }
            return new List<Course>();
        }

        public IActionResult ViewCourses(int userId)
        {
            // Lấy danh sách tất cả các khóa học
            var allCourses = GetAllCourses();

            // Lấy danh sách tất cả người dùng
            var allUsers = GetAllUsers();

            // Tìm người dùng với userId tương ứng
            var user = allUsers.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                // Trả về lỗi nếu không tìm thấy người dùng
                return RedirectToAction("Error", "Home");
            }

            // Lọc ra các khóa học mà người dùng đã tham gia
            var userCourses = allCourses.Where(course => course.UserIds.Contains(userId)).ToList();

            // Truyền danh sách các khóa học đã được thêm cho người dùng đến view
            return View(userCourses);
        }





        public IActionResult ViewTimetable(int courseId)
        {
            // Lấy danh sách thời khóa biểu cho khóa học có Id là courseId
            var timetable = GetTimetableByCourseId(courseId);
            if (timetable == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(timetable);
        }

        private List<Timetable> GetTimetableByCourseId(int courseId)
        {
            if (System.IO.File.Exists(_timetableFilePath))
            {
                string json = System.IO.File.ReadAllText(_timetableFilePath);
                var timetableList = JsonConvert.DeserializeObject<List<Timetable>>(json);

                // Lọc thời khóa biểu theo courseId
                return timetableList.Where(t => t.CourseId == courseId).ToList();
            }
            return null;
        }











        public IActionResult Lecture_Course_View()
        {
            var allCourses = GetAllCourses();

            // Lấy danh sách sinh viên cho mỗi khóa học
            var courseStudents = new Dictionary<Course, List<User>>();
            foreach (var course in allCourses)
            {
                var students = new List<User>();
                foreach (var userId in course.UserIds)
                {
                    var user = GetAllUsers().FirstOrDefault(u => u.Id == userId);
                    if (user != null)
                    {
                        students.Add(user);
                    }
                }
                courseStudents.Add(course, students);
            }

            return View(courseStudents);
        }




        // Action để hiển thị trang xem thời khóa biểu của giáo viên
        public IActionResult Lecture_Timetable_View()
        {
            // Lấy danh sách tất cả các khóa học
            var allCourses = GetAllCourses();

            // Lấy thời khóa biểu cho từng khóa học
            var timetableDict = new Dictionary<Course, List<(Timetable timetable, string lecturerName)>>();
            foreach (var course in allCourses)
            {
                var timetable = GetTimetableForCourse(course.Id);
                var timetableWithLecturerName = timetable.Select(t =>
                {
                    var lecturerName = GetLecturerName(t.LectureId);
                    return (t, lecturerName);
                }).ToList();
                timetableDict.Add(course, timetableWithLecturerName);
            }

            // Truyền danh sách khóa học và thời khóa biểu đến view để hiển thị
            return View(timetableDict);
        }

        // Phương thức để lấy thời khóa biểu cho một khóa học cụ thể từ tệp timetable_data.json
        private List<Timetable> GetTimetableForCourse(int courseId)
        {
            if (System.IO.File.Exists(_timetableFilePath))
            {
                string json = System.IO.File.ReadAllText(_timetableFilePath);
                var allTimetables = JsonConvert.DeserializeObject<List<Timetable>>(json);
                return allTimetables.Where(t => t.CourseId == courseId).ToList();
            }
            return new List<Timetable>();
        }


        // Phương thức để lấy tên của giảng viên từ LectureId
        private string GetLecturerName(int lectureId)
        {
            // Lấy danh sách tất cả người dùng
            var allUsers = GetAllUsers();

            // Tìm người dùng có Id là lectureId và có vai trò là "Lecture"
            var lecturer = allUsers.FirstOrDefault(u => u.Id == lectureId && u.Role == "Lecture");

            // Nếu tìm thấy giảng viên, trả về tên của giảng viên đó
            if (lecturer != null)
            {
                return lecturer.Name;
            }
            // Nếu không tìm thấy, trả về một giá trị mặc định hoặc thông báo lỗi
            else
            {
                return "Unknown Lecturer"; // hoặc bạn có thể trả về null, throw một Exception, hoặc xử lý một cách phù hợp với yêu cầu của ứng dụng
            }
        }






    }
}
