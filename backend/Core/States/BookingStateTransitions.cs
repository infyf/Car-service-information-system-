using AutoServ.Core.Enums;

namespace AutoServ.Core.States
{
    public static class BookingStateTransitions
    {
        private static readonly Dictionary<BookingStatus, List<BookingStatus>> _transitions = new()
        {
            
            { BookingStatus.Pending, new List<BookingStatus> { BookingStatus.Confirmed, BookingStatus.InProgress, BookingStatus.Cancelled } },

            { BookingStatus.Confirmed, new List<BookingStatus> { BookingStatus.InProgress, BookingStatus.Cancelled } },
            { BookingStatus.InProgress, new List<BookingStatus> { BookingStatus.Completed, BookingStatus.Cancelled } },
            { BookingStatus.Completed, new List<BookingStatus> { } }
        };

        public static bool CanTransition(BookingStatus current, BookingStatus next)
        {
            return _transitions.TryGetValue(current, out var allowedStates) && allowedStates.Contains(next);
        }

        public static string GetErrorMessage(BookingStatus current, BookingStatus next)
        {
            return $"Перехід зі статусу '{current}' у '{next}' заборонений бізнес-логікою.";
        }
    }
}