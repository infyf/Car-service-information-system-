import { useState, useEffect } from "react";
import api from "../api/api";
import { useAuth } from "../context/AuthContext";
import { TIME_SLOTS } from "../constants/config"; 

export default function ServiceSummary({ selectedItems }) {
  const { user } = useAuth();
  const total = selectedItems.reduce((sum, i) => sum + (i.price || i.priceFrom || 0), 0);

  const [name, setName] = useState("");
  const [phone, setPhone] = useState("");
  const [carNumber, setCarNumber] = useState("");
  const [comment, setComment] = useState("");
  const [showCarInput, setShowCarInput] = useState(false);
  const [showCommentInput, setShowCommentInput] = useState(false);
  const [loading, setLoading] = useState(false);
  const [serverMessage, setServerMessage] = useState(null);

  const today = new Date();
  const [currentMonth, setCurrentMonth] = useState(today.getMonth());
  const [currentYear, setCurrentYear] = useState(today.getFullYear());
  const [selectedDate, setSelectedDate] = useState(today.getDate());
  const [selectedTime, setSelectedTime] = useState(null);

  const monthNames = ["Січень", "Лютий", "Березень", "Квітень", "Травень", "Червень", "Липень", "Серпень", "Вересень", "Жовтень", "Листопад", "Грудень"];
  const dayNames = ["Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Нд"];

  useEffect(() => {
    if (!user?.id) return;
    api.get(`/Profile/${user.id}`).then(data => {
      const fullName = `${data.firstName || ''} ${data.lastName || ''}`.trim();
      if (fullName) setName(fullName);
      if (data.phone) setPhone(data.phone);
      if (data.carPlate && data.carPlate !== "Не вказано") { setCarNumber(data.carPlate); setShowCarInput(true); }
    }).catch(console.error);
  }, [user]);

  const handleBooking = async () => {
    setServerMessage(null);
    if (!user?.id) { alert("Увійдіть в систему."); return; }
    if (!name || !phone || !selectedTime) { alert("Заповніть ім'я, телефон та оберіть час!"); return; }
    if (selectedItems.length === 0) { alert("Оберіть послугу!"); return; }

    setLoading(true);
    try {
      const bookingDateObj = new Date(currentYear, currentMonth, selectedDate);
      await api.post('/Bookings', {
        userId: user.id, bookingDate: bookingDateObj.toISOString(), bookingTime: selectedTime,
        customerName: name, customerPhone: phone, carPlate: carNumber || null, comment: comment || null, totalPrice: total,
        services: selectedItems.map(item => ({ serviceItemId: item.id, priceAtBooking: item.price || item.priceFrom, duration: item.durationMinutes || 60 }))
      });
      alert('Ваш запис прийнято.');
      setComment(""); setSelectedTime(null);
    } catch (err) {
      let errorMessage = err.message || "Сталася помилка.";
      if (err.data?.data?.length > 0 && err.data.data[0].suggestedTime) errorMessage += `\nРекомендований час: ${err.data.data[0].suggestedTime}`;
      setServerMessage(errorMessage);
    } finally { setLoading(false); }
  };

  const daysInMonth = new Date(currentYear, currentMonth + 1, 0).getDate();
  const firstDayOfMonth = new Date(currentYear, currentMonth, 1).getDay();
  const startDay = firstDayOfMonth === 0 ? 6 : firstDayOfMonth - 1;
  const prevMonth = () => { if (currentMonth === 0) { setCurrentMonth(11); setCurrentYear(currentYear - 1); } else { setCurrentMonth(currentMonth - 1); } };
  const nextMonth = () => { if (currentMonth === 11) { setCurrentMonth(0); setCurrentYear(currentYear + 1); } else { setCurrentMonth(currentMonth + 1); } };
  const isPast = (day) => new Date(currentYear, currentMonth, day) < new Date(today.getFullYear(), today.getMonth(), today.getDate());

  const calendarDays = [];
  for (let i = 0; i < startDay; i++) calendarDays.push({ day: 0, current: false });
  for (let i = 1; i <= daysInMonth; i++) calendarDays.push({ day: i, current: true });

  return (
    <aside className="col-span-4 h-fit sticky top-24">
      <div className="border border-gray-200 rounded-xl bg-white overflow-hidden shadow-sm">
        <div className="px-5 py-3 border-b border-gray-100 bg-gray-50/50"><h3 className="font-semibold text-gray-900">Попередній запис</h3></div>
        <div className="px-5 pt-4 pb-3 border-b border-gray-100">
          <p className="text-xs text-gray-400 uppercase tracking-wide">Приблизна вартість</p>
          <p className="text-3xl font-bold text-gray-900 mt-1">{total} <span className="text-xl">₴</span></p>
        </div>
        {serverMessage && <div className="bg-red-50 text-red-700 p-3 text-sm border-b border-gray-100 whitespace-pre-line">{serverMessage}</div>}
        
        <div className="px-5 py-4 border-b border-gray-100">
          <div className="flex items-center justify-between mb-3">
            <button onClick={prevMonth} className="p-1 rounded hover:bg-gray-100">←</button>
            <span className="text-sm font-medium">{monthNames[currentMonth]} {currentYear}</span>
            <button onClick={nextMonth} className="p-1 rounded hover:bg-gray-100">→</button>
          </div>
          <div className="grid grid-cols-7 gap-1 text-center text-[10px] font-medium text-gray-400">{dayNames.map(d => <div key={d}>{d}</div>)}</div>
          <div className="grid grid-cols-7 gap-1 text-center mt-1">
            {calendarDays.map((item, idx) => (
              <button key={idx} disabled={!item.current || isPast(item.day)} onClick={() => item.current && setSelectedDate(item.day)} className={`text-xs py-1.5 rounded-md transition-all ${!item.current || isPast(item.day) ? "text-gray-200 cursor-default" : "hover:bg-orange-50 text-gray-700"} ${selectedDate === item.day && item.current ? "bg-orange-500 text-white font-bold" : ""}`}>{item.day !== 0 ? item.day : ""}</button>
            ))}
          </div>
        </div>

        <div className="px-5 py-4 border-b border-gray-100">
          <p className="text-xs font-medium text-gray-500 mb-2 uppercase tracking-wide">Оберіть час</p>
          <div className="grid grid-cols-4 gap-1.5">{TIME_SLOTS.map(time => (<button key={time} onClick={() => setSelectedTime(time)} className={`text-xs py-2 rounded-md font-medium transition-all ${selectedTime === time ? "bg-orange-500 text-white" : "bg-gray-50 text-gray-600 hover:bg-orange-50"}`}>{time}</button>))}</div>
        </div>

        <div className="px-5 py-4 space-y-2.5">
          <input type="text" placeholder="Ваше ім'я" value={name} onChange={(e) => setName(e.target.value)} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-orange-400 outline-none" />
          <input type="tel" placeholder="Номер телефону" value={phone} onChange={(e) => setPhone(e.target.value)} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-orange-400 outline-none" />
          {showCarInput ? <input type="text" placeholder="Номер авто" value={carNumber} onChange={(e) => setCarNumber(e.target.value)} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-orange-400" /> : <button onClick={() => setShowCarInput(true)} className="w-full px-3 py-2 border border-dashed border-gray-300 rounded-lg text-xs text-gray-500 text-left hover:text-orange-500">+ Номер автомобіля</button>}
          {showCommentInput ? <textarea placeholder="Коментар" value={comment} onChange={(e) => setComment(e.target.value)} className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-orange-400" rows={2} /> : <button onClick={() => setShowCommentInput(true)} className="w-full px-3 py-2 border border-dashed border-gray-300 rounded-lg text-xs text-gray-500 text-left hover:text-orange-500">+ Коментар</button>}
          <button onClick={handleBooking} disabled={loading} className={`w-full py-3 bg-orange-500 text-white rounded-lg font-semibold text-sm transition-colors mt-2 ${loading ? "opacity-50 cursor-not-allowed" : "hover:bg-orange-600 shadow-md"}`}>{loading ? "Обробка..." : "Записатися"}</button>
        </div>
      </div>
    </aside>
  );
}