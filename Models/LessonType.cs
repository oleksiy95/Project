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
    
    public partial class LessonType
    {
        public LessonType()
        {
            this.Schedules = new HashSet<Schedule>();
        }
    
        public int Lesson_ID { get; set; }
        public string Type { get; set; }
    
        public virtual ICollection<Schedule> Schedules { get; set; }
    }
}
