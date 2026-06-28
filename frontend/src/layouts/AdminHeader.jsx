import { Link, NavLink } from "react-router-dom";
import { Shield } from "lucide-react"; 
import { useAuth } from "../context/AuthContext";

export default function AdminHeader() {
  const { user, logout } = useAuth();
  
  const navLinkClass = ({ isActive }) => 
    `transition-colors hover:text-orange-500 ${isActive ? "text-orange-500 font-medium" : "text-gray-700"}`;

  return (
    <header className="fixed top-0 left-0 right-0 bg-white shadow-md z-50">
      <div className="max-w-7xl mx-auto px-6">
        <div className="flex justify-between items-center h-16">
          <Link to="/admin/dashboard" className="flex items-center gap-2 font-bold text-lg">
            <div className="w-8 h-8 bg-red-600 rounded flex items-center justify-center">
              <Shield className="w-4 h-4 text-white" />
            </div>
            <span className="text-gray-900">AutoDrive Admin</span>
          </Link>
          
          <nav className="flex gap-6 items-center">
            <NavLink to="/admin/dashboard" className={navLinkClass} end>Заяви та Аналітика</NavLink>
            <span className="text-sm text-gray-600">Адмін: {user?.firstName}</span>
            <button 
              onClick={logout} 
              className="px-3 py-1.5 bg-gray-200 rounded-lg hover:bg-gray-300 transition-colors text-sm"
            >
              Вихід
            </button>
          </nav>
        </div>
      </div>
    </header>
  );
}