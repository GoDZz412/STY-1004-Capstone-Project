using IoTPlatform.Application.Abstractions.Repositories;
using IoTPlatform.Application.Features.Alerts;
using IoTPlatform.Domain.Entities;
using IoTPlatform.Domain.Enums;
using IoTPlatform.Domain.Exceptions;

namespace IoTPlatform.UnitTests.Application;

[TestClass]
public class AddAlertRuleTests
{
    [TestMethod]
    public async Task ExecuteAsync_ValidInput_AddsAndSavesAlertRule()
    {
        var repository = new FakeAlertRuleRepository();
        var unitOfWork = new FakeUnitOfWork();
        var useCase = new AddAlertRule(repository, unitOfWork);

        var deviceId = Guid.NewGuid();

        var result = await useCase.ExecuteAsync(
            deviceId,
            "High Temperature Alert",
            MetricType.Temperature,
            ComparisonOperator.GreaterThan,
            40m,
            AlertSeverity.Warning);

        Assert.IsNotNull(result);
        Assert.AreEqual(deviceId, result.DeviceAssetId);
        Assert.AreEqual("High Temperature Alert", result.Name);
        Assert.AreEqual(MetricType.Temperature, result.MetricType);
        Assert.AreEqual(40m, result.Threshold);
        Assert.AreEqual(AlertSeverity.Warning, result.Severity);
        Assert.IsTrue(result.IsEnabled);

        Assert.AreSame(result, repository.AddedAlertRule);
        Assert.AreEqual(1, repository.AddCallCount);
        Assert.AreEqual(1, unitOfWork.SaveCallCount);
    }

    [TestMethod]
    public async Task ExecuteAsync_InvalidDeviceId_ThrowsAndDoesNotSave()
    {
        var repository = new FakeAlertRuleRepository();
        var unitOfWork = new FakeUnitOfWork();
        var useCase = new AddAlertRule(repository, unitOfWork);

        await Assert.ThrowsExactlyAsync<DomainValidationException>(
            async () => await useCase.ExecuteAsync(
                Guid.Empty,
                "High Temperature Alert",
                MetricType.Temperature,
                ComparisonOperator.GreaterThan,
                40m,
                AlertSeverity.Warning));

        Assert.AreEqual(0, repository.AddCallCount);
        Assert.AreEqual(0, unitOfWork.SaveCallCount);
    }

    [TestMethod]
    public async Task ExecuteAsync_InvalidName_ThrowsAndDoesNotSave()
    {
        var repository = new FakeAlertRuleRepository();
        var unitOfWork = new FakeUnitOfWork();
        var useCase = new AddAlertRule(repository, unitOfWork);

        await Assert.ThrowsExactlyAsync<DomainValidationException>(
            async () => await useCase.ExecuteAsync(
                Guid.NewGuid(),
                " ",
                MetricType.Temperature,
                ComparisonOperator.GreaterThan,
                40m,
                AlertSeverity.Warning));

        Assert.AreEqual(0, repository.AddCallCount);
        Assert.AreEqual(0, unitOfWork.SaveCallCount);
    }

    [TestMethod]
    public async Task ExecuteAsync_RepositoryFails_DoesNotSave()
    {
        var repository = new FakeAlertRuleRepository
        {
            ShouldThrow = true
        };

        var unitOfWork = new FakeUnitOfWork();
        var useCase = new AddAlertRule(repository, unitOfWork);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await useCase.ExecuteAsync(
                Guid.NewGuid(),
                "High Temperature Alert",
                MetricType.Temperature,
                ComparisonOperator.GreaterThan,
                40m,
                AlertSeverity.Warning));

        Assert.AreEqual(0, unitOfWork.SaveCallCount);
    }

    [TestMethod]
    public async Task ExecuteAsync_PassesCancellationTokenToDependencies()
    {
        var repository = new FakeAlertRuleRepository();
        var unitOfWork = new FakeUnitOfWork();
        var useCase = new AddAlertRule(repository, unitOfWork);

        using var cancellationSource = new CancellationTokenSource();
        var token = cancellationSource.Token;

        await useCase.ExecuteAsync(
            Guid.NewGuid(),
            "High Temperature Alert",
            MetricType.Temperature,
            ComparisonOperator.GreaterThan,
            40m,
            AlertSeverity.Warning,
            token);

        Assert.AreEqual(token, repository.ReceivedCancellationToken);
        Assert.AreEqual(token, unitOfWork.ReceivedCancellationToken);
    }

    private sealed class FakeAlertRuleRepository : IAlertRuleRepository
    {
        public AlertRule? AddedAlertRule { get; private set; }

        public int AddCallCount { get; private set; }

        public bool ShouldThrow { get; init; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task AddAsync(
            AlertRule alertRule,
            CancellationToken cancellationToken = default)
        {
            AddCallCount++;
            ReceivedCancellationToken = cancellationToken;

            if (ShouldThrow)
            {
                throw new InvalidOperationException("Repository failure.");
            }

            AddedAlertRule = alertRule;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCallCount { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveCallCount++;
            ReceivedCancellationToken = cancellationToken;

            return Task.FromResult(1);
        }
    }
}
