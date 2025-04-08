

using FluentValidation;

namespace TaskManagement.Models
{
    public class AuthJwtRegistration
    {
        public string User { get; set; }
        public string Password { get; set; }
    }

    public class AuthJwtRegistrationValidator : AbstractValidator<AuthJwtRegistration>
    {
        public AuthJwtRegistrationValidator()
        {
            RuleFor(x => x.User)
                .NotEmpty().WithMessage("User is required.")
                .EmailAddress().WithMessage("User must be a valid email address.");

            RuleFor(p => p.Password).Matches(@"[a-z]+").WithMessage("Your password must contain at least one lowercase letter.");
            RuleFor(p => p.Password).Matches(@"[0-9]+").WithMessage("Your password must contain at least one number.");
            RuleFor(x => x.Password).Matches(@"[\!\?\*\.]*$").WithMessage("Your password must contain at least one (!? *.).");
        }
    }
}