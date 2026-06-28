import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { LayoutGrid, Users, CalendarDays, Wrench, UserCog, ClipboardList, LogOut, Car } from "lucide-react";
import { useAuth } from "../context/AuthContext";

const navItems = [
  { path: "/admin/dashboard", icon: LayoutGrid, label: "Головна" },
  { path: "/admin/bookings", icon: ClipboardList, label: "Заявки" },
  { path: "/admin/clients", icon: Users, label: "Клієнти" },
  { path: "/admin/masters", icon: UserCog, label: "Майстри" },
  { path: "/admin/schedule", icon: CalendarDays, label: "Розклад" },
  { path: "/admin/services", icon: Wrench, label: "Послуги" },
];

export default function AdminLayout() {
  const { logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/");
  };

  const navLinkClass = ({ isActive }) =>
    `flex flex-col items-center gap-1 w-full py-4 transition-all ${
      isActive
        ? 'bg-[#404040] border-r-4 border-orange-500 text-white'
        : 'text-gray-400 hover:text-white'
    }`;

  return (
    <div className="flex min-h-screen bg-gray-100 font-sans">

      <aside className="w-20 md:w-24 bg-[#2D2D2D] flex flex-col items-center py-6 gap-4 min-h-screen sticky top-0">
        
        {/* Логотип */}
        <div className="p-2 mb-4">
          <div className="w-8 h-8 bg-orange-500 rounded flex items-center justify-center">
            <Car className="w-5 h-5 text-white" />
          </div>
        </div>

        <nav className="flex flex-col w-full">
          {navItems.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              className={navLinkClass}
            >
              <item.icon className="w-6 h-6" />
              <span className="text-[10px] font-medium uppercase mt-1 text-center">
                {item.label}
              </span>
            </NavLink>
          ))}
        </nav>

       
        <div className="flex-1"></div> 
        
        <button
          onClick={handleLogout}
          className="flex flex-col items-center gap-1 w-full py-4 text-gray-400 hover:text-red-400 transition-colors cursor-pointer"
        >
          <LogOut className="w-6 h-6" />
          <span className="text-[10px] uppercase mt-1 text-center">Вихід</span>
        </button>
      </aside>

      <main className="flex-1 p-6 md:p-8 overflow-y-auto">
        <Outlet />
      </main>
    </div>
  );
}