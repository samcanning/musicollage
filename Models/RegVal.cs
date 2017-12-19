using System.ComponentModel.DataAnnotations;
namespace Musicollage.Models
{
    public class RegVal
    {
        [Required(ErrorMessage = "Username is required.")]
        [MinLength(2, ErrorMessage = "Username must be at least 2 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username must be only alphanumeric characters.")]
        public string username {get;set;}
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        public string password {get;set;}
        [Required(ErrorMessage = "Must enter password confirmation.")]
        [Compare("password", ErrorMessage = "Passwords must match.")]
        [DataType(DataType.Password)]
        public string pwconfirm {get;set;}
    }
}