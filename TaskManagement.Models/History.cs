using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Models
{
    public class History
    {
        [Key]
        public int Id { get; set; }
        public string FromStatus { get; set; }
        public string ToStatus { get; set; }
        public DateTime ChangeDate { get; set; }
        public int TaskItemId { get; set; }
        [ForeignKey("TaskItemId")]
        public TaskItem TaskItem { get; set; }
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

    }
}
