using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CreativeCollab_yj_sy.Models.ViewModels
{
    public class EditJourney
    {
        public JourneyDto SeletedJourney { get; set; }
        public IEnumerable<RestaurantDto> RestaurantList { get; set; }
        public IEnumerable<RestaurantDto> JourneyRestaurant { get; set; }
        public IEnumerable<DestinationDto> DestinationList { get; set; }
        public IEnumerable<DestinationDto> JourneyDestination { get; set; }
    }
}