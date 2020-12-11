using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogic.Models
{
    public class Country
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<Address> Addresses = new List<Address>();
    }
}
