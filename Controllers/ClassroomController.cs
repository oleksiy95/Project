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
    public class ClassroomController : Controller
    {
        private ScheduleEntities db = new ScheduleEntities();

        //
        // GET: /Classroom/

        public ActionResult Index()
        {
            return View(db.Classrooms.ToList());
        }

        //
        // GET: /Classroom/Details/5

        public ActionResult Details(int id = 0)
        {
            Classroom classroom = db.Classrooms.Find(id);
            if (classroom == null)
            {
                return HttpNotFound();
            }
            return View(classroom);
        }

        //
        // GET: /Classroom/Create

        public ActionResult Create()
        {
            return View();
        }


        private bool Checking(Classroom classroom)
        {
            bool roomAlready = true;
            foreach (var checkRoom in db.Classrooms.ToList())
            {
                if (classroom.Number == checkRoom.Number)
                {
                    roomAlready = false;
                    break;
                }
            }
            
            return roomAlready;
        }

        //
        // POST: /Classroom/Create

        [HttpPost]
        public ActionResult Create(Classroom classroom)
        {
            bool roomAlready = true;
            string errorMessage = String.Empty;
            if (ModelState.IsValid)
            {
                //check if such classroom isn't in DB
                roomAlready = Checking(classroom);      
               

                if (roomAlready)
                {
                    db.Classrooms.Add(classroom);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                
            }
            errorMessage = "Аудиторія " + classroom.Number + " є в базі даних";
            ModelState.AddModelError(string.Empty, errorMessage);
            return View(classroom);
        }

        //
        // GET: /Classroom/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Classroom classroom = db.Classrooms.Find(id);
            if (classroom == null)
            {
                return HttpNotFound();
            }
            return View(classroom);
        }

        //
        // POST: /Classroom/Edit/5

        [HttpPost]
        public ActionResult Edit(Classroom classroom)
        {
            bool roomAlready = true;
            string errorMessage = String.Empty;

            if (ModelState.IsValid)
            {
                //check if such classroom isn't in DB
                roomAlready = Checking(classroom);
                     
                if (roomAlready)
                {
                    Classroom r = db.Classrooms.Find(classroom.Room_ID);
                    ((IObjectContextAdapter)db).ObjectContext.Detach(r);
                    
                    db.Entry(classroom).State = EntityState.Modified;                    
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
              
            errorMessage = "Аудиторія " + classroom.Number + " є в базі даних";
            ModelState.AddModelError(string.Empty, errorMessage);
            return View(classroom);
        }

        //
        // GET: /Classroom/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Classroom classroom = db.Classrooms.Find(id);
            if (classroom == null)
            {
                return HttpNotFound();
            }
            return View(classroom);
        }

        //
        // POST: /Classroom/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Classroom classroom = db.Classrooms.Find(id);
            db.Classrooms.Remove(classroom);
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