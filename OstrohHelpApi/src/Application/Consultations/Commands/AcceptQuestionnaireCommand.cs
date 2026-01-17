using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Consultations.Exceptions;
using Application.Questionnaire.Exceptions;
using Domain.Conferences;
using Domain.Conferences.Statuses;
using Domain.Inventory;
using Domain.Inventory.Statuses;
using Domain.Users;
using MediatR;
using Optional;

namespace Application.Consultations.Commands;

public record AcceptQuestionnaireCommand(Guid QuestionaryId, Guid PsychologistId, DateTime ScheduledTime)
    : IRequest<Result<Domain.Conferences.Consultations, Exception>>;


public class AcceptQuestionnaireCommandHandler(
    IQuestionnaireQuery _questionnaireQuery,
    IUserQuery _userQuery,
    IConsultationStatusQuery _consultationStatusQuery,
    IQuestionnaireStatusQuery _questionnaireStatusQuery,
    IQuestionnaireRepository _questionnaireRepository,
    IConsultationRepository _consultationRepository,
    IRoleQuery _roleQuery)
    : IRequestHandler<AcceptQuestionnaireCommand, Result<Domain.Conferences.Consultations, Exception>>
{
    public async Task<Result<Domain.Conferences.Consultations, Exception>> Handle(AcceptQuestionnaireCommand command, CancellationToken ct)
    {
        var questionnaireId = new QuestionaryId(command.QuestionaryId);
        var psychologistId = new UserId(command.PsychologistId);
        var scheduledTime = command.ScheduledTime;

        // 1. Отримай анкету
        var questionnaireOption = await _questionnaireQuery.GetByIdAsync(questionnaireId, ct);
        if (!questionnaireOption.HasValue)
            return new Exception($"Questionary with ID '{questionnaireId}' not found.");
        var q = questionnaireOption.ValueOr((Questionary)null);
        if (q == null)
            return new Exception($"Questionary with ID '{questionnaireId}' not found.");

        // 2. Перевірка статуса анкети
        var statusOption = await _questionnaireStatusQuery.GetByIdAsync(q.StatusId, ct);
        if (!statusOption.HasValue)
            return new Exception("Questionary status could not be determined.");
        var status = statusOption.ValueOr((QuestionaryStatuses)null);
        if (status == null)
            return new Exception("Questionary status could not be determined.");
        if (status.Name == "Принято")
            return new Exception($"This questionary is already accepted. ID: {q.Id}");

        // 3. Отримай статус для консультації
        var consultationStatusOption = await _consultationStatusQuery.GetByNameAsync("Назначено", ct);
        if (!consultationStatusOption.HasValue)
            return new Exception("Consultation status 'Назначено' not found.");
        var consultationStatus = consultationStatusOption.ValueOr((ConsultationStatuses)null);
        if (consultationStatus == null)
            return new Exception("Consultation status 'Назначено' not found.");

        // 4. Отримай психолога
        var psychologistOption = await _userQuery.GetByIdAsync(psychologistId, ct);
        if (!psychologistOption.HasValue)
            return new Exception("Psychologist not found.");
        var p = psychologistOption.ValueOr((Domain.Users.User)null);
        if (p == null)
            return new Exception("Psychologist not found.");

        // 5. Перевірка ролі
        var roleOption = await _roleQuery.GetByIdAsync(p.RoleId, ct);
        if (!roleOption.HasValue)
            return new Exception("Role not found.");
        var r = roleOption.ValueOr((Domain.Users.Roles.Role)null);
        if (r == null)
            return new Exception("Role not found.");
        if (r.Name != "Психолог")
            return new Exception($"User with ID '{psychologistId}' is not a psychologist.");

        // 6. Створення консультації
        var studentId = q.UserId ?? throw new Exception("Student ID is null");
        var consultation = Domain.Conferences.Consultations.Create(
            id: ConsultationsId.New(),
            questionnaireId: q.Id,
            studentId: studentId,
            psychologistId: psychologistId,
            statusId: consultationStatus.Id,
            scheduledTime: scheduledTime,
            createdAt: DateTime.UtcNow
        );

        await _consultationRepository.AddAsync(consultation, ct);
        return consultation;
    }
}