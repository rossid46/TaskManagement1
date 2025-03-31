using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Models.ViewModels
{
    public class TaskItemVM
    {
        public TaskItem TaskItem { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> UserList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> PriorityList { get; set; }
    }
}
