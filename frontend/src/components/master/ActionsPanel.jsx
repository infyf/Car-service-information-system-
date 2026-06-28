import { useState, useEffect, useRef } from 'react'; // Додано useRef
import { CheckCircle, XCircle, PlayCircle, MessageCircle, Loader2 } from 'lucide-react';



const isConsultationItem = (item) => {
  if (!item) return false;
  const checkString = (str) => str && typeof str === 'string' && str.toLowerCase().includes('консультац');
  return item.bookingType === 'consultation' || checkString(item.name) || checkString(item.service) || checkString(item.service?.title);
};

export default function ActionsPanel({ selectedTask, onUpdateStatus }) {
  const [phase, setPhase] = useState('idle');
  const [localStatus, setLocalStatus] = useState(null);
  const [lastAction, setLastAction] = useState(null);
  

  const prevTaskIdRef = useRef(null);

  
  useEffect(() => {
    const currentId = selectedTask?.id;

    // якщо завдання є, і його ID не збігається з попереднім
    if (selectedTask && currentId !== prevTaskIdRef.current) {
      setLocalStatus(selectedTask.status);
      setPhase('idle');
      prevTaskIdRef.current = currentId; // ref для наступного рендеру
    }
  }, [selectedTask]); 

  const handleStatusUpdate = async (newStatus) => {
    setPhase('loading');
    setLastAction(newStatus);

    try {
      await onUpdateStatus(newStatus);
      
      setLocalStatus(newStatus);
      setPhase('success'); 
      
      setTimeout(() => setPhase('idle'), 1500);
      
    } catch (error) {
      setPhase('idle'); 
    }
  };

  const isConsultation = isConsultationItem(selectedTask);
  const currentStatus = localStatus || selectedTask?.status;

  const successTexts = {
    'in_progress': 'Статус змінено на в прогресі',
    'completed': 'Статус успішно завершено',
    'cancelled': 'Запис успішно скасовано'
  };

  return (
    <div className="w-full lg:w-72 space-y-6">
      <div className="bg-gray-50/50 p-6 rounded-xl border border-gray-100 sticky top-24">
        <h2 className="text-xl font-bold mb-2 text-gray-800">Мої дії</h2>

        {!selectedTask ? (
          <>
            <p className="text-gray-400 text-sm mb-8">Оберіть завдання в графіку</p>
              <div className="pt-4 border-t border-gray-200">
              <p className="text-xs text-gray-500 mb-3 font-semibold uppercase">Статуси</p>
              <div className="space-y-2 text-xs text-gray-600">
                <div className="flex items-center gap-2"><div className="w-3 h-3 bg-green-500 border border-gray-300 rounded-sm"></div> Виконано</div>
                <div className="flex items-center gap-2"><div className="w-3 h-3 bg-blue-500 border border-gray-300 rounded-sm"></div> Виконується</div>
                <div className="flex items-center gap-2"><div className="w-3 h-3 bg-amber-400 border border-gray-300 rounded-sm"></div> Очікує</div>
                <div className="flex items-center gap-2"><div className="w-3 h-3 bg-red-400 border border-gray-300 rounded-sm"></div> Скасовано</div>
                <div className="flex items-center gap-2"><div className="w-3 h-3 bg-purple-400 border border-gray-300 rounded-sm"></div> Консультація</div>
              </div>
            </div>
          </>
        ) : (
          <>
            <div className="bg-white p-3 rounded-lg mb-4 border text-xs text-gray-600">
              <p className="font-bold text-gray-800 text-sm truncate">{selectedTask.name}</p>
              <p className="truncate">{selectedTask.service}</p>
              <p className="text-orange-600 font-bold mt-1">{selectedTask.price}</p>
            </div>

            {isConsultation ? (
              <div className="bg-purple-50 border border-purple-200 rounded-lg p-4 text-center">
                <MessageCircle className="w-8 h-8 text-purple-500 mx-auto mb-2" />
                <p className="text-sm font-semibold">Це консультація</p>
              </div>
            ) : (
              <div className="min-h-[120px] flex flex-col justify-center">
                
                {phase === 'loading' && (
                  <div className="flex flex-col items-center justify-center py-4 text-blue-500">
                    <Loader2 size={32} className="animate-spin mb-2" />
                    <p className="text-sm font-medium">Оновлення...</p>
                  </div>
                )}

                {phase === 'success' && (
                  <div className="flex flex-col items-center justify-center py-4 text-green-600 animate-[fadeIn_0.3s_ease-in-out]">
                    <CheckCircle size={36} className="mb-2 stroke-[1.5]" />
                    <p className="text-sm font-bold text-center px-2">
                      {successTexts[lastAction]}
                    </p>
                  </div>
                )}

                {phase === 'idle' && (
                  <div className="flex flex-col gap-3">
                    
                    {(currentStatus === 'confirmed' || currentStatus === 'pending') && (
                      <button 
                        onClick={() => handleStatusUpdate('in_progress')} 
                        className="flex items-center justify-center gap-2 w-full bg-blue-500 hover:bg-blue-600 text-white p-3 rounded-md transition-all font-medium shadow-sm text-sm hover:shadow-md active:scale-[0.98]"
                      >
                        <PlayCircle size={18} /> Почати роботу
                      </button>
                    )}
                    
                    {currentStatus === 'in_progress' && (
                      <button 
                        onClick={() => handleStatusUpdate('completed')} 
                        className="flex items-center justify-center gap-2 w-full bg-green-600 hover:bg-green-700 text-white p-3 rounded-md transition-all font-medium shadow-sm text-sm hover:shadow-md active:scale-[0.98]"
                      >
                        <CheckCircle size={18} /> Завершити
                      </button>
                    )}
                    
                    {currentStatus !== 'completed' && currentStatus !== 'cancelled' && (
                      <button 
                        onClick={() => handleStatusUpdate('cancelled')} 
                        className="flex items-center justify-center gap-2 w-full bg-white border border-red-200 text-red-600 hover:bg-red-50 p-3 rounded-md transition-all font-medium shadow-sm text-sm active:scale-[0.98]"
                      >
                        <XCircle size={18} /> Скасувати запис
                      </button>
                    )}

                    {(currentStatus === 'completed' || currentStatus === 'cancelled') && (
                      <div className="text-center text-gray-400 text-sm py-4 border border-dashed border-gray-200 rounded-md">
                        Статус фінальний
                      </div>
                    )}
                  </div>
                )}
              </div>
            )}

             <div className="pt-4 border-t border-gray-200">
              <p className="text-xs text-gray-500 mb-3 font-semibold uppercase">Статуси</p>
              <div className="space-y-2 text-xs text-gray-600">
                <div className="flex items-center gap-2"><div className="w-3 h-3 bg-green-500 border border-gray-300 rounded-sm"></div> Виконано</div>
                <div className="flex items-center gap-2"><div className="w-3 h-3 bg-blue-500 border border-gray-300 rounded-sm"></div> Виконується</div>
                <div className="flex items-center gap-2"><div className="w-3 h-3 bg-amber-400 border border-gray-300 rounded-sm"></div> Очікує</div>
                <div className="flex items-center gap-2"><div className="w-3 h-3 bg-red-400 border border-gray-300 rounded-sm"></div> Скасовано</div>
                <div className="flex items-center gap-2"><div className="w-3 h-3 bg-purple-400 border border-gray-300 rounded-sm"></div> Консультація</div>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}