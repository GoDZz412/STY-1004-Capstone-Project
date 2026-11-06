using IoTPlatform.Application.DTOs;
using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;

namespace IoTPlatform.UnitTests.Application;

[TestClass]
public class MaintenanceLogMappingsTests
{
    [TestMethod]
    public void ToDto_ValidEntity_CopiesAllFields()
    {
        var log = new MaintenanceLog(
            Guid.NewGuid(),
            MaintenanceType.Repair,
            "Jane Doe",
            "Replaced the pump seal",
            new DateTime(2025, 6, 1, 8, 30, 0, DateTimeKind.Utc));

        var dto = log.ToDto();

        Assert.AreEqual(log.Id, dto.Id);
        Assert.AreEqual(log.DeviceAssetId, dto.DeviceAssetId);
        Assert.AreEqual("Repair", dto.MaintenanceType);
        Assert.AreEqual(log.Technician, dto.Technician);
        Assert.AreEqual(log.Description, dto.Description);
        Assert.AreEqual(log.PerformedOnUtc, dto.PerformedOnUtc);
    }
}
