using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.DataAccess.Context;
using TaskManagement.DataAccess.Repository.IRepository;
using TaskManagement.Models;
using TaskManagementWeb.Api.Controllers;

namespace UnitTestProject
{
    public class TaskItemControllerTest
    {
        private readonly Mock<ApplicationDbContext> mockDbContext;
        private readonly TaskController taskController;
        public TaskItemControllerTest()
        {
            mockDbContext = new Mock<ApplicationDbContext>();
            taskController = new TaskController(mockDbContext.Object);
        }
        [Fact]
        public void GetAll()
        {
            // Arrange
            var taskItems = new List<TaskItem>
                {
                    new TaskItem { Id = 1, Title = "Task 1" },
                    new TaskItem { Id = 2, Title = "Task 2" }
                };

            mockDbContext.Setup(x => x.TaskItems.ToList()).Returns(taskItems);

            var result = taskController.GetTaskItems();

            Assert.NotNull(result);
            Assert.Equal(GetTaskItems().ToString(), result.ToString());
            Assert.True(taskItems.Equals(result));

        }

        private List<TaskItem> GetTaskItems()
        {
            List<TaskItem> taskItems = new List<TaskItem>
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "Task 1",
                    Status = "ToDo",
                    Priority = "High",
                    DueDate = DateTime.Now.AddDays(5),
                    Description = "Description for Task 1"
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "Task 2",
                    Status = "InProgress",
                    Priority = "Medium",
                    DueDate = DateTime.Now.AddDays(10),
                    Description = "Description for Task 2"
                }
            };
            return taskItems;
        }
    }
}
