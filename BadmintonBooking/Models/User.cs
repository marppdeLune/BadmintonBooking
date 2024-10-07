namespace BadmintonBooking.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // Roles: Player, Receptionist, Admin
    }

    public class Player : User
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CreditCard { get; set; }
    }
}
