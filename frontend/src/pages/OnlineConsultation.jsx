import { useState, useEffect } from "react";
import api from "../api/api";
import { useAuth } from "../context/AuthContext";
import { TIME_SLOTS } from "../constants/config";
import { Paperclip, X, Film, Image as ImageIcon } from "lucide-react"; 

export default function OnlineConsultation() {
  const { user } = useAuth();
  const [problem, setProblem] = useState("");
  const [date, setDate] = useState("");
  const [time, setTime] = useState("");
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");
  const [file, setFile] = useState(null);
  

  const [hasConsultation, setHasConsultation] = useState(false);


  useEffect(() => {
    const checkExistingConsultation = async () => {
      if (!date || !user) {
        setHasConsultation(false);
        return;
      }

      try {
        const response = await api.get(`/Bookings/user/${user.id}`);

        const consultations = response.filter(b => b.bookingType === "consultation");
        

        const exists = consultations.some(c => {
          // "2026-04-17T00:00:00") в "YYYY-MM-DD"
          const dbDate = new Date(c.bookingDate).toISOString().split('T')[0];
          return dbDate === date;
        });

        setHasConsultation(exists);
      } catch (err) {
        console.error("Помилка перевірки консультацій:", err);
      }
    };


    const timer = setTimeout(checkExistingConsultation, 300);
    return () => clearTimeout(timer);
  }, [date, user]);

  if (!user) {
    return (<div className="max-w-4xl mx-auto mt-32 p-6 border border-orange-300 bg-orange-50 text-orange-600 rounded-lg p-4 text-center">Для створення консультації необхідно увійти в акаунт</div>);
  }

  const submitConsultation = async () => {
    if (!problem || !date || !time) { setMessage("Заповніть всі поля"); return; }
    if (hasConsultation) { setMessage("Ви вже маєте консультацію на цю дату"); return; }
    
    setLoading(true); setMessage("");
    try {
      const formData = new FormData();
      formData.append("UserId", user.id);
      formData.append("ProblemDescription", problem);
      formData.append("ConsultationDate", date);
      formData.append("ConsultationTime", time);
      
      if (file) {
        formData.append("attachmentFile", file);
      }

      const response = await api.postFormData("/consultations", formData);
      
      if (response.message) setMessage(response.message);
      else { 
        setMessage("Консультацію успішно створено!"); 
        setProblem(""); 
        setDate(""); 
        setTime(""); 
        setFile(null); 
        setHasConsultation(false); 
      }
    } catch (err) {
     
      if (err.response?.data?.message) {
         setMessage(err.response.data.message);
      } else {
         setMessage(`Помилка: ${err.message}`);
      }
    } finally { setLoading(false); }
  };

  const isVideoFile = file && (file.type.startsWith("video/"));

  return (
    <div className="max-w-7xl mx-auto px-4 mt-32">
      <h1 className="text-3xl font-bold text-center mb-2">Онлайн-консультація</h1>
      <p className="text-center text-gray-500 mb-10">Опишіть проблему та прикріпіть відео- майстер підбере послуги та надасть рекомендації.</p>
      
      <div className="grid md:grid-cols-2 gap-8">
        <div className="bg-white rounded-xl border p-6 flex flex-col">
          <h3 className="font-semibold mb-3">Опис проблеми</h3>
          <textarea 
            value={problem} 
            onChange={(e) => setProblem(e.target.value)} 
            placeholder="Детально опишіть проблему (стукає, скрипить, горить лампочка...)" 
            className="w-full h-40 border rounded-lg p-4 resize-none focus:ring-2 focus:ring-orange-400 outline-none" 
          />
          
          <div className="mt-1">
            <h3 className="font-semibold mb-1 flex items-center gap-2">
              <Paperclip className="h-4 w-4" /> Відео або фото проблеми (необов'язково)
            </h3>
            
            {!file ? (
              <label className="flex flex-col items-center justify-center w-full h-32 border-2 border-dashed rounded-lg cursor-pointer hover:bg-gray-50 transition">
                <div className="flex flex-col items-center justify-center pt-5 pb-6">
                  {isVideoFile ? <Film className="w-8 h-8 text-gray-400 mb-2" /> : <ImageIcon className="w-8 h-8 text-gray-400 mb-2" />}
                  <p className="mb-1 text-sm text-gray-500">Натисніть для вибору файлу</p>
                  <p className="text-xs text-gray-400">Відео (MP4) до 100 МБ або фото</p>
                </div>
                <input 
                  type="file" 
                  accept="image/*,video/*" 
                  onChange={(e) => setFile(e.target.files[0])} 
                  className="hidden" 
                />
              </label>
            ) : (
              <div className="flex items-center justify-between p-3 border rounded-lg bg-gray-50">
                <div className="flex items-center gap-3 overflow-hidden">
                  {isVideoFile ? <Film className="w-5 h-5 text-blue-500 flex-shrink-0" /> : <ImageIcon className="w-5 h-5 text-green-500 flex-shrink-0" />}
                  <div className="truncate">
                    <p className="text-sm font-medium text-gray-700 truncate">{file.name}</p>
                    <p className="text-xs text-gray-400">{(file.size / (1024 * 1024)).toFixed(1)} МБ</p>
                  </div>
                </div>
                <button 
                  type="button" 
                  onClick={() => setFile(null)} 
                  className="p-1 hover:bg-red-100 rounded-full text-red-500 transition"
                >
                  <X className="w-5 h-5" />
                </button>
              </div>
            )}
          </div>
        </div>

        <div className="bg-white rounded-xl border p-6">
          <h3 className="font-semibold mb-3">Оберіть дату</h3>
          <input type="date" value={date} onChange={(e) => setDate(e.target.value)} className="border rounded-lg p-3 w-full mb-6" />
       
          {hasConsultation && (
            <div className="mb-4 p-3 bg-yellow-50 border border-yellow-300 rounded-lg text-yellow-700 text-sm">
              На цю дату ви вже створили консультацію. Оберіть іншу дату.
            </div>
          )}

          <h3 className="font-semibold mb-3">Оберіть час</h3>
          <div className={`grid grid-cols-4 gap-2 ${hasConsultation ? "opacity-50 pointer-events-none" : ""}`}>
            {TIME_SLOTS.map(t => (
              <button 
                key={t} 
                onClick={() => setTime(t)} 
                className={`py-2 rounded-lg border text-sm transition ${time === t ? "bg-orange-500 text-white border-orange-500" : "hover:border-orange-400"}`}
              >
                {t}
              </button>
            ))}
          </div>
        </div>
      </div>
      
      {message && (
        <div className={`mt-6 text-center font-medium ${message.includes("Помилка") || message.includes("вже створили") ? "text-red-500" : "text-green-600"}`}>
          {message}
        </div>
      )}
      
      <div className="flex justify-center mt-10">
        <button 
          onClick={submitConsultation} 
          disabled={loading || hasConsultation} 
          className={`px-10 py-3 bg-gradient-to-r from-orange-500 to-red-600 text-white rounded-xl font-medium hover:shadow-lg transition ${loading || hasConsultation ? "opacity-50 cursor-not-allowed" : ""}`}
        >
          {loading ? "Завантаження..." : hasConsultation ? "Консультація на цю дату вже є" : "Відправити консультацію"}
        </button>
      </div>
    </div>
  );
}