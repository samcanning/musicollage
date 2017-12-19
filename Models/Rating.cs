namespace Musicollage.Models
{
    public class Rating
    {
        public int id {get;set;}
        public decimal rating {get;set;}
        public int user_id {get;set;}
        public int release_id {get;set;}
    }
}