using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lobby
{
    public static class ControllerExtensions
    {
        public static Session GetSession(this Controller controller)
        {
            return (Session)controller.RouteData.Values["session"];
        }

        public static core.RequestMessage GetRequest(this Controller controller)
        {
            return (core.RequestMessage)controller.RouteData.Values["request"];
        }
    }
}
