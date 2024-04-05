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
using CreativeCollab_yj_sy.Migrations;

namespace CreativeCollab_yj_sy.Controllers
{
    public class JourneyDataController : ApiController
    {
        //utlizing the database connection
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// Returns all Journeys in the system.
        /// </summary>
        /// <returns>
        /// HEADER: 200 (OK)
        /// CONTENT: all Journeys in the database
        /// </returns>
        /// <example>
        /// GET: api/JourneyData/ListJourneys
        /// </example>
        [HttpGet]
        [ResponseType(typeof(JourneyDto))]
        [Authorize(Roles = "Admin,Guest")]
        public IHttpActionResult ListJourneys()
        {
            bool isAdmin = User.IsInRole("Admin");

            //Admins see all, guests only see their own
            List<Journey> Journeys;
            Debug.WriteLine("id is " + User.Identity.GetUserId());
            if (isAdmin) Journeys = db.Journeys.ToList();
            else
            {
                string UserId = User.Identity.GetUserId();
                Journeys = db.Journeys.Where(J => J.UserID == UserId).ToList();
            }

            List<JourneyDto> JourneyDtos = new List<JourneyDto>();

            Journeys.ForEach(J => JourneyDtos.Add(new JourneyDto()
            {
                JourneyID = J.JourneyID,
                JourneyName = J.JourneyName,
                JourneyDescription = J.JourneyDescription,
                UserID = J.UserID
            }));

            return Ok(JourneyDtos);
        }

        /// <summary>
        /// return a Journey to the system
        /// </summary>
        /// <returns>
        /// HEADER: 200(Ok)
        /// Retrieve a Journey from the system matching the primary key JourneyID and UserID
        /// or
        /// HEADER: 404(Not Found)
        /// </returns>
        /// <param name="id">The primary key of the Journey</param>
        /// <example>
        /// Get: api/JourneyData/FindJourney/4
        /// </example>
        [HttpGet]
        [ResponseType(typeof(JourneyDto))]
        [Authorize(Roles = "Admin,Guest")]
        public IHttpActionResult FindJourney(int id)
        {
            Journey Journey = db.Journeys.Find(id);
            JourneyDto JourneyDto = new JourneyDto()
            {
                JourneyID = Journey.JourneyID,
                JourneyName = Journey.JourneyName,
                JourneyDescription = Journey.JourneyDescription,
                UserID = Journey.UserID
            };

            //do not process if the (user is not an admin) and (the Journey does not belong to the user)
            bool isAdmin = User.IsInRole("Admin");
            //Forbidden() isn't a natively implemented status like BadRequest()
            if (!isAdmin && (Journey.UserID != User.Identity.GetUserId())) return StatusCode(HttpStatusCode.Forbidden);

            return Ok(JourneyDto);
        }

        /// <summary>
        /// Adds a Journey to the system
        /// </summary>
        /// <param name="Journey">JSON FORM DATA of a Journey</param>
        /// <returns>
        /// HEADER: 201(Created)
        /// CONTENT: JourneyID, Journey Data
        /// or
        /// HEADER: 400(Bad Request)
        /// </returns>
        /// <example>
        /// POST: api/JourneyData/AddJourney
        /// FORM DATA: Journey JSON Object
        /// </example>
        [ResponseType(typeof(Journey))]
        [HttpPost]
        [Authorize(Roles = "Admin,Guest")]
        public IHttpActionResult AddJourney(Journey Journey)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            

            Debug.WriteLine(User.Identity.GetUserId());

            //do not process if the (user is not an admin) and (the Journey does not belong to the user)
            bool isAdmin = User.IsInRole("Admin");
            //Forbidden() isn't a natively implemented status like BadRequest()
            if (!isAdmin && (Journey.UserID != User.Identity.GetUserId())) return StatusCode(HttpStatusCode.Forbidden);

