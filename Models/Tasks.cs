using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Tassc.Models
{
    public class Tasks
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int taskkey { get; set; }

        public string TaskName { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }
    }
}