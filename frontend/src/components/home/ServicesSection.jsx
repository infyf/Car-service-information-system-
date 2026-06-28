
import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { Layers, Search, Droplets, Disc, Wind, ShieldCheck, Settings2, CircleDot, Zap, Hammer, ClipboardCheck } from "lucide-react";
import api from "../../api/api";

const iconsMap = { Layers, Search, Droplets, Disc, Wind, ShieldCheck, Settings2, CircleDot, Zap, Hammer, ClipboardCheck };

export default function ServicesSection() {
  const [services, setServices] = useState([]);

  useEffect(() => {
    api.get("/Services").then(setServices).catch(console.error);
  }, []);

  return (
    <section className="py-5 bg-white">
      <div className="container">
        <h2 className="h3 fw-light text-secondary mb-5 text-center text-uppercase tracking-wide">
          Наші послуги
        </h2>
        
        <div className="row g-0 border-top border-start border-secondary-subtle">
          {services.map(service => {
            const Icon = iconsMap[service.icon] || Layers;
            return (
              <div key={service.id} className="col-12 col-md-6 col-lg-4">
                <Link 
                  to={`/services/${service.id}`} 
                  className="service-item d-flex align-items-center p-4 border-bottom border-end border-secondary-subtle text-decoration-none"
                >
                  <div className="service-icon flex-shrink-0 d-flex align-items-center justify-content-center border border-secondary rounded-1">
                    <Icon size={24} className="text-secondary" />
                  </div>
                  <div className="ms-4 flex-grow-1">
                    <h3 className="h6 fw-light text-secondary service-title mb-0 lh-sm">
                      {service.title}
                    </h3>
                  </div>
                </Link>
              </div>
            );
          })}
        </div>
      </div>
    </section>
  );
}