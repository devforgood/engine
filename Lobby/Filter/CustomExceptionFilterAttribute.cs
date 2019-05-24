using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lobby.Filter
{
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {

        public CustomExceptionFilterAttribute()
        {
        }

        public override void OnException(ExceptionContext context)
        {


        }
    }
}
