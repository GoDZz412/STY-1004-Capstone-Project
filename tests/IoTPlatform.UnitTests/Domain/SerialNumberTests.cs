using IoTPlatform.Domain.Exceptions;
using IoTPlatform.Domain.ValueObjects;

namespace IoTPlatform.UnitTests.Domain;

[TestClass]
public class SerialNumberTests
{
    [TestMethod]
    [DataRow("IOT-0042-PX")]
    [DataRow("IOT-0000-AA")]
    [DataRow("IOT-9999-ZZ")]
    public void Create_ValidValue_Succeeds(string input)
    {
        var serial = SerialNumber.Create(input);

        Assert.AreEqual(input, serial.Value);
        Assert.AreEqual(input, serial.ToString());
    }

    [TestMethod]
    public void Create_LowercaseInput_IsNormalizedToUpperCase()
    {
        Assert.AreEqual("IOT-0042-PX", SerialNumber.Create("iot-0042-px").Value);
    }

    [TestMethod]
    public void Create_SurroundingWhitespace_IsTrimmed()
    {
        Assert.AreEqual("IOT-0042-PX", SerialNumber.Create("  IOT-0042-PX\t").Value);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("IOT-123-AB")]      // too few digits
    [DataRow("IOT-12345-AB")]    // too many digits
    [DataRow("IOT-0001-A1")]     // digit in letter part
    [DataRow("IOT-0001-ABC")]    // too many letters
    [DataRow("ABC-0001-AB")]     // wrong prefix
    [DataRow("IOT0001AB")]       // missing dashes
    [DataRow("IOT-\u0660\u0660\u0664\u0662-PX")] // Arabic-Indic digits must not pass
    public void Create_InvalidValue_Throws(string? input)
    {
        Assert.ThrowsExactly<InvalidSerialNumberException>(() => SerialNumber.Create(input));
    }

    [TestMethod]
    public void TryCreate_ValidAndInvalid()
    {
        Assert.IsTrue(SerialNumber.TryCreate("IOT-0042-PX", out var ok));
        Assert.AreEqual("IOT-0042-PX", ok!.Value);

        Assert.IsFalse(SerialNumber.TryCreate("nope", out var bad));
        Assert.IsNull(bad);
    }

    [TestMethod]
    public void Equality_IsByValue_AfterNormalization()
    {
        Assert.AreEqual(SerialNumber.Create("IOT-0042-PX"), SerialNumber.Create(" iot-0042-px "));
        Assert.AreNotEqual(SerialNumber.Create("IOT-0042-PX"), SerialNumber.Create("IOT-0043-PX"));
    }

    [TestMethod]
    public void MaxLength_MatchesMask()
    {
        Assert.AreEqual(SerialNumber.MaxLength, SerialNumber.Create("IOT-0042-PX").Value.Length);
    }
}