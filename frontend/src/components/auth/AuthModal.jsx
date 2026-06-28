
import { useState } from "react";
import { X, Mail, Lock, User, Phone } from "lucide-react";
import { useAuth } from "../../context/AuthContext";
import { GoogleLogin } from "@react-oauth/google"; 

import api from "../../api/api";

export default function AuthModal({ isOpen, onClose }) {
  const { login } = useAuth();
  const [activeTab, setActiveTab] = useState("login");
  const [formData, setFormData] = useState({ 
    email: "", 
    password: "", 
    confirmPassword: "", 
    firstName: "", 
    lastName: "", 
    phone: "" 
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  if (!isOpen) return null;

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
    setError("");
  };

  const handleGoogleSuccess = async (credentialResponse) => {
    setLoading(true);
    setError("");
    try {

      const data = await api.post("/Auth/google-login", {
        idToken: credentialResponse.credential
      });
      login(data); 
      onClose();
    } catch (err) {
      setError(err.message || "Помилка авторизації через Google");
    } finally {
      setLoading(false);
    }
  };

  
  const handleGoogleError = () => {
    setError("Не вдалося увійти через Google. Спробуйте пізніше.");
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError("");

    try {
      if (activeTab === "register") {
        if (formData.password !== formData.confirmPassword) {
          setError("Паролі не співпадають");
          setLoading(false);
          return;
        }
        const data = await api.post("/Auth/register", {
          email: formData.email, 
          password: formData.password,
          firstName: formData.firstName, 
          lastName: formData.lastName, 
          phone: formData.phone
        });
        login(data); 
      } else {
        const data = await api.post("/Auth/login", {
          email: formData.email, 
          password: formData.password
        });
        login(data);
      }
      
      onClose();
      setFormData({ email: "", password: "", confirmPassword: "", firstName: "", lastName: "", phone: "" });
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    /* Оверлей (фон) */
    <div 
      className="position-fixed top-0 start-0 w-100 h-100 d-flex align-items-center justify-content-center p-3 modal-overlay" 
      style={{ zIndex: 1055 }} 
      onClick={onClose}
    >
     
      <div 
        className="bg-white rounded-4 w-100 position-relative overflow-hidden shadow-lg" 
        style={{ maxWidth: '28rem' }} 
        onClick={(e) => e.stopPropagation()}
      >
      
        <div className="modal-header-gradient p-4 text-white d-flex justify-content-between align-items-center">
          <h2 className="fs-4 fw-bold mb-0">
            {activeTab === "login" ? "Вхід в акаунт" : "Реєстрація"}
          </h2>
          <button 
            onClick={onClose} 
            className="btn btn-link p-0 text-white text-opacity-75 hover-text-white" 
            aria-label="Close"
          >
            <X size={24} />
          </button>
        </div>

      
        <ul className="nav nav-tabs border-bottom mb-0" role="tablist">
          <li className="nav-item flex-fill" role="presentation">
            <button 
              className={`nav-link w-100 text-center rounded-0 fw-medium border-0 ${activeTab === "login" ? "active" : "text-secondary"}`} 
              onClick={() => setActiveTab("login")}
            >
              Вхід
            </button>
          </li>
          <li className="nav-item flex-fill" role="presentation">
            <button 
              className={`nav-link w-100 text-center rounded-0 fw-medium border-0 ${activeTab === "register" ? "active" : "text-secondary"}`} 
              onClick={() => setActiveTab("register")}
            >
              Реєстрація
            </button>
          </li>
        </ul>

        
        <form onSubmit={handleSubmit} className="p-4 d-flex flex-column gap-3">
          {error && <div className="alert alert-danger small py-2 mb-0">{error}</div>}
          
         
          <GoogleLogin
            onSuccess={handleGoogleSuccess}
            onError={handleGoogleError}
            useOneTap
            render={({ onClick, disabled }) => (
              <button 
                type="button" 
                onClick={onClick} 
                disabled={disabled || loading}
                className="btn btn-light border d-flex align-items-center justify-content-center gap-2 w-100 fw-medium shadow-sm"
              >
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92a5.06 5.06 0 0 1-2.2 3.32v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.1z" fill="#4285F4"/>
                  <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"/>
                  <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" fill="#FBBC05"/>
                  <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335"/>
                </svg>
                {loading ? "Завантаження..." : "Продовжити з Google"}
              </button>
            )}
          />

       
          <div className="d-flex align-items-center">
            <div className="flex-grow-1 border-top"></div>
            <span className="px-3 text-muted small">або</span>
            <div className="flex-grow-1 border-top"></div>
          </div>

          {activeTab === "register" && (
            <>
              <div className="row g-3">
                <div className="col-6">
                  <div className="position-relative">
                    <User className="input-icon position-absolute top-50 translate-middle-y text-secondary" size={18} />
                    <input 
                      type="text" 
                      name="firstName" 
                      placeholder="Ім'я" 
                      value={formData.firstName} 
                      onChange={handleChange} 
                      required 
                      className="form-control rounded-3 ps-5" 
                    />
                  </div>
                </div>
                <div className="col-6">
                  <div className="position-relative">
                    <User className="input-icon position-absolute top-50 translate-middle-y text-secondary" size={18} />
                    <input 
                      type="text" 
                      name="lastName" 
                      placeholder="Прізвище" 
                      value={formData.lastName} 
                      onChange={handleChange} 
                      required 
                      className="form-control rounded-3 ps-5" 
                    />
                  </div>
                </div>
              </div>
              <div className="position-relative">
                <Phone className="input-icon position-absolute top-50 translate-middle-y text-secondary" size={18} />
                <input 
                  type="tel" 
                  name="phone" 
                  placeholder="Телефон" 
                  value={formData.phone} 
                  onChange={handleChange} 
                  required 
                  className="form-control rounded-3 ps-5" 
                />
              </div>
            </>
          )}
          
          <div className="position-relative">
            <Mail className="input-icon position-absolute top-50 translate-middle-y text-secondary" size={18} />
            <input 
              type="email" 
              name="email" 
              placeholder="Email" 
              value={formData.email} 
              onChange={handleChange} 
              required 
              className="form-control rounded-3 ps-5" 
            />
          </div>
          
          <div className="position-relative">
            <Lock className="input-icon position-absolute top-50 translate-middle-y text-secondary" size={18} />
            <input 
              type="password" 
              name="password" 
              placeholder="Пароль" 
              value={formData.password} 
              onChange={handleChange} 
              required 
              minLength={6} 
              className="form-control rounded-3 ps-5" 
            />
          </div>
          
          {activeTab === "register" && (
            <div className="position-relative">
              <Lock className="input-icon position-absolute top-50 translate-middle-y text-secondary" size={18} />
              <input 
                type="password" 
                name="confirmPassword" 
                placeholder="Підтвердіть пароль" 
                value={formData.confirmPassword} 
                onChange={handleChange} 
                required 
                minLength={6} 
                className="form-control rounded-3 ps-5" 
              />
            </div>
          )}
          
          <button 
            type="submit" 
            disabled={loading} 
            className="btn btn-gradient-orange text-white py-2 fw-medium mt-2"
          >
            {loading ? "Завантаження..." : activeTab === "login" ? "Увійти" : "Зареєструватися"}
          </button>
          
          <p className="text-center small text-secondary mb-0">
            {activeTab === "login" ? "Немає акаунту?" : "Вже є акаунт?"}{" "}
            <button 
              type="button" 
              onClick={() => setActiveTab(activeTab === "login" ? "register" : "login")} 
              className="btn btn-link link-orange p-0 fw-medium text-decoration-underline small"
            >
              {activeTab === "login" ? "Зареєструватися" : "Увійти"}
            </button>
          </p>
        </form>
      </div>
    </div>
  );
}