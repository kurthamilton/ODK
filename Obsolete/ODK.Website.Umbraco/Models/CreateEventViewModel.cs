using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ODK.Website.Models
{
    public class CreateEventViewModel
    {
        public string Address { get; set; }

        public DateTime Date { get; set; }

        [AllowHtml]
        public string Description { get; set; }

        [DisplayName("Image URL")]
        public string ImageUrl { get; set; }

        [Required]
        public string Location { get; set; }

        [DisplayName("Map Search")]
        public string MapQuery { get; set; }

        [Required]
        public string Name { get; set; }

        public string Time { get; set; }
    }
}