using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using E_Commerce_Client.Managers;
using E_Commerce_Client.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace E_Commerce_Client.Controllers
{
    public class UsersController : Controller
    {
        string baseURL = "https://localhost:44315/";
        static bool sqlInjectionAttempt = false, emptyField = false;
        // GET: Users
        public IActionResult Login()
        {
            if (emptyField)
            {
                ViewBag.Error = "Please fill in all fields";
            }
            else if (sqlInjectionAttempt)
            {
                ViewBag.Error = "Harmful Input Detected. Please Try again";
            }
            else if (HttpContext.Session.GetString("token") == null)
            {
                ViewBag.Error = "You need to login first before continuing on our website.";
            }


            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(string username, string password, string submit)
        {

            if (submit == "Login")
            {
                if (username != null || password != null)
                {
                    if (!username.Contains("Select") || !username.Contains("insert") || !username.Contains("Drop") || !username.Contains("Alter") ||
                        !password.Contains("Select") || !password.Contains("insert") || !password.Contains("Drop") || !password.Contains("Alter"))
                    {
                        emptyField = false;
                        sqlInjectionAttempt = false;

                        Users u = new Users { Username = username, Password = password };
                        HttpClient httpClient = new HttpClient();

                        using (httpClient)
                        {
                            httpClient.BaseAddress = new Uri(baseURL);
                            string content = JsonConvert.SerializeObject(u, Formatting.Indented);
                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
                            ByteArrayContent byteArrayContent = new ByteArrayContent(buffer);
                            byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                            HttpResponseMessage responseToken = await httpClient.PostAsync("api/users/LoginCheck", byteArrayContent);
                            HttpResponseMessage responseUser = await httpClient.GetAsync("api/users/UserID");

                            if (responseToken.IsSuccessStatusCode)
                            {
                                string tokenContent = responseToken.Content.ReadAsStringAsync().Result;
                                HttpContext.Session.SetString("token", tokenContent);

                                string userContent = responseUser.Content.ReadAsStringAsync().Result;
                                Users user = JsonConvert.DeserializeObject<Users>(userContent);

                                userManager.UserID = user.UserId;

                                if (user.Customers.Count == 1)
                                    userManager.UserType = "Customer";
                                else if (user.Employees.Count == 1)
                                    userManager.UserType = "Employee";


                                return RedirectToAction("Index", "Product");
                            }
                            else
                            {
                                return RedirectToAction("Login", "Users");
                            }
                        }
                    }
                    else
                    {
                        emptyField = false;
                        sqlInjectionAttempt = true;
                        return RedirectToAction("Login", "Users");
                    }

                }
                else
                {
                    sqlInjectionAttempt = false;
                    emptyField = true;
                    return RedirectToAction("Login", "Users");
                }
            }
            else
            {
                return RedirectToAction("RegisterCustomer", "Customers");
            }

        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            userManager.UserID = 0;
            userManager.UserType = null;
            userManager.ProductID = 0;
            return View();
        }

        [HttpPost]
        public IActionResult Logout(string submit)
        {
            if (submit == "Login")
                return RedirectToAction("Login");
            else
                return View();
        }
    }
}
