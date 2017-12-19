namespace Musicollage.Models
{
    public class DisplayRelease
    {
        public string id {get;set;}
        public string title {get;set;}
        public string image {get;set;}
        public string date {get;set;}
        public string type {get;set;}
        public string artist {get;set;}
        public decimal avg_rating {get;set;}
        public decimal ratings {get;set;}
        public string arid {get;set;} //artist ID
    }
}