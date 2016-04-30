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
using System.Text;

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
            ViewBag.Group_ID = new SelectList((from s in db.Groups
                                                  select new {Group_ID = s.Group_ID,
                                                      FullGroup = s.Name + " " + s.EnrollmentYear}), "Group_ID", "FullGroup");
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
            ViewBag.Group_ID = new SelectList((from s in db.Groups
                                               select new
                                               {
                                                   Group_ID = s.Group_ID,
                                                   FullGroup = s.Name + " " + s.EnrollmentYear
                                               }), "Group_ID", "FullGroup", schedule.Group_ID);
            ViewBag.Lesson_ID = new SelectList(db.LessonTypes, "Lesson_ID", "Type", schedule.Lesson_ID);
            ViewBag.Subject_ID = new SelectList(db.Subjects, "Subject_ID", "Name", schedule.Subject_ID);
            ViewBag.Teacher_ID = new SelectList((from s in db.Teachers
                                                 select new { Teacher_ID = s.Teacher_ID,
FullName = s.Name + " " + s.Surname + " " + s.LastName}), "Teacher_ID", "FullName", schedule.Teacher_ID);

            
            return View(schedule);
            
        }


        //
        // GET: /Schedule/Copy

        public ActionResult Copy()
        {

            return AddFuctionForCopy();
        }
       
        //
        // POST: /Schedule/Copy

        [HttpPost]
        public ActionResult Copy(Schedule schedulePar, int? Teacher_ID, int? Group_ID)
        {
            if (ModelState.IsValid)
            {
                if (Teacher_ID != null && Group_ID != null)
                {
                    //search records in the database with conditios
                    var schedule = from s in db.Schedules
                                   where s.Teacher_ID == Teacher_ID && s.Group_ID == Group_ID
                                   select s;

                    bool notHaveRecords = true;
                 
                    foreach (var one in schedule)
                    {
                        //check if schedule have such records
                        notHaveRecords = false;

                        //change EnrollmentYear for the next year
                        one.EnrollmentYear = (Convert.ToInt32(one.EnrollmentYear) + 1).ToString();

                        //change date for the next year to the same day of week(intercalary year)
                        if ((one.Date.Value.Year + 1) % 4 == 0)
                        {
                            one.Date = one.Date.Value.AddYears(1);
                            one.Date = one.Date.Value.AddDays(-2.0);

                        }
                        
                        //change date for the next year to the same day of week
                        else
                        {
                            one.Date = one.Date.Value.AddYears(1);
                            one.Date = one.Date.Value.AddDays(-1.0);
                        }
                        //EnrollmentYear, which new Group must have
                        int enrYear = Convert.ToInt32(one.Group.EnrollmentYear) + 1;
                       
                        //ID of old Group
                        int gr_id = one.Group_ID;
                        
                        //Search Group with the same Name but new EnrollmentYear
                        foreach (var year in db.Groups)
                        {
                            if (enrYear == Convert.ToInt32(year.EnrollmentYear) && one.Group.Name == year.Name)
                                one.Group_ID = year.Group_ID;
                        }
                        
                        //if ID changed - we have founded Group with the same Name and new EnrollmentYear
                        if (one.Group_ID != gr_id)
                        {
                            db.Schedules.Add(one);
                        }
                        //if ID didn't change - show message.
                        else
                        {
                            ViewBag.NotFounded = "Group " + one.Group.Name + " with EnrollmentYear " + enrYear + " is not founded. Create it to copy.";
                            return AddFuctionForCopy();
                        }
                        
                    }
                    //if we don't find records to copy - show message
                    if (notHaveRecords)
                    {
                        ViewBag.NotFounded = "Schedule doesn't have such records";
                        return AddFuctionForCopy();
                    }
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }

            return AddFuctionForCopy();
        }
        
        //return method
        public ActionResult AddFuctionForCopy()
        {

            ViewBag.Group_ID = new SelectList((from s in db.Groups
                                               select new
                                               {
                                                   Group_ID = s.Group_ID,
                                                   FullGroup = s.Name + " " + s.EnrollmentYear
                                               }), "Group_ID", "FullGroup");

            ViewBag.Teacher_ID = new SelectList((from s in db.Teachers
                                                 select new
                                                 {
                                                     Teacher_ID = s.Teacher_ID,
                                                     FullName = s.Name + " " + s.Surname + " " + s.LastName
                                                 }), "Teacher_ID", "FullName");


            return View("Copy");
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
            ViewBag.Group_ID = new SelectList((from s in db.Groups
                                               select new
                                               {
                                                   Group_ID = s.Group_ID,
                                                   FullGroup = s.Name + " " + s.EnrollmentYear
                                               }), "Group_ID", "FullGroup", schedule.Group_ID);
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
            ViewBag.Group_ID = new SelectList((from s in db.Groups
                                               select new
                                               {
                                                   Group_ID = s.Group_ID,
                                                   FullGroup = s.Name + " " + s.EnrollmentYear
                                               }), "Group_ID", "FullGroup", schedule.Group_ID);
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

        public ActionResult DownloadFile()
        {
            string filename = Server.MapPath("/Content/csv/ErrorsLine.csv");
            string contentType = "text/csv";
            string downloadName = "CSV File.csv";

            return File(filename, contentType, downloadName);
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase Files)
        {
            DataTable dt = new DataTable();
            DataTable dtErrors = new DataTable();
            
            ViewBag.Complete = false;
            

            if (Files != null || Files.ContentLength > 0)
            {
                
                    if (Files.FileName.EndsWith(".csv"))
                    {
                        var fileName = Path.GetFileName(Files.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/csv"), fileName);
                        try
                        {
                            Files.SaveAs(path);
                            dt = ProcessCSV(path)[0];
                            dtErrors = ProcessCSV(path)[1];

                            CreateCSVwithErrors(dtErrors);

                            ViewBag.Feedback = ProcessBulkCopy(dt);
                            ViewBag.ErrorMassage = ErrorMassageForBulkCopy;
                            if (ErrorMassageForBulkCopy.Length > 0)
                            {
                                ViewBag.ErrorsInCSV = "true";
                            }
                            
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

        private void CreateCSVwithErrors(DataTable dtErrors)
        {
            //if file exists - delete it
            if (System.IO.File.Exists(Path.Combine(Server.MapPath("~/Content/csv"), "ErrorsLine.csv")))
            {
                System.IO.File.Delete(Path.Combine(Server.MapPath("~/Content/csv"), "ErrorsLine.csv"));
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("subject;class;lesson;date;group;teacher;enrollment");//first line for csv file
            foreach (DataRow line in dtErrors.Rows)//add records with errors in csv file
            {
                sb.AppendLine(string.Format("{0};{1};{2};{3};{4};{5};{6}", line.ItemArray[1], line.ItemArray[2], line.ItemArray[3], line.ItemArray[4], line.ItemArray[5], line.ItemArray[6], line.ItemArray[7]));
            }

            System.IO.File.AppendAllText(Path.Combine(Server.MapPath("~/Content/csv"), "ErrorsLine.csv"), sb.ToString());//save csv file
        }
        
        private string ErrorMassageForBulkCopy;
        
        private List<DataTable> ProcessCSV(string fileName)
        {
            //Set up our variables
            string Feedback = string.Empty;
            string ErrorMassage = string.Empty;
            string line = string.Empty;
            string[] strArray;
            DataTable dt = new DataTable();
            DataTable dtErrors = new DataTable();
            List<DataTable> dtList = new List<DataTable>();
            bool subjects, classrooms, lessontypes, groups, teachers;
            int lineNumber = 1;
            int errorNumber = 0;

            DataRow row;
            DataRow rowErrors;
            
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
            Array.ForEach(strArray, s => dtErrors.Columns.Add(new DataColumn()));

            

            //Read each line in the CVS file until it’s empty
            while ((line = sr.ReadLine()) != null)
            {
                subjects = false;
                classrooms = false;
                lessontypes = false;
                groups = false;
                teachers = false;
                
                //ID values
                line = " ;" + line;
                row = dt.NewRow();
                rowErrors = dtErrors.NewRow();
                

                //add our current value to our data row
                row.ItemArray = r.Split(line);
                rowErrors.ItemArray = r.Split(line);
                
                //search coincidence by name and replace it for IDs values and checking if it is founded
                foreach (var name in db.Subjects.ToList())
                {
                    if (row.ItemArray[1].ToString() == name.Name)
                    {
                        data_id[1] = name.Subject_ID;
                        subjects = true;
                        break;
                    }
                }

                foreach (var name in db.Classrooms.ToList())
                {
                    if (row.ItemArray[2].ToString() == name.Number)
                    {
                        data_id[2] = name.Room_ID;
                        classrooms = true;
                        break;
                    }
                }

                foreach (var name in db.LessonTypes.ToList())
                {
                    if (row.ItemArray[3].ToString() == name.Type)
                    {
                        data_id[3] = name.Lesson_ID;
                        lessontypes = true;
                        break;
                    }
                }

                foreach (var name in db.Groups.ToList())
                {
                    if (row.ItemArray[5].ToString() == name.Name)
                    {
                        data_id[5] = name.Group_ID;
                        groups = true;
                        break;
                    }
                }

                foreach (var name in db.Teachers.ToList())
                {
                    string teacherName = name.Name + " " + name.Surname + " " + name.LastName;
                    if (row.ItemArray[6].ToString() == teacherName)
                    {
                        data_id[6] = name.Teacher_ID;
                        teachers = true;
                        break;
                    }
                }
                //Date and Enrollment year don't have the IDs
                data_id[4] = row.ItemArray[4];
                data_id[7] = row.ItemArray[7];

                lineNumber++;
                //if all names are founded, add row in dt, else add row in dtErrors
                if (teachers && groups && lessontypes && classrooms && subjects)
                {
                    row.ItemArray = data_id;
                    dt.Rows.Add(row);
                }
                else 
                {
                    errorNumber++;
                    //row.ItemArray = data_id;
                    dtErrors.Rows.Add(rowErrors);

                    //record only first 5 errors
                    if (errorNumber < 6)
                    {
                        if (!teachers) Feedback = "Error in line " + lineNumber + ": teacher " + row.ItemArray[6].ToString() + " isn't founded;<br/>";
                        if (!groups) Feedback = "Error in line " + lineNumber + ": group " + row.ItemArray[5].ToString() + " isn't founded;<br/>";
                        if (!lessontypes) Feedback = "Error in line " + lineNumber + ": lesson's type " + row.ItemArray[3].ToString() + " isn't founded;<br/>";
                        if (!classrooms) Feedback = "Error in line " + lineNumber + ": room " + row.ItemArray[2].ToString() + " isn't founded;<br/>";
                        if (!subjects) Feedback = "Error in line " + lineNumber + ": subject " + row.ItemArray[1].ToString() + " isn't founded;<br/>";
                        ErrorMassage = ErrorMassage + Feedback;
                    }
                }
            }
            
            //if number of errors are less than 5
            if (errorNumber < 5)
            {
                if (errorNumber == 0) ErrorMassage = string.Empty;
                ErrorMassage = "Show " + errorNumber + " errors:<br/>" + ErrorMassage;
            }
            else//if number of errors are 5 and more
            {
                ErrorMassage = "Show 5 of " + errorNumber + " errors:<br/>" + ErrorMassage;
            } 
            ErrorMassageForBulkCopy = ErrorMassage;//record error's message to global field
                                  
            //Tidy Streameader up
            sr.Dispose();
            dtList.Add(dt);
            dtList.Add(dtErrors);
            //return a the new DataTable
            return dtList;

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