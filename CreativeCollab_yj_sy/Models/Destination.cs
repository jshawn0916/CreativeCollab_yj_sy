using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CreativeCollab_yj_sy.Models
{
    public class Destination
    {
        [Key]
        public int DestinationID { get; set; }
        [Required]
        public string DestinationName { get; set; }
        [Required]
        public string DestinationDescription { get; set; }
        [Required]
        public string Location { get; set; }

        //A Destination can take care of many Journeys
        public ICollection<Journey> Journeys { get; set; }
    }
    public class DestinationDto
    {
        public int DestinationID { get; set; }
        public string DestinationName { get; set; }
        public string DestinationDescription { get; set; }
        public string Location { get; set; }
    }
}