import { NavLink } from 'react-router-dom';
import { Calendar, MessageSquare, LayoutGrid } from 'lucide-react';

export default function Sidebar() {
  return (
    <aside className="w-20 md:w-24 bg-[#2D2D2D] flex flex-col items-center py-6 gap-8 min-h-screen sticky top-0">
      <div className="p-2"><LayoutGrid className="w-6 h-6 text-gray-400 cursor-pointer hover:text-white transition" /></div>
      <NavLink to="/master/schedule" className={({ isActive }) => `flex flex-col items-center gap-1 w-full py-4 transition-all ${isActive ? 'bg-[#404040] border-r-4 border-orange-500 text-white' : 'text-gray-400 hover:text-white'}`}><Calendar className="w-6 h-6" /><span className="text-[10px] font-medium uppercase mt-1 text-center">Графік</span></NavLink>
      <NavLink to="/master/requests" className={({ isActive }) => `relative flex flex-col items-center gap-1 w-full py-4 transition-all ${isActive ? 'bg-[#404040] border-r-4 border-orange-500 text-white' : 'text-gray-400 hover:text-white'}`}><MessageSquare className="w-6 h-6" /><span className="text-[10px] uppercase mt-1 text-center">Запити</span></NavLink>
    </aside>
  );
}