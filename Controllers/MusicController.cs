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
            Release existingRelease = _context.Releases.SingleOrDefault(r => r.id_string == id);
            DisplayRelease release = new DisplayRelease();
            if(existingRelease != null)
            {
                release.title = existingRelease.title;
                release.date = existingRelease.date;
                release.id = id;
                release.artist = existingRelease.artist;
                release.arid = existingRelease.artist_id_string;
                release.avg_rating = -1;
                release.ratings = 0;
                release.image = existingRelease.image;
            }
            else
            {
                JObject releaseJson = new JObject();
                ApiCaller.GetReleaseData(id, r => {
                    releaseJson = (JObject)r;
                }).Wait();
                release.title = (string)releaseJson.SelectToken("title");
                release.date = (string)releaseJson.SelectToken("first-release-date");
                release.id = (string)releaseJson.SelectToken("id");
                release.artist = (string)releaseJson.SelectToken("artist-credit").First.SelectToken("name");
                release.arid = (string)releaseJson.SelectToken("artist-credit").First.SelectToken("artist").SelectToken("id");
                release.avg_rating = -1;
                release.ratings = 0;
                JObject art = new JObject();
                ApiCaller.GetArt(id, a => {
                    art = (JObject)a;
                }).Wait();            
                try { release.image = (string)art.SelectToken("images").First.SelectToken("thumbnails").SelectToken("small"); }
                catch { release.image = "Not found"; }
            }
            ViewBag.logged = false;
            string placeholder = null;
            if(HttpContext.Session.GetInt32("id") != null)
            {
                placeholder = "0 to 10";
                ViewBag.logged = true;
                int? release_id = null;
                if(existingRelease != null) release_id = existingRelease.id;
                if(release_id != null)
                {
                    Rating thisRating = _context.Ratings.Where(r => r.user_id == HttpContext.Session.GetInt32("id")).SingleOrDefault(r => r.release_id == release_id);
                    if(thisRating != null) placeholder = thisRating.rating.ToString();
                }   
            }
            if(existingRelease != null)
            {
                release.avg_rating = existingRelease.avg_rating;
                release.ratings = existingRelease.ratings;
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
                    string date = "";
                    string ais = "";
                    ApiCaller.GetReleaseData(id, a => {
                        title = (string)(((JObject)a).SelectToken("title"));
                        artist = (string)((JObject)a).SelectToken("artist-credit").First.SelectToken("artist").SelectToken("name");
                        date = (string)((JObject)a).SelectToken("first-release-date");
                        ais = (string)((JObject)a).SelectToken("artist-credit").First.SelectToken("artist").SelectToken("id");

                    }).Wait();
                    newRelease.title = title;
                    newRelease.artist = artist;
                    newRelease.date = date;
                    newRelease.artist_id_string = ais;
                    JObject art = new JObject();
                    ApiCaller.GetArt(id, a => {
                        art = (JObject)a;
                    }).Wait();
                    try { newRelease.image = (string)art.SelectToken("images").First.SelectToken("thumbnails").SelectToken("large"); }
                    catch { newRelease.image = "Not found"; }
                    _context.Add(newRelease);
                    _context.SaveChanges();
                    thisRelease = _context.Releases.SingleOrDefault(r => r.id_string == id);
                }
                int user_id = (int)HttpContext.Session.GetInt32("id");
                Rating thisRating = _context.Ratings.Where(r => r.user_id == user_id).SingleOrDefault(r => r.release_id == thisRelease.id);
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

        [Route("top")]
        public IActionResult TopRated()
        {
            List<Release> allReleases = _context.Releases.OrderByDescending(r => r.avg_rating).ToList();
            List<DisplayRelease> topRated = new List<DisplayRelease>();
            for(int i = 0; i < 10 && i < allReleases.Count(); i++)
            {
                DisplayRelease newDisplay = new DisplayRelease()
                {
                    id = allReleases[i].id_string,
                    title = allReleases[i].title,
                    artist = allReleases[i].artist,
                    arid = allReleases[i].artist_id_string,
                    avg_rating = allReleases[i].avg_rating,
                    ratings = allReleases[i].ratings,
                    image = allReleases[i].image
                };
                topRated.Add(newDisplay);
            }
            return View(topRated);
        }
    }
}
