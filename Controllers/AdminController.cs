using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SIMS_ASM.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SIMS_ASM.Controllers
{
    public class AdminController : Controller
    {
        private readonly string _userDataFilePath = "data_user.json";
        private readonly string _courseDataFilePath = "course_data.json";
        private readonly string _timetableDataFilePath = "timetable_data.json";


        [HttpGet]
        public IActionResult UserManagement()
        {
            return View();
        }

        // Action để hiển thị danh sách sinh viên
        [HttpPost]
        public IActionResult UserManagement(string userType)
        {
            var users_json = GetAllUsersFromJson();
            ViewBag.UserType = userType; // Lưu userType vào ViewBag
            if (userType == "Student")
            {
                var users = users_json.Where(u => u.Role == "Student").ToList();
                return View(users);
            }
            else if (userType == "Lecture")
            {
                var users = users_json.Where(u => u.Role == "Lecture").ToList();
                return View(users);
            }

            // Trường hợp mặc định: hiển thị toàn bộ người dùng
            return View(users_json);
        }



        // Lấy toàn bộ danh sách người dùng từ tệp JSON
        private List<User> GetAllUsersFromJson()
        {
            string jsonData = System.IO.File.ReadAllText(_userDataFilePath);
            var users = JsonConvert.DeserializeObject<List<User>>(jsonData);
            return users;
        }



        // Lấy thông tin sinh viên theo ID
        private User GetUserById(int id)
        {
            var users = GetAllUsersFromJson();
            return users.FirstOrDefault(u => u.Id == id);
        }

        // Chức năng chỉnh sửa user
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            var user = GetUserById(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // Xử lý chỉnh sửa user
        [HttpPost]
        public IActionResult EditUser(User updatedUser)
        {
            var users = GetAllUsersFromJson();
            var existingUser = users.FirstOrDefault(u => u.Id == updatedUser.Id);
            if (existingUser == null)
                return NotFound();

            UpdateUserDetails(existingUser, updatedUser);
            SaveUsersToJson(users);
            TempData["SuccessMessage"] = $"Account {existingUser.Email} update successfully.";

            return RedirectToAction("UserManagement");
        }

        // Cập nhật thông tin sinh viên
        private void UpdateUserDetails(User existingUser, User updatedUser)
        {
            existingUser.Name = updatedUser.Name;
            existingUser.Email = updatedUser.Email;
            existingUser.Phone = updatedUser.Phone;
            existingUser.DoB = updatedUser.DoB;
        }


        // Xóa user
        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            var users = GetAllUsersFromJson();
            var userToDelete = users.FirstOrDefault(u => u.Id == id);
            if (userToDelete == null)
                return NotFound();

            users.Remove(userToDelete);
            SaveUsersToJson(users);
            TempData["SuccessMessage"] = $"Account {userToDelete.Email} delete successfully.";
            return RedirectToAction("UserManagement");
        }

        // Hiển thị form thêm người dùng

        [HttpGet]
        public IActionResult AddUser()
        {
            return View();
        }



        // Thêm người dùng
        [HttpPost]
        public IActionResult AddUser(User newUser)
        {
            if (ModelState.IsValid)
            {
                var users = GetAllUsersFromJson();

                // Tạo ID mới cho người dùng
                int newId = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1;
                newUser.Id = newId;

                // Thêm người dùng mới vào danh sách
                users.Add(newUser);

                // Lưu danh sách người dùng vào file JSON
                SaveUsersToJson(users);

                // Đặt thông báo thành công vào TempData
                TempData["SuccessMessage"] = $"Account {newUser.Role} {newUser.Name} created successfully for {newUser.Email} and Password {newUser.Password}.";

                return RedirectToAction("UserManagement");
            }

            // Nếu dữ liệu không hợp lệ, trả về lại view Add User để người dùng nhập lại
            return View(newUser);
        }

        // Lưu danh sách người dùng vào file JSON
        private void SaveUsersToJson(List<User> users)
        {
            System.IO.File.WriteAllText(_userDataFilePath, JsonConvert.SerializeObject(users));
        }






       ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////




        // Action để hiển thị form thêm khóa học mới
        [HttpGet]
        public IActionResult AddCourse()
        {
            return View();
        }

        // Action để xử lý yêu cầu thêm khóa học mới
        [HttpPost]
        public IActionResult AddCourse(Course newCourse)
        {
            if (ModelState.IsValid)
            {
                // Lấy danh sách khóa học từ tệp JSON (nếu có)
                List<Course> courses = GetCoursesFromJson();

                // Tạo ID mới cho khóa học
                int newId = courses.Count > 0 ? courses[courses.Count - 1].Id + 1 : 1;
                newCourse.Id = newId;
                newCourse.TotalNumberOfStudents = 0;

                // Thêm khóa học mới vào danh sách
                courses.Add(newCourse);

                // Lưu danh sách khóa học mới vào tệp JSON
                SaveCoursesToJson(courses);


                TempData["SuccessMessage"] = $"Course {newCourse.Name} create sucessfully.";

                // Chuyển hướng về trang quản lý khóa học sau khi thêm thành công
                return RedirectToAction("ManageCourses");
            }



            return View(newCourse);
        }



        // Action để hiển thị trang quản lý khóa học
        public IActionResult ManageCourses()
        {
            // Lấy danh sách khóa học từ tệp JSON
            List<Course> courses = GetCoursesFromJson();

            return View(courses);
        }

        // Phương thức để lấy danh sách khóa học từ tệp JSON
        private List<Course> GetCoursesFromJson()
        {
            List<Course> courses = new List<Course>();

            if (System.IO.File.Exists(_courseDataFilePath))
            {
                string jsonData = System.IO.File.ReadAllText(_courseDataFilePath);
                courses = JsonConvert.DeserializeObject<List<Course>>(jsonData);
            }

            return courses;
        }

        // Phương thức để lưu danh sách khóa học vào tệp JSON
        private void SaveCoursesToJson(List<Course> courses)
        {
            System.IO.File.WriteAllText(_courseDataFilePath, JsonConvert.SerializeObject(courses));

        }








        // Action để hiển thị form chỉnh sửa khóa học
        public IActionResult EditCourse(int id)
        {
            // Lấy danh sách khóa học từ tệp JSON
            List<Course> courses = GetCoursesFromJson();

            // Tìm khóa học cần chỉnh sửa trong danh sách
            Course courseToEdit = courses.FirstOrDefault(c => c.Id == id);

            if (courseToEdit == null)
            {
                // Trả về trang lỗi nếu không tìm thấy khóa học
                return RedirectToAction("Error", "Home");
            }

            return View(courseToEdit);
        }

        // Action để xử lý yêu cầu chỉnh sửa khóa học
        [HttpPost]
        public IActionResult EditCourse(Course editedCourse)
        {
            if (ModelState.IsValid)
            {
                // Lấy danh sách khóa học từ tệp JSON
                List<Course> courses = GetCoursesFromJson();

                // Tìm khóa học cần chỉnh sửa trong danh sách
                Course courseToEdit = courses.FirstOrDefault(c => c.Id == editedCourse.Id);

                if (courseToEdit == null)
                {
                    // Trả về trang lỗi nếu không tìm thấy khóa học
                    return RedirectToAction("Error", "Home");
                }

                // Cập nhật thông tin của khóa học đã chỉnh sửa
                courseToEdit.Name = editedCourse.Name;
                courseToEdit.StartDate = editedCourse.StartDate;
                courseToEdit.EndDate = editedCourse.EndDate;
                courseToEdit.AttendanceSessions = editedCourse.AttendanceSessions;
                // Lưu danh sách khóa học mới vào tệp JSON
                SaveCoursesToJson(courses);

                TempData["SuccessMessage"] = $"Course {courseToEdit.Name} edit sucessfully.";

                // Chuyển hướng về trang quản lý khóa học sau khi chỉnh sửa thành công
                return RedirectToAction("ManageCourses");
            }

            // Nếu dữ liệu không hợp lệ, trả về view EditCourse với các thông báo lỗi
            return View(editedCourse);
        }







        // Action để xử lý yêu cầu xóa khóa học
        public IActionResult DeleteCourse(int id)
        {
            // Lấy danh sách khóa học từ tệp JSON
            List<Course> courses = GetCoursesFromJson();

            // Tìm khóa học cần xóa trong danh sách
            Course courseToDelete = courses.FirstOrDefault(c => c.Id == id);

            if (courseToDelete == null)
            {
                // Trả về trang lỗi nếu không tìm thấy khóa học
                return RedirectToAction("Error", "Home");
            }

            // Xóa khóa học khỏi danh sách
            courses.Remove(courseToDelete);

            // Lưu danh sách khóa học mới vào tệp JSON
            SaveCoursesToJson(courses);


            TempData["SuccessMessage"] = $"Course {courseToDelete.Name} delete sucessfully.";

            // Chuyển hướng về trang quản lý khóa học sau khi xóa thành công
            return RedirectToAction("ManageCourses");
        }








        // Action để hiển thị danh sách sinh viên trong một khóa học
        public IActionResult ViewStudentsInCourse(int id)
        {
            // Đọc dữ liệu từ tệp JSON chứa thông tin về các khóa học
            List<Course> courses = JsonConvert.DeserializeObject<List<Course>>(System.IO.File.ReadAllText(_courseDataFilePath));

            // Tìm khóa học có Id tương ứng
            Course course = courses.FirstOrDefault(c => c.Id == id);

            if (course == null)
            {
                // Nếu không tìm thấy khóa học, trả về trang không tìm thấy
                return NotFound();
            }

            // Lấy danh sách sinh viên tham gia vào khóa học
            List<User> studentsInCourse = GetStudentsInCourse(course.UserIds);
            ViewBag.courseN = course.Name;
            ViewBag.CourseId = course.Id;

            // Trả về view hiển thị danh sách sinh viên trong khóa học
            return View(studentsInCourse);
        }

        // Phương thức để lấy danh sách sinh viên tham gia vào khóa học
        private List<User> GetStudentsInCourse(List<int> userIds)
        {
            // Đọc dữ liệu từ tệp JSON chứa thông tin người dùng
            List<User> users = JsonConvert.DeserializeObject<List<User>>(System.IO.File.ReadAllText(_userDataFilePath));

            // Lọc danh sách người dùng để chỉ bao gồm những sinh viên có ID trong danh sách UserIds
            var studentsInCourse = users.Where(user => userIds.Contains(user.Id) && user.Role == "Student").ToList();

            return studentsInCourse;
        }















        // Phương thức để lấy thông tin khóa học từ ID
        private Course GetCourseById(int courseId)
        {
            var courses = JsonConvert.DeserializeObject<List<Course>>(System.IO.File.ReadAllText(_courseDataFilePath));
            return courses.FirstOrDefault(c => c.Id == courseId);
        }





        public IActionResult AddStudentToCourse(int courseId)
        {
            // Load danh sách sinh viên
            var students_list = JsonConvert.DeserializeObject<List<User>>(System.IO.File.ReadAllText(_userDataFilePath));

            // Load thông tin khóa học từ ID
            var course = GetCourseById(courseId);
            if (course == null)
            {
                // Xử lý khi không tìm thấy khóa học
                // Ví dụ: Hiển thị thông báo lỗi và chuyển hướng người dùng đến trang quản lý khóa học
                return RedirectToAction("ManageCourses");
            }
            var userNotEnrolled = students_list.Where(s => !course.UserIds.Any(u => u == s.Id));
            var result = userNotEnrolled.Where(u => u.Role == "Student");

            ViewBag.CourseId = course.Id;

            return View(result.ToList());
        }





        // Phương thức để lưu thông tin khóa học đã được cập nhật vào file
        private void SaveCourseToFile(Course course)
        {
            var courses = JsonConvert.DeserializeObject<List<Course>>(System.IO.File.ReadAllText(_courseDataFilePath));
            var existingCourse = courses.FirstOrDefault(c => c.Id == course.Id);
            if (existingCourse != null)
            {
                existingCourse.UserIds = course.UserIds;
                System.IO.File.WriteAllText(_courseDataFilePath, JsonConvert.SerializeObject(courses));
            }
        }
        private void SaveListCourseToFile(List<Course> courses)
        {
            System.IO.File.WriteAllText(_courseDataFilePath, JsonConvert.SerializeObject(courses));
        }

        [HttpPost]
        public IActionResult AddStudentsToCourse(int courseId, List<int> selectedStudents)
        {
            // Load thông tin khóa học từ ID
            var courses = JsonConvert.DeserializeObject<List<Course>>(System.IO.File.ReadAllText(_courseDataFilePath));

            // Kiểm tra nếu khóa học không tồn tại
            var course = courses.FirstOrDefault(c => c.Id == courseId);
            if (course == null)
            {
                // Xử lý khi không tìm thấy khóa học
                return RedirectToAction("ManageCourses");
            }

            // Thêm sinh viên vào khóa học
            if (course.UserIds == null)
            {
                course.UserIds = new List<int>();
            }
            course.UserIds.AddRange(selectedStudents);
            course.TotalNumberOfStudents = course.UserIds.Count;

            // Lưu lại thông tin khóa học đã được cập nhật
            SaveListCourseToFile(courses);

            TempData["SuccessMessage"] = $"{selectedStudents.Count} students have been successfully added to the course.";
            // Chuyển hướng về trang quản lý khóa học
            return RedirectToAction("ManageCourses");
        }




        public IActionResult DeleteStudentFromCourse(int courseId, int studentId)
        {
            var courses = JsonConvert.DeserializeObject<List<Course>>(System.IO.File.ReadAllText(_courseDataFilePath));

            // Kiểm tra nếu khóa học không tồn tại
            var course = courses.FirstOrDefault(c => c.Id == courseId);
            if (course == null)
            {
                // Xử lý khi không tìm thấy khóa học
                ViewBag.ErrorMessage = "Course not found.";
                return RedirectToAction("ManageCourses", "Admin");
            }

            // Kiểm tra nếu sinh viên không tồn tại trong khóa học
            if (!course.UserIds.Contains(studentId))
            {
                // Xử lý khi không tìm thấy sinh viên trong khóa học
                TempData["ErrorMessage"] = "Student not found in this course.";
                return RedirectToAction("ViewStudentsInCourse", "Admin", new { id = courseId });
            }

            // Xóa sinh viên khỏi khóa học
            course.UserIds.Remove(studentId);
            course.TotalNumberOfStudents = course.UserIds.Count;

            // Lưu lại thông tin khóa học đã được cập nhật
            SaveListCourseToFile(courses);

            // Chuyển hướng về trang danh sách sinh viên trong khóa học
            TempData["SuccessMessage"] = "Student successfully removed from the course.";

            return RedirectToAction("ViewStudentsInCourse", "Admin", new { id = courseId });
        }




















        private List<Course> LoadCourses()
        {
            if (System.IO.File.Exists(_courseDataFilePath))
            {
                string json = System.IO.File.ReadAllText(_courseDataFilePath);
                return JsonConvert.DeserializeObject<List<Course>>(json);
            }
            return new List<Course>();
        }

        private List<User> LoadLectures()
        {
            if (System.IO.File.Exists(_userDataFilePath))
            {
                string json = System.IO.File.ReadAllText(_userDataFilePath);
                List<User> allUsers = JsonConvert.DeserializeObject<List<User>>(json);

                // Lọc và chỉ trả về danh sách người dùng có Role là "Lecture"
                List<User> lectures = allUsers.Where(u => u.Role == "Lecture").ToList();

                return lectures;
            }
            return new List<User>();
        }




        public IActionResult CreateTimetable()
        {
            // Load danh sách khóa học từ tệp JSON
            var courses = LoadCourses();

            // Load danh sách giảng viên từ tệp JSON
            var lectures = LoadLectures();

            ViewBag.Courses = courses;
            ViewBag.Lectures = lectures;

            return View();
        }





        private List<Timetable> LoadTimetables()
        {
            if (System.IO.File.Exists(_timetableDataFilePath))
            {
                string json = System.IO.File.ReadAllText(_timetableDataFilePath);
                return JsonConvert.DeserializeObject<List<Timetable>>(json);
            }
            return new List<Timetable>();
        }

        private void SaveTimetables(List<Timetable> timetables)
        {
            string json = JsonConvert.SerializeObject(timetables);
            System.IO.File.WriteAllText(_timetableDataFilePath, json);
        }





        [HttpPost]
        public IActionResult SaveTimetable(int CourseId, int LectureId, Timetable timetable)
        {
            // Load danh sách thời khóa biểu từ tệp JSON
            List<Timetable> timetables = LoadTimetables();

            // Gán Id cho thời khóa biểu mới
            timetable.Id = timetables.Count + 1;
            timetable.CourseId = CourseId;
            timetable.LectureId = LectureId;

            // Thêm thời khóa biểu mới vào danh sách
            timetables.Add(timetable);

            // Lưu danh sách thời khóa biểu mới vào tệp JSON
            SaveTimetables(timetables);

            // Chuyển hướng về trang quản lý thời khóa biểu sau khi tạo thành công
            return RedirectToAction("TimetableManagement");
        }






        public IActionResult TimetableManagement()
        {
            List<Timetable> timetables = LoadTimetables();

            // Tải danh sách môn học từ tệp JSON
            List<Course> courses = LoadCourses();
            // Tải danh sách giảng viên từ tệp JSON
            List<User> lectures = LoadLectures();

            // Đặt danh sách môn học và giảng viên vào ViewBag để sử dụng trong view
            ViewBag.Courses = courses;
            ViewBag.Lectures = lectures;

            return View(timetables);
        }







        public IActionResult EditTimetable(int id)
        {
            // Load danh sách thời khóa biểu từ tệp JSON
            List<Timetable> timetables = LoadTimetables();
            // Tìm thời khóa biểu cần sửa trong danh sách
            Timetable timetableToEdit = timetables.FirstOrDefault(t => t.Id == id);
            if (timetableToEdit == null)
            {
                // Trả về trang lỗi nếu không tìm thấy thời khóa biểu
                return RedirectToAction("Error", "Home");
            }

            // Load danh sách các khóa học
            var courses = LoadCourses();

            // Đặt danh sách các khóa học vào ViewBag để sử dụng trong view
            ViewBag.Courses = courses;

            // Load danh sách giảng viên
            var lectures = LoadLectures();

            // Đặt danh sách giảng viên vào ViewBag để sử dụng trong view
            ViewBag.Lectures = lectures;

            return View(timetableToEdit);
        }


        [HttpPost]
        public IActionResult EditTimetable(Timetable timetable)
        {
            // Load danh sách thời khóa biểu từ tệp JSON
            List<Timetable> timetables = LoadTimetables();
            // Tìm thời khóa biểu cần sửa trong danh sách
            Timetable timetableToEdit = timetables.FirstOrDefault(t => t.Id == timetable.Id);
            if (timetableToEdit != null)
            {
                // Cập nhật thông tin thời khóa biểu
                timetableToEdit.CourseId = timetable.CourseId;
                timetableToEdit.LectureId = timetable.LectureId;
                timetableToEdit.Room = timetable.Room;
                timetableToEdit.StartTime = timetable.StartTime;
                timetableToEdit.EndTime = timetable.EndTime;
                timetableToEdit.DayOfWeek = timetable.DayOfWeek;
                // Lưu lại danh sách thời khóa biểu mới vào tệp JSON
                SaveTimetables(timetables);
            }
            return RedirectToAction("TimetableManagement");
        }

        public IActionResult DeleteTimetable(int id)
        {
            // Load danh sách thời khóa biểu từ tệp JSON
            List<Timetable> timetables = LoadTimetables();
            // Tìm thời khóa biểu cần xóa trong danh sách
            Timetable timetableToDelete = timetables.FirstOrDefault(t => t.Id == id);
            if (timetableToDelete != null)
            {
                // Xóa thời khóa biểu khỏi danh sách
                timetables.Remove(timetableToDelete);
                // Lưu lại danh sách thời khóa biểu mới vào tệp JSON
                SaveTimetables(timetables);
            }
            return RedirectToAction("TimetableManagement");
        }






    }
}
