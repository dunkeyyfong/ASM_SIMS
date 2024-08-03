using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Moq;
using CsvHelper;
using SIMS_ASM.Controllers;
using SIMS_ASM.Models;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;


namespace SIMS.UnitTests
{
    public class MockCsvHelper
    {
        private readonly string _filePath;

        public MockCsvHelper(string filePath)
        {
            _filePath = filePath;
        }

        public Task<List<User>> ReadUsersAsync()
        {
            using (var reader = new StreamReader(_filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                var records = csv.GetRecords<User>().ToList();
                return Task.FromResult(records);
            }
        }

        public Task AddUserAsync(User user)
        {
            var users = ReadUsersAsync().Result;
            users.Add(user);

            using (var writer = new StreamWriter(_filePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(users);
            }

            return Task.CompletedTask;
        }
    }



    public class AdminControllerTests
    {
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            _controller = new AdminController();
        }

        [Fact]
        public void UserManagement_ReturnsCorrectViewWithStudents()
        {
            string userType = "Student";
            var expectedRole = "Student";

            var result = _controller.UserManagement(userType) as ViewResult;
            var model = result.Model as List<User>;

            Assert.NotNull(result);
            Assert.Equal("Student", result.ViewData["UserType"]);
            Assert.All(model, u => Assert.Equal(expectedRole, u.Role));
        }

        [Fact]
        public void UserManagement_ReturnsCorrectViewWithLecturers()
        {

           string userType = "Lecture";
            var expectedRole = "Lecture";

            var result = _controller.UserManagement(userType) as ViewResult;
            var model = result.Model as List<User>;

            Assert.NotNull(result);
            Assert.Equal("Lecture", result.ViewData["UserType"]);
            Assert.All(model, u => Assert.Equal(expectedRole, u.Role));
        }

        [Fact]
        public void AddUser_InvalidModel_ReturnsViewWithUser()
        {
            // Arrange
            var invalidUser = new User
            {
                // Thiếu thông tin bắt buộc
                Name = "",
                Email = "invalidemail",
                Phone = "123",
                Role = "Student"
            };

            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = _controller.AddUser(invalidUser) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(invalidUser, result.Model);
        }

        [Fact]
        public void CourseManagement_ReturnsCorrectView()
        {
            var result = _controller.ManageCourses() as ViewResult;

            Assert.NotNull(result);
        }
    }
}