            db.Journeys.Add(Journey);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = Journey.JourneyID }, Journey);
        }

        /// <summary>
        /// Associates a particular Journey with a particular Restaurants
        /// </summary>
        /// <param name="JourneyID">The JourneyID primary key</param>
        /// <param name="UserID">UserID of JourneyID</param>
        /// <returns>
        /// HEADER: 200 (OK)
        /// CONTENT: Restaurantids
        /// or
        /// HEADER: 404 (NOT FOUND)
        /// </returns>
        /// <example>
        /// POST api/JourneyData/AssociateJourneyWithRestaurants/1/2
        /// </example>
        [HttpPost]
        [Route("api/JourneyData/AssociateJourneyWithRestaurants/{JourneyID}/{UserID}")]
        [Authorize(Roles = "Admin,Guest")]
        public IHttpActionResult AssociateJourneyWithRestaurants(int JourneyID, string UserID, [FromBody] int[] Restaurantids)
        {
            //do not process if the (user is not an admin) and (the Journey does not belong to the user)
            bool isAdmin = User.IsInRole("Admin");
            //Forbidden() isn't a natively implemented status like BadRequest()
            if (!isAdmin && (UserID != User.Identity.GetUserId()))
            {
                Debug.WriteLine("not allowed. Journey user" + UserID + " user " + User.Identity.GetUserId());
                return StatusCode(HttpStatusCode.Forbidden);
            }

            Journey SelectedJourney = db.Journeys.Include(J => J.Restaurants).Where(J => J.JourneyID == JourneyID).FirstOrDefault();
            
            if (SelectedJourney == null)
            {
                return NotFound();
            };

            if (Restaurantids != null)
            {
                foreach (var restaurantid in Restaurantids)
                {
                    Restaurant SelectedRestaurant = db.Restaurants.Find(restaurantid);
                    if (SelectedRestaurant == null)
                    {
                        return NotFound();
                    }
                    //SQL equivalent:
                    //insert into RestaurantJourneys (Restaurant_RestaurantID, Journey_JourneyID) values ({JourneyID},{RestaurantID})
                    SelectedJourney.Restaurants.Add(SelectedRestaurant);
                }
            }
            db.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Removes an association between a particular Journey with a particular Restaurants
        /// </summary>
        /// <param name="JourneyID">The JourneyID primary key</param>
        /// <returns>
        /// HEADER: 200 (OK)
        /// or
        /// HEADER: 404 (NOT FOUND)
        /// </returns>
        /// <example>
        /// POST api/JourneyData/UnAssociateJourneyWithRestaurants/9/1
        /// </example>
        [HttpPost]
        [Route("api/JourneyData/UnAssociateJourneyWithRestaurants/{JourneyID}")]
        public IHttpActionResult UnAssociateJourneyWithRestaurants(int JourneyID)
        {
            Journey SelectedJourney = db.Journeys.Include(J => J.Restaurants).Where(J => J.JourneyID == JourneyID).FirstOrDefault();
            if (SelectedJourney == null)
            {
                return NotFound();
            };
            var restaurantsToRemove = new List<Restaurant>(SelectedJourney.Restaurants);
            foreach (var restaurant in restaurantsToRemove)
            {
                SelectedJourney.Restaurants.Remove(restaurant);
            }

            db.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Associates a particular Journey with a particular Destinations
        /// </summary>
        /// <param name="JourneyID">The JourneyID primary key</param>
        /// <param name="UserID">UserID of JourneyID</param>
        /// <returns>
        /// HEADER: 200 (OK)
        /// CONTENT: Destinationids
        /// or
        /// HEADER: 404 (NOT FOUND)
        /// </returns>
        /// <example>
        /// POST api/JourneyData/AssociateJourneyWithDestinations/1/2
        /// </example>
        [HttpPost]
        [Route("api/JourneyData/AssociateJourneyWithDestinations/{JourneyID}/{UserID}")]
        [Authorize(Roles = "Admin,Guest")]
        public IHttpActionResult AssociateJourneyWithDestinations(int JourneyID, string UserID, [FromBody] int[] Destinationids)
        {
            //do not process if the (user is not an admin) and (the Journey does not belong to the user)
            bool isAdmin = User.IsInRole("Admin");
            //Forbidden() isn't a natively implemented status like BadRequest()
            if (!isAdmin && (UserID != User.Identity.GetUserId()))
            {
                Debug.WriteLine("not allowed. Journey user" + UserID + " user " + User.Identity.GetUserId());
                return StatusCode(HttpStatusCode.Forbidden);
            }

            Journey SelectedJourney = db.Journeys.Include(J => J.Destinations).Where(F => F.JourneyID == JourneyID).FirstOrDefault();
            if (SelectedJourney == null)
            {
                return NotFound();
            };

            if (Destinationids != null)
            {
                foreach (var destinationid in Destinationids)
                {
                    Destination SelectedDestination = db.Destinations.Find(destinationid);
                    if (SelectedDestination == null)
                    {
                        return NotFound();
                    }
                    //SQL equivalent:
                    //insert into JourneyDestinations (Journey_JourneyID, Destination_DestinationID) values ({JourneyID},{RestaurantID})
                    SelectedJourney.Destinations.Add(SelectedDestination);
                }
            }


            db.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Removes an association between a particular Journey with a particular Destinations
        /// </summary>
        /// <param name="JourneyID">The JourneyID primary key</param>
        /// <returns>
        /// HEADER: 200 (OK)
        /// or
        /// HEADER: 404 (NOT FOUND)
        /// </returns>
        /// <example>
        /// POST api/JourneyData/UnAssociateJourneyWithDestinations/1/2
        /// </example>
        [HttpPost]
        [Route("api/JourneyData/UnAssociateJourneyWithDestinations/{JourneyID}/{UserID}")]
        public IHttpActionResult UnAssociateJourneyWithDestinations(int JourneyID)
        {
            Journey SelectedJourney = db.Journeys.Include(J => J.Destinations).Where(J => J.JourneyID == JourneyID).FirstOrDefault();
            if (SelectedJourney == null)
            {
                return NotFound();
            };
            var destinationsToRemove = new List<Destination>(SelectedJourney.Destinations);
            foreach (var destination in destinationsToRemove)
            {
                SelectedJourney.Destinations.Remove(destination);
            }

            db.SaveChanges();

            return Ok();
        }


        /// <summary>
        /// Updates a particular Journey in the system with POST Data input
        /// </summary>
        /// <param name="id">Represents the Journey ID primary key</param>
        /// <param name="Journey">JSON FORM DATA of an Journey</param>
        /// <returns>
        /// HEADER: 204 (Success, No Content Response)
        /// or
        /// HEADER: 400 (Bad Request)
        /// or
        /// HEADER: 404 (Not Found)
        /// </returns>
        /// <example>
        /// POST: api/JourneyData/UpdateJourney/5
        /// FORM DATA: Journey JSON Object
        /// </example>
        [ResponseType(typeof(void))]
        [HttpPost]
        public IHttpActionResult UpdateJourney(int id, Journey Journey)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != Journey.JourneyID)
            {
                return BadRequest();
            }

            //do not process if the (user is not an admin) and (the Journey does not belong to the user)
            bool isAdmin = User.IsInRole("Admin");
            //Forbidden() isn't a natively implemented status like BadRequest()
            if (!isAdmin && (Journey.UserID != User.Identity.GetUserId()))
            {
                Debug.WriteLine("not allowed. Journey user" + Journey.UserID + " user " + User.Identity.GetUserId());
                return StatusCode(HttpStatusCode.Forbidden);
            }

            db.Entry(Journey).State = EntityState.Modified;
            //do not modify the attached user id on update
            db.Entry(Journey).Property(J => J.UserID).IsModified = false;
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JourneyExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = Journey.JourneyID }, Journey);
        }

        /// <summary>
        /// Deletes a Journey from the system by it's ID.
        /// </summary>
        /// <param name="id">The primary key of the Journey</param>
        /// <returns>
        /// HEADER: 200 (OK)
        /// or
        /// HEADER: 404 (NOT FOUND)
        /// </returns>
        /// <example>
        /// POST: api/JourneyData/DeleteJourney/5
        /// FORM DATA: (empty)
        /// </example>
        [ResponseType(typeof(Journey))]
        [HttpPost]
        [Authorize(Roles = "Admin,Guest")]
        public IHttpActionResult DeleteJourney(int id)
        {
            Journey Journey = db.Journeys.Find(id);
            if (Journey == null)
            {
                return NotFound();
            }

            //do not process if the (user is not an admin) and (the Journey does not belong to the user)
            bool isAdmin = User.IsInRole("Admin");
            //Forbidden() isn't a natively implemented status like BadRequest()
            if (!isAdmin && (Journey.UserID != User.Identity.GetUserId())) return StatusCode(HttpStatusCode.Forbidden);


            db.Journeys.Remove(Journey);
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

        private bool JourneyExists(int id)
        {
            return db.Journeys.Count(e => e.JourneyID == id) > 0;
        }
    }
}
