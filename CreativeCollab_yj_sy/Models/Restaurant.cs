using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CreativeCollab_yj_sy.Models
{
    public class Restaurant
    {
        [Key]
        public int RestaurantID { get; set; }
        [Required]
        public string RestaurantName { get; set; }
        [Required]
        public string RestaurantDescription { get; set; }
        [Required]
        public decimal Rate { get; set; }
        [Required]
        public string Location { get; set; }

        //A Restaurant can take care of many Journeys
        public ICollection<Journey> Journeys { get; set; }
    }

    public class RestaurantDto
    {
        public int RestaurantID { get; set; }
        public string RestaurantName { get; set; }
        public string RestaurantDescription { get; set; }
        public decimal Rate { get; set; }
        public string Location { get; set; }
    }
}