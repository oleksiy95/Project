//------------------------------------------------------------------------------
// <auto-generated>
//    Этот код был создан из шаблона.
//
//    Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//    Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CourseProject.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Student
    {
         public Student()
        {
            this.Grade = new HashSet<Grade>();
         }

        public int Student_ID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string LastName { get; set; }
        public int Group_ID { get; set; }
    
        public virtual Group Group { get; set; }
        public virtual ICollection<Grade> Grade { get; set; }
    }
}
