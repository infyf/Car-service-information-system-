import { Link, NavLink } from "react-router-dom";
import { Wrench, User, Menu, X, Clock } from "lucide-react";
import { useState } from "react";
import AuthModal from "./auth/AuthModal";
import { useAuth } from "../context/AuthContext";

export default function ClientHeader() {
  const { user, logout } = useAuth();
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [isAuthModalOpen, setIsAuthModalOpen] = useState(false);

  const navLinkClass = ({ isActive }) =>
    `nav-link-custom ${isActive ? "active" : ""}`;

  return (
    <>
     
      <header className="position-fixed top-0 start-0 end-0 bg-white shadow" style={{ zIndex: 1050 }}>
        <div className="container">
          <div className="d-flex align-items-center justify-content-between small text-secondary py-1 border-bottom">
            <div className="d-flex align-items-center gap-1">
              <Clock size={14} /> Пн-Сб: 9:00 - 19:00
            </div>
            <div>(044) 344-05-81</div>
          </div>

          <div className="d-flex align-items-center justify-content-between py-3 gap-3">
            {/* Логотип */}
            <Link to="/" className="d-flex align-items-center gap-2 fw-bold fs-5 text-decoration-none text-dark">
              <div className="logo-icon d-flex align-items-center justify-content-center bg-orange rounded">
                <Wrench size={16} className="text-white" />
              </div>
              AutoDrive
            </Link>
            
            <nav className="d-none d-md-flex gap-4 align-items-center">
              <NavLink to="/" className={navLinkClass}>Головна</NavLink>
              <NavLink to="/services" className={navLinkClass}>Послуги</NavLink>
              <NavLink to="/consultation" className={navLinkClass}>Консультація</NavLink>
              <NavLink to="/contacts" className={navLinkClass}>Контакти</NavLink>
            </nav>

            <div className="d-none d-md-flex align-items-center gap-3">
              <Link to="/profile" className="btn btn-light rounded-circle p-2 d-inline-flex align-items-center justify-content-center">
                <User size={18} className="text-secondary" />
              </Link>
              {!user ? (
                <button onClick={() => setIsAuthModalOpen(true)} className="btn btn-gradient-orange text-white fw-medium px-4 py-2">
                  Увійти
                </button>
              ) : (
                <div className="d-flex align-items-center gap-2">
                  <span className="small">Привіт, {user?.firstName}</span>
                  <button onClick={logout} className="btn btn-sm btn-light border">Вийти</button>
                </div>
              )}
            </div>

            <button className="d-md-none btn p-2" onClick={() => setIsMenuOpen(!isMenuOpen)}>
              {isMenuOpen ? <X size={24} /> : <Menu size={24} />}
            </button>
          </div>


          {isMenuOpen && (
            <nav className="d-md-none py-3 border-top d-flex flex-column gap-2">
              <NavLink to="/" className={navLinkClass} onClick={() => setIsMenuOpen(false)}>Головна</NavLink>
              <NavLink to="/services" className={navLinkClass} onClick={() => setIsMenuOpen(false)}>Послуги</NavLink>
              <NavLink to="/consultation" className={navLinkClass} onClick={() => setIsMenuOpen(false)}>Консультація</NavLink>
              {!user && (
                <button onClick={() => { setIsMenuOpen(false); setIsAuthModalOpen(true); }} className="btn btn-gradient-orange text-white fw-medium mt-2">
                  Увійти
                </button>
              )}
            </nav>
          )}
        </div>
      </header>
      <AuthModal isOpen={isAuthModalOpen} onClose={() => setIsAuthModalOpen(false)} />
    </>
  );
}