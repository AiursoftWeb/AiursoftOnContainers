﻿using Aiursoft.Pylon.Exceptions;
using Aiursoft.Pylon.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aiursoft.Pylon.Attributes
{
    public class APIModelStateChecker : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller as Controller;
            if (!controller.ModelState.IsValid)
            {
                context.Result = ResultGenerator.GetInvalidModelStateErrorResponse(controller.ModelState);
            }
            base.OnActionExecuting(context);
        }
    }
    public static class ResultGenerator
    {
        public static JsonResult GetInvalidModelStateErrorResponse(ModelStateDictionary modelstate)
        {
            var list = new List<string>();
            foreach (var value in modelstate)
            {
                foreach (var error in value.Value.Errors)
                {
                    list.Add(error.ErrorMessage);
                }
            }
            var arg = new AiurCollection<string>(list)
            {
                code = ErrorType.InvalidInput,
                message = "Your input contains several errors!"
            };
            return new JsonResult(arg);
        }
    }
}
