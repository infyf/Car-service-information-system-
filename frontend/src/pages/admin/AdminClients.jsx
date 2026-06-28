import { useState, useEffect } from "react";
import { Search } from "lucide-react";
import api from "../../api/api";

const ITEMS_PER_PAGE = 8;

export default function AdminClients() {
  const [allClients, setAllClients] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState("");
  const [currentPage, setCurrentPage] = useState(1);

  useEffect(() => {
    const fetchClients = async () => {
      setLoading(true);
      try {
        const res = await api.get("/admin/clients");
        const data = res?.data?.length ? res.data : (Array.isArray(res) ? res : []);
        setAllClients(data);
      } catch (err) {
        console.error(err);
        setAllClients([]);
      } finally {
        setLoading(false);
      }
    };
    fetchClients();
  }, []);


  const filteredClients = allClients.filter((c) => {
    if (!searchQuery) return true;
    const query = searchQuery.toLowerCase();
    return (
      c.fullName.toLowerCase().includes(query) ||
      c.email.toLowerCase().includes(query) ||
      (c.phone && c.phone.includes(query)) ||
      c.carInfo.toLowerCase().includes(query)
    );
  });

  const totalPages = Math.ceil(filteredClients.length / ITEMS_PER_PAGE);
  const currentTableData = filteredClients.slice(
    (currentPage - 1) * ITEMS_PER_PAGE,
    currentPage * ITEMS_PER_PAGE
  );

  const handleSearchChange = (e) => {
    setSearchQuery(e.target.value);
    setCurrentPage(1); 
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap justify-between items-end gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-800">Клієнти</h1>
          <p className="text-sm text-gray-500 mt-1">База всіх зареєстрованих клієнтів системи</p>
        </div>

        {/* поле пошуку */}
        <div className="relative w-full md:w-80">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            placeholder="Пошук по імені, email, авто..."
            value={searchQuery}
            onChange={handleSearchChange}
            className="w-full pl-10 pr-4 py-2 rounded-lg text-sm border border-gray-200 outline-none focus:border-gray-800 transition-colors"
          />
        </div>
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-gray-200">
        <div className="overflow-x-auto">
          <table className="w-full text-sm text-left">
            <thead className="text-xs text-gray-500 uppercase bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 font-medium">Клієнт</th>
                <th className="px-6 py-3 font-medium">Телефон</th>
                <th className="px-6 py-3 font-medium">Автомобіль</th>
                <th className="px-6 py-3 font-medium">Записів</th>
                <th className="px-6 py-3 font-medium">Реєстрація</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {loading ? (
                <tr><td colSpan="5" className="text-center py-10 text-gray-400">Завантаження клієнтів...</td></tr>
              ) : currentTableData.length === 0 ? (
                <tr><td colSpan="5" className="text-center py-10 text-gray-400">Клієнтів не знайдено</td></tr>
              ) : (
                currentTableData.map((c) => (
                  <tr key={c.id} className="hover:bg-gray-50/50 transition-colors">
                    <td className="px-6 py-4">
                      <div className="font-medium text-gray-800">{c.fullName}</div>
                      <div className="text-xs text-gray-500 mt-0.5">{c.email}</div>
                    </td>
                    <td className="px-6 py-4 text-gray-600">{c.phone}</td>
                    <td className="px-6 py-4 text-gray-600 max-w-[200px] truncate">{c.carInfo}</td>
                    <td className="px-6 py-4">
                      <span className={`px-2.5 py-1 rounded-full text-xs font-medium ${
                        c.totalBookings > 0 
                          ? "bg-blue-50 text-blue-700 border border-blue-100" 
                          : "bg-gray-50 text-gray-500 border border-gray-100"
                      }`}>
                        {c.totalBookings}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-gray-500 text-xs">
                      {new Date(c.registeredAt).toLocaleDateString('uk-UA', { 
                        day: '2-digit', 
                        month: '2-digit', 
                        year: 'numeric' 
                      })}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Пагінація */}
        {totalPages > 1 && (
          <div className="px-6 py-4 border-t border-gray-200 flex items-center justify-between">
            <p className="text-sm text-gray-500">
              Показано {((currentPage - 1) * ITEMS_PER_PAGE) + 1} - {Math.min(currentPage * ITEMS_PER_PAGE, filteredClients.length)} з {filteredClients.length}
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
    </div>
  );
}