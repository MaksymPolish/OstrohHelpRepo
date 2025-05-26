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

        // --- 1. Отримай анкету ---
        var questionnaireOption = await _questionnaireQuery.GetByIdAsync(questionnaireId, ct);

        return await questionnaireOption.Match(
            async q =>
            {
                // --- 2. Перевірка статуса анкети ---
                var statusOption = await _questionnaireStatusQuery.GetByIdAsync(q.StatusId, ct);

                return await statusOption.Match(
                    async status =>
                    {
                        if (status.Name == "Принято")
                            return new Exception($"This questionary is already accepted. ID: {q.Id}");

                        // --- 3. Отримай статус для консультації ---
                        var consultationStatusOption = await _consultationStatusQuery.GetByNameAsync("Назначено", ct);

                        return await consultationStatusOption.Match(
                            async consultationStatus =>
                            {
                                // --- 4. Отримай психолога ---
                                var psychologistOption = await _userQuery.GetByIdAsync(psychologistId, ct);

                                return await psychologistOption.Match(
                                    async p =>
                                    {
                                        // --- 5. Перевірка ролі ---
                                        var roleOption = await _roleQuery.GetByIdAsync(p.RoleId, ct);

                                        return await roleOption.Match(
                                            async r =>
                                            {
                                                if (r.Name != "Психолог")
                                                    return new Exception($"User with ID '{psychologistId}' is not a psychologist.");

                                                // --- 6. Створення консультації ---
                                                var consultation = Domain.Conferences.Consultations.Create(
                                                    id: ConsultationsId.New(), 
                                                    questionnaireId: q.Id,
                                                    studentId: q.UserId ?? throw new Exception("Student ID is null"),
                                                    psychologistId: psychologistId,
                                                    statusId: consultationStatus.Id,
                                                    scheduledTime: scheduledTime,
                                                    createdAt: DateTime.UtcNow
                                                );

                                                await _consultationRepository.AddAsync(consultation, ct);

                                                // --- 7. Оновлення статуса анкети ---
                                                // q.UpdateStatus(new QuestionaryStatusesId(consultationStatus.Id));
                                                await _questionnaireRepository.UpdateAsync(q, ct);

                                                return consultation;
                                            },
                                            () => Task.FromResult<Result<Domain.Conferences.Consultations, Exception>>(
                                                new Exception("Role not found.")
                                            )
                                        );
                                    },
                                    () => Task.FromResult<Result<Domain.Conferences.Consultations, Exception>>(
                                        new Exception("Psychologist not found.")
                                    )
                                );
                            },
                            () => Task.FromResult<Result<Domain.Conferences.Consultations, Exception>>(
                                new Exception("Consultation status 'Назначено' not found.")
                            )
                        );
                    },
                    () => Task.FromResult<Result<Domain.Conferences.Consultations, Exception>>(
                        new Exception("Questionary status could not be determined.")
                    )
                );
            },
            () => Task.FromResult<Result<Domain.Conferences.Consultations, Exception>>(
                new Exception($"Questionary with ID '{questionnaireId}' not found.")
            )
        );
    }
}