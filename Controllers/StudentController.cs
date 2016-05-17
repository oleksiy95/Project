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
using System.Text;
using System.Configuration;
using System.Data.SqlClient;

namespace CourseProject.Controllers
{
    public class StudentController : Controller
    {
        private ScheduleEntities db = new ScheduleEntities();

        //
        // GET: /Student/

        public ActionResult Index()
        {
            var students = db.Students.Include(s => s.Group);
            return View(students.ToList());
        }

        //
        // GET: /Student/Details/5

        public ActionResult Details(int id = 0)
        {
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        //
        // GET: /Student/Create

        public ActionResult Create()
        {
            ViewBag.Group_ID = new SelectList((from s in db.Groups
                                               select new
                                               {
                                                   Group_ID = s.Group_ID,
                                                   FullGroup = s.Name + " " + s.EnrollmentYear
                                               }), "Group_ID", "FullGroup");
            return View();
        }

        //
        // POST: /Student/Create

        [HttpPost]
        public ActionResult Create(Student student)
        {
            if (ModelState.IsValid)
            {
                db.Students.Add(student);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Group_ID = new SelectList(db.Groups, "Group_ID", "Name", student.Group_ID);
            return View(student);
        }

        //
        // GET: /Student/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            ViewBag.Group_ID = new SelectList((from s in db.Groups
                                               select new
                                               {
                                                   Group_ID = s.Group_ID,
                                                   FullGroup = s.Name + " " + s.EnrollmentYear
                                               }), "Group_ID", "FullGroup", student.Group_ID);
            return View(student);
        }

        //
        // POST: /Student/Edit/5

        [HttpPost]
        public ActionResult Edit(Student student)
        {
            if (ModelState.IsValid)
            {
                db.Entry(student).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Group_ID = new SelectList(db.Groups, "Group_ID", "Name", student.Group_ID);
            return View(student);
        }

        //
        // GET: /Student/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        //
        // POST: /Student/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Student student = db.Students.Find(id);
            db.Students.Remove(student);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        //GET: for downoload CSV file with wrong records
        public ActionResult DownloadFile()
        {
            string filename = Server.MapPath("/Content/csv/ErrorsLineStudents.csv");
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
           // DataTable dataErrors = new DataTable();
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
                        //dataErrors = list[2];


                        CreateCSVwithErrors(dtErrors, "ErrorsLineStudents.csv");//csv with wrong records for user's upload
                        //CreateCSVwithErrors(dataErrors, "Errors.csv");//csv with wrong records for edition in modal window


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
                    ViewBag.Feedback = "File format is not correct. It must be .csv";
                }


            }
            else { ViewBag.Feedback = "Please select a file"; }
            dt.Dispose();
            //}                       
            var students = db.Students.Include(s => s.Group);
            return View(students.ToList());
            //return RedirectToAction("Index");
        }

        private void CreateCSVwithErrors(DataTable dtErrors, string name)
        {
            //if file exists - delete it
            if (System.IO.File.Exists(Path.Combine(Server.MapPath("~/Content/csv"), name)))
            {
                System.IO.File.Delete(Path.Combine(Server.MapPath("~/Content/csv"), name));
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("name;surname;lastname;group");//first line for csv file

            foreach (DataRow line in dtErrors.Rows)//add records with errors in csv file
            {
                sb.AppendLine(string.Format("{0};{1};{2};{3}", line.ItemArray[1], line.ItemArray[2], line.ItemArray[3], line.ItemArray[4]));
            }

            System.IO.File.AppendAllText(Path.Combine(Server.MapPath("~/Content/csv"), name), sb.ToString());//save csv file
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
            bool name, surname, lastname, groups;
            int lineNumber = 1;
            int errorNumber = 0;

            DataRow row;
            DataRow rowErrors;
            DataRow rowDataErrors;

            object[] data_id = new object[5];

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
            Array.ForEach(strArray, s => dtErrorsName.Columns.Add(new DataColumn()));



            //Read each line in the CVS file until it’s empty
            while ((line = sr.ReadLine()) != null)
            {                
                name = false;
                surname = false;
                lastname = false;
                groups = false;
                data_id = new object[5];

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
                
                foreach (var gr in db.Groups.ToList())
                {
                    string groupName = gr.Name + " " + gr.EnrollmentYear;
                    if (row.ItemArray[4].ToString() == groupName)
                    {
                        data_id[4] = gr.Group_ID;
                        groups = true;
                        break;
                    }
                }

                //check not null fields
                if (row.ItemArray[1].ToString() != String.Empty)
                {
                    name = true;
                    data_id[1] = row.ItemArray[1];
                }

                if (row.ItemArray[2].ToString() != String.Empty)
                {
                    surname = true;
                    data_id[2] = row.ItemArray[2];
                }

                if (row.ItemArray[3].ToString() != String.Empty)
                {
                    lastname = true;
                    data_id[3] = row.ItemArray[3];
                }
                
                lineNumber++;
                //if all names are founded, add row in dt, else add row in dtErrors
                if (name && groups && surname && lastname)
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
                        Feedback = "Error in line " + lineNumber + ":";
                        if (!name) Feedback = Feedback + " field Name can't be emty;";
                        if (!groups) Feedback = Feedback + " group " + row.ItemArray[4].ToString() + " isn't founded;";
                        if (!surname) Feedback = Feedback + " field SurName can't be empty;";
                        if (!lastname) Feedback = Feedback + " field LastName can't be empty;";
                        Feedback = Feedback + "<br/>";
                        //Feedback = Feedback + "<button class='correct' id='modal-opener" + errorNumber + "'>Edit and save</button> <br/>";
                        ErrorMassage = ErrorMassage + Feedback;
                    }
                }
            }

            //if number of errors are less than 5
            if (errorNumber < 5)
            {
                if (errorNumber == 0) ErrorMassage = string.Empty;
                else ErrorMassage = "Show " + errorNumber + " errors:<br/>" + ErrorMassage;
            }
            else//if number of errors are 5 and more
            {
                ErrorMassage = "Show 5 of " + errorNumber + " errors:<br/>" + ErrorMassage;
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
                    copy.DestinationTableName = "Student";
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