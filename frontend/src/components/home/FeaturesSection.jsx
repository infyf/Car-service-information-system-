
import { Clock, Award, Shield, Users } from "lucide-react";

const features = [
  { icon: Clock, title: "Швидко", description: "Оперативна діагностика та ремонт." },
  { icon: Award, title: "Якісно", description: "Використовуємо оригінальні запчастини." },
  { icon: Shield, title: "Гарантія", description: "Надаємо гарантію на всі види робіт." },
  { icon: Users, title: "Досвід", description: "Наші майстри мають 10+ років досвіду." },
];

export default function FeaturesSection() {
  return (
    <section className="py-5 bg-light">
      <div className="container">
        <div className="text-center mb-5">
          <h2 className="fw-bold text-dark mb-3">Чому обирають нас</h2>
        </div>
        
        <div className="row g-4">
          {features.map((f, i) => (
            <div key={i} className="col-12 col-md-6 col-lg-3">
              <div className="card h-100 border-light shadow-sm feature-card">
                <div className="card-body text-center text-md-start p-4">
                  <div className="icon-box rounded-3 d-inline-flex align-items-center justify-content-center mb-3">
                    <f.icon size={24} className="text-white" />
                  </div>
                  <h3 className="h5 fw-bold text-dark mb-2">{f.title}</h3>
                  <p className="text-muted mb-0">{f.description}</p>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}