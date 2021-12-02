using FluentValidation;
using MinimalApiTesting.Dtos;

namespace MinimalApiTesting.Validators
{
    public class TodoItemInputValidator : AbstractValidator<TodoItemInput>
    {
        public TodoItemInputValidator()
        {
            RuleFor(x => x.Title)
                .NotNull().WithMessage("{PropertyName} is required")
                .MinimumLength(5).WithMessage("{PropertyName} must be longer then 5.");
        }
    }
}
