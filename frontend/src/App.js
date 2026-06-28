import { Routes, Route, Navigate } from "react-router-dom";
import { GoogleOAuthProvider } from "@react-oauth/google";
import MainLayout from "./layouts/MainLayout";
import MasterLayout from "./layouts/MasterLayout";
import AdminLayout from "./layouts/AdminLayout";

import Home from "./pages/Home";
import Services from "./pages/Services";
import ServiceDetailPage from "./components/home/ServiceDetailPage";
import Profile from "./pages/Profile";
import OnlineConsultation from "./pages/OnlineConsultation";
import MasterPage from "./pages/MasterPage";
import MasterSchedulePage from "./pages/MasterSchedulePage";

import AdminDashboard from "./pages/admin/AdminDashboard";
import AdminBookings from "./pages/admin/AdminBookings";
import AdminClients from "./pages/admin/AdminClients";
import AdminMasters from "./pages/admin/AdminMasters";
import AdminSchedule from "./pages/admin/AdminSchedule";
import AdminServices from "./pages/admin/AdminServices";

import ProtectedRoute from "./components/ProtectedRoute";
import { useAuth } from "./context/AuthContext";

const GOOGLE_CLIENT_ID = "600043111070-7jt099hc5b2f99n48kab653ussu3ks7j.apps.googleusercontent.com";

export default function App() {

  const { user, loading } = useAuth(); 
  const isMaster = user?.role === "master";
  const isAdmin = user?.role === "admin";

  if (loading) {
    return <div style={{textAlign: "center", marginTop: "50px"}}>Перевірка сесії...</div>; 
  }

  return (
    <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>
      <Routes>
        <Route element={<MainLayout />}>

          <Route 
            path="/" 
            element={
              isMaster ? <Navigate to="/master/schedule" replace /> : 
              (isAdmin ? <Navigate to="/admin/dashboard" replace /> : <Home />)
            } 
          />
          
          <Route path="/services" element={<Services />} />
          <Route path="/services/:id" element={<ServiceDetailPage />} />
          <Route path="/booking" element={<Navigate to="/services" replace />} />

          <Route path="/profile" element={
            <ProtectedRoute allowedRole="client">
              <Profile />
            </ProtectedRoute>
          } />

          <Route path="/consultation" element={<OnlineConsultation />} />
        </Route>

        {/* master  */}
        <Route path="/master" element={<ProtectedRoute allowedRole="master"><MasterLayout /></ProtectedRoute>}>
          <Route index element={<Navigate to="schedule" replace />} />
          <Route path="schedule" element={<MasterSchedulePage />} />
          <Route path="requests" element={<MasterPage />} />
        </Route>

        {/* admin */}
        <Route path="/admin" element={<ProtectedRoute allowedRole="admin"><AdminLayout /></ProtectedRoute>}>
          <Route index element={<Navigate to="dashboard" replace />} />
          <Route path="dashboard" element={<AdminDashboard />} />
          <Route path="bookings" element={<AdminBookings />} />
          <Route path="clients" element={<AdminClients />} />
          <Route path="masters" element={<AdminMasters />} />
          <Route path="schedule" element={<AdminSchedule />} />
          <Route path="services" element={<AdminServices />} />
        </Route>

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </GoogleOAuthProvider>
  );
}