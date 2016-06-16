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
    public class TeacherController : Controller
    {
        private ScheduleEntities db = new ScheduleEntities();

        //
        // GET: /Teacher/

        public ActionResult Index()
        {
            return View(db.Teachers.ToList());
        }

        //
        // GET: /Teacher/Details/5

        public ActionResult Details(int id = 0)
        {
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            return View(teacher);
        }

        //
        // GET: /Teacher/Create

        public ActionResult Create()
        {
            return View();
        }

        private bool Checking(Teacher teacher)
        {
            bool already = true;
            foreach (var check in db.Teachers.ToList())
            {
                if (check.Name == teacher.Name && check.Surname == teacher.Surname && check.LastName == teacher.LastName)
                {
                    already = false;

                }
            }
            return already;
        }

        //
        // POST: /Teacher/Create

        [HttpPost]
        public ActionResult Create(Teacher teacher)
        {
            bool already = false;
            string error = string.Empty;

            if (ModelState.IsValid)
            {
                already = Checking(teacher);

                if (already)
                {
                    db.Teachers.Add(teacher);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            error = "Викладач " + teacher.Surname + " " + teacher.Name + " " + teacher.LastName + " є в базі даних";
            ModelState.AddModelError(string.Empty, error);
            return View(teacher);
        }

        //
        // GET: /Teacher/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            return View(teacher);
        }

        //
        // POST: /Teacher/Edit/5

        [HttpPost]
        public ActionResult Edit(Teacher teacher)
        {
            bool already = false;
            string error = string.Empty;

            if (ModelState.IsValid)
            {
                already = Checking(teacher);

                if (already)
                {
                    Teacher r = db.Teachers.Find(teacher.Teacher_ID);
                    ((IObjectContextAdapter)db).ObjectContext.Detach(r);

                    db.Entry(teacher).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            error = "Викладач " + teacher.Surname + " " + teacher.Name + " " + teacher.LastName + " є в базі даних";
            ModelState.AddModelError(string.Empty, error);
            return View(teacher);
        }

        //
        // GET: /Teacher/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            return View(teacher);
        }

        //
        // POST: /Teacher/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Teacher teacher = db.Teachers.Find(id);
            db.Teachers.Remove(teacher);
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