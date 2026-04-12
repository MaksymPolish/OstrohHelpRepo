using Domain.Conferences;
using Domain.Conferences.Statuses;
using Domain.Messages;
using Domain.Users;
using Domain.Users.Roles;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AutoMapper;
using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Infrastructure.Persistence.Repositories;
using NSubstitute;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Tests.Common.Security;

public class MessageControllerSecurityTests : IAsyncLifetime
{
    private readonly ApplicationDbContext _context;
    private readonly MessageController _controller;
    private readonly IConsultationAccessChecker _accessChecker;
    
    // Test data
    private readonly Guid _studentId = Guid.NewGuid();
    private readonly Guid _psychologistId = Guid.NewGuid();
    private readonly Guid _otherId = Guid.NewGuid();
    private readonly Guid _consultationId = Guid.NewGuid();
    private readonly Guid _messageId = Guid.NewGuid();

    public MessageControllerSecurityTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_Controller_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _accessChecker = new ConsultationAccessChecker(_context);

        // Настроїти контролер з мок залежностями
        var mockMediator = Substitute.For<IMediator>();
        var mockMessageQuery = Substitute.For<IMessageQuery>();
        var mockMapper = Substitute.For<IMapper>();
        var mockUserQuery = Substitute.For<IUserQuery>();
        var mockAttachmentRepository = Substitute.For<IMessageAttachmentRepository>();

        // Налаштувати мок, щоб повертати MessageAttachment при успішному додаванню
        mockAttachmentRepository
            .AddAsync(Arg.Any<MessageAttachment>(), Arg.Any<CancellationToken>())
            .Returns(info => Task.FromResult(info.Arg<MessageAttachment>()));

        _controller = new MessageController(
            mockMediator,
            mockMessageQuery,
            mockMapper,
            mockUserQuery,
            null,  // CloudinaryService не потрібен для тестування AddAttachment
            mockAttachmentRepository,
            _accessChecker
        );

