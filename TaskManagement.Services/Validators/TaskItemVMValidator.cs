using FluentValidation;
using TaskManagement.Models.ViewModels;

public class TaskItemVMValidator : AbstractValidator<TaskItemVM>
{
    public TaskItemVMValidator()
    {
        // Validazione per il campo Title
        RuleFor(x => x.TaskItem.Title)
            .NotEmpty().WithMessage("The Title field is required.")
            .MaximumLength(100).WithMessage("The Title field must not exceed 100 characters.");

        // Validazione per il campo Status
        RuleFor(x => x.TaskItem.Status)
            .NotEmpty().WithMessage("The Status field is required.")
            .Must(status => new[] { "Pending", "In Progress", "Completed" }.Contains(status))
            .WithMessage("The Status field must be one of the following: Pending, In Progress, Completed.");

        // Validazione per il campo Priority
        RuleFor(x => x.TaskItem.Priority)
            .NotEmpty().WithMessage("The Priority field is required.")
            .Must(priority => new[] { "Low", "Medium", "High" }.Contains(priority))
            .WithMessage("The Priority field must be one of the following: Low, Medium, High.");

        // Validazione per il campo DueDate
        RuleFor(x => x.TaskItem.DueDate)
            .GreaterThanOrEqualTo(DateTime.Now).WithMessage("The Due Date must be in the future.");

        // Validazione per il campo Description
        RuleFor(x => x.TaskItem.Description)
            .NotEmpty().WithMessage("The Description field is required.")
            .MaximumLength(500).WithMessage("The Description field must not exceed 500 characters.");

        // Validazione per il campo ApplicationUserId
        RuleFor(x => x.TaskItem.ApplicationUserId)
            .NotEmpty().WithMessage("The Application User field is required.");
    }
}
