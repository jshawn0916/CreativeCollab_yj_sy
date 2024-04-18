using CreativeCollab_yj_sy.Migrations;
using CreativeCollab_yj_sy.Models;
using CreativeCollab_yj_sy.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;

namespace CreativeCollab_yj_sy.Controllers
{
    public class JourneyController : Controller
    {
        private static readonly HttpClient client;
        private JavaScriptSerializer jss = new JavaScriptSerializer();

        static JourneyController()
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

        // GET: Journey/List
        [Authorize(Roles = "Admin,Guest")]
        public ActionResult List()
        {
            GetApplicationCookie();
            //objective: communicate with our Journey data api to retrieve a list of Journeys
            //curl https://localhost:44324/api/RestaurantData/ListRestaurants


            string url = "JourneyData/ListJourneys";
            HttpResponseMessage response = client.GetAsync(url).Result;

            IEnumerable<JourneyDto> Journeys = response.Content.ReadAsAsync<IEnumerable<JourneyDto>>().Result;

            return View(Journeys);
        }

        // GET: Journey/New
        [Authorize(Roles = "Admin,Guest")]
        public ActionResult New()
        {
            RestaurantsDestinations ViewModel = new RestaurantsDestinations();

            //objective: communicate with our Restaurant data api to retrieve all Restaurants
            //curl https://localhost:44324/RestaurantData/ListRestaurants
            string url = "RestaurantData/ListRestaurants";
            HttpResponseMessage response = client.GetAsync(url).Result;
            IEnumerable<RestaurantDto> RestaurantList = response.Content.ReadAsAsync<IEnumerable<RestaurantDto>>().Result;
            ViewModel.RestaurantList = RestaurantList;

            //objective: communicate with our Destination data api to retrieve all Destinations
            //curl https://localhost:44324/DestinationData/ListDestinations
            url = "DestinationData/ListDestinations";
            response = client.GetAsync(url).Result;
            IEnumerable<DestinationDto> DestinationList = response.Content.ReadAsAsync<IEnumerable<DestinationDto>>().Result;

            ViewModel.DestinationList = DestinationList;
            return View(ViewModel);
        }

