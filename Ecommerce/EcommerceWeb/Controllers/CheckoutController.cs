﻿using Microsoft.AspNetCore.Mvc;

namespace EcommerceWeb.Controllers
{
    public class CheckoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
