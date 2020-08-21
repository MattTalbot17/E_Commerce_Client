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
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace E_Commerce_Client.Controllers
{
    public class ProductController : Controller
    {
        string baseURL = "https://localhost:44315/";
        public static List<Product> productList = new List<Product>();
        static bool incorrectImageFormat = false, emptyField = false;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> Index()
        {
                productList.Clear();
                HttpClient httpClient = new HttpClient();

                using (httpClient)
                {
                    httpClient.BaseAddress = new Uri(baseURL);
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (HttpContext.Session.GetString("token")));

                    HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("api/products/getproduct");

                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        string productResponse = httpResponseMessage.Content.ReadAsStringAsync().Result;
                        productList = JsonConvert.DeserializeObject<List<Product>>(productResponse);
                        ViewBag.data = productList;
                    }
                }
                ViewBag.searchedProduct = false;
                ViewBag.category = null;
                ViewBag.userType = userManager.UserType;
                return View(productList.Where(a => a.IsActive ==true));

        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<Product>>> Index(string category, string submit, string singleProduct)
        {
            productList.Clear();
            HttpClient httpClient = new HttpClient();

            using (httpClient)
            {
                httpClient.BaseAddress = new Uri(baseURL);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("api/products/getproduct");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    string incidentResponse = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    productList = JsonConvert.DeserializeObject<List<Product>>(incidentResponse);
                    ViewBag.data = productList;
                    ViewBag.userType = userManager.UserType;


                }
                if(submit == "?")
                {
                    ViewBag.searchedProductName = singleProduct;
                    ViewBag.searchedProduct = true;
                    IEnumerable<Product> tempList = productList.Where(a => a.ProductName.Contains(singleProduct, StringComparison.OrdinalIgnoreCase) == true && a.IsActive == true);
                    return View(tempList);
                }
                if (submit == "Search")
                {
                    ViewBag.searchedProduct = false;
                    ViewBag.category = category;
                   return View(productList.Where(a => a.ProductCategory == category && a.IsActive == true));
                }
                else
                {
                    ViewBag.searchedProduct = false;
                    ViewBag.category = null;
                    return View(productList.Where(a => a.IsActive == true));
                }                
            }            
        }

        [HttpGet]
        public ActionResult CreateProduct()
        {
            if (userManager.UserType == "Employee" && HttpContext.Session.GetString("token") != null)
            {
                ViewBag.data = productList;
                if (incorrectImageFormat)
                {
                    ViewBag.imageError = "Image must be of a JPG format";
                }
                if (emptyField)
                {
                    ViewBag.Error = "Empty field found. All Fields require input"; 
                }
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Users");
            }


        }
        [HttpPost]
        public async Task<ActionResult> CreateProduct(Product product, string category, string newCategory)
        {
            if(product.ProductName != null && product.ProductDescription != null && product.ProductImage != null && product.ProductPrice > 0 && (category  != null || newCategory != null))
            {
                if (product.ProductImage.EndsWith(".jpg"))
                {
                    incorrectImageFormat = true;
                    product.ProductCategory = category;
                    if (newCategory != null)
                    {
                        product.ProductCategory = newCategory;
                    }
                    HttpClient httpClient = new HttpClient();

                    using (httpClient)
                    {
                        httpClient.BaseAddress = new Uri(baseURL);
                        string content = JsonConvert.SerializeObject(product, Formatting.Indented);
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
                        ByteArrayContent byteArrayContent = new ByteArrayContent(buffer);
                        byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                        HttpResponseMessage response = await httpClient.PostAsync("api/products/PostProduct", byteArrayContent);

                        if (response.IsSuccessStatusCode)
                        {
                            incorrectImageFormat = false;
                            emptyField = false;
                            return RedirectToAction("Index", "Product");
                        }
                    }
                    return RedirectToAction("CreateProduct");
                }
                else
                {
                    emptyField = false;
                    incorrectImageFormat = true;
                    return RedirectToAction("CreateProduct");
                }
            }
            else
            {
                incorrectImageFormat = false;
                emptyField = true;
                return RedirectToAction("CreateProduct");
            }
            

        }

        [HttpGet]
        public async Task<ActionResult> EditProduct(int id)
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
                    HttpResponseMessage response = await httpClient.GetAsync("api/products/GetProduct/" + id);

                    if (response.IsSuccessStatusCode)
                    {
                        string productResponse = response.Content.ReadAsStringAsync().Result;
                        Product foundProduct = JsonConvert.DeserializeObject<Product>(productResponse);
                        userManager.ProductID = id;
                        ViewBag.data = productList;
                        return View(foundProduct);
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

        [HttpPost]
        public async Task<ActionResult> EditProduct(Product product, string category, string submit)
        {

            product.ProductId = userManager.ProductID;
            product.ProductCategory = category;
            HttpClient httpClient = new HttpClient();
            if (submit == "Save Changes")
            {
                using (httpClient)
                {
                    httpClient.BaseAddress = new Uri(baseURL);
                    string content = JsonConvert.SerializeObject(product, Formatting.Indented);
                    Byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
                    ByteArrayContent byteArrayContent = new ByteArrayContent(buffer);
                    byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    HttpResponseMessage response = await httpClient.PostAsync("api/products/EditProduct", byteArrayContent);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index", "Product");
                    }
                }
                return RedirectToAction("EditProduct");
            }
            else if(submit == "Remove Product")
            {
                using (httpClient)
                {
                    httpClient.BaseAddress = new Uri(baseURL);
                    string content = JsonConvert.SerializeObject(product, Formatting.Indented);
                    Byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
                    ByteArrayContent byteArrayContent = new ByteArrayContent(buffer);
                    byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("token"));

                    HttpResponseMessage response = await httpClient.DeleteAsync("api/products/DeleteProduct/" + userManager.ProductID);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index", "Product");
                    }
                }
                return RedirectToAction("EditProduct");
            }
            else
            {
                ViewBag.data = productList;
                return View();
            }

        }

        [HttpGet]

        public async Task<ActionResult> Details(int id)
        {
            HttpClient httpClient = new HttpClient();
            using (httpClient)
            {
                httpClient.BaseAddress = new Uri(baseURL);
                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("api/products/getproduct/" + id);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    string productResponse = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    Product product = JsonConvert.DeserializeObject<Product>(productResponse);

                    return View(product);
                }
            }
            return RedirectToAction("Index");
        }
    }
}