using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SIMS_ASM.Models;

namespace SIMS_ASM.Controllers
{
    public class AuthController : Controller
    {



        // GET: /Auth/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            try
            {
                // Read data from JSON file
                string jsonData = System.IO.File.ReadAllText("data_user.json");

                // Parse JSON data into a list of Auth objects
                var users = JsonConvert.DeserializeObject<User[]>(jsonData);

                // Search for users with the provided email
                User user = null;
                if (users != null)
                {
                    user = users.FirstOrDefault(u => u.Email == email);
                }

                // Check if the user exists and the password matches
                if (user != null && user.Password == password)
                {
                    ///////////////////////Tạo SESSION/////////////////////////////
                    HttpContext.Session.SetString("IsLoggedIn", "true");
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("UserName", user.Name);
                    HttpContext.Session.SetString("Role", user.Role);


                    ///////////////////////Tạo SESSION/////////////////////////////

                    // Authentication successful
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Authentication failed
                    ViewBag.ErrorMessage = "Email or password is incorrect.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                // Error handling
                ViewBag.ErrorMessage = $"Error during authentication: {ex.Message}";
                return View();
            }
        }







        // GET: /Auth/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        public ActionResult Register(User newUser)
        {
            try
            {
                // Read data from JSON file
                string jsonData = System.IO.File.ReadAllText("data_user.json");

                // Parse JSON data into a list of Auth objects
                var users = JsonConvert.DeserializeObject<List<User>>(jsonData);

                // Check if the user already exists
                if (users.Any(u => u.Email == newUser.Email))
                {
                    ViewBag.ErrorMessage = "Email is already in use.";
                    return View(newUser);
                }
                // Tạo ID mới cho người dùng
                int newId = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1;
                newUser.Id = newId;
                newUser.Role = "Student";
                // Add new users to the list
                users.Add(newUser);

                // Record the list of users to a JSON file
                System.IO.File.WriteAllText("data_user.json", JsonConvert.SerializeObject(users));

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error during registration: {ex.Message}";
                return View(newUser);
            }
        }



        // GET: /Auth/Logout
        public IActionResult Logout()
        {
            // Xóa session của người dùng
            HttpContext.Session.Clear();

            // Redirect về trang đăng nhập hoặc trang chính (tùy thuộc vào yêu cầu của bạn)
            //  return RedirectToAction("Login", "Auth"); // Chuyển hướng về trang đăng nhập
            return RedirectToAction("Index", "Home"); // Chuyển hướng về trang chính
        }




    }
}
