using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebSite.Core.Helpers
{
    public static class ModelStateExtensions
    {
        public static IEnumerable ExtractErrorMessages(this ModelStateDictionary modelState)
        {
            return modelState.Values.SelectMany(m => m.Errors).Select(e => e.ErrorMessage).ToList();
        }
    }
}
