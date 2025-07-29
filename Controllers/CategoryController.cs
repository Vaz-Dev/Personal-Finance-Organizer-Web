using Microsoft.AspNetCore.Mvc;
using PFO_Web.Models;
using PFO_Web.Services;

namespace PFO_Web.Controllers
{
    public class CategoryController : Controller
    {

        private readonly DataService _dataService = new();

        [HttpGet]
        public IActionResult NewCategory()
        {
            var data = _dataService.Load();
            ViewBag.Data = data;
            return View();
        }

        [HttpPost]

        public IActionResult NewCategoryForm(string categoryName, TransactionType type)
        {
            var data = _dataService.Load();
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
            }
            else
            {
                data.Categories.Add(newCategory);
                _dataService.Save(data);
                ViewBag.Success = "Category added successfully!";
                return View("NewCategory");
            }

        }

        [HttpGet]

        public IActionResult DeleteCategory()
        {
            var data = _dataService.Load();
            ViewBag.Data = data;
            return View();
        }

        [HttpPost]

        public IActionResult DeleteCategoryForm(int categoryToDeleteId, int categoryToReplaceId)
        {
            var data = _dataService.Load();
            ViewBag.Data = data;
            var categoryToDelete = data.Categories.FirstOrDefault(c => c.Id == categoryToDeleteId);
            var categoryToReplace = data.Categories.FirstOrDefault(c => c.Id == categoryToReplaceId);
            if (categoryToDelete == categoryToReplace)
            {
                ViewBag.Danger = "You cannot replace a category with itself.";
                return View("DeleteCategory");
            }
            else if (categoryToDelete.Type != categoryToReplace.Type)
            {
                ViewBag.Danger = "You cannot replace a category with other of another type (Income or Expense).";
                return View("DeleteCategory");
            }
            else if (categoryToDelete.Id == 1 || categoryToDelete.Id == 2)
            {
                ViewBag.Danger = "You cannot delete the default categories.";
                return View("DeleteCategory");
            }
            else if (categoryToDelete == null || categoryToReplace == null)
            {
                ViewBag.Danger = "Category to delete or replace not found.";
                return View("DeleteCategory");
            }
            else
            {
                foreach (var transaction in data.Transactions.Where(t => t.Category.Id == categoryToDelete.Id))
                {
                    transaction.Category = categoryToReplace;
                }
                data.Categories.Remove(categoryToDelete);
                _dataService.Save(data);
                ViewBag.Success = "Category deleted successfully!";
                return View("DeleteCategory");
            }
        }

        [HttpGet]

        public IActionResult ListCategories()
        {
            var data = _dataService.Load();
            ViewBag.Data = data;
            return View("ListCategories");

        }
    }
}
