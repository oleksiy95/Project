using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Web;

namespace CourseProject.Models
{
    public partial class Grade
    {

        
        public int Grade_ID { get; set; }
        public int Student_ID { get; set; }
        public int Schedule_ID { get; set; }
        public string Visiting { get; set; }
        public string Mark { get; set; }

        public virtual Student Student { get; set; }
        public virtual Schedule Schedule { get; set; }
    }
}
