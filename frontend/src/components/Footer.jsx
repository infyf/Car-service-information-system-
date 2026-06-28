import { Link } from "react-router-dom"
import { Wrench, Phone, Mail, MapPin, Clock } from "lucide-react"

export default function Footer() {
  return (
    <footer className="bg-dark text-secondary pt-5 pb-4">
      <div className="container">
        <div className="row g-4">
          <div className="col-12 col-md-6 col-lg-3">
            <Link to="/" className="d-flex align-items-center gap-2 fw-bold fs-5 text-white mb-3 text-decoration-none">
              <div className="footer-logo-icon d-flex align-items-center justify-content-center rounded">
                <Wrench size={20} className="text-white" />
              </div>
              AutoDrive
            </Link>
            <p className="small">
              Професійний автосервіс з ремонту та обслуговування автомобілів будь-яких марок.
            </p>
          </div>

          <div className="col-12 col-md-6 col-lg-3">
            <h4 className="fw-semibold text-white mb-3 h6">Послуги</h4>
            <ul className="list-unstyled d-flex flex-column gap-2 small">
              <li><Link to="/services" className="text-decoration-none text-secondary hover-orange">Ремонт двигуна</Link></li>
              <li><Link to="/services" className="text-decoration-none text-secondary hover-orange">Шиномонтаж</Link></li>
              <li><Link to="/services" className="text-decoration-none text-secondary hover-orange">Заміна масла</Link></li>
              <li><Link to="/services" className="text-decoration-none text-secondary hover-orange">Діагностика</Link></li>
            </ul>
          </div>

          <div className="col-12 col-md-6 col-lg-3">
            <h4 className="fw-semibold text-white mb-3 h6">Контакти</h4>
            <ul className="list-unstyled d-flex flex-column gap-2 small">
              <li className="d-flex align-items-center gap-2">
                <Phone size={16} className="text-orange" />
                +380 (67) 123-45-67
              </li>
              <li className="d-flex align-items-center gap-2">
                <Mail size={16} className="text-orange" />
                info@avtomaister.ua
              </li>
              <li className="d-flex align-items-center gap-2">
                <MapPin size={16} className="text-orange" />
                м. Миколаїв, вул. Автомобільна, 1
              </li>
            </ul>
          </div>

          <div className="col-12 col-md-6 col-lg-3">
            <h4 className="fw-semibold text-white mb-3 h6">Графік роботи</h4>
            <ul className="list-unstyled d-flex flex-column gap-2 small">
              <li className="d-flex align-items-center gap-2">
                <Clock size={16} className="text-orange" />
                Пн-Пт: 8:00 - 20:00
              </li>
              <li className="ps-4">Сб: 9:00 - 18:00</li>
              <li className="ps-4">Нд: 10:00 - 16:00</li>
            </ul>
          </div>
        </div>

        <div className="border-top border-secondary mt-4 pt-4 text-center small">
          © 2025 АвтоМайстер. Всі права захищені.
        </div>
      </div>
    </footer>
  )
}