using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CourseProject.Models;
using System.IO;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Data.SqlClient;

namespace CourseProject.Controllers
{
    public class ScheduleController : Controller
    {
        private ScheduleEntities db = new ScheduleEntities();

        //
        // GET: /Schedule/

        public ActionResult Index()
        {
            var schedules = db.Schedules.Include(s => s.Classroom).Include(s => s.Group).Include(s => s.LessonType).Include(s => s.Subject).Include(s => s.Teacher);
            return View(schedules.ToList());
        }

        //
        // GET: /Schedule/Details/5

        public ActionResult Details(int id = 0)
        {
            Schedule schedule = db.Schedules.Find(id);
            if (schedule == null)
            {
                return HttpNotFound();
            }
            return View(schedule);
        }

        //
        // GET: /Schedule/Create

        public ActionResult Create()
        {
            ViewBag.Room_ID = new SelectList(db.Classrooms, "Room_ID", "Number");
            ViewBag.Group_ID = new SelectList(db.Groups, "Group_ID", "Name");
            ViewBag.Lesson_ID = new SelectList(db.LessonTypes, "Lesson_ID", "Type");
            ViewBag.Subject_ID = new SelectList(db.Subjects, "Subject_ID", "Name");
            ViewBag.Teacher_ID = new SelectList((from s in db.Teachers
                                                 select new { Teacher_ID = s.Teacher_ID,
FullName = s.Name + " " + s.Surname + " " + s.LastName}), "Teacher_ID", "FullName");
            
            
            return View();

             
        }

        //
        // POST: /Schedule/Create

        [HttpPost]
        public ActionResult Create(Schedule schedule, int[] Lesson_ID, int[] Room_ID)
        {
            if (ModelState.IsValid)
            {
                if (schedule.DateList != null)
                {                   

                    for (int i = 0; i < schedule.DateList.Count; i++)
                    {
                        if (Lesson_ID.Length > 1 && Room_ID.Length > 1)
                        {
                            schedule.Lesson_ID = Lesson_ID[i];
                            schedule.Room_ID = Room_ID[i];
                        }                        
                        schedule.Date = schedule.DateList[i];
                        db.Schedules.Add(schedule);
                        db.SaveChanges();
                    }
                }

                //db.Schedules.Add(schedule);
                //db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Room_ID = new SelectList(db.Classrooms, "Room_ID", "Number", schedule.Room_ID);
            ViewBag.Group_ID = new SelectList(db.Groups, "Group_ID", "Name", schedule.Group_ID);
            ViewBag.Lesson_ID = new SelectList(db.LessonTypes, "Lesson_ID", "Type", schedule.Lesson_ID);
            ViewBag.Subject_ID = new SelectList(db.Subjects, "Subject_ID", "Name", schedule.Subject_ID);
            ViewBag.Teacher_ID = new SelectList((from s in db.Teachers
                                                 select new { Teacher_ID = s.Teacher_ID,
FullName = s.Name + " " + s.Surname + " " + s.LastName}), "Teacher_ID", "FullName", schedule.Teacher_ID);

            
            return View(schedule);
            
        }

       
        //
        // GET: /Schedule/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Schedule schedule = db.Schedules.Find(id);
            if (schedule == null)
            {
                return HttpNotFound();
            }
            ViewBag.Room_ID = new SelectList(db.Classrooms, "Room_ID", "Number", schedule.Room_ID);
            ViewBag.Group_ID = new SelectList(db.Groups, "Group_ID", "Name", schedule.Group_ID);
            ViewBag.Lesson_ID = new SelectList(db.LessonTypes, "Lesson_ID", "Type", schedule.Lesson_ID);
            ViewBag.Subject_ID = new SelectList(db.Subjects, "Subject_ID", "Name", schedule.Subject_ID);
            ViewBag.Teacher_ID = new SelectList((from s in db.Teachers
                                                 select new { Teacher_ID = s.Teacher_ID,
FullName = s.Name + " " + s.Surname + " " + s.LastName}), "Teacher_ID", "FullName", schedule.Teacher_ID);
            return View(schedule);
        }

