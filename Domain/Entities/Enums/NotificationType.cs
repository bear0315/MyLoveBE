using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Enums
{
    public enum NotificationType
    {
        BookingConfirmed = 0,
        BookingCancelled = 1,
        PaymentReceived = 2,
        TourReminder = 3,
        ReviewRequest = 4,
        PromotionalOffer = 5,
        SystemAnnouncement = 6
    }
}
