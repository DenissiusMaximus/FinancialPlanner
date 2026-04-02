using API.Domain.Calculator;
using API.Inputs;
using API.Models;
using API.Services.Aim;
using API.Utils.Notification;
using FluentAssertions;
using Moq;

namespace APITest;

public class AimServiceTest : BaseTest
{
	[Fact]
	public async Task CreateAim_ReturnsCreatedAim()
	{
		var dbContext = GetInMemoryDbContext();
		var testUserId = 1;

		var notificationContext = new NotificationContext();
		var mockUserContext = GetMockUserContext(testUserId);
		var calculatorMock = GetCalculatorMock();
		var aimService = new AimService(notificationContext, dbContext, mockUserContext, calculatorMock.Object);

		var input = new CreateAimInput
		{
			Name = "Save for vacation",
			Amount = 1200,
			Priority = 2,
			CurrencyId = null
		};

		var result = await aimService.CreateAim(input);

		result.Should().NotBeNull();
		result!.Id.Should().BeGreaterThan(0);
		result.Name.Should().Be(input.Name);
		result.Amount.Should().Be(input.Amount);
		result.Priority.Should().Be(input.Priority);
		result.UserId.Should().Be(testUserId);
		notificationContext.HasNotifications.Should().BeFalse();
	}

	[Fact]
	public async Task GetAim_ReturnsAimById()
	{
		var dbContext = GetInMemoryDbContext();
		var testUserId = 1;

		var aim = new Aim { Id = 10, Name = "Emergency fund", Amount = 5000, Priority = 1, UserId = testUserId, IsClosed = false };
		dbContext.Aims.Add(aim);
		await dbContext.SaveChangesAsync();

		var notificationContext = new NotificationContext();
		var mockUserContext = GetMockUserContext(testUserId);
		var calculatorMock = GetCalculatorMock();
		var aimService = new AimService(notificationContext, dbContext, mockUserContext, calculatorMock.Object);

		var result = await aimService.GetAim(aim.Id);

		result.Should().NotBeNull();
		result!.Id.Should().Be(aim.Id);
		result.Name.Should().Be(aim.Name);
		result.UserId.Should().Be(testUserId);
		notificationContext.HasNotifications.Should().BeFalse();
		calculatorMock.Verify(c => c.CalculateAimProgress(It.Is<List<AimDto>>(a => a.Any(x => x.Id == aim.Id))), Times.Once);
	}

	[Fact]
	public async Task GetAim_ReturnsAimFromCalculatedProgressResult()
	{
		var dbContext = GetInMemoryDbContext();
		var testUserId = 1;

		var aim = new Aim { Id = 11, Name = "Progress target", Amount = 7000, Priority = 1, UserId = testUserId, IsClosed = false };
		dbContext.Aims.Add(aim);
		await dbContext.SaveChangesAsync();

		var notificationContext = new NotificationContext();
		var mockUserContext = GetMockUserContext(testUserId);

		var calculatorMock = new Mock<IAimProgressCalculator>();
		calculatorMock
			.Setup(c => c.CalculateAimProgress(It.IsAny<List<AimDto>>()))
			.ReturnsAsync((List<AimDto> aims) =>
			{
				var dto = aims.First(a => a.Id == 11);
				dto.Name = "Calculated Name";
				return aims;
			});

		var aimService = new AimService(notificationContext, dbContext, mockUserContext, calculatorMock.Object);

		var result = await aimService.GetAim(aim.Id);

		result.Should().NotBeNull();
		result!.Name.Should().Be("Calculated Name");
		notificationContext.HasNotifications.Should().BeFalse();
	}

	[Fact]
	public async Task GetAim_ReturnsNullWhenAimNotFound()
	{
		var dbContext = GetInMemoryDbContext();
		var testUserId = 1;

		var notificationContext = new NotificationContext();
		var mockUserContext = GetMockUserContext(testUserId);
		var calculatorMock = GetCalculatorMock();
		var aimService = new AimService(notificationContext, dbContext, mockUserContext, calculatorMock.Object);

		var result = await aimService.GetAim(999);

		result.Should().BeNull();
		notificationContext.HasNotifications.Should().BeTrue();
		notificationContext.Notifications.Should().ContainSingle()
			.Which.ErrorCode.Should().Be(ErrorType.NotFound);
		calculatorMock.Verify(c => c.CalculateAimProgress(It.IsAny<List<AimDto>>()), Times.Never);
	}

