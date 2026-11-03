using IoTPlatform.Domain.Exceptions;
using IoTPlatform.Domain.ValueObjects;

namespace IoTPlatform.UnitTests.Domain;

[TestClass]
public class OperatingRangeTests
{
    [TestMethod]
    public void Create_ValidRange_Succeeds()
    {
        var range = new OperatingRange(10m, 90m, " °C ");

        Assert.AreEqual(10m, range.Min);
        Assert.AreEqual(90m, range.Max);
        Assert.AreEqual("°C", range.Unit);
    }

    [TestMethod]
    public void Create_MinOfZero_IsAllowed()
    {
        Assert.AreEqual(0m, new OperatingRange(0m, 1m, "bar").Min);
    }

    [TestMethod]
    public void Create_NegativeMin_Throws()
    {
        Assert.ThrowsExactly<InvalidOperatingRangeException>(() => new OperatingRange(-0.01m, 10m, "bar"));
    }

    [TestMethod]
    public void Create_MaxEqualToMin_Throws()
    {
        Assert.ThrowsExactly<InvalidOperatingRangeException>(() => new OperatingRange(5m, 5m, "bar"));
    }

    [TestMethod]
    public void Create_MaxLessThanMin_Throws()
    {
        Assert.ThrowsExactly<InvalidOperatingRangeException>(() => new OperatingRange(10m, 5m, "bar"));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Create_EmptyUnit_Throws(string? unit)
    {
        Assert.ThrowsExactly<InvalidOperatingRangeException>(() => new OperatingRange(0m, 10m, unit!));
    }

    [TestMethod]
    public void Create_UnitLongerThanMax_Throws()
    {
        var tooLong = new string('x', OperatingRange.UnitMaxLength + 1);
        Assert.ThrowsExactly<InvalidOperatingRangeException>(() => new OperatingRange(0m, 10m, tooLong));
    }

    [TestMethod]
    public void Create_UnitAtMaxLength_IsAllowed()
    {
        var unit = new string('x', OperatingRange.UnitMaxLength);
        Assert.AreEqual(unit, new OperatingRange(0m, 10m, unit).Unit);
    }

    [TestMethod]
    [DataRow(10, true)]    // lower boundary, inclusive
    [DataRow(90, true)]    // upper boundary, inclusive
    [DataRow(50, true)]
    [DataRow(9.99, false)]
    [DataRow(90.01, false)]
    [DataRow(-5, false)]
    public void Contains_IsInclusiveOnBothEnds(double value, bool expected)
    {
        var range = new OperatingRange(10m, 90m, "°C");

        Assert.AreEqual(expected, range.Contains((decimal)value));
    }

    [TestMethod]
    public void Equality_IsByValue()
    {
        Assert.AreEqual(new OperatingRange(0m, 10m, "bar"), new OperatingRange(0m, 10m, "bar"));
        Assert.AreNotEqual(new OperatingRange(0m, 10m, "bar"), new OperatingRange(0m, 11m, "bar"));
    }
}