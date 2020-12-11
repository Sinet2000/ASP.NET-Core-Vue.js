namespace BusinessLogic.Models
{
    public class UserOrder
    {
        public int ID { get; set; }
        public string? Name { get; set; }
        public int Price { get; set; }
        public string? Description { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
