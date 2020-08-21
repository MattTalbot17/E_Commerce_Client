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
    public class EmployeeController : Controller
    {
        string baseURL = "https://localhost:44315/";
        static bool employeecreated = false;
        [HttpGet]
        public ActionResult CreateEmployee()
        {
            if(userManager.UserType == "Employee"  && HttpContext.Session.GetString("token") != null)
            {
                if (employeecreated)
                {
                    ViewBag.Message = "A new Employee has been created!.";
                }
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Users");
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateEmployee(Employees employees)
        {
            HttpClient httpClient = new HttpClient();

            using (httpClient)
            {
                httpClient.BaseAddress = new Uri(baseURL);
                string content = JsonConvert.SerializeObject(employees, Formatting.Indented);
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
                ByteArrayContent byteArrayContent = new ByteArrayContent(buffer);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (HttpContext.Session.GetString("token")));
                byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                HttpResponseMessage response = await httpClient.PostAsync("api/employees/PostEmployees", byteArrayContent);

                
                if (response.IsSuccessStatusCode)
                {
                    employeecreated = true;
                    return RedirectToAction("CreateEmployee");
                }
            }
            employeecreated = false;
            return RedirectToAction("CreateEmployee");
        }

        [HttpGet]
        public async Task<IActionResult> EmployeeProfile()
        {
            if (userManager.UserType == "Employee" && HttpContext.Session.GetString("token") != null)
            {
                HttpClient httpClient = new HttpClient();

                using (httpClient)
                {
                    httpClient.BaseAddress = new Uri(baseURL);
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (HttpContext.Session.GetString("token")));
                    HttpResponseMessage response = await httpClient.GetAsync("api/employees/GetEmployees/" + userManager.UserID);

                    if (response.IsSuccessStatusCode)
                    {
                        string employeeResponse = response.Content.ReadAsStringAsync().Result;
                        Employees foundEmployee = JsonConvert.DeserializeObject<Employees>(employeeResponse);
                        return View(foundEmployee);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Product");
                    }
                }
            }
            else
            {
                return RedirectToAction("Login", "Users");
            }


        }

        [HttpGet]
        public async Task<ActionResult> Edit()
        {
            if (userManager.UserType == "Employee" && HttpContext.Session.GetString("token") != null)
            {
                HttpClient httpClient = new HttpClient();

                using (httpClient)
                {
                    httpClient.BaseAddress = new Uri(baseURL);
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (HttpContext.Session.GetString("token")));
                    HttpResponseMessage response = await httpClient.GetAsync("api/employees/GetEmployees/" + userManager.UserID);

                    if (response.IsSuccessStatusCode)
                    {
                        string employeeResponse = response.Content.ReadAsStringAsync().Result;
                        Employees foundEmployee = JsonConvert.DeserializeObject<Employees>(employeeResponse);
                        return View(foundEmployee);
                    }
                    else
                    {
                        return RedirectToAction("EmployeeProfile", "Employee");
                    }
                }
            }
            else
            {
                return RedirectToAction("Login", "Users");
            }


        }

        [HttpPost]
        public async Task<ActionResult> Edit(Employees employees)
        {
            HttpClient httpClient = new HttpClient();

            using (httpClient)
            {
                httpClient.BaseAddress = new Uri(baseURL);
                employees.User.UserId = userManager.UserID;
                employees.UserId = userManager.UserID;
                string content = JsonConvert.SerializeObject(employees, Formatting.Indented);
                Byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
                ByteArrayContent byteArrayContent = new ByteArrayContent(buffer);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (HttpContext.Session.GetString("token")));
                byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                HttpResponseMessage response = await httpClient.PostAsync("api/employees/EditEmployee", byteArrayContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("EmployeeProfile", "Employee");
                }
            }
            return RedirectToAction("Edit");
        }
    }
}