using Application.Common;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Messages.Exceptions;
using Domain.Messages;
using MediatR;

namespace Application.Messages.Commands;

public record DeleteMessageCommand(Guid MessageId) 
    : IRequest<Result<Message, MessageExceptions>>;

public class DeleteMessageCommandHandler(
    IMessageQuery _messageQuery,
    IMessageRepository _messageRepository,
    IMessageAttachmentRepository _attachmentRepository,
    IFileUploadService _fileUploadService) : IRequestHandler<DeleteMessageCommand, Result<Message, MessageExceptions>>
{
    public async Task<Result<Message, MessageExceptions>> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var messageid = new MessageId(request.MessageId);
        
        var messageOption = await _messageQuery.GetMessageById(messageid, cancellationToken);

        return await messageOption.Match(
            async message => await DeleteEntity(message, cancellationToken),
            () => Task.FromResult<Result<Message, MessageExceptions>>(
                new MessageNotFoundException(messageid)
            )
        );
    }
    
    public async Task<Result<Message, MessageExceptions>> DeleteEntity(Message message, CancellationToken cancellationToken)
    {
        try
        {
            // Step 1: Get all attachments for this message
            var attachments = await _attachmentRepository.GetByMessageIdAsync(message.Id, cancellationToken);
            
            // Step 2: Delete files from Cloudinary and remove attachments from database
            foreach (var attachment in attachments)
            {
                // Delete file from Cloudinary
                await _fileUploadService.DeleteFileAsync(attachment.CloudinaryPublicId);
                
                // Delete attachment record from database
                await _attachmentRepository.DeleteAsync(attachment, cancellationToken);
            }
            
            // Step 3: Delete the message
            var result = await _messageRepository.DeleteAsync(message, cancellationToken);
            return result;
        }
        catch (Exception e)
        {
            return new MessageNotFoundException(message.Id);
        }
    } 
}
    