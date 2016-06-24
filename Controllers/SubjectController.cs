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
    public class SubjectController : Controller
    {
        private ScheduleEntities db = new ScheduleEntities();

        //
        // GET: /Subject/

        public ActionResult Index()
        {
            return View(db.Subjects.ToList());
        }

        //
        // GET: /Subject/Details/5

        public ActionResult Details(int id = 0)
        {
            Subject subject = db.Subjects.Find(id);
            if (subject == null)
            {
                return HttpNotFound();
            }
            return View(subject);
        }

        //
        // GET: /Subject/Create

        public ActionResult Create()
        {
            return View();
        }

        private bool Checking(Subject sub)
        {
            bool already = true;
            foreach (var check in db.Subjects.ToList())
            {
                if (check.Name == sub.Name)
                {
                    already = false;

                }
            }
            return already;
        }

        //
        // POST: /Subject/Create

        [HttpPost]
        public ActionResult Create(Subject subject)
        {
            bool already = true;
            string error = string.Empty;

            if (ModelState.IsValid)
            {

                already = Checking(subject);

                if (already)
                {
                    db.Subjects.Add(subject);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            error = "Предмет " + subject.Name + " є в базі даних";
            ModelState.AddModelError(string.Empty, error);
            return View(subject);
        }

        //
        // GET: /Subject/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Subject subject = db.Subjects.Find(id);
            if (subject == null)
            {
                return HttpNotFound();
            }
            return View(subject);
        }

        //
        // POST: /Subject/Edit/5

        [HttpPost]
        public ActionResult Edit(Subject subject)
        {
            bool already = true;
            string error = string.Empty;

            if (ModelState.IsValid)
            {

                 already = Checking(subject);

                 if (already)
                 {
                     Subject r = db.Subjects.Find(subject.Subject_ID);
                     ((IObjectContextAdapter)db).ObjectContext.Detach(r);

                     db.Entry(subject).State = EntityState.Modified;
                     db.SaveChanges();
                     return RedirectToAction("Index");
                 }
            }
            error = "Предмет " + subject.Name + " є в базі даних";
            ModelState.AddModelError(string.Empty, error);
            return View(subject);
        }

        //
        // GET: /Subject/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Subject subject = db.Subjects.Find(id);
            if (subject == null)
            {
                return HttpNotFound();
            }
            return View(subject);
        }

        //
        // POST: /Subject/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Subject subject = db.Subjects.Find(id);
            db.Subjects.Remove(subject);
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