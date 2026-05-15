using Gems.TechSupport.Application.Abstractions.Telegram;
using Gems.TechSupport.Application.Test.HelperMethods;
using Gems.TechSupport.Infrastructure.Services.Telegram;
using Moq;
using NUnit.Framework;
using Telegram.Bot;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Requests;
namespace Gems.TechSupport.Application.Test.TelegramTests;

[TestFixture]
public class TelegramSendIssueNewCommentNotificationAsyncTests
{
    private Mock<ITelegramClientProvider> botClientProvider = null!;
    private Mock<ITelegramBotClient> botClient = null!;
    private string? message = null;
    private CancellationToken ct;

    private int MAX_MESSAGE_LENGTH = 4096;
    private string _ellipses = "...";
    private static readonly string[] AllowedTags =
    {
        "b", "strong", "i", "em", "u", "ins", "s", "strike", "del", "code", "pre", "a", "span"
    };
    private ITelegramService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        botClientProvider = new Mock<ITelegramClientProvider>();
        botClient = new Mock<ITelegramBotClient>();
        botClient
        .Setup(x => x.SendRequest<Message>(
        It.IsAny<IRequest<Message>>(),
        It.IsAny<CancellationToken>()))
        .Callback<IRequest<Message>, CancellationToken>((req, ct) =>
        {
            if (req is SendMessageRequest sendMessageRequest)
            {
                message = sendMessageRequest.Text;
            }
        })
        .ReturnsAsync(new Message());

        botClientProvider.Setup(x => x.Client).Returns(botClient.Object);


        _sut = CreateSut(TelegramOptionsSetup.DefaultOptions());

        ct  = CancellationToken.None;
    }


    [Test]
    [TestCase(5, "Issue [id] from [contact]: [comment]", "Issue 1 from name: *****...")]
    [TestCase(20, "Issue [id] from [contact]: [comment]", "Issue 1 from name: **********")]
    public async Task GivenCommentLengthExceedsMaxCommentLength_ShouldTruncateComment_AndSendMessage(int maxCommentLength, string template, string expected)
    {
        var options = TelegramOptionsSetup.DefaultOptions(
            maxCommentLength: maxCommentLength,
            issueCommentCreatedMessageTemplate: template);

        _sut = CreateSut(options);

        long issueId = 1;
        long assigneeId = 1;
        string contactFullName = "name";
        string commentContent = new string('*', 10);

        await _sut.SendIssueNewCommentNotificationAsync(issueId, assigneeId, contactFullName, commentContent, ct);

        Assert.That(message, Is.Not.Null);
        botClient.Verify(x => x.SendRequest<Message>(
            It.IsAny<IRequest<Message>>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.That(message, Is.EqualTo(expected));
    }

    [Test]
    public async Task GivenCommentLengthExceedsMaxMessageLength_ShouldTruncateComment_AndSendMessage()
    {
        long issueId = 1;
        long assigneeId = 1;
        string contactFullName = "name";
        string commentContent = new string('*', MAX_MESSAGE_LENGTH + 100);

        await _sut.SendIssueNewCommentNotificationAsync(issueId, assigneeId, contactFullName, commentContent, ct);

        Assert.That(message, Is.Not.Null);
        botClient.Verify(x => x.SendRequest<Message>(
            It.IsAny<IRequest<Message>>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.That(message.Length, Is.LessThanOrEqualTo(MAX_MESSAGE_LENGTH));
        Assert.That(message.EndsWith(_ellipses), Is.True);
    }

    [Test]
    public async Task GivenTemplateLengthExceedsMaxMessageLength_ShouldThrowException()
    {
        var template = new string('*', MAX_MESSAGE_LENGTH + 100);
        var options = TelegramOptionsSetup.DefaultOptions(
            issueCommentCreatedMessageTemplate: template);

        _sut = CreateSut(options);

        long issueId = 1;
        long assigneeId = 1;
        string contactFullName = "name";
        string commentContent = new string('*', 10);


        Assert.Throws<InvalidOperationException>(() => _sut.SendIssueNewCommentNotificationAsync(issueId, assigneeId, contactFullName, commentContent, ct));

        botClient.Verify(x => x.SendRequest<Message>(
            It.IsAny<IRequest<Message>>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]


    public async Task GivenNegativeCommentTemplate_ShouldThrowException(int maxCommentLength)
    {
        var options = TelegramOptionsSetup.DefaultOptions(
            maxCommentLength:maxCommentLength
            );

        _sut = CreateSut(options);

        long issueId = 1;
        long assigneeId = 1;
        string contactFullName = "name";
        string commentContent = new string('*', 10);


        Assert.Throws<InvalidOperationException>(() => _sut.SendIssueNewCommentNotificationAsync(issueId, assigneeId, contactFullName, commentContent, ct));

        botClient.Verify(x => x.SendRequest<Message>(
            It.IsAny<IRequest<Message>>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    [TestCaseSource(nameof(AllowedTags))]
    public async Task GivenAllowedTags_ShouldKeepThem_AndSendMessage(string tag)
    {
        long issueId = 1;
        long assigneeId = 1;
        string contactFullName = "name";
        string commentContent = $"<{tag}> message content </{tag}>";

        await _sut.SendIssueNewCommentNotificationAsync(issueId, assigneeId, contactFullName, commentContent, ct);

        Assert.That(message, Is.Not.Null);
        botClient.Verify(x => x.SendRequest<Message>(
            It.IsAny<IRequest<Message>>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.That(message.Contains(commentContent), Is.True);
    }
    [TestCase("br")]
    [Test]
    public async Task GivenNotAllowedTags_ShouldNotKeepThem_AndSendMessage_AndKeepMessageContent(string tag)
    {
        long issueId = 1;
        long assigneeId = 1;
        string contactFullName = "name";
        string commentContent = $"<{tag}> message content </{tag}>";
        Console.WriteLine(commentContent);

        await _sut.SendIssueNewCommentNotificationAsync(issueId, assigneeId, contactFullName, commentContent, ct);

        Assert.That(message, Is.Not.Null);

        botClient.Verify(x => x.SendRequest<Message>(
            It.IsAny<IRequest<Message>>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
        Console.WriteLine(message);
        Assert.That(message.Contains($"<{tag}>"), Is.False);
        Assert.That(message.Contains($"</{tag}>"), Is.False);
        Assert.That(message.Contains("message content"), Is.True);
    }

    private TelegramService CreateSut(TelegramOptions options)
    {
        var monitor = TelegramOptionsSetup.CreateOptionsMonitor(options);
        return new TelegramService(botClientProvider.Object, monitor.Object);
    }
}

