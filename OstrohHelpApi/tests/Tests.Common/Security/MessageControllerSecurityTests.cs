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
using Application.Common.Services;
using Infrastructure.Persistence.Repositories;
using NSubstitute;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
        var mockPreviewService = Substitute.For<Application.Common.Interfaces.Services.IPreviewGenerationService>();
        var mockAuditLogService = Substitute.For<IAuditLogService>();
        var mockLogger = Substitute.For<ILogger<MessageController>>();

        _controller = new MessageController(
            mockMediator,
            mockMessageQuery,
            mockMapper,
            mockUserQuery,
            _accessChecker,
            mockPreviewService,
            mockAuditLogService,
            mockLogger
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
            Id = new Guid("00000000-0000-0000-0000-000000000001"),
            Name = "Student"
        };

        var psychologistRole = new Role
        {
            Id = new Guid("00000000-0000-0000-0000-000000000002"),
            Name = "Psychologist"
        };

        _context.Roles.AddRange(studentRole, psychologistRole);

        // Додати користувачів
        var student = new User
        {
            Id = _studentId,
            RoleId = studentRole.Id,
            FullName = "Test Student",
            Email = "student@test.com",
            GoogleId = "google-student",
            CreatedAt = DateTime.UtcNow
        };

        var psychologist = new User
        {
            Id = _psychologistId,
            RoleId = psychologistRole.Id,
            FullName = "Test Psychologist",
            Email = "psychologist@test.com",
            GoogleId = "google-psychologist",
            CreatedAt = DateTime.UtcNow
        };

        var otherUser = new User
        {
            Id = _otherId,
            RoleId = studentRole.Id,
            FullName = "Other User",
            Email = "other@test.com",
            GoogleId = "google-other",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.AddRange(student, psychologist, otherUser);

        // Додати консультацію
        var consultation = Consultations.Create(
            id: _consultationId,
            questionnaireId: null,
            studentId: _studentId,
            psychologistId: _psychologistId,
            statusId: Guid.NewGuid(),
            scheduledTime: DateTime.UtcNow.AddHours(1),
            createdAt: DateTime.UtcNow
        );

        _context.Consultations.Add(consultation);

        // Додати повідомлення
        var message = Message.Create(
            id: _messageId,
            consultationId: _consultationId,
            senderId: _studentId,
            receiverId: _psychologistId,
            text: "Test message",
            isRead: false,
            sentAt: DateTime.UtcNow,
            deletedAt: null
        );

        _context.Messages.Add(message);

        await _context.SaveChangesAsync();
    }
}
