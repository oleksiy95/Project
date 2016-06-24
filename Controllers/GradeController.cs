using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CourseProject.Models;

namespace CourseProject.Controllers
{
    public class GradeController : Controller
    {
        private ScheduleEntities db = new ScheduleEntities();

        //
        // GET: /Grade/

       

        public ActionResult Index(int? groupID, int? scheduleID)
        {
            var students = from s in db.Students
                           where s.Group_ID == groupID
                           select s;
            Grade gr;
            List<Grade> grList = new List<Grade>();

            bool checkGR = true;

            foreach (var st in students)
            {
                gr = new Grade();
                checkGR = true;
                gr.Student_ID = st.Student_ID;
                gr.Schedule_ID = Convert.ToInt32(scheduleID);
                gr.Grade_ID = 1;

                foreach (var check in db.Grades)
                {
                    if (gr.Student_ID == check.Student_ID && gr.Schedule_ID == check.Schedule_ID)
                    {
                        checkGR = false;
                    }
                }

                if (checkGR)
                {
                    grList.Add(gr);
                                        
                }
                

            }
            foreach (var item in grList)
            {
                db.Grades.Add(item);
                db.SaveChanges();
            }
            var grade = db.Grades.Include(g => g.Schedule).Include(g => g.Student);
            grade = from s in grade
                    where s.Schedule_ID == scheduleID
                    orderby s.Student.Surname
                    select s;
            

            
            return View(grade.ToList());
        }

        //
        // GET: /Grade/Details/5

        public ActionResult Details(int id = 0)
        {
            Grade grade = db.Grades.Find(id);
            if (grade == null)
            {
                return HttpNotFound();
            }
            return View(grade);
        }

        //
        // GET: /Grade/Create

        public ActionResult Create()
        {
            ViewBag.Schedule_ID = new SelectList(db.Schedules, "Schedule_ID", "EnrollmentYear");
            ViewBag.Student_ID = new SelectList(db.Students, "Student_ID", "Name");
            return View();
        }

        //
        // POST: /Grade/Create

        [HttpPost]
        public ActionResult Create(Grade grade)
        {
            if (ModelState.IsValid)
            {
                db.Grades.Add(grade);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Schedule_ID = new SelectList(db.Schedules, "Schedule_ID", "EnrollmentYear", grade.Schedule_ID);
            ViewBag.Student_ID = new SelectList(db.Students, "Student_ID", "Name", grade.Student_ID);
            return View(grade);
        }

        //
        // GET: /Grade/Edit/5

        public ActionResult _Correct(int id = 0)
        {
            Grade grade = db.Grades.Find(id);
            if (grade == null)
            {
                return HttpNotFound();
            }
            ViewBag.Schedule_ID = new SelectList(db.Schedules, "Schedule_ID", "EnrollmentYear", grade.Schedule_ID);
            ViewBag.Student_ID = new SelectList(db.Students, "Student_ID", "Name", grade.Student_ID);
            return PartialView(grade);
        }

        [HttpPost]
        public ActionResult CorrectRecordPost(Grade grade)
        {
            if (ModelState.IsValid)
            {
                db.Entry(grade).State = EntityState.Modified;
                db.SaveChanges();
                
            }

            var gr = db.Grades.Include(g => g.Schedule).Include(g => g.Student);
            gr = from s in gr
                     where s.Schedule_ID == grade.Schedule_ID
                 orderby s.Student.Surname
                     select s;

            return PartialView("_Grades",gr);
        }

        //
        // POST: /Grade/Edit/5

        [HttpPost]
        public ActionResult Edit(Grade grade)
        {
            if (ModelState.IsValid)
            {
                db.Entry(grade).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Schedule_ID = new SelectList(db.Schedules, "Schedule_ID", "EnrollmentYear", grade.Schedule_ID);
            ViewBag.Student_ID = new SelectList(db.Students, "Student_ID", "Name", grade.Student_ID);
            return View(grade);
        }

        //
        // GET: /Grade/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Grade grade = db.Grades.Find(id);
            if (grade == null)
            {
                return HttpNotFound();
            }
            return View(grade);
        }

        //
        // POST: /Grade/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Grade grade = db.Grades.Find(id);
            db.Grades.Remove(grade);
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