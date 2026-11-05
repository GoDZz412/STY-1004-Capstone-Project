using IoTPlatform.Domain.Common;

namespace IoTPlatform.UnitTests.Domain;

[TestClass]
public class EntityTests
{
    private sealed class TestEntityA : Entity
    {
        public TestEntityA(Guid id) : base(id) { }
    }

    private sealed class TestEntityB : Entity
    {
        public TestEntityB(Guid id) : base(id) { }
    }

    [TestMethod]
    public void Equals_SameIdSameType_ReturnsTrue()
    {
        var id = Guid.NewGuid();

        Assert.IsTrue(new TestEntityA(id).Equals(new TestEntityA(id)));
    }

    [TestMethod]
    public void Equals_DifferentId_ReturnsFalse()
    {
        Assert.IsFalse(new TestEntityA(Guid.NewGuid()).Equals(new TestEntityA(Guid.NewGuid())));
    }

    [TestMethod]
    public void Equals_SameIdDifferentType_ReturnsFalse()
    {
        var id = Guid.NewGuid();

        Assert.IsFalse(new TestEntityA(id).Equals(new TestEntityB(id)));
    }

    [TestMethod]
    public void EqualityOperator_SameIdSameType_ReturnsTrue()
    {
        var id = Guid.NewGuid();

        Assert.IsTrue(new TestEntityA(id) == new TestEntityA(id));
    }

    [TestMethod]
    public void InequalityOperator_DifferentId_ReturnsTrue()
    {
        Assert.IsTrue(new TestEntityA(Guid.NewGuid()) != new TestEntityA(Guid.NewGuid()));
    }

    [TestMethod]
    public void GetHashCode_EqualEntities_ReturnsSameHashCode()
    {
        var id = Guid.NewGuid();

        Assert.AreEqual(new TestEntityA(id).GetHashCode(), new TestEntityA(id).GetHashCode());
    }

    [TestMethod]
    public void Equals_Null_ReturnsFalse()
    {
        Assert.IsFalse(new TestEntityA(Guid.NewGuid()).Equals(null));
    }
}
