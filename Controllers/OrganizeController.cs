using Microsoft.AspNetCore.Mvc;
using PFO_Web.Models;
using PFO_Web.Services;

namespace PFO_Web.Controllers
{
    public class OrganizeController : Controller
    {

        private readonly XmlService _xmlService = new();

        [HttpGet]
        public IActionResult NewCategory()
        {
            var data = _xmlService.Load();
            return View(data);
        }

        [HttpPost]

        public IActionResult NewCategoryForm(string name, TransactionType type) 
        { 
            
        }

    }
}
