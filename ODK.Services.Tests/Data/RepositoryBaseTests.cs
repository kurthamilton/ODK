using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Events;
using ODK.Core.Payments;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Data;

/// <summary>
/// The one-instance-per-key rule, which the app meets rather than avoids: reads are not tracked, so two
/// reads of a row give two instances of it, and EF rejects the second one attached.
/// </summary>
[Parallelizable]
public static class RepositoryBaseTests
{
    [Test]
    public static async Task Delete_RowAlreadyTracked_RemovesTheTrackedInstance()
    {
        /* Arrange - updated and then deleted in one unit of work, which is two instances of one row. Remove
           attaches what it is given, so without reusing the tracked entry the second would be rejected. */
        using var context = CreateMockOdkContext();

        var payment = context.CreatePayment();

        var unitOfWork = MockUnitOfWorkFactory.Create(context);
        context.ChangeTracker.Clear();

        var toUpdate = await unitOfWork.PaymentRepository.GetById(payment.Id).Run();
        var toDelete = await unitOfWork.PaymentRepository.GetById(payment.Id).Run();

        toUpdate.Reference = "updated";
        unitOfWork.PaymentRepository.Update(toUpdate);

        // Act
        unitOfWork.PaymentRepository.Delete(toDelete);
        await unitOfWork.SaveChanges();

        // Assert
        context.Set<Payment>().Should().BeEmpty();
    }

    [Test]
    public static async Task Update_RowAlreadyTracked_WritesOntoTheTrackedInstance()
    {
        // Arrange - two reads of one row, which is two instances of it
        using var context = CreateMockOdkContext();

        var payment = context.CreatePayment();

        var unitOfWork = MockUnitOfWorkFactory.Create(context);
        context.ChangeTracker.Clear();

        var first = await unitOfWork.PaymentRepository.GetById(payment.Id).Run();
        var second = await unitOfWork.PaymentRepository.GetById(payment.Id).Run();

        first.Reference = "first";
        unitOfWork.PaymentRepository.Update(first);

        // Act
        second.Reference = "second";
        unitOfWork.PaymentRepository.Update(second);
        await unitOfWork.SaveChanges();

        // Assert - the later write wins, rather than the second instance being rejected
        context.Set<Payment>().Single(x => x.Id == payment.Id)
            .Reference.Should().Be("second");
    }

    [Test]
    public static async Task Update_RowKeyedOnSomethingOtherThanId_WritesOntoTheTrackedInstance()
    {
        /* Arrange - the key comes from the model, not from IDatabaseEntity: ticket settings are keyed on
           their event and have no Id at all, so keying off Id would skip the check and attach twice. */
        using var context = CreateMockOdkContext();

        var currency = context.CreateCurrency();
        var chapter = context.CreateChapter(country: context.CreateCountry(currency));
        var @event = context.CreateEvent(chapter);

        context.Create(new EventTicketSettings
        {
            Cost = 10m,
            CurrencyId = currency.Id,
            EventId = @event.Id
        });

        var unitOfWork = MockUnitOfWorkFactory.Create(context);
        context.ChangeTracker.Clear();

        var first = await unitOfWork.EventTicketSettingsRepository.GetByEventId(@event.Id).Run();
        var second = await unitOfWork.EventTicketSettingsRepository.GetByEventId(@event.Id).Run();

        first!.Cost = 20m;
        unitOfWork.EventTicketSettingsRepository.Update(first);

        // Act
        second!.Cost = 30m;
        unitOfWork.EventTicketSettingsRepository.Update(second);
        await unitOfWork.SaveChanges();

        // Assert
        context.Set<EventTicketSettings>().Single(x => x.EventId == @event.Id)
            .Cost.Should().Be(30m);
    }

    /* Without tracking, as the app runs: a tracking context would hand back one instance for both reads and
       none of this could happen. */
    private static MockOdkContext CreateMockOdkContext() => new(noTracking: true);
}
