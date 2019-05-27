using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lobby.Filter
{
    public class SerializeActionFilterAttribute : TypeFilterAttribute
    {
        public SerializeActionFilterAttribute() : base(typeof(SerializeActionFilterImpl))
        {
        }

        private class SerializeActionFilterImpl : IActionFilter
        {
            public SerializeActionFilterImpl()
            {
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                // perform some business logic work
                byte[] buff = new byte[5];
                var i = context.HttpContext.Request.Body.Read(buff, 0, 4);
                var str = Encoding.UTF8.GetString(buff);

                context.HttpContext.Response.Headers.Add("Content-Type", new string[] { "text/html" });

            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
                // perform some business logic work
                string str = "good2";
                byte[] buff = Encoding.UTF8.GetBytes(str);
                context.HttpContext.Response.Body.Write(buff);
            }
        }
    }
}
