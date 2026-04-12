using Domain.Conferences;
using Domain.Conferences.Statuses;
using Domain.Messages;
using Domain.Users;
using Domain.Users.Roles;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;

namespace Tests.Common.Security;

public class ConsultationAccessCheckerTests : IAsyncLifetime
{
    private readonly ApplicationDbContext _context;
    private readonly ConsultationAccessChecker _accessChecker;
    
    // Test data
    private readonly Guid _studentId = Guid.NewGuid();
    private readonly Guid _psychologistId = Guid.NewGuid();
    private readonly Guid _otherId = Guid.NewGuid();
    private readonly Guid _consultationId = Guid.NewGuid();
    private readonly Guid _messageId = Guid.NewGuid();

    public ConsultationAccessCheckerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _accessChecker = new ConsultationAccessChecker(_context);
    }

    public async Task InitializeAsync()
    {
        // Створити тестові дані
        await SetupTestData();
    }

    public Task DisposeAsync()
    {
        _context?.Dispose();
        return Task.CompletedTask;
    }

    private async Task SetupTestData()
    {
        // Створити ролі
        var studentRole = new Role
        {
            Id = new RoleId(new Guid("00000000-0000-0000-0000-000000000001")),
            Name = "Student"
        };

        var psychologistRole = new Role
        {
            Id = new RoleId(new Guid("00000000-0000-0000-0000-000000000002")),
            Name = "Psychologist"
        };

        _context.Roles.AddRange(studentRole, psychologistRole);

        // Додати користувачів
        var student = new User
        {
            Id = new UserId(_studentId),
            RoleId = studentRole.Id,
            FullName = "Test Student",
            Email = "student@test.com",
            GoogleId = "google-student",
            CreatedAt = DateTime.UtcNow
        };

        var psychologist = new User
        {
            Id = new UserId(_psychologistId),
            RoleId = psychologistRole.Id,
            FullName = "Test Psychologist",
            Email = "psychologist@test.com",
            GoogleId = "google-psychologist",
            CreatedAt = DateTime.UtcNow
        };

        var otherUser = new User
        {
            Id = new UserId(_otherId),
            RoleId = studentRole.Id,
            FullName = "Other User",
            Email = "other@test.com",
            GoogleId = "google-other",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.AddRange(student, psychologist, otherUser);

        // Додати консультацію
        var consultation = Consultations.Create(
            id: new ConsultationsId(_consultationId),
            questionnaireId: null,
            studentId: new UserId(_studentId),
            psychologistId: new UserId(_psychologistId),
            statusId: new ConsultationStatusesId(Guid.NewGuid()),
            scheduledTime: DateTime.UtcNow.AddHours(1),
            createdAt: DateTime.UtcNow
        );

        _context.Consultations.Add(consultation);

        // Додати повідомлення (від студента)
        var message = Message.Create(
            id: new MessageId(_messageId),
            consultationId: new ConsultationsId(_consultationId),
            senderId: new UserId(_studentId),
            receiverId: new UserId(_psychologistId),
            text: "Test message",
            isRead: false,
            sentAt: DateTime.UtcNow,
            deletedAt: null
        );

        _context.Messages.Add(message);

        await _context.SaveChangesAsync();
    }

    // ============ IsConsultationMember Tests ============

    [Fact]
    public async Task IsConsultationMember_WhenUserIsStudent_ShouldReturnTrue()
    {
        // Act
        var result = await _accessChecker.IsConsultationMember(_studentId, _consultationId, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsConsultationMember_WhenUserIsPsychologist_ShouldReturnTrue()
    {
        // Act
        var result = await _accessChecker.IsConsultationMember(_psychologistId, _consultationId, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsConsultationMember_WhenUserIsNotMember_ShouldReturnFalse()
    {
        // Act
        var result = await _accessChecker.IsConsultationMember(_otherId, _consultationId, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsConsultationMember_WhenConsultationNotFound_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentConsultationId = Guid.NewGuid();

        // Act
        var result = await _accessChecker.IsConsultationMember(_studentId, nonExistentConsultationId, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    // ============ IsMessageOwner Tests ============

    [Fact]
    public async Task IsMessageOwner_WhenUserIsSender_ShouldReturnTrue()
    {
        // Act
        var result = await _accessChecker.IsMessageOwner(_studentId, _messageId, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsMessageOwner_WhenUserIsNotSender_ShouldReturnFalse()
    {
        // Act
        var result = await _accessChecker.IsMessageOwner(_psychologistId, _messageId, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsMessageOwner_WhenUserIsNotRelated_ShouldReturnFalse()
    {
        // Act
        var result = await _accessChecker.IsMessageOwner(_otherId, _messageId, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsMessageOwner_WhenMessageNotFound_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentMessageId = Guid.NewGuid();

        // Act
        var result = await _accessChecker.IsMessageOwner(_studentId, nonExistentMessageId, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    // ============ GetUserRoleInConsultation Tests ============

    [Fact]
    public async Task GetUserRoleInConsultation_WhenUserIsStudent_ShouldReturnStudent()
    {
        // Act
        var result = await _accessChecker.GetUserRoleInConsultation(_studentId, _consultationId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ConsultationRole.Student, result);
    }

    [Fact]
    public async Task GetUserRoleInConsultation_WhenUserIsPsychologist_ShouldReturnPsychologist()
    {
        // Act
        var result = await _accessChecker.GetUserRoleInConsultation(_psychologistId, _consultationId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ConsultationRole.Psychologist, result);
    }

    [Fact]
    public async Task GetUserRoleInConsultation_WhenUserIsNotMember_ShouldReturnNull()
    {
        // Act
        var result = await _accessChecker.GetUserRoleInConsultation(_otherId, _consultationId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserRoleInConsultation_WhenConsultationNotFound_ShouldReturnNull()
    {
        // Arrange
        var nonExistentConsultationId = Guid.NewGuid();

        // Act
        var result = await _accessChecker.GetUserRoleInConsultation(_studentId, nonExistentConsultationId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    // ============ CanViewConsultationMessages Tests ============

    [Fact]
    public async Task CanViewConsultationMessages_WhenUserIsStudent_ShouldReturnTrue()
    {
        // Act
        var result = await _accessChecker.CanViewConsultationMessages(_studentId, _consultationId, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanViewConsultationMessages_WhenUserIsPsychologist_ShouldReturnTrue()
    {
        // Act
        var result = await _accessChecker.CanViewConsultationMessages(_psychologistId, _consultationId, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanViewConsultationMessages_WhenUserIsNotMember_ShouldReturnFalse()
    {
        // Act
        var result = await _accessChecker.CanViewConsultationMessages(_otherId, _consultationId, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    // ============ GetConsultationIdForMessage Tests ============

    [Fact]
    public async Task GetConsultationIdForMessage_WhenMessageExists_ShouldReturnConsultationId()
    {
        // Act
        var result = await _accessChecker.GetConsultationIdForMessage(_messageId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_consultationId, result);
    }

    [Fact]
    public async Task GetConsultationIdForMessage_WhenMessageNotFound_ShouldReturnNull()
    {
        // Arrange
        var nonExistentMessageId = Guid.NewGuid();

        // Act
        var result = await _accessChecker.GetConsultationIdForMessage(nonExistentMessageId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    // ============ Security Attack Scenarios ============

    [Fact]
    [Trait("Category", "SecurityAttack")]
    public async Task Security_StudentCannotAccessPsychologistConsultation()
    {
        // Arrange: Створити консультацію між іншим психологом та студентом
        var otherPsychologistId = Guid.NewGuid();
        var otherConsultationId = Guid.NewGuid();

        var otherConsultation = Consultations.Create(
            id: new ConsultationsId(otherConsultationId),
            questionnaireId: null,
            studentId: new UserId(_otherId),
            psychologistId: new UserId(otherPsychologistId),
            statusId: new ConsultationStatusesId(Guid.NewGuid()),
            scheduledTime: DateTime.UtcNow.AddHours(1),
            createdAt: DateTime.UtcNow
        );

        _context.Consultations.Add(otherConsultation);
        await _context.SaveChangesAsync();

        // Act: Наш студент намагається доступитися до чужої консультації
        var canAccess = await _accessChecker.IsConsultationMember(_studentId, otherConsultationId, CancellationToken.None);

        // Assert: Повинен бути заборонено
        Assert.False(canAccess);
    }

    [Fact]
    [Trait("Category", "SecurityAttack")]
    public async Task Security_UserCannotDeleteOthersMessage()
    {
        // Arrange: Створити повідомлення від іншого користувача
        var otherSenderId = Guid.NewGuid();
        var otherMessageId = Guid.NewGuid();
        var studentRoleId = new RoleId(new Guid("00000000-0000-0000-0000-000000000001"));

        // Додати іншого користувача
        var otherSender = new User
        {
            Id = new UserId(otherSenderId),
            RoleId = studentRoleId,
            FullName = "Other Sender",
            Email = "othersender@test.com",
            GoogleId = "google-othersender",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(otherSender);

        var otherMessage = Message.Create(
            id: new MessageId(otherMessageId),
            consultationId: new ConsultationsId(_consultationId),
            senderId: new UserId(otherSenderId),
            receiverId: new UserId(_psychologistId),
            text: "Other user's message",
            isRead: false,
            sentAt: DateTime.UtcNow,
            deletedAt: null
        );

        _context.Messages.Add(otherMessage);
        await _context.SaveChangesAsync();

        // Act: Психолог намагається видалити повідомлення студента
        var isOwner = await _accessChecker.IsMessageOwner(_psychologistId, otherMessageId, CancellationToken.None);

        // Assert: Не повинен бути власником
        Assert.False(isOwner);
    }

    [Fact]
    [Trait("Category", "SecurityAttack")]
    public async Task Security_NonMemberCannotViewMessages()
    {
        // Act: Невідомий користувач намагається читати повідомлення
        var canView = await _accessChecker.CanViewConsultationMessages(_otherId, _consultationId, CancellationToken.None);

        // Assert: Повинен бути заборонено
        Assert.False(canView);
    }

    // ============ Edge Cases ============

    [Fact]
    [Trait("Category", "EdgeCase")]
    public async Task EdgeCase_EmptyGuidUser_ShouldReturnFalse()
    {
        // Act
        var result = await _accessChecker.IsConsultationMember(Guid.Empty, _consultationId, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public async Task EdgeCase_EmptyGuidConsultation_ShouldReturnFalse()
    {
        // Act
        var result = await _accessChecker.IsConsultationMember(_studentId, Guid.Empty, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public async Task EdgeCase_ConcurrentRequests_ShouldBeThreadSafe()
    {
        // Act: Запустити кілька паралельних запитів
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => _accessChecker.IsConsultationMember(_studentId, _consultationId, CancellationToken.None))
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert: Всі повинні повернути true
        Assert.All(results, result => Assert.True(result));
    }
}
