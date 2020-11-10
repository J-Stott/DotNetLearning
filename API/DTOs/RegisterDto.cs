using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    //used in our register post request to store the username and password sent in the request body

    //required allows us to validate those fields automatically

    public class RegisterDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}