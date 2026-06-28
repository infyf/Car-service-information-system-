import { useState, useEffect } from "react";
import { Calendar } from "lucide-react";
import api from "../../api/api";

const ITEMS_PER_PAGE = 5;

const StatusBadge = ({ status }) => {
  const styles = {
    pending: "bg-yellow-100 text-yellow-700 border-yellow-200",
    confirmed: "bg-green-100 text-green-700 border-green-200",
    inprogress: "bg-blue-100 text-blue-700 border-blue-200",
    completed: "bg-gray-100 text-gray-600 border-gray-200",
    cancelled: "bg-red-100 text-red-700 border-red-200",
  };
  const names = {
    pending: "Очікує", confirmed: "Підтверджено", inprogress: "В процесі",
    completed: "Завершено", cancelled: "Скасовано",
  };
  return (
    <span className={`px-3 py-1 rounded-full text-xs font-medium border ${styles[status] || "bg-gray-100"}`}>
      {names[status] || status}
    </span>
  );
};

export default function AdminBookings() {
  const [allBookings, setAllBookings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedDate, setSelectedDate] = useState(null); 
  const [currentPage, setCurrentPage] = useState(1);
  
  const todayStr = new Date().toISOString().split("T")[0];

  
  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      try {
   
        const res = await api.get("/admin/bookings"); 
        const data = res?.data?.length ? res.data : (Array.isArray(res) ? res : []);
        setAllBookings(data);
      } catch (err) {
        console.error(err);
        setAllBookings([]);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, []); 

  const filteredBookings = allBookings.filter((b) => {
    if (!selectedDate) return true; 
    
    // b.bookingDate приходить з беку як "2026-05-10T00:00:00" або "2026-05-10"
    // selectedDate має формат "2026-05-10"
    // використов. startsWith, щоб ігнорувати час і часові пояси
    return b.bookingDate && b.bookingDate.startsWith(selectedDate);
  });

  const totalPages = Math.ceil(filteredBookings.length / ITEMS_PER_PAGE);
  const currentTableData = filteredBookings.slice(
    (currentPage - 1) * ITEMS_PER_PAGE,
    currentPage * ITEMS_PER_PAGE
  );

  const handleDateChange = (newDate) => {
    setSelectedDate(newDate);
    setCurrentPage(1);
  };

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-200">
      <div className="p-5 border-b border-gray-200 flex flex-wrap justify-between items-center gap-3">
        <h2 className="text-xl font-semibold text-gray-800">Заяви на обслуговування</h2>
        
        <div className="flex items-center gap-2">

          <button
            onClick={() => handleDateChange(null)}
            className={`px-4 py-2 rounded-lg text-sm font-medium border transition-colors ${
              !selectedDate ? "bg-gray-800 text-white border-gray-800" : "bg-white text-gray-600 hover:bg-gray-50"
            }`}
          >
            Усі заявки
          </button>

          <button
            onClick={() => handleDateChange(todayStr)}
            className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium border transition-colors ${
              selectedDate === todayStr ? "bg-gray-800 text-white border-gray-800" : "bg-white text-gray-600 hover:bg-gray-50"
            }`}
          >
            <Calendar className="w-4 h-4" />
            Сьогодні
          </button>


          <input 
            type="date" 
            value={selectedDate || ""} 
            onChange={(e) => handleDateChange(e.target.value || null)}
            className={`px-3 py-2 rounded-lg text-sm font-medium border transition-colors outline-none ${
              selectedDate && selectedDate !== todayStr ? "border-gray-800 bg-gray-50" : "border-gray-200 hover:border-gray-300"
            }`}
          />
        </div>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full text-sm text-left">
          <thead className="text-xs text-gray-500 uppercase bg-gray-50 border-b border-gray-200">
            <tr>
              <th className="px-6 py-3 font-medium">Дата та час</th>
              <th className="px-6 py-3 font-medium">Клієнт</th>
              <th className="px-6 py-3 font-medium">Послуги</th>
              <th className="px-6 py-3 font-medium">Авто</th>
              <th className="px-6 py-3 font-medium">Майстр</th>
              <th className="px-6 py-3 font-medium">Статус</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {loading ? (
              <tr><td colSpan="6" className="text-center py-10 text-gray-400">Завантаження...</td></tr>
            ) : currentTableData.length === 0 ? (
              <tr><td colSpan="6" className="text-center py-10 text-gray-400">Заяв не знайдено</td></tr>
            ) : (
              currentTableData.map((b) => (
                <tr key={b.id} className="hover:bg-gray-50/50 transition-colors">
                  <td className="px-6 py-4">
                    <div className="font-medium text-gray-800">{new Date(b.bookingDate).toLocaleDateString('uk-UA')}</div>
                    <div className="text-xs text-gray-500 mt-0.5">{b.bookingTime}</div>
                  </td>
                  <td className="px-6 py-4">
                    <div className="font-medium text-gray-800">{b.clientName}</div>
                    <div className="text-xs text-gray-500">{b.clientPhone}</div>
                  </td>
                  <td className="px-6 py-4 text-gray-600 max-w-[200px] truncate">{b.services}</td>
                  <td className="px-6 py-4 text-gray-600">{b.carInfo}</td>
                  <td className="px-6 py-4 text-gray-800">{b.masterName}</td>
                  <td className="px-6 py-4"><StatusBadge status={b.status} /></td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {totalPages > 1 && (
        <div className="px-6 py-4 border-t border-gray-200 flex items-center justify-between">
          <p className="text-sm text-gray-500">
            Показано {((currentPage - 1) * ITEMS_PER_PAGE) + 1} - {Math.min(currentPage * ITEMS_PER_PAGE, filteredBookings.length)} з {filteredBookings.length}
          </p>
          <div className="flex gap-1">
            {[...Array(totalPages)].map((_, i) => (
              <button
                key={i}
                onClick={() => setCurrentPage(i + 1)}
                className={`w-8 h-8 rounded text-sm font-medium ${
                  currentPage === i + 1 ? "bg-gray-800 text-white" : "text-gray-600 hover:bg-gray-100"
                }`}
              >
                {i + 1}
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}