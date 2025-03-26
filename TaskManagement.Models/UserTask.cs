using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Models
{
    public class UserTask
    {
        public enum TaskStatus
        {
            ToDo,
            InProgress,
            Done
        }
        public enum TaskPriority
        {
            Low,
            Medium,
            High
        }
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public string Description { get; set; }
        public int AssignedToUserId { get; set; }
        public virtual ApplicationUser AssignedToUser { get; set; }

    }
}
