using CreativeCollab_yj_sy.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace CreativeCollab_yj_sy.Controllers
{
    public class RestaurantController : Controller
    {
        private static readonly HttpClient client;
        private JavaScriptSerializer jss = new JavaScriptSerializer();

        static RestaurantController()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                //cookies are manually set in RequestHeader
                UseCookies = false
            };

            client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://localhost:44324/api/");
        }

        /// <summary>
        /// Grabs the authentication cookie sent to this controller.
        /// For proper WebAPI authentication, you can send a post request with login credentials to the WebAPI and log the access token from the response. The controller already knows this token, so we're just passing it up the chain.
        /// 
        /// Here is a descriptive article which walks through the process of setting up authorization/authentication directly.
        /// https://docs.microsoft.com/en-us/aspnet/web-api/overview/security/individual-accounts-in-web-api
        /// </summary>
        private void GetApplicationCookie()
        {
            string token = "";
            //HTTP client is set up to be reused, otherwise it will exhaust server resources.
            //This is a bit dangerous because a previously authenticated cookie could be cached for
            //a follow-up request from someone else. Reset cookies in HTTP client before grabbing a new one.
            client.DefaultRequestHeaders.Remove("Cookie");
            if (!User.Identity.IsAuthenticated) return;

            HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies.Get(".AspNet.ApplicationCookie");
            if (cookie != null) token = cookie.Value;

            //collect token as it is submitted to the controller
            //use it to pass along to the WebAPI.
            Debug.WriteLine("Token Submitted is : " + token);
            if (token != "") client.DefaultRequestHeaders.Add("Cookie", ".AspNet.ApplicationCookie=" + token);

            return;
        }

        // GET: Restaurant/List
        public ActionResult List()
        {
            // objective: communicate with our Restaurant data api to retrieve a list of Restaurants
            //curl https://localhost:44324/api/RestaurantData/ListRestaurants


            string url = "RestaurantData/ListRestaurants";
            HttpResponseMessage response = client.GetAsync(url).Result;

            IEnumerable<RestaurantDto> Restaurants = response.Content.ReadAsAsync<IEnumerable<RestaurantDto>>().Result;

            return View(Restaurants);
        }

        // GET: Restaurant/Details/5
        public ActionResult Details(int id)
        {
            //objective: communicate with our Restaurant data api to retrieve one Restaurant
            //curl https://localhost:44324/api/RestaurantData/FindRestaurant/{id}
            string url = "RestaurantData/FindRestaurant/" + id;
            HttpResponseMessage response = client.GetAsync(url).Result;

            RestaurantDto RestaurantDto = response.Content.ReadAsAsync<RestaurantDto>().Result;
            Debug.WriteLine(RestaurantDto);
            return View(RestaurantDto);
        }

        // GET: Restaurant/New
        [Authorize(Roles = "Admin")]
        public ActionResult New()
        {
            return View();
        }

        // POST: Restaurant/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(Restaurant Restaurant)
        {
            GetApplicationCookie();//get token credentials
            //objective: add a new Restaurant into our system using the API
            //curl -H "Content-Type:application/json" -d @Restaurant.json https://localhost:44324/api/RestaurantData/AddRestaurant
            string url = "RestaurantData/AddRestaurant";


            string jsonpayload = jss.Serialize(Restaurant);
            Debug.WriteLine(jsonpayload);

            HttpContent content = new StringContent(jsonpayload);
            content.Headers.ContentType.MediaType = "application/json";

            HttpResponseMessage response = client.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("List");
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // GET: Restaurant/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            //objective: communicate with our Restaurant data api to retrieve one Restaurant
            //curl https://localhost:44324/api/RestaurantData/FindRestaurant/{id}
            string url = "RestaurantData/FindRestaurant/" + id;
            HttpResponseMessage response = client.GetAsync(url).Result;
            RestaurantDto selectedRestaurant = response.Content.ReadAsAsync<RestaurantDto>().Result;
            return View(selectedRestaurant);
        }

        // POST: Restaurant/Update/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Update(int id, Restaurant Restaurant)
        {
            //objective: communicate with our Restaurant data api to Update specific ID Restaurant
            //curl https://localhost:44324/api/RestaurantData/UpdateRestaurant/{id}

            GetApplicationCookie();//get token credentials
            string url = "RestaurantData/UpdateRestaurant/" + id;

            string jsonpayload = jss.Serialize(Restaurant);

            HttpContent content = new StringContent(jsonpayload);

            content.Headers.ContentType.MediaType = "application/json";

            HttpResponseMessage response = client.PostAsync(url, content).Result;

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("List");
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // GET: Restaurant/DeleteConfirm/5
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirm(int id)
        {
            //objective: communicate with our Restaurant data api to retrieve one Restaurant
            //curl https://localhost:44324/api/RestaurantData/FindRestaurant/{id}
            string url = "RestaurantData/FindRestaurant/" + id;
            HttpResponseMessage response = client.GetAsync(url).Result;
            RestaurantDto selectedRestaurant = response.Content.ReadAsAsync<RestaurantDto>().Result;
            return View(selectedRestaurant);
        }

        // POST: Restaurant/Delete/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            //objective: communicate with our Restaurant data api to Delete specific ID Restaurant
            //curl https://localhost:44324/api/RestaurantData/DeleteRestaurant/{id}
            GetApplicationCookie();//get token credentials

            string url = "RestaurantData/DeleteRestaurant/" + id;
            HttpContent content = new StringContent("");
            content.Headers.ContentType.MediaType = "application/json";
            HttpResponseMessage response = client.PostAsync(url, content).Result;

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("List");
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        public ActionResult Error()
        {
            return View();
        }
    }
}