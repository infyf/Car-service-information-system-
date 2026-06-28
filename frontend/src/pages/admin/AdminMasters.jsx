import { useState, useEffect } from "react";
import { Plus, X } from "lucide-react";
import api from "../../api/api";

export default function AdminMasters() {
  const [masters, setMasters] = useState([]);
  const [allServices, setAllServices] = useState([]);
  const [schedule, setSchedule] = useState([]); //Розклад на сьогодні
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
  const [showForm, setShowForm] = useState(false);
  const [formData, setFormData] = useState({ email: "", password: "", firstName: "", lastName: "", phone: "" });
  const [formLoading, setFormLoading] = useState(false);
  const [addingTo, setAddingTo] = useState(null);

  const fetchMasters = async () => {
    setError(null);
    setLoading(true);
    try {
      const mastersRes = await api.get("/admin/masters-detailed");
      const rawData = mastersRes.data !== undefined ? mastersRes.data : mastersRes;
      setMasters(Array.isArray(rawData) ? rawData : []);
    } catch (err) {
      setError(err.response?.data?.message || "Помилка завантаження даних");
    } finally {
      setLoading(false);
    }
  };

  const fetchServices = async () => {
    try {
      const servicesRes = await api.get("/services/with-items");
      const rawData = servicesRes.data !== undefined ? servicesRes.data : servicesRes;
      const items = Array.isArray(rawData) ? rawData : [];
      setAllServices(items.flatMap(s => (s.items || []).map(i => ({ id: i.id, title: `${s.title} - ${i.title}` }))));
    } catch (err) {
      console.warn("Не вдалося завантажити послуги");
    }
  };

  // masters-today-schedule
  const fetchSchedule = async () => {
    try {
      const res = await api.get("/admin/");
      const rawData = res.data !== undefined ? res.data : res;
      setSchedule(Array.isArray(rawData) ? rawData : []);
    } catch (err) {
      console.warn("Не вдалося завантажити розклад");
    }
  };

  useEffect(() => {
    fetchMasters();
    fetchServices();
    fetchSchedule();
  }, []);

  // --- ФУНКЦІЇ ---
  const handleCreateMaster = async (e) => {
    e.preventDefault();
    setFormLoading(true);
    try {
      await api.post("/auth/register-master", formData);
      setShowForm(false);
      setFormData({ email: "", password: "", firstName: "", lastName: "", phone: "" });
      fetchMasters();
    } catch (err) {
      alert(err.response?.data?.message || "Помилка створення майстра");
    } finally {
      setFormLoading(false);
    }
  };

  const handleToggleStatus = async (id) => {
    setMasters(masters.map(m => m.id === id ? { ...m, isActive: !m.isActive } : m));


    try {
      await api.patch(`/admin/masters/${id}/toggle-status`);

    } catch (err) {

    }
  };

  const handleRemoveSpec = async (masterId, serviceItemId) => {
    if (!window.confirm("Видалити цю спеціалізацію?")) return;
    try {
      await api.delete(`/admin/masters/${masterId}/specializations/${serviceItemId}`);
      setMasters(masters.map(m => m.id === masterId ? { ...m, specializations: m.specializations.filter(s => s.id !== serviceItemId) } : m));
    } catch (err) {
      alert("Помилка видалення");
    }
  };

  const handleAddSpec = async (masterId, serviceItemId) => {
    try {
      await api.post(`/masters/${masterId}/specializations/${serviceItemId}`);
      setAddingTo(null);
      fetchMasters();
    } catch (err) {
      alert(err.response?.data?.message || "Помилка додавання");
    }
  };

  // --- ІНТЕРФЕЙС ---
  if (loading) return <div className="text-gray-500 text-sm mt-10">Завантаження...</div>;
  if (error) return <div className="text-red-600 text-sm mt-10">{error}</div>;

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-200">
      
      <div className="p-5 border-b border-gray-200 flex justify-between items-center">
        <h2 className="text-xl font-semibold text-gray-800">Майстри</h2>
        <button 
          onClick={() => setShowForm(!showForm)}
          className="flex items-center gap-1.5 px-4 py-2 text-sm font-medium text-white bg-gray-800 rounded-lg hover:bg-gray-900 transition-colors"
        >
          <Plus className="w-4 h-4" />
          Додати майстра
        </button>
      </div>

      {showForm && (
        <form onSubmit={handleCreateMaster} className="p-5 bg-gray-50 border-b border-gray-200 grid grid-cols-1 md:grid-cols-3 gap-4">
          <input required type="email" placeholder="Email" value={formData.email} onChange={e => setFormData({...formData, email: e.target.value})} className="px-3 py-2 text-sm border border-gray-300 rounded-md outline-none focus:border-gray-800" />
          <input required type="password" placeholder="Пароль" value={formData.password} onChange={e => setFormData({...formData, password: e.target.value})} className="px-3 py-2 text-sm border border-gray-300 rounded-md outline-none focus:border-gray-800" />
          <input required type="text" placeholder="Прізвище" value={formData.lastName} onChange={e => setFormData({...formData, lastName: e.target.value})} className="px-3 py-2 text-sm border border-gray-300 rounded-md outline-none focus:border-gray-800" />
          <input required type="text" placeholder="Ім'я" value={formData.firstName} onChange={e => setFormData({...formData, firstName: e.target.value})} className="px-3 py-2 text-sm border border-gray-300 rounded-md outline-none focus:border-gray-800" />
          <input required type="text" placeholder="Телефон" value={formData.phone} onChange={e => setFormData({...formData, phone: e.target.value})} className="px-3 py-2 text-sm border border-gray-300 rounded-md outline-none focus:border-gray-800" />
          
          <div className="flex gap-2">
            <button type="submit" disabled={formLoading} className="flex-1 px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50">
              {formLoading ? "Створення..." : "Зберегти"}
            </button>
            <button type="button" onClick={() => setShowForm(false)} className="px-4 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-300 rounded-md hover:bg-gray-50">
              Скасувати
            </button>
          </div>
        </form>
      )}

      <div className="overflow-x-auto">
        <table className="w-full text-sm text-left">
          <thead className="text-xs text-gray-500 uppercase bg-gray-50 border-b border-gray-200">
            <tr>
              <th className="px-6 py-3 font-medium">Майстер</th>
              <th className="px-6 py-3 font-medium">Статус</th>
              <th className="px-6 py-3 font-medium">Навантаження на сьогодні</th>
              <th className="px-6 py-3 font-medium">Виконано</th>
              <th className="px-6 py-3 font-medium">Спеціалізації</th>
              <th className="px-6 py-3 font-medium text-right">Дії</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {masters.length === 0 ? (
              <tr><td colSpan="6" className="text-center py-10 text-gray-400">Список порожній</td></tr>
            ) : (
              masters.map((m) => {
                //  розклад цього майстра
                const masterSchedule = schedule.find(s => s.id === m.id);

                return (
                  <tr key={m.id} className={`hover:bg-gray-50/50 transition-colors ${!m.isActive ? "opacity-50" : ""}`}>
                    
                    <td className="px-6 py-4">
                      <div className="font-medium text-gray-800">{m.fullName}</div>
                      <div className="text-xs text-gray-500">{m.email || "Немає email"}</div>
                    </td>

                    <td className="px-6 py-4">
                      <span className={`text-xs font-medium ${m.isActive ? "text-green-600" : "text-red-600"}`}>
                        {m.isActive ? "Активний" : "Деактивований"}
                      </span>
                    </td>

                    <td className="px-6 py-4">
                      {!masterSchedule || masterSchedule.isFree ? (
                        <span className="text-xs text-gray-400">Немає записів</span>
                      ) : (
                        <div className="flex flex-col gap-1 max-w-[160px]">
                          {masterSchedule.slots.map((slot, idx) => (
                            <div key={idx} className="text-xs text-gray-700 bg-gray-100 px-2 py-1 rounded font-mono text-center">
                              {slot.start} - {slot.end}
                            </div>
                          ))}
                        </div>
                      )}
                    </td>

                    <td className="px-6 py-4 text-gray-800 font-medium">
                      {m.totalCompletedBookings}
                    </td>

                    <td className="px-6 py-4 max-w-xs">
                      <div className="flex flex-wrap gap-1">
                        {m.specializations.map((spec) => (
                          <span key={spec.id} className="inline-flex items-center gap-1 text-xs border border-gray-200 text-gray-600 px-2 py-0.5 rounded group bg-white">
                            {spec.title}
                            <button onClick={() => handleRemoveSpec(m.id, spec.id)} className="text-gray-400 hover:text-red-500 hidden group-hover:inline">
                              <X className="w-3 h-3"/>
                            </button>
                          </span>
                        ))}
                      </div>
                      
                      {addingTo === m.id ? (
                        <select 
                          autoFocus
                          onBlur={() => setAddingTo(null)}
                          onChange={(e) => { if(e.target.value) handleAddSpec(m.id, parseInt(e.target.value)) }}
                          className="mt-2 w-full text-xs border border-gray-300 rounded p-1 outline-none"
                        >
                          <option value="">Оберіть...</option>
                          {allServices.filter(s => !m.specializations.some(ms => ms.id === s.id)).map(s => <option key={s.id} value={s.id}>{s.title}</option>)}
                        </select>
                      ) : (
                        <button onClick={() => setAddingTo(m.id)} className="text-xs text-blue-600 hover:underline mt-1 block">+ Додати послугу</button>
                      )}
                    </td>

                    <td className="px-6 py-4 text-right">
                      <button 
                        onClick={() => handleToggleStatus(m.id)}
                        className={`text-xs font-medium px-3 py-1.5 rounded border transition-colors ${
                          m.isActive ? "border-red-200 text-red-600 hover:bg-red-50" : "border-green-200 text-green-600 hover:bg-green-50"
                        }`}
                      >
                        {m.isActive ? "Деактивувати" : "Активувати"}
                      </button>
                    </td>

                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}