	[Fact]
	public async Task GetAims_ReturnsOnlyCurrentUserAims()
	{
		var dbContext = GetInMemoryDbContext();
		var testUserId = 1;

		dbContext.Aims.AddRange(
			new Aim { Id = 1, Name = "A1", Amount = 100, Priority = 1, UserId = testUserId, IsClosed = false },
			new Aim { Id = 2, Name = "A2", Amount = 200, Priority = 2, UserId = testUserId, IsClosed = false },
			new Aim { Id = 3, Name = "Other user aim", Amount = 300, Priority = 3, UserId = 2, IsClosed = false }
		);
		await dbContext.SaveChangesAsync();

		var notificationContext = new NotificationContext();
		var mockUserContext = GetMockUserContext(testUserId);
		var calculatorMock = GetCalculatorMock();
		var aimService = new AimService(notificationContext, dbContext, mockUserContext, calculatorMock.Object);

		var result = await aimService.GetAims();

		result.Should().HaveCount(2);
		result.Should().OnlyContain(a => a.UserId == testUserId);
		notificationContext.HasNotifications.Should().BeFalse();
		calculatorMock.Verify(c => c.CalculateAimProgress(It.Is<List<AimDto>>(a => a.Count == 2)), Times.Once);
	}

	[Fact]
	public async Task UpdateAim_ReturnsUpdatedAim()
	{
		var dbContext = GetInMemoryDbContext();
		var testUserId = 1;

		var aim = new Aim { Id = 1, Name = "Old name", Amount = 1000, Priority = 1, UserId = testUserId, IsClosed = false };
		dbContext.Aims.Add(aim);
		await dbContext.SaveChangesAsync();

		var notificationContext = new NotificationContext();
		var mockUserContext = GetMockUserContext(testUserId);
		var calculatorMock = GetCalculatorMock();
		var aimService = new AimService(notificationContext, dbContext, mockUserContext, calculatorMock.Object);

		var input = new UpdateAimInput
		{
			Name = "Updated name",
			Amount = 1500,
			Priority = 3
		};

		var result = await aimService.UpdateAim(aim.Id, input);

		result.Should().NotBeNull();
		result!.Name.Should().Be(input.Name);
		result.Amount.Should().Be(input.Amount!.Value);
		result.Priority.Should().Be(input.Priority!.Value);
		notificationContext.HasNotifications.Should().BeFalse();
	}

	[Fact]
	public async Task UpdateAim_ReturnsNullWhenAimNotFound()
	{
		var dbContext = GetInMemoryDbContext();
		var testUserId = 1;

		var notificationContext = new NotificationContext();
		var mockUserContext = GetMockUserContext(testUserId);
		var calculatorMock = GetCalculatorMock();
		var aimService = new AimService(notificationContext, dbContext, mockUserContext, calculatorMock.Object);

		var result = await aimService.UpdateAim(999, new UpdateAimInput { Name = "x" });

		result.Should().BeNull();
		notificationContext.HasNotifications.Should().BeTrue();
		notificationContext.Notifications.Should().ContainSingle()
			.Which.ErrorCode.Should().Be(ErrorType.NotFound);
	}

	[Fact]
	public async Task DeleteAim_ReturnsTrueWhenDeleted()
	{
		var dbContext = GetInMemoryDbContext();
		var testUserId = 1;

		var aim = new Aim { Id = 1, Name = "To delete", Amount = 1000, Priority = 1, UserId = testUserId, IsClosed = false };
		dbContext.Aims.Add(aim);
		await dbContext.SaveChangesAsync();

		var notificationContext = new NotificationContext();
		var mockUserContext = GetMockUserContext(testUserId);
		var calculatorMock = GetCalculatorMock();
		var aimService = new AimService(notificationContext, dbContext, mockUserContext, calculatorMock.Object);

		var result = await aimService.DeleteAim(aim.Id);

		result.Should().BeTrue();
		dbContext.Aims.Should().BeEmpty();
		notificationContext.HasNotifications.Should().BeFalse();
	}

