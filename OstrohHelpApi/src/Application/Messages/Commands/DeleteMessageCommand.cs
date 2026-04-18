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
        var messageOption = await _messageQuery.GetMessageById(request.MessageId, cancellationToken);

        return await messageOption.Match(
            async message => await SoftDeleteMessage(message, cancellationToken),
            () => Task.FromResult<Result<Message, MessageExceptions>>(
                new MessageNotFoundException(request.MessageId)
            )
        );
    }
    

    /// Soft delete a message - clears content and marks as deleted instead of hard delete.
    /// This preserves message history while hiding content from users.
    public async Task<Result<Message, MessageExceptions>> SoftDeleteMessage(Message message, CancellationToken cancellationToken)
    {
        try
        {
            // Step 1: Get all attachments for this message
            var attachments = await _attachmentRepository.GetByMessageIdAsync(message.Id, cancellationToken);
            
            // Step 2: Soft delete all attachments - clear URLs and mark as deleted
            foreach (var attachment in attachments)
            {
                // Clear attachment data - mark as "deleted by user"
                attachment.FileUrl = "Attachment was deleted by user";
                attachment.FileType = "deleted";
                attachment.ThumbnailUrl = null;
                attachment.MediumPreviewUrl = null;
                attachment.VideoPosterUrl = null;
                attachment.PdfPagePreviewUrl = null;
                attachment.IsDeleted = true;
                
                // Update attachment record in database
                await _attachmentRepository.UpdateAsync(attachment, cancellationToken);
            }
            
            // Step 3: Soft delete the message - clear content and mark as deleted
            message.Text = "Message was deleted by user";
            message.EncryptedContent = null;
            message.Iv = null;
            message.AuthTag = null;
            message.IsDeleted = true;
            
            // Update message record in database
            var result = await _messageRepository.UpdateAsync(message, cancellationToken);
            return result;
        }
        catch (Exception e)
        {
            return new MessageNotFoundException(message.Id);
        }
    } 
}
    