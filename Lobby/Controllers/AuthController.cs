using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Lobby.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            var addr = string.Format("{0}:{1},{2}:{3}", HttpContext.Connection.RemoteIpAddress, HttpContext.Connection.RemotePort, HttpContext.Connection.LocalIpAddress, HttpContext.Connection.LocalPort);
            var session = Session.CreateSession(addr);

            return new JsonResult(session);
        }
    }
}