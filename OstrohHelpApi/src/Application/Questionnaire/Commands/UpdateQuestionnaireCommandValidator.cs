using FluentValidation;

namespace Application.Questionnaire.Commands;

public class UpdateQuestionnaireCommandValidator : AbstractValidator<UpdateQuestionnaireCommand>
{
    public UpdateQuestionnaireCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required");
    }
}
