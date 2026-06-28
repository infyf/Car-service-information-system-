import { useState } from "react";
import { Wrench, CheckCircle, AlertCircle, X, FileText } from "lucide-react";

export default function RecommendationsSection({ recs }) {
  const [loadingGroupId, setLoadingGroupId] = useState(null);
  const [successGroupId, setSuccessGroupId] = useState(null);
  const [error, setError] = useState("");
  
  const [modalGroup, setModalGroup] = useState(null);
  const [previewData, setPreviewData] = useState(null);
  const [isPreviewLoading, setIsPreviewLoading] = useState(false);

  const openModal = async (group) => {
    setModalGroup(group);
    setIsPreviewLoading(true);
    setError("");
    setPreviewData(null);
    const result = await recs.previewBatch({ recommendationIds: group.items.map(item => item.id), timeStrategy: "nearest" });
    if (result.isSuccess) setPreviewData(result);
    else setError(result.error);
    setIsPreviewLoading(false);
  };

  const handleFinalAccept = async () => {
    if (!modalGroup) return;
    setLoadingGroupId(modalGroup.bookingId);
    setError("");
    try {
      const result = await recs.acceptRecommendationsBatch({ recommendationIds: modalGroup.items.map(item => item.id), timeStrategy: "nearest" });
      if (result.isSuccess) {
        setSuccessGroupId(modalGroup.bookingId);
        setModalGroup(null);
        if (recs.fetchRecommendations) setTimeout(() => recs.fetchRecommendations(), 1500);
      } else { setError(result.error); }
    } catch (err) { setError(err.response?.data?.message || "Помилка"); } 
    finally { setLoadingGroupId(null); }
  };

  return (
    <>
      <div className="rounded-4 bg-white p-4 border shadow-sm">
        <div className="d-flex align-items-center gap-2 mb-4">
          <Wrench size={20} className="text-orange" />
          <h3 className="fw-bold mb-0">Рекомендації майстра</h3>
        </div>

        {recs.groupsList.length > 0 ? (
          recs.groupsList.map((group) => {
            const totalPrice = group.items.reduce((sum, item) => sum + (item.price || 0), 0);
            const isGroupSuccess = successGroupId === group.bookingId;

            return (
              <div key={group.bookingId} className={`rounded-3 border p-3 mb-3 transition-all ${isGroupSuccess ? 'border-success bg-soft-success' : 'hover-lift'}`}>
                <div className="d-flex justify-content-between align-items-center mb-2">
                  <p className="small text-muted mb-0">Візит #{group.bookingId}</p>
                  <p className="fw-bold mb-0">{totalPrice} грн</p>
                </div>

                <div className="mb-3 p-2 bg-light rounded-2 small text-dark border d-flex justify-content-between">
                  <span>Дата огляду:</span>
                  <span className="fw-medium">{group.bookingDate || "---"}</span>
                </div>

                <ul className="list-unstyled mb-3 d-flex flex-column gap-1 small">
                  {group.items.map((item) => (
                    <li key={item.id} className="d-flex justify-content-between"><span>{item.title}</span><span className="fw-semibold">{item.price} грн</span></li>
                  ))}
                </ul>

                {isGroupSuccess ? (
                  <div className="w-100 d-flex align-items-center justify-content-center gap-2 rounded-3 bg-soft-success text-success px-3 py-2 small fw-semibold border border-success border-opacity-25">
                    <CheckCircle size={16} /> Успішно записано!
                  </div>
                ) : (
                  <button onClick={() => openModal(group)} disabled={loadingGroupId !== null} className="btn btn-modern-orange text-white w-100 d-flex align-items-center justify-content-center gap-2 px-3 py-2 fw-semibold border-0">
                    <FileText size={16} /> Ознайомитись та прийняти
                  </button>
                )}
              </div>
            );
          })
        ) : (
          <p className="small text-muted text-center py-4">Немає нових рекомендацій</p>
        )}
      </div>

      {/* МОДАЛЬНЕ ВІКНО */}
      {modalGroup && (
        <div className="modal-overlay-modern">
          <div className="modal-content-modern bg-white rounded-4 shadow-lg overflow-hidden" style={{ maxWidth: '28rem' }}>
            <div className="p-4 border-bottom d-flex justify-content-between align-items-center">
              <h3 className="fw-bold mb-0">Деталі запису</h3>
              <button onClick={() => setModalGroup(null)} className="btn p-0 text-secondary hover-dark border-0"><X size={20} /></button>
            </div>
            
            <div className="p-4 d-flex flex-column gap-3">
              {isPreviewLoading ? (
                <div className="d-flex flex-column align-items-center justify-content-center py-5 text-secondary">
                  <div className="spinner-border text-orange mb-3" role="status"></div>
                  <p className="small fw-medium mb-0">Підбираємо найближчий час...</p>
                </div>
              ) : previewData ? (
                <>
                  <div className="bg-soft-primary border border-primary border-opacity-25 rounded-3 p-3 text-center">
                    <p className="text-muted small mb-1">Запропонована дата та час</p>
                    <p className="fs-5 fw-bold text-dark mb-0">{previewData.proposedDate}</p>
                    <p className="fs-4 fw-bold text-orange mb-0">{previewData.startTime} - {previewData.endTime}</p>
                  </div>

                  <div className="d-flex flex-column gap-2 overflow-auto" style={{ maxHeight: '12rem' }}>
                    {previewData.items.map((item) => (
                      <div key={item.recommendationId} className="d-flex align-items-start gap-2 p-2 bg-light rounded-2 border">
                        <div className="bg-white border rounded-1 px-2 py-1 small text-muted fw-mono flex-shrink-0">{item.startTime}</div>
                        <div className="flex-grow-1">
                          <p className="fw-medium small mb-0">{item.serviceTitle}</p>
                          <p className="text-muted mb-0" style={{ fontSize: '0.75rem' }}>Майстер: {item.masterName}</p>
                        </div>
                        <p className="fw-semibold small mb-0">{item.price} грн</p>
                      </div>
                    ))}
                  </div>

                  {previewData.items.length < modalGroup.items.length && (
                    <p className="small text-warning bg-soft-warning p-2 rounded-2 border border-warning border-opacity-25 mb-0">
                      ⚠️ Деякі послуги не ввійшли через відсутність вільних майстрів.
                    </p>
                  )}
                </>
              ) : null}

              {error && (
                <div className="d-flex align-items-center gap-2 bg-soft-danger text-danger small p-2 rounded-2 border border-danger border-opacity-25">
                  <AlertCircle size={16} className="flex-shrink-0" /><span>{error}</span>
                </div>
              )}
            </div>

            <div className="p-4 bg-light border-top d-flex gap-2">
              <button onClick={() => setModalGroup(null)} className="btn btn-light border flex-fill px-4 py-2 fw-medium">
                Відхилити
              </button>
              <button 
                onClick={handleFinalAccept} 
                disabled={loadingGroupId !== null || isPreviewLoading || !previewData} 
                className="btn btn-modern-orange text-white flex-fill px-4 py-2 fw-semibold border-0 d-flex align-items-center justify-content-center gap-2"
              >
                {loadingGroupId !== null ? <><div className="spinner-border spinner-border-sm text-white" role="status"></div> Записуємо...</> : "Підтвердити запис"}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}