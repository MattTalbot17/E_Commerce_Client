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
    public class CustomersController : Controller
    {
        string baseURL = "https://localhost:44315/";
        static List<Sale> saleList = new List<Sale>();
        static bool sqlInjectionAttempt = false, emptyField = false;
        public ActionResult RegisterCustomer()
        {
            if (emptyField)
            {
                ViewBag.Error = "Please fill in all fields";
            }
            else if (sqlInjectionAttempt)
            {
                ViewBag.Error = "Harmful Input Detected. Please Try again";
            }
            return View();
        }

        //creating a new Customer
        [HttpPost]
        public async Task<ActionResult> RegisterCustomerAsync(Customers customer)
        {
            if (customer.User.Username != null || customer.User.Password != null || customer.StreetNumber != 0 || customer.StreetName != null || customer.Suburb != null)
            {
                if (!customer.User.Username.Contains("Select") || !customer.User.Username.Contains("insert") || !customer.User.Username.Contains("Drop") || !customer.User.Username.Contains("Alter") ||
                    !customer.User.Password.Contains("Select") || !customer.User.Password.Contains("insert") || !customer.User.Password.Contains("Drop") || !customer.User.Password.Contains("Alter"))
                {
                    HttpClient httpClient = new HttpClient();

                    using (httpClient)
                    {
                        httpClient.BaseAddress = new Uri(baseURL);
                        string content = JsonConvert.SerializeObject(customer, Formatting.Indented);
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
                        ByteArrayContent byteArrayContent = new ByteArrayContent(buffer);

                        httpClient.DefaultRequestHeaders.Clear();
                        httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));


                        HttpResponseMessage response = await httpClient.PostAsync("api/customers/PostCustomer", byteArrayContent);

                        if (response.IsSuccessStatusCode)
                        {
                            sqlInjectionAttempt = false;
                            emptyField = false;
                            return RedirectToAction("Login", "Users");
                        }
                    }
                    return RedirectToAction("RegisterCustomer");
                }
                else
                {
                    emptyField = false;
                    sqlInjectionAttempt = true;
                    return RedirectToAction("RegisterCustomer", "Customers");
                }
            }
            else
            {
                sqlInjectionAttempt = false;
                emptyField = true;
                return RedirectToAction("RegisterCustomer", "Customers");
            }
        }

        //viewing Customer Profile

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (userManager.UserType == "Customer" && HttpContext.Session.GetString("token") != null)
            {
                HttpClient httpClient = new HttpClient();

                using (httpClient)
                {
                    httpClient.BaseAddress = new Uri(baseURL);
                    HttpResponseMessage response = await httpClient.GetAsync("api/customers/GetCustomer/" + userManager.UserID);

                    if (response.IsSuccessStatusCode)
                    {
                        string customerResponse = response.Content.ReadAsStringAsync().Result;
                        Customers foundCustomer = JsonConvert.DeserializeObject<Customers>(customerResponse);
                        return View(foundCustomer);
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
            if (userManager.UserType == "Customer" && HttpContext.Session.GetString("token") != null)
            {
                HttpClient httpClient = new HttpClient();

                using (httpClient)
                {
                    httpClient.BaseAddress = new Uri(baseURL);
                    HttpResponseMessage response = await httpClient.GetAsync("api/customers/GetCustomer/" + userManager.UserID);

                    if (response.IsSuccessStatusCode)
                    {
                        string customerResponse = response.Content.ReadAsStringAsync().Result;
                        Customers foundCustomer = JsonConvert.DeserializeObject<Customers>(customerResponse);
                        return View(foundCustomer);
                    }
                    else
                    {
                        return RedirectToAction("Profile", "Customers");
                    }
                }
            }
            else
            {
                return RedirectToAction("Login", "Users");
            }

        }

        [HttpPost]
        public async Task<ActionResult> Edit(Customers customer)
        {
            if (customer.User.Username != null || customer.User.Password != null || customer.StreetNumber != 0 || customer.StreetName != null || customer.Suburb != null)
            {
                if (!customer.User.Username.Contains("Select") || !customer.User.Username.Contains("insert") || !customer.User.Username.Contains("Drop") || !customer.User.Username.Contains("Alter") ||
                    !customer.User.Password.Contains("Select") || !customer.User.Password.Contains("insert") || !customer.User.Password.Contains("Drop") || !customer.User.Password.Contains("Alter"))
                {
                    HttpClient httpClient = new HttpClient();

                    using (httpClient)
                    {
                        httpClient.BaseAddress = new Uri(baseURL);
                        customer.User.UserId = userManager.UserID;
                        customer.UserId = userManager.UserID;
                        string content = JsonConvert.SerializeObject(customer, Formatting.Indented);
                        Byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
                        ByteArrayContent byteArrayContent = new ByteArrayContent(buffer);
                        byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                        HttpResponseMessage response = await httpClient.PostAsync("api/customers/EditCustomer", byteArrayContent);

                        if (response.IsSuccessStatusCode)
                        {
                            return RedirectToAction("Profile", "Customers");
                        }
                    }
                    return RedirectToAction("Edit");
                }
                else
                {
                    sqlInjectionAttempt = true;
                    return RedirectToAction("Edit");
                }
            }
            else
            {
                emptyField = true;
                return RedirectToAction("Edit");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sale>>> CustomerSaleHistory()
        {
            if (userManager.UserType == "Customer" && HttpContext.Session.GetString("token") != null)
            {
                HttpClient httpClient = new HttpClient();

                using (httpClient)
                {
                    httpClient.BaseAddress = new Uri(baseURL);
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (HttpContext.Session.GetString("token")));
                    HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("api/sales");

                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        string saleResponse = httpResponseMessage.Content.ReadAsStringAsync().Result;
                        saleList = JsonConvert.DeserializeObject<List<Sale>>(saleResponse);
                    }
                }
                return View(saleList.Where(s => s.UserId == userManager.UserID).ToList());
            }
            else
            {
                return RedirectToAction("Login", "Users");
            }
        }
    }
}