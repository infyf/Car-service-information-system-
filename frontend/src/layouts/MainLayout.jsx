import { useEffect } from "react";
import { Outlet, useLocation } from "react-router-dom";
import ClientHeader from "../components/ClientHeader"; 
import Footer from "../components/Footer";

export default function MainLayout() {
  const { pathname } = useLocation();
  useEffect(() => { window.scrollTo(0, 0); }, [pathname]);
  
  return (
    <div className="min-h-screen flex flex-col bg-white">
      <ClientHeader />
      <main className="flex-1 pt-16"><Outlet /></main>
      <Footer />
    </div>
  );
}