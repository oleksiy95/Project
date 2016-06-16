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
using System.Data.Entity.Infrastructure;

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

        //GET: Schedule/ScheduleFor
        public ActionResult ScheduleFor(string scheduleFor)
        {
            ViewBag.scheduleFor = scheduleFor;

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

            return View();
        }

        //POST: Schedule/ScheduleFor
        public PartialViewResult _ScheduleFor(int? Teacher_ID, int? Group_ID)
        {
            if (Teacher_ID != null || Group_ID != null)
                ViewBag.request = "true";

            var schedules = db.Schedules.Include(s => s.Classroom).Include(s => s.Group).Include(s => s.LessonType).Include(s => s.Subject).Include(s => s.Teacher);

            if (Teacher_ID != null)
            {
                schedules = schedules.Where(s => s.Teacher_ID == Teacher_ID);
                            }
            if (Group_ID != null)
            {
                schedules = schedules.Where(s => s.Group_ID == Group_ID);
            }

            schedules = from s in schedules
                        where s.EnrollmentYear == "2015"
                        orderby s.Date, s.LessonNumber
                        select s;
            
            return PartialView("_ScheduleFor", schedules);
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
                                               select new
                                               {
                                                   Group_ID = s.Group_ID,
                                                   FullGroup = s.Name + " " + s.EnrollmentYear
                                               }), "Group_ID", "FullGroup");
            ViewBag.Lesson_ID = new SelectList(db.LessonTypes, "Lesson_ID", "Type");
            ViewBag.Subject_ID = new SelectList(db.Subjects, "Subject_ID", "Name");
            ViewBag.Teacher_ID = new SelectList((from s in db.Teachers
                                                 select new
                                                 {
                                                     Teacher_ID = s.Teacher_ID,
                                                     FullName = s.Name + " " + s.Surname + " " + s.LastName
                                                 }), "Teacher_ID", "FullName");


            return View();


        }

        //
        // POST: /Schedule/Create

        [HttpPost]
        public ActionResult Create(Schedule schedule, DateTime[] Date, int[] Lesson_ID, int[] Room_ID)
        {
            bool roomFree = true;
            
            bool alternative1 = true;
            bool alternative2 = true;
            string roomNotFreeMessage = String.Empty;
            string alternativeMessage = String.Empty;
            string roomNumber = String.Empty;
            string alternativeLessonNumber1 = String.Empty;
            string alternativeLessonNumber2 = String.Empty;
            Schedule sch = new Schedule();//new oject
            if (ModelState.IsValid)
            {
                if (Date != null)
                {
                    for (int i = 0; i < Date.Length; i++)
                    {
                        if (Lesson_ID.Length > 1 && Room_ID.Length > 1)
                        {
                            schedule.Lesson_ID = Lesson_ID[i];
                            schedule.Room_ID = Room_ID[i];
                        }
                        schedule.Date = Date[i];
                        
                        sch.Group_ID = schedule.Group_ID; 
                        sch.Lesson_ID = schedule.Lesson_ID; 
                        sch.Room_ID = schedule.Room_ID; 
                        sch.Subject_ID = schedule.Subject_ID; 
                        sch.Teacher_ID = schedule.Teacher_ID;
                        sch.Date = schedule.Date;
                        sch.EnrollmentYear = schedule.EnrollmentYear;
                        sch.LessonNumber = schedule.LessonNumber;
                        
                        roomFree = true;

                        //check if classroom is free
                        foreach (var checkRoom in db.Schedules.ToList())
                        {
                            if (checkRoom.Room_ID == sch.Room_ID && checkRoom.Subject_ID == sch.Subject_ID && checkRoom.Teacher_ID == sch.Teacher_ID
                                && checkRoom.Room_ID == sch.Room_ID && checkRoom.Group_ID == sch.Group_ID && checkRoom.Lesson_ID == sch.Lesson_ID
                                && checkRoom.LessonNumber == sch.LessonNumber && checkRoom.LessonType == sch.LessonType && checkRoom.Date == sch.Date)
                            {
                                roomNotFreeMessage = roomNotFreeMessage + "Пара вже є в базі даних<br/>";
                                roomFree = false;

                                continue;
                            }

                            if (checkRoom.Teacher_ID == sch.Teacher_ID && checkRoom.Date == sch.Date && checkRoom.LessonNumber == sch.LessonNumber
                                && checkRoom.EnrollmentYear == sch.EnrollmentYear)
                            {
                                roomNotFreeMessage = roomNotFreeMessage + "Викладач має іншу пару " + Convert.ToDateTime(sch.Date).ToString("yyyy-MM-dd") + " на " + sch.LessonNumber + " парі<br/>";
                                roomFree = false;
                                continue;
                            }

                            if (checkRoom.Group_ID == sch.Group_ID && checkRoom.Date == sch.Date && checkRoom.LessonNumber == sch.LessonNumber
                                && checkRoom.EnrollmentYear == sch.EnrollmentYear)
                            {
                                roomNotFreeMessage = roomNotFreeMessage + "Група має іншу пару " + Convert.ToDateTime(sch.Date).ToString("yyyy-MM-dd") + " на " + sch.LessonNumber + " парі<br/>";
                                roomFree = false;
                                continue;
                            }
                            
                            if (checkRoom.Room_ID == sch.Room_ID && checkRoom.LessonNumber == sch.LessonNumber && checkRoom.Date == sch.Date
                                && checkRoom.EnrollmentYear == sch.EnrollmentYear)
                            {
                                alternative1 = true;
                                alternative2 = true;

                                foreach (var room in db.Classrooms.ToList())
                                {
                                    if (room.Room_ID == sch.Room_ID)
                                    {
                                        roomNumber = room.Number;//search number of room by id
                                        break;
                                    }
                                }
                                DateTime date = new DateTime();
                                date = Convert.ToDateTime(sch.Date);

                                roomFree = false;
                                //write message about occupied;
                                roomNotFreeMessage = roomNotFreeMessage + "Аудиторія " + roomNumber + " зайнята " + date.ToString("yyyy-MM-dd") + " на " + sch.LessonNumber + " парі<br/>";

                                //propose another lessonNumber
                                foreach (var alternativeRoom in db.Schedules.ToList())
                                {
                                    

                                    if (sch.LessonNumber == "1")
                                    {
                                        alternativeLessonNumber1 = "2"; alternativeLessonNumber2 = "3";
                                    }
                                    else if (sch.LessonNumber == "6")
                                    {
                                        alternativeLessonNumber1 = "5"; alternativeLessonNumber2 = "4";
                                    }
                                    else if (Convert.ToInt32(sch.LessonNumber) > 1 && Convert.ToInt32(sch.LessonNumber) < 6)
                                    {
                                        alternativeLessonNumber1 = (Convert.ToInt32(sch.LessonNumber) - 1).ToString();
                                        alternativeLessonNumber2 = (Convert.ToInt32(sch.LessonNumber) + 1).ToString();
                                    }

                                    if (alternativeRoom.Room_ID == sch.Room_ID && alternativeRoom.LessonNumber == alternativeLessonNumber1 && alternativeRoom.Date == sch.Date)
                                    {
                                        alternative1 = false;
                                    }

                                    if (alternativeRoom.Room_ID == sch.Room_ID && alternativeRoom.LessonNumber == alternativeLessonNumber2 && alternativeRoom.Date == sch.Date)
                                    {
                                        alternative2 = false;
                                    }
                                }

                                if (alternative1) alternativeMessage = alternativeMessage + "Аудиторія " + roomNumber + " вільна " + date.ToString("yyyy-MM-dd") + " на " + alternativeLessonNumber1 + " парі<br/>";
                                if (alternative2) alternativeMessage = alternativeMessage + "Аудиторія " + roomNumber + " вільна " + date.ToString("yyyy-MM-dd") + " на " + alternativeLessonNumber2 + " парі<br/>";

                                
                            }
                        }
                        //if classroom is free - add record
                        if (roomFree)
                        {
                            db.Schedules.Add(sch);
                            db.SaveChanges();
                        }
                        sch = new Schedule();
                    }
                }
                //if we have message about occupied - go to create page again and show message
                if (roomNotFreeMessage != String.Empty)
                    goto leaveOnCreate;

                //db.Schedules.Add(schedule);
                //db.SaveChanges();
                return RedirectToAction("Index");

            }

        leaveOnCreate:
            ViewBag.roomNotFreeMessage = roomNotFreeMessage;
            ViewBag.alternativeMessage = alternativeMessage;
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
                                                 select new
                                                 {
                                                     Teacher_ID = s.Teacher_ID,
                                                     FullName = s.Name + " " + s.Surname + " " + s.LastName
                                                 }), "Teacher_ID", "FullName", schedule.Teacher_ID);


            
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
                            ViewBag.NotFounded = "Група " + one.Group.Name + " з роком вступу " + enrYear + " не знайдена. Створіть таку групу, щоб копіювати.";
                            return AddFuctionForCopy();
                        }

                    }
                    //if we don't find records to copy - show message
                    if (notHaveRecords)
                    {
                        ViewBag.NotFounded = "Таких пар не знайдено";
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
            bool alternative1 = true;
            bool alternative2 = true;
            string alternativeLessonNumber1 = String.Empty;
            string alternativeLessonNumber2 = String.Empty;
            string roomNumber = String.Empty;
            bool roomFree = true;
            string roomNotFreeMessage = String.Empty;

            if (ModelState.IsValid)
            {
                //check if classroom is free
                foreach (var checkRoom in db.Schedules.ToList())
                {
                    if (checkRoom.Room_ID == schedule.Room_ID && checkRoom.Subject_ID == schedule.Subject_ID && checkRoom.Teacher_ID == schedule.Teacher_ID
                                && checkRoom.Room_ID == schedule.Room_ID && checkRoom.Group_ID == schedule.Group_ID && checkRoom.Lesson_ID == schedule.Lesson_ID
                                && checkRoom.LessonNumber == schedule.LessonNumber && checkRoom.LessonType == schedule.LessonType && checkRoom.Date == schedule.Date)
                    {
                        roomNotFreeMessage = roomNotFreeMessage + "Пара вже є в базі даних<br/>";
                        roomFree = false;

                        continue;
                    }

                    if (checkRoom.Teacher_ID == schedule.Teacher_ID && checkRoom.Date == schedule.Date && checkRoom.LessonNumber == schedule.LessonNumber
                        && checkRoom.EnrollmentYear == schedule.EnrollmentYear)
                    {
                        roomNotFreeMessage = roomNotFreeMessage + "Викладач має іншу пару " + Convert.ToDateTime(schedule.Date).ToString("yyyy-MM-dd") + " на " + schedule.LessonNumber + " парі<br/>";
                        roomFree = false;
                        continue;
                    }

                    if (checkRoom.Group_ID == schedule.Group_ID && checkRoom.Date == schedule.Date && checkRoom.LessonNumber == schedule.LessonNumber
                        && checkRoom.EnrollmentYear == schedule.EnrollmentYear)
                    {
                        roomNotFreeMessage = roomNotFreeMessage + "Група має іншу пару " + Convert.ToDateTime(schedule.Date).ToString("yyyy-MM-dd") + " на " + schedule.LessonNumber + " парі<br/>";
                        roomFree = false;
                        continue;
                    }

                    if (checkRoom.Room_ID == schedule.Room_ID && checkRoom.LessonNumber == schedule.LessonNumber && checkRoom.Date == schedule.Date
                        && checkRoom.EnrollmentYear == schedule.EnrollmentYear)
                    {
                        alternative1 = true;
                        alternative2 = true;

                        foreach (var room in db.Classrooms.ToList())
                        {
                            if (room.Room_ID == schedule.Room_ID)
                            {
                                roomNumber = room.Number;//search number of room by id
                                break;
                            }
                        }
                        DateTime date = new DateTime();
                        date = Convert.ToDateTime(schedule.Date);

                        roomFree = false;
                        //write message about occupied;
                        roomNotFreeMessage = roomNotFreeMessage + "Аудиторія " + roomNumber + " зайнята " + date.ToString("yyyy-MM-dd") + " на " + schedule.LessonNumber + " парі<br/>";

                        //propose another lessonNumber
                        foreach (var alternativeRoom in db.Schedules.ToList())
                        {
                            if (schedule.LessonNumber == "1")
                            {
                                alternativeLessonNumber1 = "2"; alternativeLessonNumber2 = "3";
                            }
                            else if (schedule.LessonNumber == "6")
                            {
                                alternativeLessonNumber1 = "5"; alternativeLessonNumber2 = "4";
                            }
                            else if (Convert.ToInt32(schedule.LessonNumber) > 1 && Convert.ToInt32(schedule.LessonNumber) < 6)
                            {
                                alternativeLessonNumber1 = (Convert.ToInt32(schedule.LessonNumber) - 1).ToString();
                                alternativeLessonNumber2 = (Convert.ToInt32(schedule.LessonNumber) + 1).ToString();
                            }

                            if (alternativeRoom.Room_ID == schedule.Room_ID && alternativeRoom.LessonNumber == alternativeLessonNumber1 && alternativeRoom.Date == schedule.Date)
                            {
                                alternative1 = false;
                            }

                            if (alternativeRoom.Room_ID == schedule.Room_ID && alternativeRoom.LessonNumber == alternativeLessonNumber2 && alternativeRoom.Date == schedule.Date)
                            {
                                alternative2 = false;
                            }
                        }

                        if (alternative1) roomNotFreeMessage = roomNotFreeMessage + "Аудиторія " + roomNumber + " вільна " + date.ToString("yyyy-MM-dd") + " на " + alternativeLessonNumber1 + " парі<br/>";
                        if (alternative2) roomNotFreeMessage = roomNotFreeMessage + "Аудиторія " + roomNumber + " вільна " + date.ToString("yyyy-MM-dd") + " на " + alternativeLessonNumber2 + " парі<br/>";

                    }
                }

                if (roomFree)
                {
                    Schedule r = db.Schedules.Find(schedule.Schedule_ID);
                    ((IObjectContextAdapter)db).ObjectContext.Detach(r);

                    db.Entry(schedule).State = EntityState.Modified;
                    db.SaveChanges();
                }

                if (roomNotFreeMessage != String.Empty)
                    goto leaveOnCreate;

                return RedirectToAction("Index");
            }

           leaveOnCreate:
            ViewBag.roomNotFreeMessage = roomNotFreeMessage;
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
        //GET: /Schedule/Index/_CorrectRecord
        
        //methods for edition wrong records on page with modal window
        public ActionResult CorrectRecord1()
        {
            ViewBag.numberID = "1";//id for field with Date(in order to datepicker works)
            return GeneralCorrectRecord(0);
            
        }

        public ActionResult CorrectRecord2()
        {
            ViewBag.numberID = "2";
            return GeneralCorrectRecord(1);
        }

        public ActionResult CorrectRecord3()
        {
            ViewBag.numberID = "3";
            return GeneralCorrectRecord(2);
        }

        public ActionResult CorrectRecord4()
        {
            ViewBag.numberID = "4";
            return GeneralCorrectRecord(3);
        }
        
        public ActionResult CorrectRecord5()
        {
            ViewBag.numberID = "5";
            return GeneralCorrectRecord(4);
        }
       
        //method which fill in fields in modal window
        private ActionResult GeneralCorrectRecord(int lineNumber)
        {

            DataTable dataErrors = MethodForCorrection();
            Schedule schedule = new Schedule();
            if (dataErrors.Rows.Count > lineNumber)
            {
                
                ViewBag.Room_ID = new SelectList(db.Classrooms, "Room_ID", "Number", dataErrors.Rows[lineNumber].ItemArray[1]);
                ViewBag.Group_ID = new SelectList((from s in db.Groups
                                                   select new
                                                   {
                                                       Group_ID = s.Group_ID,
                                                       FullGroup = s.Name + " " + s.EnrollmentYear
                                                   }), "Group_ID", "FullGroup", dataErrors.Rows[lineNumber].ItemArray[4]);
                ViewBag.Lesson_ID = new SelectList(db.LessonTypes, "Lesson_ID", "Type", dataErrors.Rows[lineNumber].ItemArray[2]);
                ViewBag.Subject_ID = new SelectList(db.Subjects, "Subject_ID", "Name", dataErrors.Rows[lineNumber].ItemArray[0]);
                ViewBag.Teacher_ID = new SelectList((from s in db.Teachers
                                                     select new
                                                     {
                                                         Teacher_ID = s.Teacher_ID,
                                                         FullName = s.Name + " " + s.Surname + " " + s.LastName
                                                     }), "Teacher_ID", "FullName", dataErrors.Rows[lineNumber].ItemArray[5]);
                

                ViewBag.Date = dataErrors.Rows[lineNumber].ItemArray[3];
                ViewBag.EnrollmentYear = dataErrors.Rows[lineNumber].ItemArray[6];
                ViewBag.LessonNumber = dataErrors.Rows[lineNumber].ItemArray[7];
            }
            
            return PartialView("_CorrectRecord",schedule);
        }
        

        //method which read CSV file with errors for modal window
        private DataTable MethodForCorrection()
        {
            //Set up our variables
            
            string line = string.Empty;
            string[] strArray;
            DataTable dt = new DataTable();
              
            DataRow row;      

            // work out where we should split on comma, but not in a sentence
            Regex r = new Regex(";(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            //Set the filename in to our stream
            StreamReader sr = new StreamReader((Path.Combine(Server.MapPath("~/Content/csv"), "Errors.csv")));

            //Read the first line and split the string at , with our regular expression in to an array
            line = sr.ReadLine();
            //Column for ID            

            strArray = r.Split(line);

            //For each item in the new split array, dynamically builds our Data columns. Save us having to worry about it.
            Array.ForEach(strArray, s => dt.Columns.Add(new DataColumn()));
            
            //Read each line in the CVS file until it’s empty
            while ((line = sr.ReadLine()) != null)
            {
                row = dt.NewRow();

                //add our current value to our data row
                row.ItemArray = r.Split(line);
                
                dt.Rows.Add(row);
            }              

            //Tidy Streameader up
            sr.Dispose();
            
            return dt;
        }

        //POST: /Schedule/Index/CorrectRecord
        [HttpPost]
        public ActionResult CorrectRecordPost(Schedule schedule)
        {
            bool alternative1 = true;
            bool alternative2 = true;
            string alternativeLessonNumber1 = String.Empty;
            string alternativeLessonNumber2 = String.Empty;
            string roomNumber = String.Empty;
            bool roomFree = true;
            string roomNotFreeMessage = String.Empty;
            //check if classroom is free
            if (ModelState.IsValid)
            {
                foreach (var checkRoom in db.Schedules.ToList())
                {
                    if (checkRoom.Room_ID == schedule.Room_ID && checkRoom.Subject_ID == schedule.Subject_ID && checkRoom.Teacher_ID == schedule.Teacher_ID
                               && checkRoom.Room_ID == schedule.Room_ID && checkRoom.Group_ID == schedule.Group_ID && checkRoom.Lesson_ID == schedule.Lesson_ID
                               && checkRoom.LessonNumber == schedule.LessonNumber && checkRoom.LessonType == schedule.LessonType && checkRoom.Date == schedule.Date)
                    {
                        roomNotFreeMessage = roomNotFreeMessage + "Пара вже є в базі даних<br/>";
                        roomFree = false;

                        continue;
                    }

                    if (checkRoom.Teacher_ID == schedule.Teacher_ID && checkRoom.Date == schedule.Date && checkRoom.LessonNumber == schedule.LessonNumber
                        && checkRoom.EnrollmentYear == schedule.EnrollmentYear)
                    {
                        roomNotFreeMessage = roomNotFreeMessage + "Викладач має іншу пару " + Convert.ToDateTime(schedule.Date).ToString("yyyy-MM-dd") + " на " + schedule.LessonNumber + " парі<br/>";
                        roomFree = false;
                        continue;
                    }

                    if (checkRoom.Group_ID == schedule.Group_ID && checkRoom.Date == schedule.Date && checkRoom.LessonNumber == schedule.LessonNumber
                        && checkRoom.EnrollmentYear == schedule.EnrollmentYear)
                    {
                        roomNotFreeMessage = roomNotFreeMessage + "Група має іншу пару " + Convert.ToDateTime(schedule.Date).ToString("yyyy-MM-dd") + " на " + schedule.LessonNumber + " парі<br/>";
                        roomFree = false;
                        continue;
                    }


                    if (checkRoom.Room_ID == schedule.Room_ID && checkRoom.LessonNumber == schedule.LessonNumber && checkRoom.Date == schedule.Date)
                    {
                        alternative1 = true;
                        alternative2 = true;

                        foreach (var room in db.Classrooms.ToList())
                        {
                            if (room.Room_ID == schedule.Room_ID)
                            {
                                roomNumber = room.Number;//search number of room by id
                                break;
                            }
                        }
                        DateTime date = new DateTime();
                        date = Convert.ToDateTime(schedule.Date);

                        roomFree = false;
                        //write message about occupied;
                        roomNotFreeMessage = roomNotFreeMessage + "Аудиторія " + roomNumber + " зайнята " + date.ToString("yyyy-MM-dd") + " на " + schedule.LessonNumber + " парі<br/>";

                        //propose another lessonNumber
                        foreach (var alternativeRoom in db.Schedules.ToList())
                        {


                            if (schedule.LessonNumber == "1")
                            {
                                alternativeLessonNumber1 = "2"; alternativeLessonNumber2 = "3";
                            }
                            else if (schedule.LessonNumber == "6")
                            {
                                alternativeLessonNumber1 = "5"; alternativeLessonNumber2 = "4";
                            }
                            else if (Convert.ToInt32(schedule.LessonNumber) > 1 && Convert.ToInt32(schedule.LessonNumber) < 6)
                            {
                                alternativeLessonNumber1 = (Convert.ToInt32(schedule.LessonNumber) - 1).ToString();
                                alternativeLessonNumber2 = (Convert.ToInt32(schedule.LessonNumber) + 1).ToString();
                            }

                            if (alternativeRoom.Room_ID == schedule.Room_ID && alternativeRoom.LessonNumber == alternativeLessonNumber1 && alternativeRoom.Date == schedule.Date)
                            {
                                alternative1 = false;
                            }

                            if (alternativeRoom.Room_ID == schedule.Room_ID && alternativeRoom.LessonNumber == alternativeLessonNumber2 && alternativeRoom.Date == schedule.Date)
                            {
                                alternative2 = false;
                            }
                        }

                        if (alternative1) roomNotFreeMessage = roomNotFreeMessage + "Аудиторія " + roomNumber + " вільна " + date.ToString("yyyy-MM-dd") + " на " + alternativeLessonNumber1 + " парі<br/>";
                        if (alternative2) roomNotFreeMessage = roomNotFreeMessage + "Аудиторія " + roomNumber + " вільна " + date.ToString("yyyy-MM-dd") + " на " + alternativeLessonNumber2 + " парі<br/>";
                                    
                    }
                }

                if (roomFree)
                {
                    db.Schedules.Add(schedule);
                    db.SaveChanges();
                }
            }
            else { roomNotFreeMessage = "Дата введена неправильно"; }
            return ScheduleData(roomNotFreeMessage);
            
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

        //method for update data on Index page
        public ActionResult ScheduleData(string message = "")
        {
            ViewBag.roomNotFree = message;
            var schedules = db.Schedules.Include(s => s.Classroom).Include(s => s.Group).Include(s => s.LessonType).Include(s => s.Subject).Include(s => s.Teacher);

            return PartialView("ScheduleData",schedules.ToList());
        }

        //GET: for downoload CSV file with wrong records
        public ActionResult DownloadFile()
        {
            string filename = Server.MapPath("/Content/csv/ErrorsLine.csv");
            string contentType = "text/csv";
            string downloadName = "CSV File.csv";

            return File(filename, contentType, downloadName);
        }

        //POST: for upload CSV file with data
        [HttpPost]
        public ActionResult Index(HttpPostedFileBase Files)
        {
            DataTable dt = new DataTable();
            DataTable dtErrors = new DataTable();
            DataTable dataErrors = new DataTable();
            List<DataTable> list = new List<DataTable>();
            
            ViewBag.Complete = false;
            

            if (Files != null && Files.ContentLength > 0)
            {
                
                    if (Files.FileName.EndsWith(".csv"))
                    {
                        var fileName = Path.GetFileName(Files.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/csv"), fileName);
                        try
                        {
                            Files.SaveAs(path);
                            list = ProcessCSV(path);
                            dt = list[0];
                            dtErrors = list[1];
                            dataErrors = list[2];


                            CreateCSVwithErrors(dtErrors, "ErrorsLine.csv");//csv with wrong records for user's upload
                            CreateCSVwithErrors(dataErrors, "Errors.csv");//csv with wrong records for edition in modal window


                            ViewBag.Feedback = ProcessBulkCopy(dt);
                            ViewBag.ErrorMassage = ErrorMassageForBulkCopy;
                            ViewBag.ErrorNumber = NumberOfErrorMassage;
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
                        ViewBag.Feedback = "Розширення файлу неправильне. Файл повинен бути .csv";
                    }

                
            }
            else { ViewBag.Feedback = "Виберіть файл"; }
            dt.Dispose();
            //}                       
            var schedules = db.Schedules.Include(s => s.Classroom).Include(s => s.Group).Include(s => s.LessonType).Include(s => s.Subject).Include(s => s.Teacher);
            return View(schedules.ToList());
            //return RedirectToAction("Index");
        }

        //creation csv file with wrong data
        private void CreateCSVwithErrors(DataTable dtErrors, string name)
        {
            //if file exists - delete it
            if (System.IO.File.Exists(Path.Combine(Server.MapPath("~/Content/csv"), name)))
            {
                System.IO.File.Delete(Path.Combine(Server.MapPath("~/Content/csv"), name));
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("subject;class;lesson;date;group;teacher;enrollment;number");//first line for csv file
                       
            foreach (DataRow line in dtErrors.Rows)//add records with errors in csv file
            {
                sb.AppendLine(string.Format("{0};{1};{2};{3};{4};{5};{6};{7}", line.ItemArray[1], line.ItemArray[2], line.ItemArray[3], line.ItemArray[4], line.ItemArray[5], line.ItemArray[6], line.ItemArray[7], line.ItemArray[8]));
            }

            System.IO.File.AppendAllText(Path.Combine(Server.MapPath("~/Content/csv"), name), sb.ToString(), System.Text.Encoding.GetEncoding(1251));//save csv file
        }
        
        private string ErrorMassageForBulkCopy;
        private int NumberOfErrorMassage;
        
        private List<DataTable> ProcessCSV(string fileName)
        {
            //Set up our variables
            string Feedback = string.Empty;
            string ErrorMassage = string.Empty;
            string line = string.Empty;
            string[] strArray;
            DataTable dt = new DataTable();
            DataTable dtErrors = new DataTable();
            DataTable dtErrorsName = new DataTable();
            
            List<DataTable> dtList = new List<DataTable>();
            bool subjects, classrooms, lessontypes, groups, teachers, roomFree, date, enrollment, lessonNumber, notExist, teacherFree, groupFree;
            int lineNumber = 1;
            int errorNumber = 0;

            DataRow row;
            DataRow rowErrors;
            DataRow rowDataErrors;
            
            object[] data_id = new object[9];
            
            // work out where we should split on comma, but not in a sentence
            Regex r = new Regex(";(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            //Set the filename in to our stream
            StreamReader sr = new StreamReader(fileName, System.Text.Encoding.GetEncoding(1251));

            //Read the first line and split the string at , with our regular expression in to an array
            line = sr.ReadLine();
            //Column for ID
            line = "id;" + line;

            strArray = r.Split(line);
            
            //For each item in the new split array, dynamically builds our Data columns. Save us having to worry about it.
            Array.ForEach(strArray, s => dt.Columns.Add(new DataColumn()));
            Array.ForEach(strArray, s => dtErrors.Columns.Add(new DataColumn()));
            Array.ForEach(strArray, s => dtErrorsName.Columns.Add(new DataColumn()));

            

            //Read each line in the CVS file until it’s empty
            while ((line = sr.ReadLine()) != null)
            {
                subjects = false;
                classrooms = false;
                lessontypes = false;
                groups = false;
                teachers = false;
                date = false;
                enrollment = false;
                lessonNumber = false;
                roomFree = true;
                notExist = true;
                teacherFree = true;
                groupFree = true;
                data_id = new object[9];
                
                //ID values
                line = " ;" + line;
                row = dt.NewRow();
                rowErrors = dtErrors.NewRow();
                rowDataErrors = dtErrorsName.NewRow();
                

                //add our current value to our data row
                row.ItemArray = r.Split(line);
                rowErrors.ItemArray = r.Split(line);
                rowDataErrors.ItemArray = r.Split(line);
                
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
                    string groupName = name.Name + " " + name.EnrollmentYear;
                    if (row.ItemArray[5].ToString() == groupName)
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
                //Date, Enrollment year and LessonNumber don't have the IDs

                DateTime dateToParse;
                if(DateTime.TryParse(row.ItemArray[4].ToString(), out dateToParse))
                {
                    date = true;
                    data_id[4] = dateToParse;
                    data_id[4] = row.ItemArray[4];
                }

                if (Convert.ToInt32(row.ItemArray[7]) > 2014 && Convert.ToInt32(row.ItemArray[7]) < 2020)
                {
                    enrollment = true;
                    data_id[7] = row.ItemArray[7];
                }

                if (Convert.ToInt32(row.ItemArray[8]) > 0 && Convert.ToInt32(row.ItemArray[8]) < 7)
                {
                    lessonNumber = true;
                    data_id[8] = row.ItemArray[8];
                }

                //data_id[4] = row.ItemArray[4];
                //data_id[7] = row.ItemArray[7];
                //data_id[8] = row.ItemArray[8];

                //checking if classroom is occupied
                foreach (var checkRoom in db.Schedules.ToList())
                {
                    if (checkRoom.Room_ID == Convert.ToInt32(data_id[2]) 
                        && checkRoom.LessonNumber == data_id[8].ToString() 
                        && checkRoom.Date == Convert.ToDateTime(data_id[4])
                        && checkRoom.Group_ID == Convert.ToInt32(data_id[5])
                        && checkRoom.Lesson_ID == Convert.ToInt32(data_id[3])
                        && checkRoom.Subject_ID == Convert.ToInt32(data_id[1])
                        && checkRoom.Teacher_ID == Convert.ToInt32(data_id[6])
                        && checkRoom.EnrollmentYear == data_id[7].ToString())
                    {
                        notExist = false;
                        goto alreadyExist;
                    }
                }

                foreach (var checkRoom in db.Schedules.ToList())
                {
                    if (checkRoom.LessonNumber == data_id[8].ToString()
                        && checkRoom.Date == Convert.ToDateTime(data_id[4])
                        && checkRoom.Group_ID == Convert.ToInt32(data_id[5])
                        && checkRoom.EnrollmentYear == data_id[7].ToString())
                    {
                        groupFree = false;
                    }
                }

                foreach (var checkRoom in db.Schedules.ToList())
                {
                    if (checkRoom.LessonNumber == data_id[8].ToString()
                        && checkRoom.Date == Convert.ToDateTime(data_id[4])
                        && checkRoom.Teacher_ID == Convert.ToInt32(data_id[6])
                        && checkRoom.EnrollmentYear == data_id[7].ToString())
                    {
                        teacherFree = false;
                    }
                }

                

                foreach (var checkRoom in db.Schedules.ToList())
                {
                    if (checkRoom.Room_ID == Convert.ToInt32(data_id[2]) && checkRoom.LessonNumber == data_id[8].ToString() && checkRoom.Date == Convert.ToDateTime(data_id[4]))
                    {
                        roomFree = false;
                    }
                }

            alreadyExist:
                lineNumber++;
                //if all names are founded, add row in dt, else add row in dtErrors
                if (teachers && groups && lessontypes && classrooms && subjects && roomFree && date && enrollment && lessonNumber && notExist && groupFree && teacherFree)
                {
                    row.ItemArray = data_id;
                    dt.Rows.Add(row);
                }
                else 
                {
                    errorNumber++;
                    //row.ItemArray = data_id;
                    dtErrors.Rows.Add(rowErrors);

                    rowDataErrors.ItemArray = data_id;
                    dtErrorsName.Rows.Add(rowDataErrors);

                    //record only first 5 errors
                    if (errorNumber < 6)
                    {
                        Feedback = "Помилка в стрічці " + lineNumber + ":";
                        if (!teachers) Feedback = Feedback + " викладач " + row.ItemArray[6].ToString() + " не знайдений в базі;";
                        if (!groups) Feedback = Feedback + " група " + row.ItemArray[5].ToString() + " не знайдена в базі;";
                        if (!lessontypes) Feedback = Feedback + " тип заняття " + row.ItemArray[3].ToString() + " не знайдений в базі;";
                        if (!classrooms) Feedback = Feedback + " аудиторія " + row.ItemArray[2].ToString() + " не знайдена в базі;";
                        if (!subjects) Feedback = Feedback + " предмет " + row.ItemArray[1].ToString() + " не знайдений в базі;";
                        if (!roomFree) Feedback = Feedback + " аудиторія " + row.ItemArray[2].ToString() + " зайнята " + row.ItemArray[4].ToString() + " на " + row.ItemArray[8].ToString() + " парі;";
                        if (!notExist) Feedback = Feedback + " пара вже є базі даних;";
                        if (!teacherFree) Feedback = Feedback + " викладач " + row.ItemArray[6].ToString() + " зайнятий " + row.ItemArray[4].ToString() + " на " + row.ItemArray[8].ToString() + " парі;";
                        if (!groupFree) Feedback = Feedback + " група " + row.ItemArray[5].ToString() + " зайнята " + row.ItemArray[4].ToString() + " на " + row.ItemArray[8].ToString() + " парі;";
                        if (!date) Feedback = Feedback + " поле Дата заповнено неправилно;";
                        if (!enrollment) Feedback = Feedback + " поле Навчальний Рік заповнене неправильно;";
                        if (!lessonNumber) Feedback = Feedback + " поле № пари заповнене неправильно;";
                        Feedback = Feedback + "<button class='correct' id='modal-opener"+errorNumber+"'>Редагувати</button> <br/>";
                        ErrorMassage = ErrorMassage + Feedback;
                    }
                }
            }
            
            //if number of errors are less than 5
            if (errorNumber < 5)
            {
                if (errorNumber == 0) ErrorMassage = string.Empty;
                else ErrorMassage = "Показано " + errorNumber + " помилки:<br/>" + ErrorMassage;
            }
            else//if number of errors are 5 and more
            {
                ErrorMassage = "Показано 5 з " + errorNumber + " помилок:<br/>" + ErrorMassage;
            } 
            ErrorMassageForBulkCopy = ErrorMassage;//record error's message to global field
            NumberOfErrorMassage = errorNumber;
            //Tidy Streameader up
            sr.Dispose();
            dtList.Add(dt);
            dtList.Add(dtErrors);
            dtList.Add(dtErrorsName);
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
                        Feedback = "Завантаження завершено";
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