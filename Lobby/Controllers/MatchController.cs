using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lobby.Filter;
using Microsoft.AspNetCore.Mvc;

namespace Lobby.Controllers
{
    public class MatchController : Controller
    {
        [SerializeActionFilter]
        public IActionResult Index()
        {
            //throw new Exception("test");


            //return new ContentResult()
            //{
            //    Content = "Resource unavailable - header should not be set"
            //};

            return new EmptyResult();
        }
    }
}