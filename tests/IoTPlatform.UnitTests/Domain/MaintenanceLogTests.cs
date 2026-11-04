using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;
using IoTPlatform.Domain.Exceptions;

namespace IoTPlatform.UnitTests.Domain;

[TestClass]
public class MaintenanceLogTests
{
    private static MaintenanceLog CreateLog(
        Guid? deviceAssetId = null,
        MaintenanceType maintenanceType = MaintenanceType.Inspection,
        string technician = "Jane Doe",
        string description = "Quarterly pump inspection",
        DateTime? performedOnUtc = null)
    {
        return new MaintenanceLog(
            deviceAssetId ?? Guid.NewGuid(),
            maintenanceType,
            technician,
            description,
            performedOnUtc ?? DateTime.UtcNow.AddDays(-1));
    }

    [TestMethod]
    public void Create_ValidLog_SetsAllFields()
    {
        var deviceAssetId = Guid.NewGuid();
        var performedOnUtc = DateTime.UtcNow.AddDays(-2);

        var log = CreateLog(
            deviceAssetId,
            MaintenanceType.Repair,
            "Jane Doe",
            "Replaced the impeller seal",
            performedOnUtc);

        Assert.AreNotEqual(Guid.Empty, log.Id);
        Assert.AreEqual(deviceAssetId, log.DeviceAssetId);
        Assert.AreEqual(MaintenanceType.Repair, log.MaintenanceType);
        Assert.AreEqual("Jane Doe", log.Technician);
        Assert.AreEqual("Replaced the impeller seal", log.Description);
        Assert.AreEqual(performedOnUtc, log.PerformedOnUtc);
    }

    [TestMethod]
    public void Create_TextWithSurroundingSpaces_IsTrimmed()
    {
        var log = CreateLog(
            technician: "  Jane Doe  ",
            description: "  Quarterly pump inspection  ");

        Assert.AreEqual("Jane Doe", log.Technician);
        Assert.AreEqual("Quarterly pump inspection", log.Description);
    }

    [TestMethod]
    public void Create_TwoLogs_HaveDifferentIds()
    {
        var first = CreateLog();
        var second = CreateLog();

        Assert.AreNotEqual(first.Id, second.Id);
    }

    [TestMethod]
    public void Create_EmptyDeviceId_Throws()
    {
        Assert.ThrowsExactly<DomainValidationException>(() =>
            CreateLog(deviceAssetId: Guid.Empty));
    }

    [TestMethod]
    public void Create_UndefinedMaintenanceType_Throws()
    {
        Assert.ThrowsExactly<DomainValidationException>(() =>
            CreateLog(maintenanceType: (MaintenanceType)999));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Create_InvalidTechnician_Throws(string? technician)
    {
        Assert.ThrowsExactly<DomainValidationException>(() =>
            CreateLog(technician: technician!));
    }

    [TestMethod]
    public void Create_TechnicianLongerThanMax_Throws()
    {
        var technician = new string('a', MaintenanceLog.TechnicianMaxLength + 1);

        Assert.ThrowsExactly<DomainValidationException>(() =>
            CreateLog(technician: technician));
    }

    [TestMethod]
    public void Create_TechnicianAtMaxLength_Succeeds()
    {
        var technician = new string('a', MaintenanceLog.TechnicianMaxLength);

        var log = CreateLog(technician: technician);

        Assert.AreEqual(MaintenanceLog.TechnicianMaxLength, log.Technician.Length);
    }

    [TestMethod]
    public void Create_TechnicianAtMaxLengthWithSurroundingSpaces_Succeeds()
    {
        var technician = "  " + new string('a', MaintenanceLog.TechnicianMaxLength) + "  ";

        var log = CreateLog(technician: technician);

        Assert.AreEqual(MaintenanceLog.TechnicianMaxLength, log.Technician.Length);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Create_InvalidDescription_Throws(string? description)
    {
        Assert.ThrowsExactly<DomainValidationException>(() =>
            CreateLog(description: description!));
    }

    [TestMethod]
    public void Create_DescriptionLongerThanMax_Throws()
    {
        var description = new string('a', MaintenanceLog.DescriptionMaxLength + 1);

        Assert.ThrowsExactly<DomainValidationException>(() =>
            CreateLog(description: description));
    }

    [TestMethod]
    public void Create_DescriptionAtMaxLength_Succeeds()
    {
        var description = new string('a', MaintenanceLog.DescriptionMaxLength);

        var log = CreateLog(description: description);

        Assert.AreEqual(MaintenanceLog.DescriptionMaxLength, log.Description.Length);
    }

    [TestMethod]
    public void Create_PerformedInFuture_Throws()
    {
        Assert.ThrowsExactly<DomainValidationException>(() =>
            CreateLog(performedOnUtc: DateTime.UtcNow.AddMinutes(1)));
    }

    [TestMethod]
    public void Create_PerformedNow_Succeeds()
    {
        var performedOnUtc = DateTime.UtcNow;

        var log = CreateLog(performedOnUtc: performedOnUtc);

        Assert.AreEqual(performedOnUtc, log.PerformedOnUtc);
    }
}
