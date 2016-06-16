﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CourseProject.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Сторінка описання програми.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Сторінка контактів.";

            return View();
        }
    }
}
