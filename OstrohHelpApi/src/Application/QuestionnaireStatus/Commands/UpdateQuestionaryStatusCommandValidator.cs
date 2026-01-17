using FluentValidation;

namespace Application.QuestionnaireStatus.Commands;

public class UpdateQuestionaryStatusCommandValidator : AbstractValidator<UpdateQuestionaryStatusCommand>
{
    public UpdateQuestionaryStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
    }
}
