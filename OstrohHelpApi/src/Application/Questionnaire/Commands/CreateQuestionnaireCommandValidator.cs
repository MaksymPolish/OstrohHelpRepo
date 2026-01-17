using FluentValidation;

namespace Application.Questionnaire.Commands;

public class CreateQuestionnaireCommandValidator : AbstractValidator<CreateQuestionnaireCommand>
{
    public CreateQuestionnaireCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required");
    }
}
