using Gems.TechSupport.Application.Abstractions.Masking;
using Gems.TechSupport.Application.Responses;
using Gems.TechSupport.Application.Responses.Models;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Gems.TechSupport.Application.Test.MaskingTests;

[TestFixture]
public class ResponseToDomainMapperTests
{
	private Mock<IMasker> masker = null!;
    private CancellationToken ct;
    private ResponseToDomainMapper _sut = null!;

    [SetUp]
	public void SetUp()
	{
		masker = new Mock<IMasker>();
		ct = CancellationToken.None;
		_sut = new ResponseToDomainMapper(masker.Object);
	}

	[Test]
	public async Task Handle_WhenToContactDomain_ShouldAnonymize()
	{
		// arrange
        var testContactResponse = new ContactResponse (1, "Иван Иванов" ); 
		masker.Setup(x => x.MaskFullName("Иван Иванов")).Returns("Иван И.");
		// act
		var contact = _sut.ToDomain(testContactResponse);

		// assert

		ClassicAssert.IsNotNull(contact);

		Assert.That(contact.FullName, Is.EqualTo("Иван И."));

	}
    [Test]
    public async Task Handle_WhenToAssigneeDomain_ShouldAnonymize()
    {
        // arrange
        var testAssigneeResponse = new AssigneeResponse(1, "Иван Иванов");
        masker.Setup(x => x.MaskFullName("Иван Иванов")).Returns("Иван И.");
        // act
        var assignee = _sut.ToDomain(testAssigneeResponse);

        // assert

        ClassicAssert.IsNotNull(assignee);

        Assert.That(assignee.FullName, Is.EqualTo("Иван И."));

    }
    [Test]
    public async Task Handle_WhenToCommentDomain_ShouldAnonymize()
    {
        var testCommentAuthorResponse = new CommentAuthorResponse(1, "Иван Иванов", "Contact");
        // arrange
        var testCommentResponse = new CommentResponse
        {
            Id = 1,
            Content = "Content",
            Public = true,
            PublishedAt = DateTime.UtcNow,
            Author = testCommentAuthorResponse
        };
        masker.Setup(x => x.MaskFullName("Иван Иванов")).Returns("Иван И.");
        // act
        var comment = _sut.ToDomain(testCommentResponse, 11);

        // assert

        ClassicAssert.IsNotNull(comment);

        Assert.That(comment.Contact.FullName, Is.EqualTo("Иван И."));

    }
}
