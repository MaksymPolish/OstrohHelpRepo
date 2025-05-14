using Api.Dtos;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Questionnaire.Commands;
using Domain.Inventory;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;


[ApiController]
[Route("api/questionnaire")]
public class QuestionnaireController(IMediator _mediator, 
    IQuestionnaireQuery _questionnaireQuery, 
    IUserQuery _userQuery,
    IQuestionnaireStatusQuery _statusQuery) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuestionnaireCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        return result.Match<IActionResult>(
            q => CreatedAtAction(nameof(Create), new { id = q.Id }, q),
            ex => BadRequest(new { Error = ex.Message })
        );
    }
    
    [HttpGet("all")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        // Retrieve all questionnaires from the database
        var questionnaires = await _questionnaireQuery.GetAllAsync(ct);

        // Check if any questionnaires exist, return NotFound if none found
        if (!questionnaires.Any())
            return NotFound(new { Message = "No questionnaires found." });

        // Initialize a list to store DTOs
        var dtos = new List<QuestionnaireDto>();

        // Iterate over each questionnaire
        foreach (var q in questionnaires)
        {
            // Initialize default values for user details and status name
            string fullName = "Анонімно";
            string email = "Анонімно";
            string statusName = "Unknown";

            // Check if the questionnaire is not anonymous and has a user ID
            if (!q.IsAnonymous && q.UserId is not null)
            {
                // Retrieve user details based on user ID
                var userOption = await _userQuery.GetByIdAsync(q.UserId, ct);

                // If user exists, extract full name and email
                await userOption.Match(
                    async user =>
                    {
                        fullName = user.FullName;
                        email = user.Email;
                    },
                    // Do nothing if user not found
                    () => Task.CompletedTask
                );
            }

            // Retrieve status details based on status ID
            var statusOption = await _statusQuery.GetByIdAsync(q.StatusId, ct);

            // If status exists, extract status name
            await statusOption.Match(
                async status => statusName = status.Name,
                // Do nothing if status not found
                () => Task.CompletedTask
            );

            // Create a DTO for the questionnaire and add it to the list
            dtos.Add(new QuestionnaireDto
            {
                Id = q.Id.ToString(),
                UserId = q.UserId?.ToString(),
                FullName = fullName,
                Email = email,
                StatusId = q.StatusId.ToString(),
                StatusName = statusName,
                Description = q.Description,
                IsAnonymous = q.IsAnonymous,
                SubmittedAt = q.SubmittedAt
            });
        }

        // Return the list of DTOs as the response
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        // Retrieve a questionnaire by its ID
        var questionnaireOption = await _questionnaireQuery.GetByIdAsync(new QuestionaryId(id), ct);

        // Check if the questionnaire exists
        return await questionnaireOption.Match<Task<IActionResult>>(
            async q =>
            {
                // Initialize default values for user details and status name
                string fullName = "Анонімно";
                string email = "Анонімно";
                string statusName = "Unknown";

                // Check if the questionnaire is not anonymous and has a user ID
                if (!q.IsAnonymous && q.UserId is not null)
                {
                    // Retrieve user details based on user ID
                    var userOption = await _userQuery.GetByIdAsync(q.UserId, ct);

                    // If user exists, extract full name and email
                    await userOption.Match(
                        async u =>
                        {
                            fullName = u.FullName;
                            email = u.Email;
                        },
                        // Do nothing if user not found
                        () => Task.CompletedTask
                    );
                }

                // Retrieve status details based on status ID
                var statusOption = await _statusQuery.GetByIdAsync(q.StatusId, ct);

                // If status exists, extract status name
                await statusOption.Match(
                    async s => statusName = s.Name,
                    // Do nothing if status not found
                    () => Task.CompletedTask
                );

                // Create a DTO for the questionnaire
                var dto = new QuestionnaireDto
                {
                    Id = q.Id.ToString(),
                    UserId = q.UserId?.ToString(),
                    FullName = fullName,
                    Email = email,
                    StatusId = q.StatusId.ToString(),
                    StatusName = statusName,
                    Description = q.Description,
                    IsAnonymous = q.IsAnonymous,
                    SubmittedAt = q.SubmittedAt
                };

                // Return the DTO as the response
                return Ok(dto);
            },
            // Return NotFound if the questionnaire does not exist
            () => Task.FromResult<IActionResult>(NotFound(new { Message = "Questionary not found" }))
        );
    }
    
    [HttpDelete("Delete-Questionnaire")]
    public async Task<IActionResult> Delete([FromBody]Guid id, CancellationToken ct)
    {
        var command = new DeleteQuestionnaireCommand(id);
        var result = await _mediator.Send(command, ct);

        return result.Match<IActionResult>(
            _ => NoContent(),
            ex => BadRequest(new { Error = ex.Message })
        );
    }
    
    [HttpPut("Update-Questionnaire")]
    public async Task<IActionResult> Update([FromBody] UpdateQuestionnaireCommand command, CancellationToken ct)
    {
        // Send command to mediator
        var result = await _mediator.Send(command, ct);

        // Return Ok if successful, BadRequest otherwise
        return result.Match<IActionResult>(
            _ => NoContent(), // successful
            ex => BadRequest(new { Error = ex.Message }) // failed
        );
    }
    
    [HttpPut("Update-StatusQuestionnaire")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.Match<IActionResult>(
            _ => NoContent(),
            ex => BadRequest(new { Error = ex.Message })
        );
    }
}