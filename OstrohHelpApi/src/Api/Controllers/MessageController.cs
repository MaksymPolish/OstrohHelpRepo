using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/Message")]
public class MessageController(IMediator _mediator, 
    IMessageQuery _messageQuery, 
    IMessageRepository _messageRepository) : ControllerBase
{
    //Recive
    
    //Send
    
    //Recive by consultation
    
    //Delete
    
    //Update
    
    
}