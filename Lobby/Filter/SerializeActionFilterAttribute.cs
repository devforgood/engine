using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
                using (var sr = new StreamReader(context.HttpContext.Request.Body))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    var serializer = new JsonSerializer();
                    var msg =  serializer.Deserialize<core.RequestMessage>(jsonTextReader);
                    context.RouteData.Values["request"] = msg;

                    if(string.IsNullOrEmpty(msg.session_id)==false)
                    {
                        context.RouteData.Values["session"] = Session.GetSession(msg.session_id);
                    }
                }

                // perform some business logic work
                //byte[] buff = new byte[5];
                //var i = context.HttpContext.Request.Body.Read(buff, 0, 4);
                //var str = Encoding.UTF8.GetString(buff);

                //context.RouteData.Values["str"] = str;

                //context.HttpContext.Response.Headers.Add("Content-Type", new string[] { "text/html" });

            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
                using (StreamWriter writer = new StreamWriter(context.HttpContext.Response.Body))
                using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
                {
                    JsonSerializer ser = new JsonSerializer();
                    ser.Serialize(jsonWriter, context.RouteData.Values["response"]);
                    jsonWriter.Flush();
                }

                // perform some business logic work
                //string str = "good2";
                //byte[] buff = Encoding.UTF8.GetBytes(str);
                //context.HttpContext.Response.Body.Write(buff);
            }
        }
    }
}
