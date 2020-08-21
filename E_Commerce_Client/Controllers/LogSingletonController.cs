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
    public class LogSingletonController : Controller
    {
        string baseURL = "https://localhost:44315/";
        bool viewAll = true;
        List<Records> recordsList = new List<Records>();
        static List<LogSingleton> logList = new List<LogSingleton>();

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            if(userManager.UserType == "Employee" && HttpContext.Session.GetString("token") != null)
            {
                HttpClient httpClient = new HttpClient();


                using (httpClient)
                {
                    httpClient.BaseAddress = new Uri(baseURL);
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (HttpContext.Session.GetString("token")));
                    HttpResponseMessage responseMessage = await httpClient.GetAsync("api/logsingletons");

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        string response = responseMessage.Content.ReadAsStringAsync().Result;
                        logList = JsonConvert.DeserializeObject<List<LogSingleton>>(response);
                    }
                }
                foreach (var item in ProductController.productList.Select(a => a.ProductCategory).Distinct().ToList())
                {
                    recordsList.Add(new Records(logList.Where(a => a.Product.ProductCategory == item).Where(a => a.LogType == "Sale Completed").Select(b => b.LogId).Count(), logList.Where(a => a.Product.ProductCategory == item).Select(b => b.Product.ProductCategory).FirstOrDefault()));
                }
                ViewBag.data = ProductController.productList;
                ViewBag.viewAll = viewAll;
                ViewBag.recordsList = recordsList;
                return View(logList);
            }
            else
            {
                return RedirectToAction("Login", "Users");
            }
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<LogSingleton>>> Index(string submit, string category, DateTime start, DateTime end)
        {
            if (category == null || start == null || end == null)
            {
                return BadRequest("Error: Details Missing");
            }

            List<LogSingleton> records = new List<LogSingleton>();

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseURL);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (HttpContext.Session.GetString("token")));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage responseMessage = await client.GetAsync("api/logsingletons");

                if (responseMessage.IsSuccessStatusCode)
                {
                    string response = responseMessage.Content.ReadAsStringAsync().Result;
                    records = JsonConvert.DeserializeObject<List<LogSingleton>>(response);
                }
            }
            ViewBag.data = ProductController.productList;

            if (submit == "Search")
            {
                ViewBag.category = category;
                viewAll = false;
                ViewBag.viewAll = viewAll;
                return View(records.Where(s => s.Product.ProductCategory == category && s.LogDate >= start && s.LogDate <= end));
            }
            else
            {
                foreach(var item in ProductController.productList.Select(a => a.ProductCategory).Distinct().ToList())
                {
                    recordsList.Add(new Records(records.Where(a => a.Product.ProductCategory == item).Select(b => b.Product.ProductCategory).Count(), records.Where(a => a.Product.ProductCategory == item).Select(b => b.Product.ProductCategory).FirstOrDefault()));
                }
                ViewBag.recordsList = recordsList;
                viewAll = true;
                ViewBag.viewAll = viewAll;
                ViewBag.catgory = null;
                return View(records);
            }
            
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            if (userManager.UserType == "Employee" && HttpContext.Session.GetString("token") != null)
            {
                return View(logList.Where(a => a.LogId == id).First());
            }
            else
            {
                return RedirectToAction("Login", "Users");
            }
        }
    }
}