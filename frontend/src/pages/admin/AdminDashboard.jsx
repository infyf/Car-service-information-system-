import { useState, useEffect } from "react";
import { UserCog, CalendarCheck, CircleCheckBig, FileClock } from "lucide-react";
import api from "../../api/api";

export default function AdminDashboard() {
  const [stats, setStats] = useState(null);
  const [todayData, setTodayData] = useState({
    total: 0,
    completed: 0,
    pending: 0,
    recentList: []
  });
  const [mastersSchedule, setMastersSchedule] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        const todayStr = new Date().toLocaleDateString("sv-SE");

        const [statsRes, bookingsRes, scheduleRes] = await Promise.all([
          api.get("/admin/masters-stats", { params: { date: todayStr } }),
          api.get("/admin/bookings", { params: { dateFilter: todayStr, t: Date.now() } }),
          api.get("/admin/masters-today-schedule", { params: { date: todayStr, t: Date.now() } })
        ]);

        const rawStats =
          statsRes?.data?.totalMasters !== undefined ? statsRes.data : statsRes;

        const rawBookings =
          bookingsRes?.data?.length
            ? bookingsRes.data
            : Array.isArray(bookingsRes)
            ? bookingsRes
            : [];

        const rawSchedule =
          scheduleRes?.data?.length
            ? scheduleRes.data
            : Array.isArray(scheduleRes)
            ? scheduleRes
            : [];

        setStats(rawStats);

        setTodayData({
          total: rawStats.scheduledToday || 0,
          completed: rawStats.completedToday || 0,
          pending: rawStats.pendingToday || 0,
          recentList: rawBookings
            .filter(b => b.status !== "completed" && b.status !== "cancelled")
            .slice(0, 3)
            .map(b => ({
              time: b.bookingTime,
              client: b.clientName,
              service: b.services
            }))
        });

        setMastersSchedule(rawSchedule);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchDashboardData();
    const interval = setInterval(fetchDashboardData, 15000);
    return () => clearInterval(interval);
  }, []);

  if (loading)
    return (
      <div className="mt-10 text-center text-gray-400 animate-pulse">
        Завантаження аналітики...
      </div>
    );

  return (
    <div className="space-y-10">
      {/* HEADER */}
      <div className="flex justify-between items-end">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 tracking-tight">
            Панель керування
          </h1>
          <p className="text-sm text-gray-500 mt-1">
            Живий стан автосервісу в реальному часі
          </p>
        </div>
        <span className="text-xs px-3 py-1 rounded-full bg-gray-100 text-gray-500">
          15s
        </span>
      </div>

      {/* STATS */}
      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-6">

        <div className="rounded-2xl p-5 shadow-md bg-gradient-to-br from-slate-50 to-white border border-slate-100 hover:shadow-lg transition">
          <div className="flex items-center gap-4">
            <div className="p-3 rounded-xl bg-slate-100 text-slate-600">
              <UserCog />
            </div>
            <div>
              <p className="text-sm text-gray-500">Майстрів на зміні</p>
              <p className="text-2xl font-bold">{stats?.totalMasters || 0}</p>
            </div>
          </div>
        </div>

        <div className="rounded-2xl p-5 shadow-md bg-gradient-to-br from-blue-50 to-white border border-blue-100 hover:shadow-lg transition">
          <div className="flex items-center gap-4">
            <div className="p-3 rounded-xl bg-blue-100 text-blue-600">
              <FileClock />
            </div>
            <div>
              <p className="text-sm text-gray-500">Записів сьогодні</p>
              <p className="text-2xl font-bold text-blue-700">
                {todayData.total}
              </p>
            </div>
          </div>
        </div>

        <div className="rounded-2xl p-5 shadow-md bg-gradient-to-br from-orange-50 to-white border border-orange-100 hover:shadow-lg transition">
          <div className="flex items-center gap-4">
            <div className="p-3 rounded-xl bg-orange-100 text-orange-600">
              <CalendarCheck />
            </div>
            <div>
              <p className="text-sm text-gray-500">Очікують</p>
              <p className="text-2xl font-bold text-orange-600">
                {todayData.pending}
              </p>
            </div>
          </div>
        </div>

        <div className="rounded-2xl p-5 shadow-md bg-gradient-to-br from-emerald-50 to-white border border-emerald-100 hover:shadow-lg transition">
          <div className="flex items-center gap-4">
            <div className="p-3 rounded-xl bg-emerald-100 text-emerald-600">
              <CircleCheckBig />
            </div>
            <div>
              <p className="text-sm text-gray-500">Виконано</p>
              <p className="text-2xl font-bold text-emerald-600">
                {todayData.completed}
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* BOTTOM BLOCKS */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">

        {/* RECENT */}
        <div className="rounded-2xl p-6 bg-white shadow-md border border-gray-100">
          <h3 className="text-lg font-semibold mb-4">
            Найближчі записи
          </h3>

          {todayData.recentList.length === 0 ? (
            <p className="text-sm text-gray-400">
              Немає активних записів
            </p>
          ) : (
            <div className="space-y-4">
              {todayData.recentList.map((item, idx) => (
                <div
                  key={idx}
                  className="flex items-center justify-between p-3 rounded-xl bg-gray-50 hover:bg-gray-100 transition"
                >
                  <span className="text-xs font-mono text-gray-500">
                    {item.time}
                  </span>
                  <div className="flex-1 ml-4">
                    <p className="font-medium text-gray-800">
                      {item.client}
                    </p>
                    <p className="text-xs text-gray-400">
                      {item.service}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* MASTERS */}
        <div className="rounded-2xl p-6 bg-white shadow-md border border-gray-100">
          <h3 className="text-lg font-semibold mb-4">
            Розклад майстрів
          </h3>

          <div className="space-y-4 max-h-72 overflow-y-auto pr-2">
            {mastersSchedule.length === 0 ? (
              <p className="text-sm text-gray-400">
                Дані відсутні
              </p>
            ) : (
              mastersSchedule.map(m => (
                <div
                  key={m.id}
                  className="flex gap-3 p-3 rounded-xl hover:bg-gray-50 transition"
                >
                  <div
                    className={`w-2 h-2 mt-2 rounded-full ${
                      m.isFree ? "bg-emerald-500" : "bg-orange-400"
                    }`}
                  />
                  <div>
                    <p className="font-medium text-gray-800">
                      {m.fullName}
                    </p>

                    {m.isFree ? (
                      <p className="text-xs text-emerald-600">
                        Вільний весь день
                      </p>
                    ) : (
                      <p className="text-xs text-gray-500 font-mono">
                        {m.slots
                          .map(s => `${s.start}-${s.end}`)
                          .join(" | ")}
                      </p>
                    )}
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      </div>
    </div>
  );
}