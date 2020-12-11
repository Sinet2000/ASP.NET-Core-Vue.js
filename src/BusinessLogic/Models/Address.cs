using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogic.Models
{
    public class Address
    {
        public int Id { get; set; }

        public string? StreetAddress { get; set; }

        public string? City { get; set; }

        public string ZipCode { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int CountryId { get; set; }
        public Country Country { get; set; }
    }
}
