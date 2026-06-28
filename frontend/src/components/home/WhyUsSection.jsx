
import { CheckCircle } from "lucide-react";

const benefits = [
  "Безкоштовна діагностика при ремонті", 
  "Оригінальні запчастини", 
  "Гарантія на всі види робіт", 
  "Зручне розташування та парковка", 
  "Можливість спостерігати за ремонтом", 
  "Фіксовані ціни"
];

export default function WhyUsSection() {
  return (
    <section className="py-5 bg-light">
      <div className="container">
        <div className="row align-items-center g-5">
          <div className="col-md-6">
            <img src="2.jpg" alt="Інтер'єр" className="img-fluid rounded-4 shadow" />
          </div>
          <div className="col-md-6">
            <h2 className="fw-bold text-dark mb-4">Професійний підхід до кожного авто</h2>
            <ul className="list-unstyled d-flex flex-column gap-3 mb-0">
              {benefits.map((b, i) => (
                <li key={i} className="d-flex align-items-center gap-3">
                  <CheckCircle size={20} className="text-success flex-shrink-0" />
                  <span className="text-secondary">{b}</span>
                </li>
              ))}
            </ul>
          </div>
        </div>
      </div>
    </section>
  );
}