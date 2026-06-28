import { Link } from "react-router-dom"
import { Phone, Calendar, MessageCircle } from "lucide-react"

export default function CTASection() {
  return (
    <section className="cta-gradient py-5 text-white">
      <div className="container text-center">

        <div className="row justify-content-center mb-4">
          <div className="col-lg-8">
            <h2 className="display-5 fw-bold mb-3">Потрібен ремонт автомобіля?</h2>
            <p className="lead opacity-75">
              Запишіться на безкоштовну діагностику або зателефонуйте нам для консультації
            </p>
          </div>
        </div>

       
        <div className="d-flex flex-column flex-sm-row gap-3 justify-content-sm-center align-items-sm-center">
          

          <Link
            to="/booking"
            className="btn btn-light btn-lg fw-semibold shadow d-inline-flex align-items-center justify-content-center btn-scale w-100 w-sm-auto"
          >
            <Calendar className="me-2" size={20} />
            Записатися онлайн
          </Link>

          <a
            href="tel:+380671234567"
            className="btn btn-ghost-white btn-lg fw-semibold d-inline-flex align-items-center justify-content-center btn-scale w-100 w-sm-auto"
          >
            <Phone className="me-2" size={20} />
            +380 (67) 123-45-67
          </a>

    
          <a
            href="https://t.me/service_bot"
            target="_blank"
            rel="noopener noreferrer"
            className="btn btn-ghost-white btn-lg fw-semibold d-inline-flex align-items-center justify-content-center btn-scale w-100 w-sm-auto"
          >
            <MessageCircle className="me-2" size={20} />
            Написати в Telegram
          </a>
        </div>
      </div>
    </section>
  )
}