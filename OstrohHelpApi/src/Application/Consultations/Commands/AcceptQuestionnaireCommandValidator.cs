using FluentValidation;

namespace Application.Consultations.Commands;

public class AcceptQuestionnaireCommandValidator : AbstractValidator<AcceptQuestionnaireCommand>
{
    public AcceptQuestionnaireCommandValidator()
    {
        RuleFor(x => x.QuestionaryId).NotEmpty().WithMessage("QuestionaryId is required");
        RuleFor(x => x.PsychologistId).NotEmpty().WithMessage("PsychologistId is required");
        RuleFor(x => x.ScheduledTime).NotEmpty().WithMessage("ScheduledTime is required");
    }
}
