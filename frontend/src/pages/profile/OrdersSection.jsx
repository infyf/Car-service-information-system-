import { Clock } from "lucide-react";
import { BOOKING_STATUS } from "../../constants/statuses";
import api from "../../api/api";

function formatDate(dateStr) {
  if (!dateStr) return "---";
  try {
    return new Date(dateStr).toLocaleDateString("uk-UA", { day: "2-digit", month: "2-digit", year: "numeric" });
  } catch { return dateStr; }
}

export default function OrdersSection({ ordersTab, setOrdersTab, displayBookings }) {
  const findBookingId = (obj) => {
    if (!obj) return null;
    if (obj.ids && Array.isArray(obj.ids) && obj.ids.length > 0) { const id = parseInt(obj.ids[0], 10); if (!isNaN(id)) return id; }
    if (obj.parentId) return parseInt(obj.parentId, 10);
    if (obj.mainOrderId) return parseInt(obj.mainOrderId, 10);
    for (const key in obj) {
      if (obj.hasOwnProperty(key)) {
        const val = obj[key];
        if (typeof val === 'number' && val > 1000) return val;
        if (typeof val === 'string' && !isNaN(val) && parseInt(val, 10) > 1000) return parseInt(val, 10);
        if (Array.isArray(val)) { for (let i = 0; i < val.length; i++) { const arrVal = parseInt(val[i], 10); if (!isNaN(arrVal) && arrVal > 1000) return arrVal; } }
      }
    }
    return null;
  };

  const handleRealPayment = async (bookingObj) => {
    try {
      const finalId = findBookingId(bookingObj);
      if (!finalId) { alert("Помилка: Не вдалося знайти ID замовлення."); return; }
      const response = await api.post("/payment/generate", { orderId: finalId }); 
      const { data, signature } = response;
      if (!data || !signature) { alert("Бекенд не повернув дані для оплати"); return; }
      if (typeof window.LiqPayCheckout === 'undefined') { alert("Скрипт LiqPay заблоковано! Вимкніть AdBlock."); return; }
      window.LiqPayCheckout.init({ data, signature, locale: "uk", mode: "popup" });
    } catch (error) {
      console.error("ПОМИЛКА:", error);
      alert("Сталася помилка: " + error.message);
    }
  };

  return (
    <div className="rounded-4 bg-white p-4 border shadow-sm">
      {/* Таби */}
      <div className="btn-group mb-4" role="group">
        <button
          onClick={() => setOrdersTab("history")}
          className={`btn ${ordersTab === "history" ? "btn-modern-orange text-white" : "btn-light border"}`}
        >
          Історія
        </button>
        <button
          onClick={() => setOrdersTab("active")}
          className={`btn ${ordersTab === "active" ? "btn-modern-orange text-white" : "btn-light border"}`}
        >
          Активні
        </button>
      </div>

      {displayBookings.length > 0 ? (
        <div className="table-responsive">
          <table className="table table-borderless table-hover align-middle mb-0">
            <thead>
              <tr className="border-bottom">
                <th className="text-muted small text-uppercase fw-medium py-3 ps-0" style={{fontSize: '0.75rem'}}>Дата</th>
                <th className="text-muted small text-uppercase fw-medium py-3" style={{fontSize: '0.75rem'}}>Час</th>
                <th className="text-muted small text-uppercase fw-medium py-3 w-100" style={{fontSize: '0.75rem'}}>Послуги</th>
                <th className="text-muted small text-uppercase fw-medium py-3" style={{fontSize: '0.75rem'}}>Статус</th>
                <th className="text-muted small text-uppercase fw-medium py-3 text-end pe-0" style={{fontSize: '0.75rem'}}>Сума</th>
              </tr>
            </thead>
            <tbody>
              {displayBookings.map((b, idx) => (
                <tr key={idx} className="border-bottom">
                  <td className="py-3 ps-0 fw-medium">{formatDate(b.bookingDate)}</td>
                  <td className="py-3 text-secondary d-flex align-items-center gap-1.5">
                    <Clock size={14} className="text-secondary" />
                    {b.displayTime}
                  </td>
                  <td className="py-3">
                    {b.serviceObjects.map((s, i) => (
                      <span key={i} className={`small ${ordersTab === "active" && s.status === BOOKING_STATUS.COMPLETED ? "text-decoration-line-through text-muted" : ""}`}>
                        {s.name}{i < b.serviceObjects.length - 1 ? ", " : ""}
                      </span>
                    ))}
                  </td>
                  
                  <td className="py-3">
                    {b.aggregateStatus === BOOKING_STATUS.COMPLETED && b.paymentStatus !== "success" ? (
                      <button onClick={() => handleRealPayment(b)} className="btn btn-sm btn-modern-orange text-white fw-bold px-3 py-1 border-0">
                        Оплатити
                      </button>
                    ) : b.paymentStatus === "success" ? (
                      <span className="badge-soft-success rounded-pill px-3 py-1 small fw-semibold">
                        Оплачено ✓
                      </span>
                    ) : (
                      <span className={`badge-soft-secondary rounded-pill px-3 py-1 small fw-semibold`}>
                        {b.statusLabel}
                      </span>
                    )}
                  </td>
                  
                  <td className="py-3 text-end fw-medium pe-0">{b.displayPrice}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <div className="py-5 text-center small text-muted">
          {ordersTab === "active" ? "Немає активних записів" : "Порожньо"}
        </div>
      )}
    </div>
  );
}