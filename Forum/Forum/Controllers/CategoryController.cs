using Forum.App_Start;
using Forum.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace Forum.Controllers
{
    [MessageFilter]
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
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

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






        

        private static Dictionary<int, int> getSubjectVoteDictionary()
        {
            var map = new Dictionary<int, int>();
            foreach (var subjectVote in (new ApplicationDbContext()).SubjectUpvotes)
            {
                var key = subjectVote.SubjectId;
                var value = (map.ContainsKey(key)) ? map[key] : 0;
                map[key] = value + 1;
            }

            return map;
        }

        public static string CriteriaParamName { get { return "criteria"; } }
        public static string CriteriaDate { get { return "Date"; } }
        public static string CriteriaVotes { get { return "Votes"; } }
        public static string OrderParamName { get { return "order"; } }
        public static string OrderAsc { get { return "Asc"; } }
        public static string OrderDesc { get { return "Desc"; } }
        private static int SubjectsPerPage { get { return 3; } }

        public ActionResult Show(int id)
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            string criteria = Request.Params.Get(CriteriaParamName);
            if (criteria == null) { criteria = CriteriaDate; }
            string order = Request.Params.Get(OrderParamName);
            if (order == null) { order = OrderDesc; }

            Category category = db.Categories.Find(id);

            if (category == null)
            {
                return RedirectToAction("ErrorWithMessage", "Error", new { message = "Invalid category id" });
            }


            List<Subject> subjects;
            if (criteria == CriteriaDate && order == OrderAsc)
            {
                subjects = category.Subjects.OrderBy(s => s.Date).ToList();
            }
            else if (criteria == CriteriaDate && order == OrderDesc)
            {
                subjects = category.Subjects.OrderByDescending(s => s.Date).ToList();
            }
            else if (criteria == CriteriaVotes && order == OrderAsc)
            {
                var map = CategoryController.getSubjectVoteDictionary();
                subjects = category.Subjects.OrderBy(s => map.ContainsKey(s.SubjectId) ? map[s.SubjectId] : 0).ToList();
            }
            else if (criteria == CriteriaVotes && order == OrderDesc)
            {
                var map = CategoryController.getSubjectVoteDictionary();
                subjects = category.Subjects.OrderByDescending(s => map.ContainsKey(s.SubjectId) ? map[s.SubjectId] : 0).ToList();
            }
            else
            {
                return RedirectToAction("ErrorWithMessage", "Error", new { message = "Invalid sorting criteria or order!" });
            }


            // pagination
            int totalItems = subjects.Count();
            ViewBag.totalItems = totalItems;

            int lastPage = 1;
            if (totalItems > 0) { lastPage = (totalItems + SubjectsPerPage - 1) / SubjectsPerPage; }

            int currentPage = 1;
            try
            {
                var p = Request.Params.Get("page");
                if (p != null)
                {
                    currentPage = Convert.ToInt32(p);
                }
            }
            finally { }

            if (!(1 <= currentPage && currentPage <= lastPage))
            {
                return RedirectToAction("ErrorWithMessage", "Error", new { message = "Invalid page number!" });
            }

            int offset = (currentPage - 1) * SubjectsPerPage;
            subjects = subjects.Skip(offset).Take(SubjectsPerPage).ToList();

            ViewBag.subjects = subjects;
            ViewBag.lastPage = lastPage;
            ViewBag.currentPage = currentPage;


            // data for the sort dropdown
            { 
                var selectListCriteria = new List<SelectListItem>();
                foreach (var loopCriteria in (new string[] { CriteriaDate, CriteriaVotes }))
                {
                    selectListCriteria.Add(new SelectListItem
                    {
                        Value = loopCriteria,
                        Text = loopCriteria
                    });
                }
                ViewBag.criteriaList = selectListCriteria;
                ViewBag.criteriaValue = criteria;
                ViewBag.criteriaGetParam = CriteriaParamName;
            }

            // data for the sort dropdown
            {
                var selectListOrder = new List<SelectListItem>();
                foreach (var loopOrder in (new string[] { OrderAsc, OrderDesc }))
                {
                    selectListOrder.Add(new SelectListItem
                    {
                        Value = loopOrder,
                        Text = loopOrder + "ending"
                    });
                }
                ViewBag.orderList = selectListOrder;
                ViewBag.orderValue = order;
                ViewBag.orderGetParam = OrderParamName;
            }


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
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

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
            try
            {
                Category category = db.Categories.Find(id);

                // remove the subjects of this category
                var subjectController = DependencyResolver.Current.GetService<SubjectController>(); // get a valid controller
                subjectController.ControllerContext = new ControllerContext(this.Request.RequestContext, subjectController);
                foreach (var subject in category.Subjects.ToList())
                {
                    subjectController.DeleteSubject(db, subject.SubjectId);
                }

                db.Categories.Remove(category);
                db.SaveChanges();

                TempData["message"] = "Categoria a fost stearsa!";
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);

                return RedirectToAction(
                    "ErrorWithMessage",
                    "Error",
                    new { message = "Failed to remove the category!" }
                );
            }
        }

    }
}