//Author﻿: Akash Jain (akashjain1205@gmail.com, jain2ar@mail.uc.edu, github: akash1205)
//        Nikhil Gupta (nikhil.damoh@gmail.com, guptan6@mail.uc.edu, github: guptanikhil88)
//      This controller handles the landing of user on the application.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication3.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}