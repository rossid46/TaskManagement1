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
        public string Title { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public string Description { get; set; }

    }
}
