using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;


[ApiController]
[Route("api/questionnaire")]
public class QuestionnaireController(IMediator mediator, 
    IQuestionnaireQuery questionnaireQuery, 
    IQuestionnaireRepository questionnaireRepository) : ControllerBase
{
    
    
    
    
    
}