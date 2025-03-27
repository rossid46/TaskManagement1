using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int UserTaskId { get; set; }
        [ForeignKey("UserTaskId")]
        public TaskItem TaskItem { get; set; }
        public string CommentText { get; set; }
        public DateTime CreationDate { get; set; }

    }
}
