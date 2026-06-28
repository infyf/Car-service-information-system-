import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import api from "../api/api";
import { BOOKING_STATUS, statusMap } from "../constants/statuses";

function getMinutes(timeStr) {
  if (!timeStr) return 0;
  const parts = timeStr.split(':');
  return parseInt(parts[0], 10) * 60 + parseInt(parts[1], 10);
}

export const useBookings = (userId, ordersTab) => {
  const queryClient = useQueryClient();

  const { data: bookings = [], isLoading } = useQuery({
    queryKey: ['userBookings', userId],
    queryFn: () => api.get(`/Bookings/user/${userId}`),
    enabled: !!userId
  });

  const handleMockPayment = useMutation({
    mutationFn: (idsString) => {
      alert("Оплата успішно пройшла! Дякуємо.");
      const targetIds = idsString.split(', ').map(Number);

      queryClient.setQueryData(['userBookings', userId], (oldData) =>
        oldData.map(b => targetIds.includes(b.id) ? { ...b, status: BOOKING_STATUS.PAID } : b)
      );
    }
  });


  const displayBookings = (() => {
    const parentIdsWithChildren = new Set();
    bookings.forEach(b => { if (b.parentBookingId) parentIdsWithChildren.add(b.parentBookingId); });

    const allGroups = bookings.reduce((acc, b) => {
      if (b.bookingType === 'service_order' && parentIdsWithChildren.has(b.id)) return acc;
      const groupKey = b.parentBookingId ? `parent_${b.parentBookingId}_date_${b.bookingDate}` : `self_${b.id}`;
      if (!acc[groupKey]) acc[groupKey] = { bookingDate: b.bookingDate, times: [], statuses: [], ids: [], serviceObjects: [], totalPrice: 0, bookingTypes: [] };
      if (b.bookingTime) { const timeShort = b.bookingTime.substring(0, 5); if (!acc[groupKey].times.includes(timeShort)) acc[groupKey].times.push(timeShort); }
      acc[groupKey].ids.push(b.id); acc[groupKey].statuses.push(b.status); acc[groupKey].totalPrice += b.totalPrice || 0;
      if (b.bookingType && !acc[groupKey].bookingTypes.includes(b.bookingType)) acc[groupKey].bookingTypes.push(b.bookingType);
      if (b.services && b.services.length > 0) { b.services.forEach(s => { const sName = s.title || s.serviceItemTitle || "Послуга"; if (!acc[groupKey].serviceObjects.find(x => x.name === sName)) acc[groupKey].serviceObjects.push({ name: sName, status: b.status }); }); }
      if (b.bookingType === "consultation" && !acc[groupKey].serviceObjects.find(x => x.name === "Консультація")) acc[groupKey].serviceObjects.push({ name: "Консультація", status: b.status });
      return acc;
    }, {});

    return Object.values(allGroups).map(g => {
      let displayTime = "---";
      if (g.times.length > 0) { const sorted = g.times.sort((a, b) => getMinutes(a) - getMinutes(b)); displayTime = sorted[0] === sorted[sorted.length - 1] ? sorted[0] : `${sorted[0]} - ${sorted[sorted.length - 1]}`; }
      
      let aggregateStatus = BOOKING_STATUS.PENDING; 
      const uniqueStatuses = [...new Set(g.statuses)];
      if (uniqueStatuses.includes(BOOKING_STATUS.CANCELLED)) aggregateStatus = BOOKING_STATUS.CANCELLED;
      else if (uniqueStatuses.every(s => s === BOOKING_STATUS.PAID)) aggregateStatus = BOOKING_STATUS.PAID;
      else if (uniqueStatuses.every(s => s === BOOKING_STATUS.COMPLETED)) aggregateStatus = BOOKING_STATUS.COMPLETED;
      else if (uniqueStatuses.includes(BOOKING_STATUS.IN_PROGRESS)) aggregateStatus = BOOKING_STATUS.IN_PROGRESS;
      else if (uniqueStatuses.includes(BOOKING_STATUS.CONFIRMED)) aggregateStatus = BOOKING_STATUS.CONFIRMED;
      
      const st = statusMap[aggregateStatus] || statusMap[BOOKING_STATUS.PENDING];
      return { ids: g.ids.join(", "), bookingDate: g.bookingDate, displayTime, serviceObjects: g.serviceObjects, displayPrice: g.totalPrice > 0 ? `${g.totalPrice.toLocaleString("uk-UA")} грн` : "Безкоштовно", statusLabel: st.label, statusBg: st.bg, aggregateStatus };
    }).filter(g => {
      if (ordersTab === "active") return [BOOKING_STATUS.PENDING, BOOKING_STATUS.CONFIRMED, BOOKING_STATUS.IN_PROGRESS, BOOKING_STATUS.COMPLETED].includes(g.aggregateStatus);
      return g.aggregateStatus === BOOKING_STATUS.PAID || g.aggregateStatus === BOOKING_STATUS.CANCELLED;
    });
  })();

  return { bookings, displayBookings, handleMockPayment: handleMockPayment.mutate, isLoading };
};