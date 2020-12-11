﻿using System.Collections.Generic;

namespace BusinessLogic.Models
{
    public class Company
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
