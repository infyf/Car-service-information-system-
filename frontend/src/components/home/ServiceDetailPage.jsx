
import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import api from "../../api/api";

export default function ServiceDetailPage() {
  const { id } = useParams();
  const [detail, setDetail] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    setLoading(true);
    api.get(`/Services/${id}/details`)
      .then(data => { setDetail(data); setLoading(false); })
      .catch(err => { setError("Сталася помилка при завантаженні."); setLoading(false); });
  }, [id]);

  if (loading) return (
    <div className="min-vh-100 bg-light d-flex justify-content-center align-items-center">
      <p className="text-muted fs-5 pulse-loading">Завантаження...</p>
    </div>
  );
  
  if (error) return (
    <div className="min-vh-100 bg-light d-flex justify-content-center align-items-center">
      <p className="text-danger fs-5">{error}</p>
    </div>
  );
  
  if (!detail) return null;

  const descriptionLines = detail.description ? detail.description.split("\n").filter(line => line.trim() !== "") : [];

  return (
    <section className="bg-light min-vh-100">

      <div className="position-relative hero-detail-header overflow-hidden">
        {detail.imageUrl && <img src={detail.imageUrl} alt={detail.title} className="w-100 h-100 of-cover" />}
        <div className="overlay-dark position-absolute top-0 start-0 w-100 h-100 d-flex align-items-center">
          <div className="container w-100 text-white">
            <Link to="/services" className="small text-white-50 text-decoration-none hover-orange">
              ← Повернутися до послуг
            </Link>
            <h1 className="mt-3 display-5 fw-bold lh-sm" style={{ maxWidth: '48rem' }}>
              {detail.title}
            </h1>
          </div>
        </div>
      </div>

      <div className="container py-5" style={{ maxWidth: '64rem' }}>
        <div className="card border-0 shadow-lg rounded-4 p-4 p-md-5">
          <div className="fs-5 text-body-secondary lh-lg">
            {descriptionLines.map((line, index) => (
              <p key={index}>{line}</p>
            ))}
          </div>
          
          <div className="mt-5 border-top pt-4 d-flex flex-column flex-md-row align-items-md-center justify-content-between gap-4">
            <h3 className="h4 fw-semibold text-dark mb-0">Потрібна консультація або запис?</h3>
            <Link to="/services" className="btn btn-gradient-orange text-white fw-semibold px-5 py-3 shadow-sm">
              Записатися онлайн
            </Link>
          </div>
        </div>
      </div>
    </section>
  );
}