	[Fact]
	public async Task DeleteAim_ReturnsFalseWhenAimNotFound()
	{
		var dbContext = GetInMemoryDbContext();
		var testUserId = 1;

		var notificationContext = new NotificationContext();
		var mockUserContext = GetMockUserContext(testUserId);
		var calculatorMock = GetCalculatorMock();
		var aimService = new AimService(notificationContext, dbContext, mockUserContext, calculatorMock.Object);

		var result = await aimService.DeleteAim(999);

		result.Should().BeFalse();
		notificationContext.HasNotifications.Should().BeTrue();
		notificationContext.Notifications.Should().ContainSingle()
			.Which.ErrorCode.Should().Be(ErrorType.NotFound);
	}

	[Fact]
	public async Task AddSourceToAim_ReturnsSourceWhenAdded()
	{
		var dbContext = GetInMemoryDbContext();
		var testUserId = 1;

		var currency = new Currency { Id = 1, Name = "USD", UsdExchangeRate = 1 };
		var aim = new Aim { Id = 1, Name = "House", Amount = 50000, Priority = 1, UserId = testUserId, IsClosed = false };
		var source = new Source
		{
			Id = 2,
			Name = "Savings",
			Amount = 2000,
			UserId = testUserId,
			CurrencyId = currency.Id,
			Currency = currency,
			IsArchived = false
		};

		dbContext.Currencies.Add(currency);
		dbContext.Aims.Add(aim);
		dbContext.Sources.Add(source);
		await dbContext.SaveChangesAsync();

		var notificationContext = new NotificationContext();
		var mockUserContext = GetMockUserContext(testUserId);
		var calculatorMock = GetCalculatorMock();
		var aimService = new AimService(notificationContext, dbContext, mockUserContext, calculatorMock.Object);

		var result = await aimService.AddSourceToAim(aim.Id, source.Id);

		result.Should().NotBeNull();
		result!.Id.Should().Be(source.Id);
		dbContext.SourceAims.Should().ContainSingle(sa => sa.AimId == aim.Id && sa.SourceId == source.Id);
		notificationContext.HasNotifications.Should().BeFalse();
	}

	[Fact]
	public async Task AddSourceToAim_ReturnsNullWhenAimNotFound()
	{
		var dbContext = GetInMemoryDbContext();
		var testUserId = 1;

		var source = new Source { Id = 2, Name = "Savings", Amount = 2000, UserId = testUserId, CurrencyId = 1, IsArchived = false };
		dbContext.Sources.Add(source);
		await dbContext.SaveChangesAsync();

		var notificationContext = new NotificationContext();
		var mockUserContext = GetMockUserContext(testUserId);
		var calculatorMock = GetCalculatorMock();
		var aimService = new AimService(notificationContext, dbContext, mockUserContext, calculatorMock.Object);

		var result = await aimService.AddSourceToAim(999, source.Id);

		result.Should().BeNull();
		notificationContext.HasNotifications.Should().BeTrue();
		notificationContext.Notifications.Should().ContainSingle()
			.Which.ErrorCode.Should().Be(ErrorType.NotFound);
	}

	[Fact]
	public async Task AddSourceToAim_ReturnsNullWhenSourceNotFound()
	{
		var dbContext = GetInMemoryDbContext();
		var testUserId = 1;

		var aim = new Aim { Id = 1, Name = "House", Amount = 50000, Priority = 1, UserId = testUserId, IsClosed = false };
		dbContext.Aims.Add(aim);
		await dbContext.SaveChangesAsync();

		var notificationContext = new NotificationContext();
		var mockUserContext = GetMockUserContext(testUserId);
		var calculatorMock = GetCalculatorMock();
		var aimService = new AimService(notificationContext, dbContext, mockUserContext, calculatorMock.Object);

		var result = await aimService.AddSourceToAim(aim.Id, 999);

		result.Should().BeNull();
		notificationContext.HasNotifications.Should().BeTrue();
		notificationContext.Notifications.Should().ContainSingle()
			.Which.ErrorCode.Should().Be(ErrorType.NotFound);
	}

