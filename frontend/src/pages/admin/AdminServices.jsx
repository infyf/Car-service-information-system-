import { useState, useEffect } from "react";
import { X, Pencil } from "lucide-react";
import api from "../../api/api";

export default function AdminServices() {
  const [services, setServices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  

  const [addingTo, setAddingTo] = useState(null);
  const [newSubData, setNewSubData] = useState({ title: "", priceFrom: "", durationMinutes: "60" });


  const [editingDescId, setEditingDescId] = useState(null);
  const [editDescData, setEditDescData] = useState("");

  const [editingTitleId, setEditingTitleId] = useState(null);
  const [editTitleData, setEditTitleData] = useState("");


  const [editingSubId, setEditingSubId] = useState(null);
  const [editSubData, setEditSubData] = useState({ title: "", priceFrom: "", durationMinutes: "" });

  const fetchServices = async () => {
    setError(null);
    setLoading(true);
    try {
      const res = await api.get("/Admin/services");
      const rawData = res.data !== undefined ? res.data : res;
      setServices(Array.isArray(rawData) ? rawData : []);
    } catch (err) {
      setError(err.response?.data?.message || "Помилка завантаження послуг");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchServices();
  }, []);

  // === НОВІ ФУНКЦІЇ НАЗВИ ===
  const startEditTitle = (service) => {
    setEditingTitleId(service.id);
    setEditTitleData(service.title || "");
  };

  const saveTitle = async (id) => {
    if (!editTitleData.trim()) return alert("Назва не може бути порожньою");
    try {
      await api.put(`/Admin/services/${id}/title`, { title: editTitleData });
      setEditingTitleId(null);
      fetchServices(); 
    } catch (err) {
      alert("Помилка збереження назви");
    }
  };

  // === ФУНКЦІЇ ОПИСУ ===
  const startEditDesc = (service) => {
    setEditingDescId(service.id);
    setEditDescData(service.description || "");
  };

  const saveDesc = async (id) => {
    try {
      await api.put(`/Admin/services/${id}/description`, { description: editDescData });
      setEditingDescId(null);
      fetchServices(); 
    } catch (err) {
      alert("Помилка збереження опису");
    }
  };

  // === ФУНКЦІЇ ПІДКАТЕГОРІЙ ===
  const handleRemoveSubcategory = async (serviceId, itemId) => {
    if (!window.confirm("Видалити цю підкатегорію?")) return;
    try {
      await api.delete(`/Admin/items/${itemId}`);
      setServices(services.map(s => 
        s.id === serviceId 
          ? { ...s, items: s.items.filter(i => i.id !== itemId) } 
          : s
      ));
    } catch (err) {
      alert("Помилка видалення");
    }
  };

  const handleAddSubcategory = async (serviceId) => {
    if (!newSubData.title.trim()) return alert("Введіть назву підкатегорії");
    try {
      const payload = {
        title: newSubData.title,
        priceFrom: parseFloat(newSubData.priceFrom) || 0,
        durationMinutes: parseInt(newSubData.durationMinutes) || 60
      };
      await api.post(`/Admin/services/${serviceId}/items`, payload);
      setAddingTo(null);
      setNewSubData({ title: "", priceFrom: "", durationMinutes: "60" });
      fetchServices();
    } catch (err) {
      alert("Помилка додавання");
    }
  };

  const startEditSub = (item) => {
    setEditingSubId(item.id);
    setEditSubData({ 
      title: item.title, 
      priceFrom: item.priceFrom, 
      durationMinutes: item.durationMinutes 
    });
    setAddingTo(null); 
  };

  const saveSub = async (id) => {
    try {
      const payload = {
        ...editSubData,
        priceFrom: parseFloat(editSubData.priceFrom) || 0,
        durationMinutes: parseInt(editSubData.durationMinutes) || 60
      };
      await api.put(`/Admin/items/${id}`, payload);
      setEditingSubId(null);
      fetchServices();
    } catch (err) {
      alert("Помилка збереження підкатегорії");
    }
  };

  const handleDeleteCategory = async (id) => {
    if (!window.confirm("Видалити всю категорію та всі її підкатегорії?")) return;
    try {
      await api.delete(`/Admin/services/${id}`);
      setServices(services.filter(s => s.id !== id));
    } catch (err) {
      alert("Помилка видалення категорії");
    }
  };

  if (loading) return <div className="text-gray-500 text-sm mt-10">Завантаження...</div>;
  if (error) return <div className="text-red-600 text-sm mt-10">{error}</div>;

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-200">
      
      <div className="p-5 border-b border-gray-200 flex justify-between items-center">
        <h2 className="text-xl font-semibold text-gray-800">Каталог послуг</h2>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full text-sm text-left">
          <thead className="text-xs text-gray-500 uppercase bg-gray-50 border-b border-gray-200">
            <tr>
              <th className="px-6 py-3 font-medium">Послуга</th>
              <th className="px-6 py-3 font-medium">Загальний опис</th>
              <th className="px-6 py-3 font-medium">Підкатегорії (Ціна / Час)</th>
              <th className="px-6 py-3 font-medium text-right">Дії</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {services.length === 0 ? (
              <tr><td colSpan="4" className="text-center py-10 text-gray-400">Список порожній</td></tr>
            ) : (
              services.map((service) => (
                <tr key={service.id} className="hover:bg-gray-50/50 transition-colors">
                  
                  <td className="px-6 py-4">
                    {editingTitleId === service.id ? (
                      <div className="flex flex-col gap-2">
                        <input 
                          autoFocus
                          type="text" 
                          className="px-3 py-1.5 text-xs border border-gray-300 rounded-md outline-none focus:border-gray-800" 
                          value={editTitleData} 
                          onChange={e => setEditTitleData(e.target.value)} 
                        />
                        <div className="flex gap-3">
                          <button onClick={() => saveTitle(service.id)} className="text-xs font-medium text-blue-600 hover:underline">Зберегти</button>
                          <button onClick={() => setEditingTitleId(null)} className="text-xs text-gray-500 hover:underline">Скасувати</button>
                        </div>
                      </div>
                    ) : (
                      <>
                        <div className="font-medium text-gray-800">{service.title}</div>
                        <button 
                          onClick={() => startEditTitle(service)} 
                          className="text-xs text-blue-600 hover:underline mt-1 block"
                        >
                          Редагувати
                        </button>
                      </>
                    )}
                  </td>


                  <td className="px-6 py-4 max-w-md">
                    {editingDescId === service.id ? (
                      <div className="flex flex-col gap-2">
                        <textarea 
                          autoFocus
                          className="px-3 py-2 text-xs border border-gray-300 rounded-md outline-none focus:border-gray-800 h-20 resize-none" 
                          value={editDescData} 
                          onChange={e => setEditDescData(e.target.value)} 
                        />
                        <div className="flex gap-3">
                          <button onClick={() => saveDesc(service.id)} className="text-xs font-medium text-blue-600 hover:underline">Зберегти</button>
                          <button onClick={() => setEditingDescId(null)} className="text-xs text-gray-500 hover:underline">Скасувати</button>
                        </div>
                      </div>
                    ) : (
                      <>
                        <div className="text-xs text-gray-500 leading-relaxed">
                          {service.description || <span className="italic text-gray-400">Немає опису</span>}
                        </div>
                        <button 
                          onClick={() => startEditDesc(service)} 
                          className="text-xs text-blue-600 hover:underline mt-1 block"
                        >
                          Редагувати
                        </button>
                      </>
                    )}
                  </td>

                  <td className="px-6 py-4 max-w-xs">
                    <div className="flex flex-wrap gap-1">
                      {service.items
                        .filter(item => item.id !== editingSubId)
                        .map((item) => (
                        <span 
                          key={item.id} 
                          className="inline-flex items-center gap-1.5 text-xs border border-gray-200 text-gray-600 px-2.5 py-1 rounded group bg-white"
                        >
                          <span className="font-medium">{item.title}</span>
                          <span className="text-gray-400">({item.priceFrom}грн, {item.durationMinutes}хв)</span>
                          
                          <button 
                            onClick={() => startEditSub(item)} 
                            className="text-gray-400 hover:text-blue-500 hidden group-hover:inline ml-1"
                          >
                            <Pencil className="w-3 h-3"/>
                          </button>

                          <button 
                            onClick={() => handleRemoveSubcategory(service.id, item.id)} 
                            className="text-gray-400 hover:text-red-500 hidden group-hover:inline ml-1"
                          >
                            <X className="w-3 h-3"/>
                          </button>
                        </span>
                      ))}
                    </div>
                    
                    {editingSubId && service.items.some(i => i.id === editingSubId) && (
                      <form 
                        onSubmit={(e) => { e.preventDefault(); saveSub(editingSubId); }} 
                        className="mt-2 flex flex-col gap-2 bg-blue-50 p-2 rounded-md border border-blue-200"
                      >
                        <div className="flex gap-2">
                          <input required type="text" className="flex-1 px-2 py-1 text-xs border border-gray-300 rounded outline-none focus:border-gray-800" value={editSubData.title} onChange={e => setEditSubData({...editSubData, title: e.target.value})} autoFocus />
                          <input type="number" placeholder="Ціна" className="w-16 px-2 py-1 text-xs border border-gray-300 rounded outline-none focus:border-gray-800" value={editSubData.priceFrom} onChange={e => setEditSubData({...editSubData, priceFrom: e.target.value})} />
                          <input type="number" placeholder="Хв" className="w-14 px-2 py-1 text-xs border border-gray-300 rounded outline-none focus:border-gray-800" value={editSubData.durationMinutes} onChange={e => setEditSubData({...editSubData, durationMinutes: e.target.value})} />
                        </div>
                        <div className="flex gap-2 justify-end">
                          <button type="submit" className="text-xs font-medium text-blue-600 hover:underline">Зберегти</button>
                          <button type="button" onClick={() => setEditingSubId(null)} className="text-xs text-gray-500 hover:underline">Скасувати</button>
                        </div>
                      </form>
                    )}

                    {addingTo === service.id && (
                      <form 
                        onSubmit={(e) => { e.preventDefault(); handleAddSubcategory(service.id); }} 
                        className="mt-2 flex flex-col gap-2 bg-gray-50 p-2 rounded-md border border-gray-200"
                      >
                        <div className="flex gap-2">
                          <input required type="text" placeholder="Назва" className="flex-1 px-2 py-1 text-xs border border-gray-300 rounded outline-none focus:border-gray-800" value={newSubData.title} onChange={e => setNewSubData({...newSubData, title: e.target.value})} autoFocus />
                          <input type="number" placeholder="Ціна" className="w-16 px-2 py-1 text-xs border border-gray-300 rounded outline-none focus:border-gray-800" value={newSubData.priceFrom} onChange={e => setNewSubData({...newSubData, priceFrom: e.target.value})} />
                          <input type="number" placeholder="Хв" className="w-14 px-2 py-1 text-xs border border-gray-300 rounded outline-none focus:border-gray-800" value={newSubData.durationMinutes} onChange={e => setNewSubData({...newSubData, durationMinutes: e.target.value})} />
                        </div>
                        <div className="flex gap-2 justify-end">
                          <button type="submit" className="text-xs font-medium text-blue-600 hover:underline">Додати</button>
                          <button type="button" onClick={() => setAddingTo(null)} className="text-xs text-gray-500 hover:underline">Скасувати</button>
                        </div>
                      </form>
                    )}

                    {addingTo !== service.id && !(editingSubId && service.items.some(i => i.id === editingSubId)) && (
                      <button onClick={() => setAddingTo(service.id)} className="text-xs text-blue-600 hover:underline mt-1.5 block">
                        + Додати підкатегорію
                      </button>
                    )}
                  </td>

                  <td className="px-6 py-4 text-right">
                    <button 
                      onClick={() => handleDeleteCategory(service.id)}
                      className="text-xs font-medium px-3 py-1.5 rounded border border-red-200 text-red-600 hover:bg-red-50 transition-colors"
                    >
                      Видалити
                    </button>
                  </td>

                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}