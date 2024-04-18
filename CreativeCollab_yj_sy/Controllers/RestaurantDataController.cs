using System;
using System.IO;
using System.Web;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using CreativeCollab_yj_sy.Models;
using System.Diagnostics;
using Microsoft.AspNet.Identity;

namespace CreativeCollab_yj_sy.Controllers
{
    public class RestaurantDataController : ApiController
    {
        //utlizing the database connection
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// Returns all Restaurants in the system.
        /// </summary>
        /// <returns>
        /// HEADER: 200 (OK)
        /// CONTENT: all Restaurants in the database, including their associated Restaurants.
        /// </returns>
        /// <example>
        /// GET: api/RestaurantData/ListRestaurants
        /// </example>
        [HttpGet]
        [ResponseType(typeof(DestinationDto))]
        public IHttpActionResult ListRestaurants()
        {
            List<Restaurant> Restaurants = db.Restaurants.ToList();
            List<RestaurantDto> RestaurantDtos = new List<RestaurantDto>();

            Restaurants.ForEach(R => RestaurantDtos.Add(new RestaurantDto()
            {
                RestaurantID = R.RestaurantID,
                RestaurantName = R.RestaurantName,
                RestaurantDescription = R.RestaurantDescription,
                Rate = R.Rate,
                Location = R.Location
            }));
            return Ok(RestaurantDtos);
        }

        /// <summary>
        /// Grthers informatioon about Restaurants related to a particular Journey
        /// </summary>
        /// <returns>
        /// HEADER: 200(Ok)
        /// CONTENT: Returns all Restaurants in a database associated with a particular Journey
        /// </returns>
        /// <example>
        /// GET: api/RestaurantData/ListRestaurantsforJourney/4
        /// </example>
        [HttpGet]
        [Route("api/RestaurantData/ListRestaurantsforJourney/{JourneyID}")]
        [Authorize(Roles = "Admin,Guest")]
        public IHttpActionResult ListRestaurantsforJourney(int JourneyID)
        {
            Journey Journey = db.Journeys.Find(JourneyID);
            if (Journey == null)
            {
                return NotFound();
            }
            //do not process if the (user is not an admin) and (the Journey does not belong to the user)
            bool isAdmin = User.IsInRole("Admin");
            //Forbidden() isn't a natively implemented status like BadRequest()
            if (!isAdmin && (Journey.UserID != User.Identity.GetUserId())) return StatusCode(HttpStatusCode.Forbidden);

            //sending a query to the database
            //select * from Restaurants...
            List<Restaurant> Restaurants = db.Restaurants.Where(
                R => R.Journeys.Any(J => J.JourneyID == JourneyID)).ToList();

            List<RestaurantDto> RestaurantDtos = new List<RestaurantDto>();

            Restaurants.ForEach(R => RestaurantDtos.Add(new RestaurantDto()
            {
                RestaurantID = R.RestaurantID,
                RestaurantName = R.RestaurantName,
                RestaurantDescription = R.RestaurantDescription,
                Rate = R.Rate,
                Location = R.Location,
            }));

            //push the results to the list of Restaurants to return
            return Ok(RestaurantDtos);
        }

        /// <summary>
        /// Returns Restaurant in the system.
        /// </summary>
        /// <returns>
        /// HEADER: 200 (OK)
        /// CONTENT: A Restaurant in the system matching up to the Restaurant ID primary key
        /// or
        /// HEADER: 404 (NOT FOUND)
        /// </returns>
        /// <param name="id">The primary key of the Restaurant</param>
        /// <example>
        /// GET: api/RestaurantData/FindRestaurant/5
        /// </example>
        [ResponseType(typeof(RestaurantDto))]
        [HttpGet]
        public IHttpActionResult FindRestaurant(int id)
        {
            Restaurant Restaurant = db.Restaurants.Find(id);
            RestaurantDto RestaurantDto = new RestaurantDto()
            {
                RestaurantID = Restaurant.RestaurantID,
                RestaurantName = Restaurant.RestaurantName,
                RestaurantDescription = Restaurant.RestaurantDescription,
                Rate = Restaurant.Rate,
                Location = Restaurant.Location
            };
            if (Restaurant == null)
            {
                return NotFound();
            }
            Debug.WriteLine(RestaurantDto.RestaurantID);
            return Ok(RestaurantDto);
        }

        /// <summary>
        /// Adds a Restaurant to the system
        /// </summary>
        /// <param name="Restaurant">JSON FORM DATA of an Restaurant</param>
        /// <returns>
        /// HEADER: 201 (Created)
        /// CONTENT: Restaurant ID, Restaurant Data
        /// or
        /// HEADER: 400 (Bad Request)
        /// </returns>
        /// <example>
        /// POST: api/RestaurantData/AddRestaurant
        /// FORM DATA: Restaurant JSON Object
        /// </example>
        [ResponseType(typeof(Restaurant))]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult AddRestaurant(Restaurant Restaurant)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Restaurants.Add(Restaurant);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = Restaurant.RestaurantID }, Restaurant);
        }

        /// <summary>
        /// Updates a particular Restaurant in the system with POST Data input
        /// </summary>
        /// <param name="id">Represents the Restaurant ID primary key</param>
        /// <param name="Restaurant">JSON FORM DATA of a Restaurant</param>
        /// <returns>
        /// HEADER: 204 (Success, No Content Response)
        /// or
        /// HEADER: 400 (Bad Request)
        /// or
        /// HEADER: 404 (Not Found)
        /// </returns>
        /// <example>
        /// POST: api/RestaurantData/UpdateRestaurant/5
        /// FORM DATA: Restaurant JSON Object
        /// </example>
        [ResponseType(typeof(void))]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult UpdateRestaurant(int id, Restaurant Restaurant)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != Restaurant.RestaurantID)
            {

                return BadRequest();
            }

            db.Entry(Restaurant).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RestaurantExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Deletes a Restaurant from the system by it's ID.
        /// </summary>
        /// <param name="id">The primary key of the Restaurant</param>
        /// <returns>
        /// HEADER: 200 (OK)
        /// or
        /// HEADER: 404 (NOT FOUND)
        /// </returns>
        /// <example>
        /// POST: api/RestaurantData/DeleteRestaurant/5
        /// FORM DATA: (empty)
        /// </example>
        [ResponseType(typeof(Restaurant))]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult DeleteRestaurant(int id)
        {
            Restaurant Restaurant = db.Restaurants.Find(id);
            if (Restaurant == null)
            {
                return NotFound();
            }

            db.Restaurants.Remove(Restaurant);
            db.SaveChanges();

            return Ok();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool RestaurantExists(int id)
        {
            return db.Restaurants.Count(e => e.RestaurantID == id) > 0;
        }
    }
}
