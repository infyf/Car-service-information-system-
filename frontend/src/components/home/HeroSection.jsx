
import { Link } from "react-router-dom";
import { Calendar, ChevronRight } from "lucide-react";
import { motion } from "framer-motion";

export default function HeroSection() {
  return (
    <section className="position-relative bg-dark text-white overflow-hidden">
 
      <div className="position-absolute top-0 start-0 w-100 h-100">
        <motion.div 
          animate={{ x: [0, 50, 0], y: [0, -30, 0] }} 
          transition={{ duration: 15, repeat: Infinity, ease: "easeInOut" }} 
          className="blob blob-orange position-absolute top-20 start-10" 
        />
        <motion.div 
          animate={{ x: [0, -50, 0], y: [0, 30, 0] }} 
          transition={{ duration: 20, repeat: Infinity, ease: "easeInOut" }} 
          className="blob blob-red position-absolute bottom-20 end-10" 
        />
      </div>

      <div className="container py-5 py-md-9 position-relative z-3">
        <div className="row align-items-center g-5">
          <div className="col-md-6">
        
            <div className="badge-pulse d-inline-flex align-items-center gap-2 px-3 py-2 rounded-pill mb-4">
              <span className="dot bg-orange"></span>
              <span className="text-orange small fw-medium">Працюємо без вихідних</span>
            </div>

            <h1 className="display-3 fw-bold mb-4 lh-sm">
              Професійний <span className="text-gradient">автосервіс</span> у Миколаєві
            </h1>
            
            <p className="lead text-secondary mb-4" style={{ maxWidth: '32rem' }}>
              Ремонт, діагностика, шиномонтаж та технічне обслуговування.
            </p>

            <div className="d-flex flex-column flex-sm-row gap-3">
              <Link to="/services" className="btn btn-gradient-orange text-white fw-medium d-inline-flex align-items-center justify-content-center gap-2">
                <Calendar size={20} /> Записатися онлайн
              </Link>
              <Link to="/services" className="btn btn-ghost-white fw-medium d-inline-flex align-items-center justify-content-center gap-2">
                Наші послуги <ChevronRight size={20} />
              </Link>
            </div>

            {/* Статистика */}
            <div className="row g-4 mt-4 pt-4 border-top border-white-20">
              <div className="col-4 text-center">
                <div className="h3 fw-bold text-orange mb-1">10+</div>
                <div className="small text-secondary">Років досвіду</div>
              </div>
              <div className="col-4 text-center">
                <div className="h3 fw-bold text-orange mb-1">5000+</div>
                <div className="small text-secondary">Клієнтів</div>
              </div>
              <div className="col-4 text-center">
                <div className="h3 fw-bold text-orange mb-1">98%</div>
                <div className="small text-secondary">Задоволених</div>
              </div>
            </div>
          </div>

          <div className="col-md-6 d-none d-md-block position-relative">

          </div>
        </div>
      </div>
    </section>
  );
}