        // Post: Journey/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Guest")]
        public ActionResult Create(Journey Journey, int[] RestaurantIds, int[] DestinationIds)
        {
            GetApplicationCookie();

            // objective: add a new Journey into our system using the API
            // curl https://localhost:44324/api/JourneyData/AddJourney
            string url = "JourneyData/AddJourney";
            string UserId = User.Identity.GetUserId();

            Journey.UserID = UserId;

            string jsonpayload = jss.Serialize(Journey);
            HttpContent content = new StringContent(jsonpayload);

            content.Headers.ContentType.MediaType = "application/json";

            HttpResponseMessage response = client.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string journeyData = response.Content.ReadAsStringAsync().Result;
                JourneyDto createdJourney = jss.Deserialize<JourneyDto>(journeyData);
                int createdJourneyId = createdJourney.JourneyID;

                //objective: Create seleted Restaurants under the care of this Journey
                //curl https://localhost:44324/api/JourneyData/AssociateJourneyWithRestaurants/{JourneyID}
                url = "JourneyData/AssociateJourneyWithRestaurants/" + createdJourneyId;

                jsonpayload = jss.Serialize(RestaurantIds);
                content = new StringContent(jsonpayload);
                content.Headers.ContentType.MediaType = "application/json";
                response = client.PostAsync(url, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    //objective: Create seleted Destinations under the care of this Journey
                    //curl https://localhost:44324/api/JourneyData/AssociateJourneyWithDestinations/{JourneyID}
                    url = "JourneyData/AssociateJourneyWithDestinations/" + createdJourneyId;
                    jsonpayload = jss.Serialize(DestinationIds);
                    content = new StringContent(jsonpayload);
                    content.Headers.ContentType.MediaType = "application/json";
                    response = client.PostAsync(url, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("List");
                    }
                    else
                    {
                        return RedirectToAction("Error");
                    }
                }
                else
                {
                    return RedirectToAction("Error");
                }
                
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // GET: Journey/Details/3
        [Authorize(Roles = "Admin,Guest")]
        public ActionResult Details(int id)
        {
            GetApplicationCookie();

            DetailJourney ViewModel = new DetailJourney();

            //objective: communicate with our Journey data api to retrieve one Journey
            //curl https://localhost:44324/api/JourneyData/FindJourney/{JourneyID}

            string url = "JourneyData/FindJourney/" + id;
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (!response.IsSuccessStatusCode) return RedirectToAction("Error");
            JourneyDto JourneyDto = response.Content.ReadAsAsync<JourneyDto>().Result;

            ViewModel.JourneyDto = JourneyDto;

            //objective: Show all Restaurants under the care of this Journey
            //curl https://localhost:44324/api/RestaurantData/ListRestaurantsforJourney/{JourneyID}
            url = "RestaurantData/ListRestaurantsforJourney/" + id;
            response = client.GetAsync(url).Result;
            if (!response.IsSuccessStatusCode) return RedirectToAction("Error");
            IEnumerable<RestaurantDto> RestaurantList = response.Content.ReadAsAsync<IEnumerable<RestaurantDto>>().Result;

            ViewModel.RestaurantList = RestaurantList;

            //objective: Show all Destinations under the care of this Journey
            //curl https://localhost:44324/api/DestinationData/ListDestinationsforJourney/{JourneyID}/{UserID}
            url = "DestinationData/ListDestinationsforJourney/" + id;
            response = client.GetAsync(url).Result;
            if (!response.IsSuccessStatusCode) return RedirectToAction("Error");
            IEnumerable<DestinationDto> DestinationList = response.Content.ReadAsAsync<IEnumerable<DestinationDto>>().Result;

            ViewModel.DestinationList = DestinationList;


            return View(ViewModel);
        }

        // GET: Journey/Edit/5
        [Authorize(Roles = "Admin,Guest")]
        public ActionResult Edit(int id)
        {
            EditJourney ViewModel = new EditJourney();

            //objective: communicate with our user data api to retrieve one Journey
            //curl https://localhost:44324/api/JourneyData/FindJourney/{JourneyID}

            string url = "JourneyData/FindJourney/" + id;
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (!response.IsSuccessStatusCode) return RedirectToAction("Error");

            JourneyDto SeletedJourney = response.Content.ReadAsAsync<JourneyDto>().Result;

            ViewModel.SeletedJourney = SeletedJourney;

            //objective: Show all Restaurants
            //curl https://localhost:44324/api/RestaurantData/ListRestaurants
            url = "RestaurantData/ListRestaurants";
            response = client.GetAsync(url).Result;
            if (!response.IsSuccessStatusCode) return RedirectToAction("Error");

            IEnumerable<RestaurantDto> RestaurantList = response.Content.ReadAsAsync<IEnumerable<RestaurantDto>>().Result;
            ViewModel.RestaurantList = RestaurantList;

            //objective: Show all Restaurants under the care of this Journey
            //curl https://localhost:44324/api/RestaurantData/ListRestaurantsforJourney/{JourneyID}
            url = "RestaurantData/ListRestaurantsforJourney/" + id;
            response = client.GetAsync(url).Result;
            if (!response.IsSuccessStatusCode) return RedirectToAction("Error");
            IEnumerable<RestaurantDto> JourneyRestaurant = response.Content.ReadAsAsync<IEnumerable<RestaurantDto>>().Result;
            ViewModel.JourneyRestaurant = JourneyRestaurant;


            //objective: Show all Destinations
            //curl https://localhost:44324/api/DestinationData/ListDestinations
            url = "DestinationData/ListDestinations";
            response = client.GetAsync(url).Result;
            if (!response.IsSuccessStatusCode) return RedirectToAction("Error");

            IEnumerable<DestinationDto> DestinationList = response.Content.ReadAsAsync<IEnumerable<DestinationDto>>().Result;

            ViewModel.DestinationList = DestinationList;

            //objective: Show all Destinations under the care of this Journey
            //curl https://localhost:44324/api/DestinationData/ListDestinationsforJourney/{JourneyID}
            
            url = "DestinationData/ListDestinationsforJourney/" + id;
            response = client.GetAsync(url).Result;
            if (!response.IsSuccessStatusCode) return RedirectToAction("Error");

            IEnumerable<DestinationDto> JourneyDestination = response.Content.ReadAsAsync<IEnumerable<DestinationDto>>().Result;

            ViewModel.JourneyDestination = JourneyDestination;

            return View(ViewModel);
        }

        //POST: Journey/Update/5
        [HttpPost]
        [Authorize(Roles = "Admin,Guest")]
        public ActionResult Update(int id, Journey Journey, int[] RestaurantIds, int[] DestinationIds)
        {
            //objective: Update Journey
            //curl https://localhost:44324/api/JourneyData/UpdateJourney/{JourneyID}
            string url = "JourneyData/UpdateJourney/" + id;
            Debug.WriteLine(url);
            string jsonpayload = jss.Serialize(Journey);
            HttpContent content = new StringContent(jsonpayload);
            content.Headers.ContentType.MediaType = "application/json";
            HttpResponseMessage response = client.PostAsync(url, content).Result;
            if (!response.IsSuccessStatusCode) return RedirectToAction("Error");

            string JourneyData = response.Content.ReadAsStringAsync().Result;
            JourneyDto updateJourney = jss.Deserialize<JourneyDto>(JourneyData);
            int updateJourneyId = updateJourney.JourneyID;

            //objective: Delete all Restaurants under the care of this Journey
            //curl https://localhost:44324/api/JourneyData/UnAssociateJourneyWithRestaurants/{JourneyID}
            url = "JourneyData/UnAssociateJourneyWithRestaurants/" + updateJourneyId;
            response = client.PostAsync(url, null).Result;
            if (!response.IsSuccessStatusCode) return RedirectToAction("Error");

            //objective: Delete all Destinations under the care of this Journey
            //curl https://localhost:44324/api/JourneyData/UnAssociateJourneyWithDestinations/{JourneyID}
            url = "JourneyData/UnAssociateJourneyWithDestinations/" + updateJourneyId;
            response = client.PostAsync(url, null).Result;
            if (!response.IsSuccessStatusCode) return RedirectToAction("Error");

            //objective: Create seleted Restaurants under the care of this Journey
            //curl https://localhost:44324/api/JourneyData/AssociateJourneyWithRestaurants/{JourneyID}
            url = "JourneyData/AssociateJourneyWithRestaurants/" + updateJourneyId;
            jsonpayload = jss.Serialize(RestaurantIds);
            content = new StringContent(jsonpayload);
            content.Headers.ContentType.MediaType = "application/json";
            response = client.PostAsync(url, content).Result;
            if (!response.IsSuccessStatusCode) return RedirectToAction("Error");

            //objective: Create seleted Destinations under the care of this Journey
            //curl https://localhost:44324/api/JourneyData/AssociateJourneyWithDestinations/{JourneyID}
            url = "JourneyData/AssociateJourneyWithDestinations/" + updateJourneyId;
            jsonpayload = jss.Serialize(DestinationIds);
            content = new StringContent(jsonpayload);
            content.Headers.ContentType.MediaType = "application/json";
            response = client.PostAsync(url, content).Result;
            if (!response.IsSuccessStatusCode) return RedirectToAction("Error");

            return RedirectToAction("Details/" + id);

        }

        // GET: Journey/DeleteConfirm/5
        [Authorize(Roles = "Admin,Guest")]
        public ActionResult DeleteConfirm(int id)
        {
            //objective: communicate with our user data api to retrieve one Journey
            //curl https://localhost:44324/api/JourneyData/FindJourney/{id}
            string url = "JourneyData/FindJourney/" + id;
            HttpResponseMessage response = client.GetAsync(url).Result;

            JourneyDto JourneyDto = response.Content.ReadAsAsync<JourneyDto>().Result;

            return View(JourneyDto);
        }

        // POST: Journey/Delete/5
        [HttpPost]
        [Authorize(Roles = "Admin,Guest")]
        public ActionResult Delete(int id)
        {
            string url = "JourneyData/DeleteJourney/" + id;
            HttpContent content = new StringContent("");
            content.Headers.ContentType.MediaType = "application/json";
            HttpResponseMessage response = client.PostAsync(url, content).Result;

            if (response.IsSuccessStatusCode)
            {
                int travelerId = response.Content.ReadAsAsync<int>().Result;
                return RedirectToAction("List");
            }
            else
            {
                return RedirectToAction("Error");
            }
        }
    }
}