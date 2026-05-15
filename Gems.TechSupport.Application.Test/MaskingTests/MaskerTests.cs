using Gems.TechSupport.Application.Abstractions.Masking;
using Gems.TechSupport.Application.Masking;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Gems.TechSupport.Application.Test.MaskingTests;
[TestFixture]
public class MaskerTests
{
    private Mock<IOptionsMonitor<MaskingOptions>> maskingOptions = null!;
    private IReadOnlyCollection<string> keywords = null!;
    private IMasker _sut = null!;

    [SetUp]
    public void SetUp()
    {
        keywords = new List<string> {
              "Администрация",
              "Город",
              "Район",
              "Отдел",
              "Управление"
        };

        maskingOptions = new Mock<IOptionsMonitor<MaskingOptions>>();
        maskingOptions.Setup(x => x.CurrentValue).Returns(
            new MaskingOptions { Keywords = keywords }
            );
        _sut = new Masker(maskingOptions.Object);
    }
    [Test]
    [TestCase("Иванов Иван", "И. Иван")]
    [TestCase("Ю Лань", "Ю Лань")]
    [TestCase("Лань Ю", "Л. Ю")]

    public void GivenNameAndLastName_ShouldReturnExpectedValue(string name, string expected)
    {
        string result = _sut.MaskFullName(name);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(expected));
    }
    [Test]
    public void GivenNull_ShouldReturnNull()
    {
        string name = null;
        string result = _sut.MaskFullName(name);

        Assert.That(result, Is.Null);
    }
    [Test]
    [TestCase("", "")]
    [TestCase(" ", " ")]
    [TestCase("   ", "   ")]
    [TestCase("\n", "\n")]
    [TestCase("\t", "\t")]
    public void GivenEmptyString_ShouldReturnExpectedValue(string name, string expected)
    {
        string result = _sut.MaskFullName(name);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("Иван", "Иван")]
    [TestCase("Ю", "Ю")]

    public void GivenOneWordName_ShouldReturnExpectedValue(string name, string expected)
    {
        string result = _sut.MaskFullName(name);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("Администрация города Москва", "Администрация города Москва")]
    [TestCase("Западный район", "Западный район")]
    [TestCase("Отдел по управлению кадрами", "Отдел по управлению кадрами")]

    public void GivenNameWithKeyword_ShouldReturnExpectedValue(string name, string expected)
    {
        string result = _sut.MaskFullName(name);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(expected));
    }
    [Test]
    [TestCase("youremail@gmail.com", "youremail@*****.com")]
    [TestCase("youremail@yandex.ru", "youremail@*****.ru")]
    [TestCase("youremail@gmail.com youremail@gmail.com", "youremail@*****.com youremail@*****.com")]
    public void GivenNameWithEmail_ShouldAnonymize(string name, string expected)
    {
        string result = _sut.MaskFullName(name);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(expected));
    }
}