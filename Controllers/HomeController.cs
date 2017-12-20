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
    public class HomeController : Controller
    {
        private MusicContext _context;
 
        public HomeController(MusicContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public List<DisplayRating> RecentRatings()
        {
            int user_id = (int)HttpContext.Session.GetInt32("id");
            List<Rating> mostRecent = _context.Ratings.Where(r => r.user_id != user_id).ToList();
            List<DisplayRating> recentDisplay = new List<DisplayRating>();
            for(int i = mostRecent.Count - 1; i > 0 && i > mostRecent.Count - 6; i--)
            {
                Release thisRelease = _context.Releases.SingleOrDefault(r => r.id == mostRecent[i].release_id);
                User thisRater = _context.Users.SingleOrDefault(u => u.id == mostRecent[i].user_id);
                DisplayRating newDisplay = new DisplayRating()
                {
                    title = thisRelease.title,
                    release_id_string = thisRelease.id_string,
                    artist = thisRelease.artist,
                    rating = mostRecent[i].rating,
                    rater = thisRater.username
                };
                string imgString = "";
                ApiCaller.GetArt(newDisplay.release_id_string, a => {
                    imgString = (string)((JObject)a).SelectToken("images").First.SelectToken("thumbnails").SelectToken("small");
                }).Wait();
                newDisplay.image = imgString;
                recentDisplay.Add(newDisplay);
            }
            return recentDisplay;
        }

        [HttpPost]
        [Route("register")]
        public IActionResult Register(RegVal model)
        {
            if(ModelState.IsValid)
            {
                User existingUser = _context.Users.SingleOrDefault(u => u.username == model.username);
                if(existingUser != null)
                {
                    ModelState.AddModelError("username", "Username is already in use.");
                    return View("Index");
                }
                PasswordHasher<User> hasher = new PasswordHasher<User>();
                User newUser = new User()
                {
                    username = model.username,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };
                newUser.password = hasher.HashPassword(newUser, model.password);
                _context.Add(newUser);
                _context.SaveChanges();
                newUser = _context.Users.SingleOrDefault(u => u.username == model.username);
                HttpContext.Session.SetInt32("id", newUser.id);
                HttpContext.Session.SetString("name", newUser.username);
                return RedirectToAction("Main");
            }
            return View("Index");
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login(LogVal model)
        {
            if(ModelState.IsValid)
            {
                User existingUser = _context.Users.SingleOrDefault(u => u.username == model.logname);
                if(existingUser == null)
                {
                    ModelState.AddModelError("logname", "Username does not exist.");
                    return View("Index");
                }
                PasswordHasher<User> hasher = new PasswordHasher<User>();
                if(0 == hasher.VerifyHashedPassword(existingUser, existingUser.password, model.logpw))
                {
                    ModelState.AddModelError("logpw", "Incorrect password.");
                    return View("Index");
                }
                HttpContext.Session.SetInt32("id", existingUser.id);
                HttpContext.Session.SetString("name", existingUser.username);
                return RedirectToAction("Main");
            }
            return View("Index");
        }

        [HttpGet]
        [Route("search")]
        public IActionResult Search(string name, string type)
        {
            if(type == "artist") return RedirectToAction("SearchArtist", new {name = name});
            return RedirectToAction("SearchRelease", new {title = name});
        }

        [HttpGet]
        [Route("search/artist")]
        public IActionResult SearchArtist(string name)
        {
            JObject result = new JObject();
            ApiCaller.SearchArtistName(name, r => {
                result = (JObject)r;
            }).Wait();
            ViewBag.name = name;
            ViewBag.count = result.SelectToken("count");
            ViewBag.results = result.SelectToken("artists");
            return View();
        }

        [HttpGet]
        [Route("search/release")]
        public IActionResult SearchRelease(string title)
        {
            JObject result = new JObject();
            ApiCaller.SearchReleaseName(title, r => {
                result = (JObject)r;
            }).Wait();
            ViewBag.rTitle = title;
            ViewBag.count = result.SelectToken("count");
            ViewBag.results = result.SelectToken("release-groups");
            return View();
        }

        [Route("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [Route("main")]
        public IActionResult Main()
        {
            if(HttpContext.Session.GetInt32("id") == null) return RedirectToAction("Index");
            User loggedUser = _context.Users.SingleOrDefault(u => u.id == HttpContext.Session.GetInt32("id"));
            ViewBag.name = loggedUser.username;
            List<DisplayRating> recentRatings = RecentRatings();
            return View(recentRatings);
        }

        [Route("user/{username}/top")]
        public IActionResult TopX(string username, int num, string from = null)
        {
            User thisUser = _context.Users.SingleOrDefault(u => u.username == username);
            if(thisUser == null) return RedirectToAction("Main");
            ViewBag.name = username;
            ViewBag.num = num;
            ViewBag.period = from;
            List<DisplayRating> list = CreateList(num, thisUser.id, from);
            return View(list);
        }

        public List<DisplayRating> CreateList(int num, int user_id, string period)
        {
            List<DisplayRating> list = new List<DisplayRating>();
            List<Rating> allRatings = _context.Ratings.Where(r => r.user_id == user_id).OrderByDescending(r => r.rating).ToList();
            List<Release> releases = _context.Releases.ToList();
            if(period != null)
            {
                allRatings = allRatings.Where(r => {
                    Release ratedRelease = releases.SingleOrDefault(re => re.id == r.release_id);
                    if(ratedRelease.date.Substring(0, 3) == period.Substring(0, 3)) return true;
                    return false;
                }).ToList();
            }
            for(int i = 0; i < allRatings.Count && i < num; i++)
            {
                try
                {
                    Release thisRelease = releases.Single(r => r.id == allRatings[i].release_id);
                    DisplayRating newDisplay = new DisplayRating()
                    {
                        title = thisRelease.title,
                        release_id_string = thisRelease.id_string,
                        artist = thisRelease.artist,
                        rating = allRatings[i].rating,
                        release_date = thisRelease.date
                    };
                    try
                    {
                        JObject art = new JObject();
                        ApiCaller.GetArt(thisRelease.id_string, a => {
                            art = (JObject)a;
                        }).Wait();
                        newDisplay.image = (string)art.SelectToken("images").First.SelectToken("thumbnails").SelectToken("small");
                    } catch { System.Console.WriteLine("No image available."); }
                    list.Add(newDisplay);
                }
                catch {}
            }
            return list;
        }

        [Route("user/{username}")]
        public IActionResult Profile(string username)
        {
            if(_context.Users.SingleOrDefault(u => u.username == username) == null) return View("NoUser");
            ViewBag.name = username;
            return View();
        }
    }
}