        //
        // POST: /Schedule/Edit/5

        [HttpPost]
        public ActionResult Edit(Schedule schedule)
        {
            if (ModelState.IsValid)
            {
                db.Entry(schedule).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Room_ID = new SelectList(db.Classrooms, "Room_ID", "Number", schedule.Room_ID);
            ViewBag.Group_ID = new SelectList(db.Groups, "Group_ID", "Name", schedule.Group_ID);
            ViewBag.Lesson_ID = new SelectList(db.LessonTypes, "Lesson_ID", "Type", schedule.Lesson_ID);
            ViewBag.Subject_ID = new SelectList(db.Subjects, "Subject_ID", "Name", schedule.Subject_ID);
            ViewBag.Teacher_ID = new SelectList((from s in db.Teachers
                                                 select new { Teacher_ID = s.Teacher_ID,
FullName = s.Name + " " + s.Surname + " " + s.LastName}), "Teacher_ID", "FullName", schedule.Teacher_ID);
            return View(schedule);
        }
        
        //
        // GET: /Schedule/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Schedule schedule = db.Schedules.Find(id);
            if (schedule == null)
            {
                return HttpNotFound();
            }
            return View(schedule);
        }

        //
        // POST: /Schedule/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Schedule schedule = db.Schedules.Find(id);
            db.Schedules.Remove(schedule);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult ScheduleData()
        {
            var schedules = db.Schedules.Include(s => s.Classroom).Include(s => s.Group).Include(s => s.LessonType).Include(s => s.Subject).Include(s => s.Teacher);
            return PartialView(schedules.ToList());
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult YourAction(HttpPostedFileBase file)
        //{
        //    //if (files != null)
        //    //{
        //      //  foreach (var file in files)
        //        //{
        //            // Verify that the user selected a file
        //            if (file != null && file.ContentLength > 0)
        //            {
        //                // extract only the fielname
        //                var fileName = Path.GetFileName(file.FileName);
        //                // TODO: need to define destination
        //                var path = Path.Combine(Server.MapPath("~/Content/csv"), fileName);
        //                file.SaveAs(path);
        //            }
        //            return View();
        //        //}
        //    }
        
               
        //[HttpPost]
        //public ActionResult Upload()
        //{
        //    for (int i = 0; i < Request.Files.Count; i++)
        //    {
        //        HttpPostedFileBase file = Request.Files[i]; //Uploaded file
        //        //Use the following properties to get file's name, size and MIMEType
        //        int fileSize = file.ContentLength;
        //        string fileName = file.FileName;
        //        string mimeType = file.ContentType;
        //        System.IO.Stream fileContent = file.InputStream;
        //        //To save file, use SaveAs method
        //        file.SaveAs(Server.MapPath("~/Content/csv/")+fileName); //File will be saved in application root
        //    }

        //    var schedules = db.Schedules.Include(s => s.Classroom).Include(s => s.Group).Include(s => s.LessonType).Include(s => s.Subject).Include(s => s.Teacher);
        //    return PartialView(schedules.ToList());
        //    //return Json("Uploaded " + Request.Files.Count + " files");
        //}

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase Files)
        {
            DataTable dt = new DataTable();
            ViewBag.Complete = false;
            //foreach (var file in schedule.Files)
            //{

            if (Files != null || Files.ContentLength > 0)
            {
                
                    if (Files.FileName.EndsWith(".csv"))
                    {
                        var fileName = Path.GetFileName(Files.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/csv"), fileName);
                        try
                        {
                            Files.SaveAs(path);
                            dt = ProcessCSV(path);

                            ViewBag.Feedback = ProcessBulkCopy(dt);
                            
                        }
                        catch (Exception ex)
                        {
                            ViewBag.Feedback = ex.Message;

                        }
                    }

                    else
                    {
                        ViewBag.Feedback = "File format is not correct. It must be .csv";
                    }

                
            }
            else { ViewBag.Feedback = "Please select a file"; }
            dt.Dispose();
            //}                       
            var schedules = db.Schedules.Include(s => s.Classroom).Include(s => s.Group).Include(s => s.LessonType).Include(s => s.Subject).Include(s => s.Teacher);
            return View(schedules.ToList());
            //return RedirectToAction("Index");
        }

        private DataTable ProcessCSV(string fileName)
        {
            //Set up our variables
            string Feedback = string.Empty;
            string line = string.Empty;
            string[] strArray;
            DataTable dt = new DataTable();
            
            DataRow row;
            
            object[] data_id = new object[8];
            
            // work out where we should split on comma, but not in a sentence
            Regex r = new Regex(";(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            //Set the filename in to our stream
            StreamReader sr = new StreamReader(fileName);

            //Read the first line and split the string at , with our regular expression in to an array
            line = sr.ReadLine();
            //Column for ID
            line = "id;" + line;

            strArray = r.Split(line);
            
            //For each item in the new split array, dynamically builds our Data columns. Save us having to worry about it.
            Array.ForEach(strArray, s => dt.Columns.Add(new DataColumn()));

            //Read each line in the CVS file until it’s empty
            while ((line = sr.ReadLine()) != null)
            {
                //ID values
                line = " ;" + line;
                row = dt.NewRow();
                

                //add our current value to our data row
                row.ItemArray = r.Split(line);
                
                //
                foreach (var name in db.Subjects.ToList())
                {
                    if (row.ItemArray[1].ToString() == name.Name)
                    {
                        data_id[1] = name.Subject_ID;
                        break;
                    }
                }

                foreach (var name in db.Classrooms.ToList())
                {
                    if (row.ItemArray[2].ToString() == name.Number)
                    {
                        data_id[2] = name.Room_ID;
                        break;
                    }
                }

                foreach (var name in db.LessonTypes.ToList())
                {
                    if (row.ItemArray[3].ToString() == name.Type)
                    {
                        data_id[3] = name.Lesson_ID;
                        break;
                    }
                }

                foreach (var name in db.Groups.ToList())
                {
                    if (row.ItemArray[5].ToString() == name.Name)
                    {
                        data_id[5] = name.Group_ID;
                        break;
                    }
                }

                foreach (var name in db.Teachers.ToList())
                {
                    string teacherName = name.Name + " " + name.Surname + " " + name.LastName;
                    if (row.ItemArray[6].ToString() == teacherName)
                    {
                        data_id[6] = name.Teacher_ID;
                        break;
                    }
                }
                data_id[4] = row.ItemArray[4];
                data_id[7] = row.ItemArray[7];
                row.ItemArray = data_id;
                dt.Rows.Add(row);
            }
            
            
            //Tidy Streameader up
            sr.Dispose();

            //return a the new DataTable
            return dt;

        }

        private static String ProcessBulkCopy(DataTable dt)
        {
            string Feedback = string.Empty;
            string connString = ConfigurationManager.ConnectionStrings["Schedule"].ConnectionString;

            //make our connection and dispose at the end
            using (SqlConnection conn = new SqlConnection(connString))
            {
                //make our command and dispose at the end
                using (var copy = new SqlBulkCopy(conn))
                {

                    //Open our connection
                    conn.Open();

                    ///Set target table and tell the number of rows
                    copy.DestinationTableName = "Schedule";
                    copy.BatchSize = dt.Rows.Count;
                    try
                    {
                        //Send it to the server
                        copy.WriteToServer(dt);
                        Feedback = "Upload complete";
                    }
                    catch (Exception ex)
                    {
                        Feedback = ex.Message;
                    }
                }
            }

            return Feedback;
        }
       
    }


}