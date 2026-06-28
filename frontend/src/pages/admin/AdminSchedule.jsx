import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import api from '../../api/api';

export default function AdminSchedule() {
  const generateDays = () => {
    const days = [];
    const options = { weekday: 'short', day: 'numeric', month: 'short' };
    for (let i = 0; i < 7; i++) {
      const date = new Date();
      date.setDate(date.getDate() + i);
      days.push({
        label: date.toLocaleDateString('uk-UA', options),
        dateValue: date.toLocaleDateString('sv-SE')
      });
    }
    return days;
  };

  const days = generateDays();
  const [selectedDate, setSelectedDate] = useState(days[0]);

  const { data: tasks = [], isLoading } = useQuery({
    queryKey: ['admin-full-schedule', selectedDate.dateValue],
    queryFn: () => api.get(`/admin/full-schedule?date=${selectedDate.dateValue}`),
    refetchInterval: 30000,
  });

  const START_HOUR = 9;
  const END_HOUR = 18;


  const scheduleByHour = {};
  for (let h = START_HOUR; h < END_HOUR; h++) {
    const key = String(h).padStart(2, '0');
    scheduleByHour[key] = [];
  }

  tasks.forEach(task => {
    if (task.time) {
      const startHour = task.time.substring(0, 2);
      if (scheduleByHour[startHour]) {
        scheduleByHour[startHour].push(task);
      }
    }
  });

  const hours = Array.from({ length: END_HOUR - START_HOUR }, (_, i) => START_HOUR + i);
  const uniqueMasters = [...new Set(tasks.map(t => t.masterName))];


  const getStatusBadge = (status) => {
    switch (status) {
      case 'Confirmed': return 'bg-blue-100 text-blue-800';
      case 'InProgress': return 'bg-amber-100 text-amber-800';
      case 'Completed': return 'bg-green-100 text-green-800';
      case 'Cancelled': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusText = (status) => {
    switch (status) {
      case 'Confirmed': return 'Очікує';
      case 'InProgress': return 'В процесі';
      case 'Completed': return 'Завершено';
      default: return status;
    }
  };

  return (
    <div className="w-full">

      <div className="mb-6 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 border-b border-gray-200 pb-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-800">Розклад майстрів</h1>
          <p className="text-sm text-gray-500 mt-1">Період: {selectedDate.label}</p>
        </div>
        <div className="flex items-center gap-3 text-sm text-gray-600">
          <span>Майстрів на лінії: <strong>{uniqueMasters.length}</strong></span>
          <span className="text-gray-300">|</span>
          <span>Всього завдань: <strong>{tasks.length}</strong></span>
        </div>
      </div>
      
      <div className="flex items-center gap-2 mb-6 overflow-x-auto pb-2">
        {days.map((day) => (
          <button 
            key={day.dateValue} 
            onClick={() => setSelectedDate(day)} 
            className={`px-4 py-2 text-sm font-medium rounded border whitespace-nowrap ${
              selectedDate.dateValue === day.dateValue 
                ? 'bg-gray-800 text-white border-gray-800' 
                : 'bg-white text-gray-600 border-gray-300 hover:bg-gray-50'
            }`}
          >
            {day.label}
          </button>
        ))}
      </div>

      {isLoading ? (
        <div className="text-center py-16 text-gray-400">
            <p>Завантаження даних...</p>
        </div>
      ) : tasks.length === 0 ? (
        <div className="border border-gray-200 bg-gray-50 p-12 text-center text-gray-500 rounded">
          <p>На обраний день відсутні записи в розкладі.</p>
        </div>
      ) : (
        /* ==========================================
           ОСНОВНА ТАБЛИЦЯ-ГРАФІК 
           ========================================== */
        <div className="border border-gray-200 rounded bg-white overflow-hidden">
          

          <div className="divide-y divide-gray-200">
            {hours.map(hour => {
              const hourKey = String(hour).padStart(2, '0');
              const hourTasks = scheduleByHour[hourKey];

              return (
                <div key={hourKey} className="flex min-h-[130px]">

                  <div className="w-24 flex-shrink-0 bg-gray-50 border-r border-gray-200 flex items-center justify-center">
                    <span className="text-sm font-mono font-semibold text-gray-600">
                      {hourKey}:00
                    </span>
                  </div>

                  <div className="flex-1 p-3 flex flex-wrap gap-3 content-start">
                    
                    {hourTasks.length === 0 ? (
                      <div className="text-sm text-gray-300 italic pt-2">---вільно---</div>
                    ) : (
                      hourTasks.map(task => (
                        <div 
                          key={task.id}
                          className="w-72 h-fit bg-white border border-gray-200 rounded shadow-sm hover:shadow transition-shadow overflow-hidden"
                        >
                          <div className="px-3 py-2 bg-gray-50 border-b border-gray-200 flex items-center justify-between gap-2">
                           
                            <span className="text-sm font-bold text-gray-800 whitespace-normal break-words">
                              {task.masterName}
                            </span>
                    
                            <span className={`text-[10px] font-semibold px-2 py-0.5 rounded whitespace-nowrap ${getStatusBadge(task.status)}`}>
                              {getStatusText(task.status)}
                            </span>
                          </div>
                          <div className="px-3 py-2.5 space-y-1">
                            <div className="flex items-center gap-2 text-xs text-gray-500">
                              <span className="font-mono font-semibold">{task.time} - {task.endTime}</span>
                            </div>
                            
                            <p className="text-sm font-semibold text-gray-900 truncate">
                              {task.clientName}
                            </p>
                            
                            <p className="text-xs text-gray-500 line-clamp-2" style={{ display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical', overflow: 'hidden' }}>
                              {task.service}
                            </p>

                            {task.carInfo !== "Не вказано" && (
                              <p className="text-xs text-gray-400 font-mono pt-1 mt-1 border-t border-gray-100">
                                {task.carInfo}
                              </p>
                            )}
                          </div>
                        </div>
                      ))
                    )}
                  </div>

                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}