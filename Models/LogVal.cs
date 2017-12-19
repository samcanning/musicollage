using System.ComponentModel.DataAnnotations;
namespace Musicollage.Models
{
    public class LogVal
    {
        [Required(ErrorMessage = "Username is required.")]
        public string logname {get;set;}
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string logpw {get;set;}
    }
}