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
    public class LessonTypeController : Controller
    {
        private ScheduleEntities db = new ScheduleEntities();

        //
        // GET: /LessonType/

        public ActionResult Index()
        {
            return View(db.LessonTypes.ToList());
        }

        //
        // GET: /LessonType/Details/5

        public ActionResult Details(int id = 0)
        {
            LessonType lessontype = db.LessonTypes.Find(id);
            if (lessontype == null)
            {
                return HttpNotFound();
            }
            return View(lessontype);
        }

        //
        // GET: /LessonType/Create

        public ActionResult Create()
        {
            return View();
        }

        private bool Checking(LessonType type)
        {
            bool already = true;
            foreach (var check in db.LessonTypes.ToList())
            {
                if (check.Type == type.Type)
                {
                    already = false;

                }
            }
            return already;
        }

        //
        // POST: /LessonType/Create

        [HttpPost]
        public ActionResult Create(LessonType lessontype)
        {
            bool already = true;
            string error = string.Empty;

            if (ModelState.IsValid)
            {
                already = Checking(lessontype);

                if (already)
                {
                    db.LessonTypes.Add(lessontype);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            error = "Тип заняття " + lessontype.Type + " є в базі даних";
            ModelState.AddModelError(string.Empty, error);
            return View(lessontype);
        }

        //
        // GET: /LessonType/Edit/5

        public ActionResult Edit(int id = 0)
        {
            LessonType lessontype = db.LessonTypes.Find(id);
            if (lessontype == null)
            {
                return HttpNotFound();
            }
            return View(lessontype);
        }

        //
        // POST: /LessonType/Edit/5

        [HttpPost]
        public ActionResult Edit(LessonType lessontype)
        {
            bool already = true;
            string error = string.Empty;

            if (ModelState.IsValid)
            {

                already = Checking(lessontype);

                if (already)
                {
                    LessonType r = db.LessonTypes.Find(lessontype.Lesson_ID);
                    ((IObjectContextAdapter)db).ObjectContext.Detach(r);

                    db.Entry(lessontype).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            error = "Тип заняття " + lessontype.Type + " є в базі даних";
            ModelState.AddModelError(string.Empty, error);
            return View(lessontype);
        }

        //
        // GET: /LessonType/Delete/5

        public ActionResult Delete(int id = 0)
        {
            LessonType lessontype = db.LessonTypes.Find(id);
            if (lessontype == null)
            {
                return HttpNotFound();
            }
            return View(lessontype);
        }

        //
        // POST: /LessonType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            LessonType lessontype = db.LessonTypes.Find(id);
            db.LessonTypes.Remove(lessontype);
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