using Forum.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace Forum.Controllers
{
    public class CategoryController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.Categories =
                from category in db.Categories
                orderby category.CategoryName
                select category;

            return View();
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult New()
        {
            return View();
        }
        
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult New(Category cat)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    cat.Date = DateTime.Now;
                    db.Categories.Add(cat);
                    db.SaveChanges();
                    TempData["message"] = "Categoria a fost adaugata!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception e)
            {

            }

            return View(cat);
        }

        public ActionResult Show(int id)
        {
            Category category = db.Categories.Find(id);

            if (category == null)
            {
                return RedirectToAction("ErrorWithMessage", "Error", new { message = "Invalid category id" });
                //return RedirectToAction("Error", "Error");
            }

            ViewBag.Subjects = category.Subjects.ToList<Subject>();
            return View(category);
        }

        public ActionResult CreateSubject(int id)
        {
            Category cat = db.Categories.Find(id);
            return RedirectToAction("New", "Subject", new { categoryId = cat.CategoryId, categoryName = cat.CategoryName });   
        }


        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int id)
        {
            Category category = db.Categories.Find(id);
            return View(category);
        }



        [Authorize(Roles = "Administrator")]
        [HttpPut]
        public ActionResult Edit(int id, Category requestCategory)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Category category = db.Categories.Find(id);
                    if (TryUpdateModel(category))
                    {
                        category.CategoryName = requestCategory.CategoryName;
                        TempData["message"] = "Categoria a fost modificata!";
                        db.SaveChanges();
                    }
                    
                    return RedirectToAction("Index");
                }
            }
            catch (Exception e) { }

            return View(requestCategory);
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
            db.SaveChanges();

            TempData["message"] = "Categoria a fost stearsa!";
            return RedirectToAction("Index");
        }

    }
}