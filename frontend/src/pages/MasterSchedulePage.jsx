import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import ScheduleGrid from '../components/master/ScheduleGrid';
import ActionsPanel from '../components/master/ActionsPanel';
import api from '../api/api';
import { useAuth } from '../context/AuthContext';

export default function MasterSchedulePage() {
  const { user } = useAuth();
  const queryClient = useQueryClient(); 
  const generateDays = () => {
    const days = [];
    const options = { weekday: 'short', day: 'numeric', month: 'short' };
    for (let i = 0; i < 7; i++) {
      const date = new Date(); date.setDate(date.getDate() + i);
      days.push({ label: date.toLocaleDateString('uk-UA', options), dateValue: date.toISOString().split('T')[0] });
    }
    return days;
  };

  const days = generateDays();
  const [selectedDate, setSelectedDate] = useState(days[0]);
  const [selectedTask, setSelectedTask] = useState(null);


  const { data: scheduleData = [], isLoading: loading } = useQuery({
    queryKey: ['schedule', user?.id, selectedDate.dateValue], 
    queryFn: () => api.get(`/Bookings/my-schedule?userId=${user.id}&date=${selectedDate.dateValue}`),
    refetchInterval: 30000, 
  });

  // ==========================================

  const updateStatusMutation = useMutation({
    mutationFn: ({ taskId, newStatus }) => api.patch(`/Bookings/${taskId}/status`, { status: newStatus }),
    onSuccess: () => {

      queryClient.invalidateQueries({ queryKey: ['schedule'] });
    },
    onError: (err) => alert(`Помилка: ${err.message}`)
  });


  const handleStatusUpdate = (newStatus) => {
    if (!selectedTask) return;
    updateStatusMutation.mutate({ taskId: selectedTask.id, newStatus });
  };

  return (
    <div className="w-full">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Мій графік</h1>
        <p className="text-gray-400 text-sm mt-1">Графік роботи на {selectedDate.label}</p>
      </div>
      
      
      <div className="flex items-center gap-2 mb-6 border-b border-gray-100 pb-2 overflow-x-auto">
        {days.map((day) => (
          <button 
            key={day.dateValue} 
            onClick={() => setSelectedDate(day)} 
            className={`px-4 py-2 text-sm font-medium transition-all rounded-md whitespace-nowrap ${
              selectedDate.dateValue === day.dateValue ? 'bg-[#E54D43] text-white shadow-md' : 'text-gray-500 hover:bg-gray-50'
            }`}
          >
            {day.label}
          </button>
        ))}
        
      </div>

  
      {loading ? (
        <p className="text-gray-500 text-center py-10">Завантаження...</p>
      ) : scheduleData.length > 0 ? (
        <div className="flex flex-col lg:flex-row gap-8">
  
          <ScheduleGrid data={scheduleData} selectedTask={selectedTask} onSelectTask={setSelectedTask} />
          
      
          <ActionsPanel 
            selectedTask={selectedTask} 
            onUpdateStatus={handleStatusUpdate} 
            isUpdating={updateStatusMutation.isLoading} 
          />
        </div>
      ) : (
        <div className="flex flex-col lg:flex-row gap-8">
          <div className="flex-1 text-center py-20 text-gray-400 border border-dashed rounded-xl">Немає замовлень</div>
          <ActionsPanel selectedTask={null} />
        </div>
      )}
    </div>
  );
}