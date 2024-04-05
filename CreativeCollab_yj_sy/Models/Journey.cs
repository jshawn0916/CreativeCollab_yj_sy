using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;

namespace CreativeCollab_yj_sy.Models
{
    public class Journey
    {
        [Key]
        public int JourneyID { get; set; }
        [Required]
        public string JourneyName { get; set; }
        [Required]
        public string JourneyDescription { get; set; }

        //A Journey belongs to one Users
        //A Users can have many Journeys
        [Required]
        public string UserID { get; set; }
        public virtual ApplicationUser User { get; set; }

        //A Journey can take care of many Restaurants
        public ICollection<Restaurant> Restaurants { get; set; }

        //A Journey can take care of many Destinations
        public ICollection<Destination> Destinations { get; set; }
    }

    public class JourneyDto
    {
        public int JourneyID { get; set; }
        public string JourneyName { get; set; }
        public string JourneyDescription { get; set; }
        public string UserID { get; set; }
    }
}