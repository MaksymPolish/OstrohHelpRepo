using FluentValidation;

namespace Application.Questionnaire.Commands;

public class DeleteQuestionnaireCommandValidator : AbstractValidator<DeleteQuestionnaireCommand>
{
    public DeleteQuestionnaireCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");
    }
}
