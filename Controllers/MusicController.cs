using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Musicollage.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Musicollage.Controllers
{
    public class MusicController : Controller
    {
        private MusicContext _context;
 
        public MusicController(MusicContext context)
        {
            _context = context;
        }

        [Route("artist/{id}")]
        public IActionResult Artist(string id)
        {
            JObject artist = new JObject();
            ApiCaller.GetArtistData(id, a => {
                artist = (JObject)a;
            }).Wait();
            ViewBag.artist = artist;
            List<DisplayRelease> releases = new List<DisplayRelease>();
            JToken temp = artist.SelectToken("release-groups");
            foreach(var release in temp)
            {
                DisplayRelease toAdd = new DisplayRelease()
                {
                    id = (string)release["id"],
                    title = (string)release["title"],
                    date = (string)release["first-release-date"]
                };
                if((string)release["primary-type"] == "Album" && release["secondary-types"].Count() == 0)
                {
                    toAdd.type = "Album";
                }
                if((string)release["primary-type"] == "EP" && release["secondary-types"].Count() == 0)
                {
                    toAdd.type = "EP";
                }
                if(release["secondary-types"].Count() > 0)
                {
                    if((string)release["secondary-types"].First == "Live" || (string)release["secondary-types"].Last() == "Live")
                    {
                        toAdd.type = "Live";
                    }
                }
                releases.Add(toAdd);
            }
            ViewBag.albums = releases.Where(r => r.type == "Album").OrderBy(r => r.date).ToList();
            ViewBag.eps = releases.Where(r => r.type == "EP").OrderBy(r => r.date).ToList();
            ViewBag.live = releases.Where(r => r.type == "Live").OrderBy(r => r.date).ToList();
            return View();
        }

        [Route("release/{id}")]
        public IActionResult Release(string id)
        {
            JObject releaseJson = new JObject();
            ApiCaller.GetReleaseData(id, r => {
                releaseJson = (JObject)r;
            }).Wait();
            JObject art = new JObject();
            ApiCaller.GetArt(id, a => {
                art = (JObject)a;
            }).Wait();
            DisplayRelease release = new DisplayRelease()
            {
                title = (string)releaseJson.SelectToken("title"),
                date = (string)releaseJson.SelectToken("first-release-date"),
                id = (string)releaseJson.SelectToken("id"),
                artist = (string)releaseJson.SelectToken("artist-credit").First.SelectToken("name"),
                arid = (string)releaseJson.SelectToken("artist-credit").First.SelectToken("artist").SelectToken("id"),
                avg_rating = -1,
                ratings = 0
            };
            try
            {
                release.image = (string)art.SelectToken("images").First.SelectToken("image");
            }
            catch { System.Console.WriteLine("No image available"); }
            ViewBag.logged = false;
            string placeholder = null;
            Release thisRelease = _context.Releases.SingleOrDefault(r => r.id_string == id);
            if(HttpContext.Session.GetInt32("id") != null)
            {
                placeholder = "0 to 10";
                ViewBag.logged = true;
                int? release_id = null;
                if(thisRelease != null) release_id = thisRelease.id;
                if(release_id != null)
                {
                    Rating thisRating = _context.Ratings.Where(r => r.user_id == HttpContext.Session.GetInt32("id")).SingleOrDefault(r => r.release_id == release_id);
                    if(thisRating != null) placeholder = thisRating.rating.ToString();
                }   
            }
            if(thisRelease != null)
            {
                release.avg_rating = thisRelease.avg_rating;
                release.ratings = thisRelease.ratings;
            }
            ViewBag.placeholder = placeholder;
            return View(release);
        }

        [HttpPost]
        [Route("release/{id}/rate")]
        public IActionResult Rate(string id, decimal? rating)
        {
            if(rating != null)
            {
                Release thisRelease = _context.Releases.SingleOrDefault(r => r.id_string == id);
                if(thisRelease == null)
                {
                    Release newRelease = new Release(){id_string = id};
                    string title = "";
                    string artist = "";
                    ApiCaller.GetReleaseData(id, a => {
                        title = (string)(((JObject)a).SelectToken("title"));
                        artist = (string)((JObject)a).SelectToken("artist-credit").First.SelectToken("artist").SelectToken("name");
                    }).Wait();
                    newRelease.title = title;
                    newRelease.artist = artist;
                    _context.Add(newRelease);
                    _context.SaveChanges();
                    thisRelease = _context.Releases.SingleOrDefault(r => r.id_string == id);
                }
                int user_id = (int)HttpContext.Session.GetInt32("id");
                Rating thisRating = _context.Ratings.Where(r => r.id == user_id).SingleOrDefault(r => r.release_id == thisRelease.id);
                if(thisRating == null)
                {
                    Rating newRating = new Rating()
                    {
                        user_id = user_id,
                        release_id = thisRelease.id,
                        rating = (decimal)rating
                    };
                    _context.Add(newRating);
                    decimal temp = thisRelease.avg_rating * thisRelease.ratings;
                    temp += newRating.rating;
                    thisRelease.ratings++;
                    thisRelease.avg_rating = temp / thisRelease.ratings;
                }
                else
                {
                    decimal temp = thisRelease.avg_rating * thisRelease.ratings;
                    temp -= thisRating.rating;
                    temp += (decimal)rating;
                    thisRating.rating = (decimal)rating;
                    thisRelease.avg_rating = temp / thisRelease.ratings;
                }                
                _context.Update(thisRelease);
                _context.SaveChanges();
            }
            return RedirectToAction("Release", new {id = id});
        }
    }
}
