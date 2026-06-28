import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api/api";
import { useAuth } from "../context/AuthContext";
import { User, Phone, Mail, CheckCircle, X } from "lucide-react";
import { useBookings } from "../hooks/useBookings";
import { useRecommendations } from "../hooks/useRecommendations";
import CarSection from "./profile/CarSection";
import OrdersSection from "./profile/OrdersSection";
import RecommendationsSection from "./profile/RecommendationsSection";

export default function Profile() {
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const userId = user?.id;

  const [profile, setProfile] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isEditingCar, setIsEditingCar] = useState(false);
  const [isSavingCar, setIsSavingCar] = useState(false);
  const [carData, setCarData] = useState({ carBrand: "", carModel: "", carYear: "", carEngine: "", carPlate: "" });
  const [ordersTab, setOrdersTab] = useState("history");

  const [dismissedIds, setDismissedIds] = useState(() => {
    try {
      return JSON.parse(localStorage.getItem("dismissedNotifications") || "[]");
    } catch { return []; }
  });

  const { bookings, displayBookings, handleMockPayment, isLoading: isBookingsLoading } = useBookings(userId, ordersTab);
  const recs = useRecommendations(userId);

  useEffect(() => {
    if (!userId) { setIsLoading(false); return; }
    setIsLoading(true);
    api.get(`/Profile/${userId}`).then(data => {
      setProfile(data);
      setCarData({ carBrand: data?.carBrand || "", carModel: data?.carModel || "", carYear: data?.carYear || "", carEngine: data?.carEngine || "", carPlate: data?.carPlate || "" });
    }).catch(err => {
      if (err.message.includes('401')) logout();
    }).finally(() => setIsLoading(false));
  }, [userId, logout]);

  const handleCarSave = async (extraData = {}) => {
    if (!userId || !profile) return;
    setIsSavingCar(true);
    try {
      const payload = { ...profile, ...carData, ...extraData, carYear: carData.carYear ? parseInt(carData.carYear, 10) : null };
      await api.post(`/Profile/${userId}`, payload);
      setProfile(prev => ({ ...prev, ...carData, ...extraData }));
      setIsEditingCar(false);
    } catch (err) { console.error(err); } finally { setIsSavingCar(false); }
  };

  const handleCarCancel = () => {
    setCarData({ carBrand: profile?.carBrand || "", carModel: profile?.carModel || "", carYear: profile?.carYear || "", carEngine: profile?.carEngine || "", carPlate: profile?.carPlate || "" });
    setIsEditingCar(false);
  };

  const handleDismiss = (id) => {
    setDismissedIds(prev => {
      const newIds = [...prev, id];
      localStorage.setItem("dismissedNotifications", JSON.stringify(newIds));
      return newIds;
    });
  };

  const handleClearAll = (idsToClear) => {
    setDismissedIds(prev => {
      const newIds = [...prev, ...idsToClear];
      localStorage.setItem("dismissedNotifications", JSON.stringify(newIds));
      return newIds;
    });
  };

  // ВИПРАВЛЕНО: Додано підтримку українських статусів
  const activeNotifications = (() => {
    // Фільтруємо підтверджені та завершені (українською та англійською)
    const validBookings = bookings.filter(b => 
      b.status === "confirmed" || b.status === "Підтверджено" || 
      b.status === "completed" || b.status === "Завершено"
    );
    
    const groupedMap = new Map();

    validBookings.forEach(b => {
      const key = `${b.bookingDate}-${b.bookingTime}`;
      if (!groupedMap.has(key)) {
        groupedMap.set(key, b);
      } else {
        const existing = groupedMap.get(key);
        const isExistingCompleted = existing.status === "completed" || existing.status === "Завершено";
        const isCurrentCompleted = b.status === "completed" || b.status === "Завершено";
        
        if (!isExistingCompleted && isCurrentCompleted) {
          groupedMap.set(key, b);
        }
      }
    });

    return Array.from(groupedMap.values())
      .sort((a, b) => new Date(b.bookingDate) - new Date(a.bookingDate))
      .slice(0, 10)
      .map(b => {
        let dateStr = "";
        if (b.bookingDate) {
          try {
            dateStr = new Date(b.bookingDate).toLocaleDateString('uk-UA', { day: 'numeric', month: 'long' });
          } catch (e) {}
        }
        const timeStr = b.bookingTime ? b.bookingTime.slice(0, 5) : "";
        
        // ВИПРАВЛЕНО: Перевіряємо статус обома мовами
        const isConfirmed = b.status === "confirmed" || b.status === "Підтверджено";
        const statusText = isConfirmed ? "Запис підтверджено" : "Роботу завершено";
        
        return {
          id: `notif-${b.bookingDate}-${b.bookingTime}`,
          message: `${statusText}${dateStr ? ` на ${dateStr}` : ""}${timeStr ? ` о ${timeStr}` : ""}`
        };
      })
      .filter(n => !dismissedIds.includes(n.id));
  })();

  if (isLoading || isBookingsLoading) return (
    <div className="d-flex align-items-center justify-content-center" style={{ minHeight: '60vh' }}>
      <div className="spinner-border text-orange" role="status"></div>
    </div>
  );

  if (!userId) return (
    <div className="container mt-5 pt-5">
      <div className="text-center p-5 mx-auto rounded-4" style={{ maxWidth: '32rem', borderStyle: 'dashed' }}>
        <User className="mx-auto text-secondary mb-3 opacity-25" size={48} />
        <h2 className="fw-bold mb-3">Увійдіть в систему</h2>
        <button onClick={() => navigate("/")} className="btn btn-modern-orange text-white px-5 py-2 rounded-3 fw-semibold border-0">На головну</button>
      </div>
    </div>
  );

  return (
    <div className="container py-5">
      <div className="d-flex flex-column flex-md-row align-items-start align-items-md-center justify-content-between gap-3 rounded-4 bg-white p-4 border shadow-sm mb-4">
        <div className="d-flex align-items-center gap-3">
          <div className="d-flex align-items-center justify-content-center rounded-circle bg-light border" style={{ width: '3.5rem', height: '3.5rem' }}>
            <User size={28} className="text-secondary" />
          </div>
          <h1 className="h4 fw-bold mb-0">{profile?.firstName} {profile?.lastName || ""}</h1>
        </div>
        <div className="d-flex flex-wrap align-items-center gap-3">
          <div className="d-flex align-items-center gap-2 small text-secondary">
            <Phone size={16} className="text-secondary" />
            <span>{profile?.phone || "Не вказано"}</span>
          </div>
          <div className="d-flex align-items-center gap-2 small text-secondary">
            <Mail size={16} className="text-secondary" />
            <span>{profile?.email || "Не вказано"}</span>
          </div>
        </div>
      </div>

      <div className="row g-4">
        <div className="col-lg-8 d-flex flex-column gap-4">
          <CarSection 
            profile={profile} isEditingCar={isEditingCar} setIsEditingCar={setIsEditingCar} 
            isSavingCar={isSavingCar} carData={carData} setCarData={setCarData} 
            handleCarSave={handleCarSave} handleCarCancel={handleCarCancel} userId={userId}
          />
          <OrdersSection 
            ordersTab={ordersTab} setOrdersTab={setOrdersTab} 
            displayBookings={displayBookings} handleMockPayment={handleMockPayment} 
          />
        </div>

        <div className="col-lg-4 d-flex flex-column gap-4">
          <div className="rounded-4 bg-white p-4 border shadow-sm" style={{ borderWidth: '2px', borderColor: '#fb923c' }}>
            <div className="d-flex justify-content-between align-items-center mb-3">
              <h3 className="fw-bold mb-0">Повідомлення</h3>
              {activeNotifications.length > 0 && (
                <button 
                  onClick={() => handleClearAll(activeNotifications.map(n => n.id))} 
                  className="btn btn-sm btn-link text-decoration-none text-muted hover-orange p-0"
                >
                  Очистити все
                </button>
              )}
            </div>
            
            {activeNotifications.length > 0 ? (
              activeNotifications.map(n => (
                <div key={n.id} className="d-flex align-items-start gap-3 rounded-3 p-3 mb-2 bg-soft-success border border-success border-opacity-25">
                  <CheckCircle size={20} className="text-success flex-shrink-0 mt-1" />
                  <p className="small text-dark mb-0 flex-grow-1">{n.message}</p>
                  <button onClick={() => handleDismiss(n.id)} className="btn btn-sm p-0 text-secondary hover-dark border-0 flex-shrink-0" title="Прибрати">
                    <X size={16} />
                  </button>
                </div>
              ))
            ) : (
              <p className="small text-muted">Немає нових повідомлень</p>
            )}
          </div>
          
          <RecommendationsSection recs={recs} bookings={bookings} />
        </div>
      </div>
    </div>
  );
}