import { useEffect, useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import api from "../api/api";
import { useAuth } from "../context/AuthContext";

const BACKEND_URL = "https://localhost:7064";

const getFileUrl = (url) => {
  if (!url) return null;
  if (url.startsWith("http")) return url;
  return `${BACKEND_URL}${url}`;
};

const isVideoUrl = (url) => {
  if (!url) return false;
  const lower = url.toLowerCase();
  return lower.endsWith(".mp4") || lower.endsWith(".mov") || lower.endsWith(".avi") || lower.endsWith(".webm");
};

const StatusBadge = ({ status }) => {
  const map = {
    new: { label: "Очікує", color: "#b45309", bg: "#fef3c7" },
    accepted: { label: "Прийнято", color: "#065f46", bg: "#d1fae5" },
  };
  const s = map[status] || { label: status, color: "#374151", bg: "#f3f4f6" };
  return (
    <span style={{
      fontSize: "0.7rem",
      fontWeight: 600,
      letterSpacing: "0.05em",
      textTransform: "uppercase",
      color: s.color,
      background: s.bg,
      padding: "3px 8px",
      borderRadius: "4px",
    }}>
      {s.label}
    </span>
  );
};

const MediaBadge = () => (
  <span style={{
    fontSize: "0.7rem",
    fontWeight: 600,
    letterSpacing: "0.05em",
    textTransform: "uppercase",
    color: "#1d4ed8",
    background: "#dbeafe",
    padding: "3px 8px",
    borderRadius: "4px",
  }}>
    Медіа
  </span>
);

export default function MasterPage() {
  const { user } = useAuth();
  const queryClient = useQueryClient();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [modalLoading, setModalLoading] = useState(false);
  const [activeConsultation, setActiveConsultation] = useState(null);
  const [services, setServices] = useState([]);
  const [selectedServices, setSelectedServices] = useState([]);

  const [completedIds, setCompletedIds] = useState(() => {
    try {
      return JSON.parse(localStorage.getItem("completedConsultations") || "[]");
    } catch {
      return [];
    }
  });

  const { data: consultations = [], isLoading: loading, error } = useQuery({
    queryKey: ["masterConsultations", user?.id],
    queryFn: () => api.get("/consultations/my-list"),
    refetchInterval: 15000,
    enabled: !!user?.id,
  });

  useEffect(() => {
    if (!isModalOpen) { setServices([]); return; }
    if (services.length > 0) return;
    api.get("/services/with-items")
      .then(data =>
        setServices(data.flatMap(s => s.items.map(item => ({ ...item, serviceTitle: s.title }))))
      )
      .catch(console.error);
  }, [isModalOpen, services.length]);

  const acceptMutation = useMutation({
    mutationFn: (id) => api.put(`/consultations/${id}/accept`),
    onSuccess: (data, id) => {
      if (!data.bookingId) return alert("Сервер не повернув ID запису");
      queryClient.setQueryData(["masterConsultations", user?.id], (oldData) =>
        oldData.map(c => c.id === id ? { ...c, status: "accepted", bookingId: data.bookingId } : c)
      );
    },
    onError: () => alert("Не вдалося прийняти"),
  });

  const openRecommendationModal = async (consultation) => {
    if (consultation.status !== "accepted") return;
    setActiveConsultation(consultation);
    setSelectedServices([]);
    setIsSuccess(false);
    setIsModalOpen(true);
    setModalLoading(true);

    if (!consultation.bookingId) {
      try {
        const data = await api.put(`/consultations/${consultation.id}/accept?userId=${user.id}`);
        if (data.bookingId) {
          const updated = { ...consultation, bookingId: data.bookingId };
          setActiveConsultation(updated);
          queryClient.setQueryData(["masterConsultations", user?.id], (oldData) =>
            oldData.map(c => c.id === consultation.id ? updated : c)
          );
        } else { throw new Error("No ID"); }
      } catch (e) {
        console.error(e);
        alert("Помилка отримання даних запису. Спробуйте оновити сторінку.");
        setIsModalOpen(false);
      }
    }
    setModalLoading(false);
  };

  const markAsCompleted = (id) => {
    setCompletedIds(prev => {
      const newIds = [...prev, id];
      localStorage.setItem("completedConsultations", JSON.stringify(newIds));
      return newIds;
    });
  };

  const submitRecsMutation = useMutation({
    mutationFn: () =>
      Promise.all(
        selectedServices.map(service =>
          api.post("/recommendations", {
            bookingId: activeConsultation.bookingId,
            serviceItemId: service.id,
            comment: `Рекомендація #${activeConsultation.id}`,
          })
        )
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["masterConsultations"] });
      markAsCompleted(activeConsultation.id);
      setIsSuccess(true);
    },
    onError: (err) => alert(`Помилка: ${err.message}`),
  });

  const closeModal = () => { setIsModalOpen(false); setIsSuccess(false); };

  if (!user) return (
    <div style={{ minHeight: "100vh", display: "flex", alignItems: "center", justifyContent: "center" }}>
      <p style={{ color: "#9ca3af" }}>Увійдіть як майстер</p>
    </div>
  );

  const activeItems = consultations.filter(c => !c.hasRecommendations && !completedIds.includes(c.id));
  const newCount = activeItems.filter(c => c.status === "new").length;

  return (
    <div className="master-page">
      <div className="page-inner">

        <div className="page-header">
          <div>
            <h1 className="page-title">Консультації</h1>
            <p className="page-subtitle">Активні запити, що очікують на обробку</p>
          </div>
          {newCount > 0 && (
            <span className="new-counter">{newCount} нових</span>
          )}
        </div>

        {loading && <p className="state-message">Завантаження…</p>}
        {error && <p className="state-message" style={{ color: "#dc2626" }}>Помилка завантаження. Перевірте з'єднання.</p>}

        {!loading && activeItems.length > 0 && activeItems.map(c => {
          const hasMedia = !!c.attachmentUrl;
          const carName = [c.carBrand, c.carModel, c.carYear ? String(c.carYear) : null].filter(Boolean).join(" ");
          const hasCar = !!(carName || c.carPlate);
          return (
            <div
              key={c.id}
              className={`consult-card ${c.status === "accepted" ? "is-accepted" : ""}`}
              onClick={() => openRecommendationModal(c)}
            >
              <div className="card-body">
                <div className="card-row-top">
                  <div className="card-left">
                    <div className="card-meta-line">
                      <span className="consult-id">#{c.id}</span>
                      <StatusBadge status={c.status} />
                      {hasMedia && <MediaBadge />}
                    </div>
                    <div className="client-name">{c.clientName}</div>
                  </div>

                  {c.status === "new" && (
                    <button
                      className="btn-accept"
                      disabled={acceptMutation.isLoading}
                      onClick={(e) => { e.stopPropagation(); acceptMutation.mutate(c.id); }}
                    >
                      {acceptMutation.isLoading ? "…" : "Прийняти"}
                    </button>
                  )}
                </div>

                <div className="card-details-line" style={{ marginTop: 8 }}>
                  <span>{c.date} · {c.time?.slice(0, 5)}</span>
                  <span>{c.phone}</span>
                </div>

                {hasCar && (
                  <div className="card-details-line" style={{ marginTop: 0, marginBottom: 10 }}>
                    {carName && <span>{carName}</span>}
                    {c.carPlate && (
                      <span style={{ fontWeight: 600, letterSpacing: "0.04em" }}>{c.carPlate}</span>
                    )}
                  </div>
                )}

                <p className="problem-text">{c.problem}</p>

                {c.status === "accepted" && (
                  <p className="hint-text">Натисніть, щоб додати рекомендовані послуги →</p>
                )}
              </div>

              {hasMedia && (
                <div className="media-block">
                  {isVideoUrl(c.attachmentUrl) ? (
                    <video controls src={getFileUrl(c.attachmentUrl)} />
                  ) : (
                    <img src={getFileUrl(c.attachmentUrl)} alt="Прикріплений матеріал" />
                  )}
                </div>
              )}
            </div>
          );
        })}

        {!loading && activeItems.length === 0 && (
          <div className="empty-state">
            <div className="empty-icon">
              <svg width="22" height="22" fill="none" stroke="#059669" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24">
                <polyline points="20 6 9 17 4 12" />
              </svg>
            </div>
            <h2 className="empty-title">Всі запити оброблено</h2>
            <p className="empty-sub">Нових консультацій на даний момент немає.</p>
          </div>
        )}
      </div>

      {isModalOpen && (
        <div className="modal-backdrop" onClick={closeModal}>
          <div className="modal-box" onClick={e => e.stopPropagation()}>

            <div className="modal-header">
              <div>
                <h2 className="modal-title">
                  {isSuccess ? "Готово" : "Рекомендовані послуги"}
                </h2>
                {!isSuccess && activeConsultation && (
                  <p className="modal-client">{activeConsultation.clientName} · #{activeConsultation.id}</p>
                )}
              </div>
              <button className="modal-close" onClick={closeModal} aria-label="Закрити">&times;</button>
            </div>

            {isSuccess ? (
              <div className="success-pane">
                <div className="success-icon">
                  <svg width="24" height="24" fill="none" stroke="#059669" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24">
                    <polyline points="20 6 9 17 4 12" />
                  </svg>
                </div>
                <h3 className="success-title">Рекомендації надіслано</h3>
                <p className="success-sub">
                  Клієнт <strong>{activeConsultation?.clientName}</strong> отримав перелік послуг.
                </p>
                <button className="btn-close-success" onClick={closeModal}>Закрити</button>
              </div>
            ) : (
              <>
                <div className="modal-body">
                  {modalLoading && (
                    <div className="modal-overlay-loading">
                      <div className="spinner" />
                    </div>
                  )}
                  <div className="services-grid">
                    {services.map(service => {
                      const selected = selectedServices.some(s => s.id === service.id);
                      return (
                        <div
                          key={service.id}
                          className={`service-item ${selected ? "selected" : ""}`}
                          onClick={() =>
                            setSelectedServices(prev =>
                              prev.some(s => s.id === service.id)
                                ? prev.filter(s => s.id !== service.id)
                                : [...prev, service]
                            )
                          }
                        >
                          <p className="service-title">{service.title}</p>
                          <p className="service-category">{service.serviceTitle}</p>
                          <span className="service-price">{service.priceFrom} грн</span>
                        </div>
                      );
                    })}
                  </div>
                </div>

                <div className="modal-footer">
                  <span className="selection-count">
                    Обрано: <b>{selectedServices.length}</b>
                  </span>
                  <button
                    className="btn-submit"
                    disabled={selectedServices.length === 0 || modalLoading || submitRecsMutation.isLoading}
                    onClick={() => submitRecsMutation.mutate()}
                  >
                    {submitRecsMutation.isLoading ? "Надсилання…" : "Надіслати"}
                  </button>
                </div>
              </>
            )}

          </div>
        </div>
      )}
    </div>
  );
}