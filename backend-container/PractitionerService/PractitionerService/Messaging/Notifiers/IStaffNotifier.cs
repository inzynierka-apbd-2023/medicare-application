namespace PractitionerService.Messaging.Notifiers;

public interface IStaffNotifier
{
    Task NotifyDoctorArchived(Guid doctorId, Guid? userId, CancellationToken cancellationToken = default);
}
