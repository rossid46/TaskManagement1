using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Api.Models
{
        public class AuthJwtRegistrationValidator : AbstractValidator<AuthJwtRegistration>
        {
            public AuthJwtRegistrationValidator()
            {
                RuleFor(x => x.User)
                    .NotEmpty().WithMessage("User is required.")
                    .EmailAddress().WithMessage("User must be a valid email address.");

                RuleFor(x => x.Password)
                    .NotEmpty().WithMessage("Password is required.")
                    .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
            }
        }
}
