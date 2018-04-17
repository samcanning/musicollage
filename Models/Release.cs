namespace Musicollage.Models
{
    public class Release
    {
        public int id {get;set;}
        public decimal avg_rating {get;set;}
        public int ratings {get;set;}
        public string id_string {get;set;}
        public string title {get;set;}
        public string artist {get;set;}
        public string artist_id_string {get;set;}
        public string date {get;set;}
        public string image {get;set;}
    }
}