import React from 'react';

const isConsultationItem = (item) => {
  if (!item) return false;
  const checkString = (str) => str && typeof str === 'string' && str.toLowerCase().includes('консультац');
  return item.bookingType === 'consultation' || 
         checkString(item.name) || 
         checkString(item.service) || 
         checkString(item.service?.title);
};


const getStatusColor = (item) => {
  if (isConsultationItem(item)) return 'bg-purple-500';
  switch (item.status) {
    case 'completed': return 'bg-green-500';
    case 'in_progress': return 'bg-blue-500';
    case 'pending': return 'bg-amber-400';
    case 'cancelled': return 'bg-red-400';
    default: return 'bg-gray-300';
  }
};

export default function ScheduleGrid({ data, selectedTask, onSelectTask }) {
  return (
    <div className="flex-1 border border-gray-200 rounded bg-white overflow-hidden h-fit">
      <div className="grid grid-cols-[80px_1fr] bg-white">
        {data.map((item) => {
          const isSelected = selectedTask?.id === item.id;
          
          return (
            <React.Fragment key={item.id}>
              {/* Колонка часу */}
              <div className="p-4 border-b border-r border-gray-100 text-sm font-bold text-gray-900 flex items-start justify-center bg-gray-50/50">
                {item.time}
              </div>
              
              {/* колонка завдання */}
              <div className="p-2 border-b border-gray-100 min-h-[100px]">
                <div 
                  onClick={() => onSelectTask(item)} 
                  className={`
                    relative p-3 h-full bg-white border rounded shadow-sm 
                    hover:shadow-md transition-all cursor-pointer focus:outline-none
                    ${isSelected ? 'border-gray-800 shadow-md' : 'border-gray-200'}
                  `}
                >
                  {/* квадрат тасків  */}
                  <div className={`absolute top-2 left-2 w-3 h-3 rounded-sm ${getStatusColor(item)}`}></div>

                
                  <div className="pl-4">
                    <div className="flex justify-between items-start">
                      <div className="flex-1 min-w-0 mr-2">
                   
                        <h3 className="font-bold text-gray-800 text-sm truncate">{item.name}</h3>
                       
                        <p className="text-xs text-gray-500 font-medium truncate">{item.car}</p>
                       
                        <p className="text-[11px] text-gray-400 mt-1 truncate">{item.service}</p>
                      </div>

                      <div className="text-right text-[11px] font-semibold text-gray-600 whitespace-nowrap">
                        <div>{item.duration}</div>
                        <div className="mt-0.5">{item.price}</div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </React.Fragment>
          );
        })}
      </div>
    </div>
  );
}