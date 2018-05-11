using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AspnetCore.OAuth2.Github.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AspnetCore.OAuth2.Github.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            ViewData["github:login"] = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name).Value;
            ViewData["github:repo:count"] = User.Claims.FirstOrDefault(x => x.Type == "urn:github:public_repos:count").Value;
            ViewData["github:repos_url"] = User.Claims.FirstOrDefault(x => x.Type == "urn:github:repos_url").Value;
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
