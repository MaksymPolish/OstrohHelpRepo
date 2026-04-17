using Application.Common;
using Application.Common.Interfaces.Repositories;
using Application.Messages.Exceptions;
using Domain.Messages;
using MediatR;

namespace Application.Messages.Commands;

/// <summary>
/// Soft delete a single attachment - clears its data and marks as deleted.
/// Used when user wants to remove an individual file from a message.
/// </summary>
public record DeleteAttachmentCommand(Guid AttachmentId) 
    : IRequest<Result<MessageAttachment, string>>;

public class DeleteAttachmentCommandHandler(
    IMessageAttachmentRepository _attachmentRepository) 
    : IRequestHandler<DeleteAttachmentCommand, Result<MessageAttachment, string>>
{
    public async Task<Result<MessageAttachment, string>> Handle(
        DeleteAttachmentCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Get attachment
            var attachment = await _attachmentRepository.GetByIdAsync(request.AttachmentId, cancellationToken);
            
            if (attachment == null)
                return "Attachment not found";

            // Soft delete - clear data and mark as deleted
            attachment.FileUrl = "Attachment was deleted by user";
            attachment.FileType = "deleted";
            attachment.ThumbnailUrl = null;
            attachment.MediumPreviewUrl = null;
            attachment.VideoPosterUrl = null;
            attachment.PdfPagePreviewUrl = null;
            attachment.IsDeleted = true;
            
            // Update in database
            await _attachmentRepository.UpdateAsync(attachment, cancellationToken);
            
            return attachment;
        }
        catch (Exception ex)
        {
            return $"Error deleting attachment: {ex.Message}";
        }
    }
}
