using DataAccessLayer;
using FluentValidation;


public class UKNameCreateCustomerDTOValidator :  AbstractValidator<CreateCustomerDTO>
{

}

public class CreateCustomerDTOValidator : AbstractValidator<CreateCustomerDTO>
{
    public CreateCustomerDTOValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MinimumLength(10)
            .WithMessage("Name must be at least 10 characters long.");

        // RuleFor(x => x.Email)
        //     .NotEmpty()
        //     .WithMessage("Email is required.")
        //     .EmailAddress()
        //     .WithMessage("Invalid email format.");

        // RuleFor(x => x.Age)
        //     .NotEmpty()
        //     .WithMessage("Age is required.")
        //     .InclusiveBetween(0, 150)
        //     .WithMessage("Age must be between 0 and 150.");
    }
}