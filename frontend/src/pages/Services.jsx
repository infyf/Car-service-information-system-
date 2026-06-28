import { useEffect, useState } from "react";
import ServiceSummary from "./ServiceSummary";
import api from "../api/api";

export default function Services() {
  const [itemsData, setItemsData] = useState([]);
  const [activeServiceId, setActiveServiceId] = useState(null);
  const [selectedItems, setSelectedItems] = useState([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    api.get("/Services/with-items")
      .then(data => {
        setItemsData(data);
        if (data.length > 0) setActiveServiceId(data[0].id);
        setIsLoading(false);
      })
      .catch(err => { console.error(err); setIsLoading(false); });
  }, []);

  const activeService = itemsData.find(s => s.id === activeServiceId);
  const toggleItem = (item) => setSelectedItems(prev => prev.some(i => i.id === item.id) ? prev.filter(i => i.id !== item.id) : [...prev, item]);

  if (isLoading) return <div className="text-center py-20">Завантаження послуг...</div>;

  return (
    <div className="max-w-7xl mx-auto px-4 py-10">
      <div className="grid grid-cols-12 gap-6">
        <aside className="col-span-3">
          <div className="border border-gray-200 rounded-xl bg-white overflow-hidden shadow-sm">
            <div className="px-4 py-3 border-b border-gray-100 bg-gray-50"><h3 className="font-semibold text-gray-900 text-sm">Категорії</h3></div>
            <div className="p-2">{itemsData.map(service => (<button key={service.id} onClick={() => setActiveServiceId(service.id)} className={`w-full text-left px-3 py-2.5 rounded-lg mb-1 text-sm transition-all ${activeServiceId === service.id ? "bg-orange-500 text-white shadow-md" : "text-gray-700 hover:bg-orange-50 hover:text-orange-600"}`}>{service.title}</button>))}</div>
          </div>
        </aside>
        <main className="col-span-5">
          <div className="border border-gray-200 rounded-xl bg-white overflow-hidden shadow-sm">
            {activeService ? (<><div className="px-5 py-3 border-b border-gray-100 bg-gray-50"><h2 className="font-semibold text-gray-900">{activeService.title}</h2></div><div className="p-5">{(!activeService.items || activeService.items.length === 0) ? <p className="text-gray-400 text-sm italic">Порожньо</p> : activeService.items.map(item => { const isSelected = selectedItems.some(i => i.id === item.id); return (<div key={item.id} onClick={() => toggleItem(item)} className={`flex items-center justify-between py-3 px-3 mb-2 border rounded-xl cursor-pointer transition-all ${isSelected ? "border-orange-500 bg-orange-50 shadow-sm" : "border-gray-100 hover:border-orange-200"}`}><div className="flex items-center gap-3"><div className={`w-5 h-5 rounded border-2 flex items-center justify-center ${isSelected ? "bg-orange-500 border-orange-500" : "border-gray-300"}`}>{isSelected && <svg className="w-3 h-3 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={4}><path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" /></svg>}</div><span className={`text-sm ${isSelected ? "text-orange-900 font-medium" : "text-gray-700"}`}>{item.title}</span></div><span className={`text-sm font-semibold ${isSelected ? "text-orange-600" : "text-gray-500"}`}>{item.priceFrom} ₴</span></div>); })}</div></>) : (<div className="p-10 text-center text-gray-400">Оберіть категорію</div>)}
          </div>
        </main>
        <ServiceSummary selectedItems={selectedItems} />
      </div>
    </div>
  );
}