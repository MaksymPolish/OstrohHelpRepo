using Domain.Conferences;
using Domain.Conferences.Statuses;
using Domain.Inventory;
using Domain.Inventory.Statuses;
using Domain.Messages;
using Domain.Users;
using Domain.Users.Roles;

namespace Tests.Common;

/// Фабрика для створення тестових даних
public static class TestDataFactory
{
    public static Role CreateRole(string name = "Студент", Guid? id = null)
    {
        return new Role
        {
            Id = new RoleId(id ?? Guid.NewGuid()),
            Name = name
        };
    }

    public static User CreateUser(
        string email = "test@example.com",
        string fullName = "Тестовий Користувач",
        RoleId? roleId = null,
        Guid? id = null,
        string photoUrl = "https://example.com/photo.jpg")
    {
        return new User
        {
            Id = new UserId(id ?? Guid.NewGuid()),
            Email = email,
            FullName = fullName,
            PhotoUrl = photoUrl,
            RoleId = roleId ?? new RoleId(Guid.NewGuid()),
            GoogleId = Guid.NewGuid().ToString()
        };
    }

    public static Questionary CreateQuestionnaire(
        UserId? userId = null,
        Guid? id = null,
        string description = "Тестовий опис проблеми",
        Guid? statusId = null,
        bool isAnonymous = false)
    {
        return Questionary.Create(
            new QuestionaryId(id ?? Guid.NewGuid()),
            userId ?? new UserId(Guid.NewGuid()),
            new questionaryStatusId(statusId ?? Guid.NewGuid()),
            description,
            isAnonymous,
            DateTime.UtcNow
        );
    }

    public static Consultations CreateConsultation(
        ConsultationsId? id = null,
        UserId? studentId = null,
        UserId? psychologistId = null,
        QuestionaryId? questionnaireId = null,
        DateTime? scheduledTime = null,
        Guid? statusId = null)
    {
        return Consultations.Create(
            id ?? new ConsultationsId(Guid.NewGuid()),
            questionnaireId,
            studentId ?? new UserId(Guid.NewGuid()),
            psychologistId ?? new UserId(Guid.NewGuid()),
            new ConsultationStatusesId(statusId ?? Guid.NewGuid()),
            scheduledTime ?? DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow
        );
    }

    public static Message CreateMessage(
        MessageId? id = null,
        UserId? senderId = null,
        UserId? receiverId = null,
        ConsultationsId? consultationId = null,
        string content = "Тестове повідомлення",
        bool isRead = false)
    {
        return Message.Create(
            id ?? new MessageId(Guid.NewGuid()),
            consultationId ?? new ConsultationsId(Guid.NewGuid()),
            senderId ?? new UserId(Guid.NewGuid()),
            receiverId ?? new UserId(Guid.NewGuid()),
            content,
            isRead,
            DateTime.UtcNow,
            null
        );
    }

    public static MessageAttachment CreateMessageAttachment(
        Guid? id = null,
        Guid? messageId = null,
        string fileUrl = "https://example.com/file.jpg",
        string fileType = "image/jpeg")
    {
        return new MessageAttachment
        {
            Id = id ?? Guid.NewGuid(),
            MessageId = messageId,
            FileUrl = fileUrl,
            FileType = fileType,
            FileSizeBytes = 1024,
            CreatedAt = DateTime.UtcNow
        };
    }
}
