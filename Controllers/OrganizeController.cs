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
            ViewBag.Data = data;
            return View();
        }

        [HttpPost]

        public IActionResult NewCategoryForm(string categoryName, TransactionType type) 
        {
            var data = _xmlService.Load();
            ViewBag.Data = data;
            var newCategory = new Category
            {
                Id = data.Categories.Count > 0 ? data.Categories.Max(c => c.Id) + 1 : 1,
                Name = categoryName,
                Type = type
            };
            if (data.Categories.Any(c => c.Name == newCategory.Name && c.Type == newCategory.Type || c.Id == newCategory.Id))
            {
                ViewBag.Danger = "Category with this name and type already exists or ID is not unique.";
                return View("NewCategory", data);
            } else
            {
                data.Categories.Add(newCategory);
                _xmlService.Save(data);
                ViewBag.Success = "Category added successfully!";
                return View("NewCategory");
            }
                
        }

    }
}
