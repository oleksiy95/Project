using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CourseProject.Models;
using System.Data.Entity.Infrastructure;

namespace CourseProject.Controllers
{
    public class GroupController : Controller
    {
        private ScheduleEntities db = new ScheduleEntities();

        //
        // GET: /Group/

        public ActionResult Index()
        {
            return View(db.Groups.ToList());
        }

        //
        // GET: /Group/Details/5

        public ActionResult Details(int id = 0)
        {
            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        //
        // GET: /Group/Create

        public ActionResult Create()
        {
            return View();
        }

        private bool Checking(Group group)
        {
            bool already = true;
            foreach (var checkGroup in db.Groups.ToList())
            {
                if (group.Name == checkGroup.Name && group.EnrollmentYear == checkGroup.EnrollmentYear)
                {
                    already = false;
                    
                }
            }
            return already;
        }

        //
        // POST: /Group/Create

        [HttpPost]
        public ActionResult Create(Group group)
        {
            bool groupAlready = true;
            string errorMessage = String.Empty;


            if (ModelState.IsValid)
            {
                //check if such group isn't in DB
                groupAlready = Checking(group);
                
                if (groupAlready)
                {
                    db.Groups.Add(group);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            errorMessage = "Group " + group.Name + " " + group.EnrollmentYear + " is in data base";
            ModelState.AddModelError(string.Empty, errorMessage);
            return View(group);
        }

        //
        // GET: /Group/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        //
        // POST: /Group/Edit/5

        [HttpPost]
        public ActionResult Edit(Group group)
        {
            bool groupAlready = true;
            string errorMessage = String.Empty;

            if (ModelState.IsValid)
            {

                groupAlready = Checking(group);
                if (groupAlready)
                {
                    Group r = db.Groups.Find(group.Group_ID);
                    ((IObjectContextAdapter)db).ObjectContext.Detach(r);

                    db.Entry(group).State = EntityState.Modified;
                    
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            errorMessage = "Group " + group.Name + " " + group.EnrollmentYear + " is in data base";
            ModelState.AddModelError(string.Empty, errorMessage);
            return View(group);
        }

        //
        // GET: /Group/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        //
        // POST: /Group/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Group group = db.Groups.Find(id);
            db.Groups.Remove(group);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}