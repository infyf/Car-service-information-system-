export const BOOKING_STATUS = {
  PENDING: 'pending',
  CONFIRMED: 'confirmed',
  IN_PROGRESS: 'in_progress',
  COMPLETED: 'completed',
  PAID: 'paid',
  CANCELLED: 'cancelled'
};

export const statusMap = {
  [BOOKING_STATUS.PENDING]: { label: "Очікує", bg: "bg-yellow-50 text-yellow-700 border border-yellow-200" },
  [BOOKING_STATUS.CONFIRMED]: { label: "Підтверджено", bg: "bg-blue-50 text-blue-700 border border-blue-200" },
  [BOOKING_STATUS.IN_PROGRESS]: { label: "В роботі", bg: "bg-indigo-50 text-indigo-700 border border-indigo-200" },
  [BOOKING_STATUS.COMPLETED]: { label: "Виконано", bg: "bg-green-50 text-green-700 border border-green-200" },
  [BOOKING_STATUS.PAID]: { label: "Оплачено", bg: "bg-emerald-100 text-emerald-800 border border-emerald-300 font-medium" },
  [BOOKING_STATUS.CANCELLED]: { label: "Скасовано", bg: "bg-red-50 text-red-700 border border-red-200" },
};