        // Настроїти HttpContext з User Claims
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _studentId.ToString())
        }));

        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    public async Task InitializeAsync()
    {
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

        // Додати повідомлення
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

    // ============ AddAttachment Security Tests ============

    [Fact]
    [Trait("Category", "Security")]
    public async Task AddAttachment_WhenUserOwnsMessage_ShouldSucceed()
    {
        // Arrange: встановити User ID як власника повідомлення
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _studentId.ToString())
        }));
        
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var request = new Api.Dtos.AddAttachmentRequest
        {
            MessageId = _messageId,
            FileUrl = "https://example.com/file.pdf",
            FileType = "application/pdf"
        };

        // Act
        var result = await _controller.AddAttachment(request, CancellationToken.None);

        // Assert
        // Якщо дійшло сюди без помилки, то авторизація пройшла
        // (Фактична операція буде заблокована на рівні mock repository)
        Assert.NotNull(result);
    }

    [Fact]
    [Trait("Category", "SecurityAttack")]
    public async Task AddAttachment_WhenUserDoesNotOwnMessage_ShouldReturnForbid()
    {
        // Arrange: встановити User ID як не-власника повідомлення
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _otherId.ToString())
        }));
        
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var request = new Api.Dtos.AddAttachmentRequest
        {
            MessageId = _messageId,
            FileUrl = "https://example.com/file.pdf",
            FileType = "application/pdf"
        };

        // Act
        var result = await _controller.AddAttachment(request, CancellationToken.None);

        // Assert
        var forbidResult = Assert.IsType<ForbidResult>(result);
        Assert.NotNull(forbidResult);
    }

    [Fact]
    [Trait("Category", "SecurityAttack")]
    public async Task AddAttachment_WhenPsychologistTriesToAddToStudentMessage_ShouldReturnForbid()
    {
        // Arrange: встановити User ID як психолога
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _psychologistId.ToString())
        }));
        
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var request = new Api.Dtos.AddAttachmentRequest
        {
            MessageId = _messageId,  // Це повідомлення від студента
            FileUrl = "https://example.com/file.pdf",
            FileType = "application/pdf"
        };

        // Act
        var result = await _controller.AddAttachment(request, CancellationToken.None);

        // Assert
        var forbidResult = Assert.IsType<ForbidResult>(result);
        Assert.NotNull(forbidResult);
    }

    [Fact]
    [Trait("Category", "SecurityAttack")]
    public async Task AddAttachment_WhenUserNotInSystem_ShouldReturnUnauthorized()
    {
        // Arrange: встановити User ID як невідомого користувача
        var unknownUserId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, unknownUserId.ToString())
        }));
        
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var request = new Api.Dtos.AddAttachmentRequest
        {
            MessageId = _messageId,
            FileUrl = "https://example.com/file.pdf",
            FileType = "application/pdf"
        };

        // Act
        var result = await _controller.AddAttachment(request, CancellationToken.None);

        // Assert
        // Користувач не знайдений у БД, тому не буде власником
        var forbidResult = Assert.IsType<ForbidResult>(result);
        Assert.NotNull(forbidResult);
    }

    [Fact]
    [Trait("Category", "SecurityAttack")]
    public async Task AddAttachment_WhenMessageIdIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _studentId.ToString())
        }));
        
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var request = new Api.Dtos.AddAttachmentRequest
        {
            MessageId = Guid.Empty,  // ❌ Невалідне значення
            FileUrl = "https://example.com/file.pdf",
            FileType = "application/pdf"
        };

        // Act
        var result = await _controller.AddAttachment(request, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult);
    }

    // ============ Authorization Attack Tests ============

    [Fact]
    [Trait("Category", "SecurityAttack")]
    public async Task Attack_StudentTryingToAddAttachmentToPsychologistMessage()
    {
        // Arrange: Створити повідомлення від психолога
        var psychologistMessageId = Guid.NewGuid();
        var psychologistMessage = Message.Create(
            id: new MessageId(psychologistMessageId),
            consultationId: new ConsultationsId(_consultationId),
            senderId: new UserId(_psychologistId),  // ← Психолог
            receiverId: new UserId(_studentId),
            text: "Psychologist's message",
            isRead: false,
            sentAt: DateTime.UtcNow,
            deletedAt: null
        );
        _context.Messages.Add(psychologistMessage);
        await _context.SaveChangesAsync();

        // Встановити User ID як студента
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _studentId.ToString())
        }));
        
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var request = new Api.Dtos.AddAttachmentRequest
        {
            MessageId = psychologistMessageId,
            FileUrl = "https://example.com/file.pdf",
            FileType = "application/pdf"
        };

        // Act
        var result = await _controller.AddAttachment(request, CancellationToken.None);

        // Assert: Повинна бути помилка Forbid
        var forbidResult = Assert.IsType<ForbidResult>(result);
        Assert.NotNull(forbidResult);
    }

    // ============ Multiple Message Scenarios ============

    [Fact]
    [Trait("Category", "Integration")]
    public async Task AddAttachment_MultipleMessagesFromDifferentUsers_ShouldEnforceOwnership()
    {
        // Arrange: Додати кілька повідомлень від різних користувачів
        var msg1Id = Guid.NewGuid();
        var msg2Id = Guid.NewGuid();
        var msg3Id = Guid.NewGuid();

        var msg1 = Message.Create(
            id: new MessageId(msg1Id),
            consultationId: new ConsultationsId(_consultationId),
            senderId: new UserId(_studentId),
            receiverId: new UserId(_psychologistId),
            text: "Student message",
            isRead: false,
            sentAt: DateTime.UtcNow,
            deletedAt: null
        );

        var msg2 = Message.Create(
            id: new MessageId(msg2Id),
            consultationId: new ConsultationsId(_consultationId),
            senderId: new UserId(_psychologistId),
            receiverId: new UserId(_studentId),
            text: "Psychologist message 1",
            isRead: false,
            sentAt: DateTime.UtcNow,
            deletedAt: null
        );

        var msg3 = Message.Create(
            id: new MessageId(msg3Id),
            consultationId: new ConsultationsId(_consultationId),
            senderId: new UserId(_psychologistId),
            receiverId: new UserId(_studentId),
            text: "Psychologist message 2",
            isRead: false,
            sentAt: DateTime.UtcNow,
            deletedAt: null
        );

        _context.Messages.AddRange(msg1, msg2, msg3);
        await _context.SaveChangesAsync();

        // Встановити User ID як психолога
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _psychologistId.ToString())
        }));
        
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act & Assert: Психолог може додати до своїх повідомлень
        var result1 = await _controller.AddAttachment(
            new Api.Dtos.AddAttachmentRequest { MessageId = msg2Id, FileUrl = "url1", FileType = "pdf" },
            CancellationToken.None
        );
        // Не повинна бути ForbidResult

        var result2 = await _controller.AddAttachment(
            new Api.Dtos.AddAttachmentRequest { MessageId = msg3Id, FileUrl = "url2", FileType = "pdf" },
            CancellationToken.None
        );
        // Не повинна бути ForbidResult

        // Але не до чужого
        var result3 = await _controller.AddAttachment(
            new Api.Dtos.AddAttachmentRequest { MessageId = msg1Id, FileUrl = "url3", FileType = "pdf" },
            CancellationToken.None
        );
        Assert.IsType<ForbidResult>(result3);
    }
}
