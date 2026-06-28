import { Outlet } from "react-router-dom";
import MasterHeader from "../components/MasterHeader"; 
import Sidebar from "../components/master/Sidebar";

export default function MasterLayout() {
  return (
    <div className="flex flex-col min-h-screen">
      <MasterHeader />
      <div className="flex flex-1 pt-16">
        <Sidebar />
        <main className="flex-1 p-6"><Outlet /></main>
      </div>
    </div>
  );
}