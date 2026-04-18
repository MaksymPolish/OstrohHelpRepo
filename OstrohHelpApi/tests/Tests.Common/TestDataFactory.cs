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
            Id = id ?? Guid.NewGuid(),
            Name = name
        };
    }

    public static User CreateUser(
        string email = "test@example.com",
        string fullName = "Тестовий Користувач",
        Guid? roleId = null,
        Guid? id = null,
        string photoUrl = "https://example.com/photo.jpg")
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Email = email,
            FullName = fullName,
            PhotoUrl = photoUrl,
            RoleId = roleId ?? Guid.NewGuid(),
            GoogleId = Guid.NewGuid().ToString()
        };
    }

    public static Questionary CreateQuestionnaire(
        Guid? userId = null,
        Guid? id = null,
        string description = "Тестовий опис проблеми",
        Guid? statusId = null,
        bool isAnonymous = false)
    {
        return Questionary.Create(
            id ?? Guid.NewGuid(),
            userId ?? Guid.NewGuid(),
            statusId ?? Guid.NewGuid(),
            description,
            isAnonymous,
            DateTime.UtcNow
        );
    }

    public static Consultations CreateConsultation(
        Guid? id = null,
        Guid? studentId = null,
        Guid? psychologistId = null,
        Guid? questionnaireId = null,
        DateTime? scheduledTime = null,
        Guid? statusId = null)
    {
        return Consultations.Create(
            id ?? Guid.NewGuid(),
            questionnaireId,
            studentId ?? Guid.NewGuid(),
            psychologistId ?? Guid.NewGuid(),
            statusId ?? Guid.NewGuid(),
            scheduledTime ?? DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow
        );
    }

    public static Message CreateMessage(
        Guid? id = null,
        Guid? senderId = null,
        Guid? receiverId = null,
        Guid? consultationId = null,
        string content = "Тестове повідомлення",
        bool isRead = false)
    {
        return Message.Create(
            id ?? Guid.NewGuid(),
            consultationId ?? Guid.NewGuid(),
            senderId ?? Guid.NewGuid(),
            receiverId ?? Guid.NewGuid(),
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