	[Fact]
	public async Task AddSourceToAim_ReturnsNullWhenAssociationAlreadyExists()
	{
		var dbContext = GetInMemoryDbContext();
		var testUserId = 1;

		var aim = new Aim { Id = 1, Name = "House", Amount = 50000, Priority = 1, UserId = testUserId, IsClosed = false };
		var source = new Source { Id = 2, Name = "Savings", Amount = 2000, UserId = testUserId, CurrencyId = 1, IsArchived = false };
		var sourceAim = new SourceAim { Id = 1, AimId = aim.Id, SourceId = source.Id, Aim = aim, Source = source };

		dbContext.Aims.Add(aim);
		dbContext.Sources.Add(source);
		dbContext.SourceAims.Add(sourceAim);
		await dbContext.SaveChangesAsync();

		var notificationContext = new NotificationContext();
		var mockUserContext = GetMockUserContext(testUserId);
		var calculatorMock = GetCalculatorMock();
		var aimService = new AimService(notificationContext, dbContext, mockUserContext, calculatorMock.Object);

		var result = await aimService.AddSourceToAim(aim.Id, source.Id);

		result.Should().BeNull();
		notificationContext.HasNotifications.Should().BeTrue();
		notificationContext.Notifications.Should().ContainSingle()
			.Which.ErrorCode.Should().Be(ErrorType.BadRequest);
	}

	[Fact]
	public async Task RemoveSourceFromAim_ReturnsTrueWhenRemoved()
	{
		var dbContext = GetInMemoryDbContext();
		var testUserId = 1;

		var aim = new Aim { Id = 1, Name = "House", Amount = 50000, Priority = 1, UserId = testUserId, IsClosed = false };
		var source = new Source { Id = 2, Name = "Savings", Amount = 2000, UserId = testUserId, CurrencyId = 1, IsArchived = false };
		var sourceAim = new SourceAim { Id = 1, AimId = aim.Id, SourceId = source.Id, Aim = aim, Source = source };

		dbContext.Aims.Add(aim);
		dbContext.Sources.Add(source);
		dbContext.SourceAims.Add(sourceAim);
		await dbContext.SaveChangesAsync();

		var notificationContext = new NotificationContext();
		var mockUserContext = GetMockUserContext(testUserId);
		var calculatorMock = GetCalculatorMock();
		var aimService = new AimService(notificationContext, dbContext, mockUserContext, calculatorMock.Object);

		var result = await aimService.RemoveSourceFromAim(aim.Id, source.Id);

		result.Should().BeTrue();
		dbContext.SourceAims.Should().BeEmpty();
		notificationContext.HasNotifications.Should().BeFalse();
	}

	[Fact]
	public async Task RemoveSourceFromAim_ReturnsFalseWhenAssociationNotFound()
	{
		var dbContext = GetInMemoryDbContext();
		var testUserId = 1;

		var notificationContext = new NotificationContext();
		var mockUserContext = GetMockUserContext(testUserId);
		var calculatorMock = GetCalculatorMock();
		var aimService = new AimService(notificationContext, dbContext, mockUserContext, calculatorMock.Object);

		var result = await aimService.RemoveSourceFromAim(1, 2);

		result.Should().BeFalse();
		notificationContext.HasNotifications.Should().BeTrue();
		notificationContext.Notifications.Should().ContainSingle()
			.Which.ErrorCode.Should().Be(ErrorType.NotFound);
	}

	private static Mock<IAimProgressCalculator> GetCalculatorMock()
	{
		var calculatorMock = new Mock<IAimProgressCalculator>();
		calculatorMock
			.Setup(c => c.CalculateAimProgress(It.IsAny<List<AimDto>>()))
			.ReturnsAsync((List<AimDto> aims) => aims);
		return calculatorMock;
	}
}