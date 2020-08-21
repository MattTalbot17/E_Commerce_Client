using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    public class SaleController : Controller
    {
        string baseURL = "https://localhost:44315/";
        static List<Sale> saleList = new List<Sale>();
        static int saleProductID, productID;
        static Sale sale;
        static bool hasQuantity = true;

        [HttpGet]
        public ActionResult Details(int id)
        {
            if (HttpContext.Session.GetString("token") != null)
            {
                ViewBag.Products = ProductController.productList;
            return View(saleList.Where(a => a.SaleId == id).First());
            }
            else
            {
                return RedirectToAction("login", "Users");
            }
        }

        [HttpGet]
        public ActionResult AddTocart(int id)
        {
            if (HttpContext.Session.GetString("token") != null)
            {
                if(hasQuantity != true)
                {
                    ViewBag.Error = "Please indicate how many items you would prefer";
                }
                ViewBag.Products = ProductController.productList;
                productID = id;
                ViewBag.id = id;
                saleProductID = id;
                ViewBag.userType = userManager.UserType;
                return View();
            }
            else
            {
                return RedirectToAction("login", "Users");
            }

        }
        [HttpPost]
        public ActionResult AddToCart(int quantity)
        {
            if(quantity > 0 &&  quantity % 1 == 0 )
            {
                if (HttpContext.Session.GetString("token") == null)
                {
                    return RedirectToAction("Login", "Users");
                }
                else
                {
                    hasQuantity = true;
                    Sale s = new Sale();
                    s.ProductId = saleProductID;
                    s.Quantity = quantity;
                    s.DateOfSale = DateTime.Now;
                    s.UserId = userManager.UserID;

                    return RedirectToAction("ConfirmSale", s);
                }
            }
            else
            {
                hasQuantity = false;
                return RedirectToAction("AddToCart", productID);
            }

        }
        [HttpGet]
        public IActionResult ConfirmSale(Sale s)
        {
            if (HttpContext.Session.GetString("token") != null)
            {
                ViewBag.Products = ProductController.productList;
                ViewBag.id = s.ProductId;
                sale = s;
                ViewBag.userType = userManager.UserType;
                return View(s);
            }
            else
            {
                return RedirectToAction("login", "Users");
            }

        }
        [HttpPost]
        public async Task<ActionResult> ConfirmSale()
        {

            HttpClient httpClient = new HttpClient();

            using (httpClient)
            {
                httpClient.BaseAddress = new Uri(baseURL);
                string content = JsonConvert.SerializeObject(sale, Formatting.Indented);
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
                ByteArrayContent byteArrayContent = new ByteArrayContent(buffer);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (HttpContext.Session.GetString("token")));
                byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await httpClient.PostAsync("api/sales", byteArrayContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("SaleCompleted", "Sale", sale);
                }
            }
            return RedirectToAction("AddToCart", "Sale");           
        }

        [HttpGet]
        public ActionResult SaleCompleted()
        {
            if (HttpContext.Session.GetString("token") != null)
            {
                ViewBag.userType = userManager.UserType;
                return View();
            }
            else
            {
                return RedirectToAction("login", "Users");
            }

        }
    }
}