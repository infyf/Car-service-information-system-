import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import api from "../api/api";

export const useRecommendations = (userId) => {
  const queryClient = useQueryClient();

  const { data: recommendations = [] } = useQuery({
    queryKey: ['recommendations', userId],
    queryFn: () => api.get(`/Recommendations/user/${userId}`).then(data => 
      (data || []).map(r => ({ 
        ...r, 
        price: r.price || r.priceFrom || 0, 
        title: r.title || "Послуга", 
        id: r.id, 
        status: r.status || "pending",

        durationMinutes: r.durationMinutes || 60 
      }))
    ),
    enabled: !!userId
  });

  const groupedRecommendations = recommendations.reduce((acc, rec) => {
    if (rec.status === "booked" || rec.status === "declined") return acc;
    if (!acc[rec.bookingId]) {
      acc[rec.bookingId] = { 
        bookingId: rec.bookingId, 
        bookingDate: rec.bookingDate, 
        masterId: rec.masterId, 
        items: [], 
        recIds: [] 
      };
    }
    acc[rec.bookingId].items.push(rec);
    acc[rec.bookingId].recIds.push(rec.id);
    return acc;
  }, {});


  const previewBatch = async ({ recommendationIds, timeStrategy }) => {
    try {
      const response = await api.post(`/Recommendations/preview-batch`, { recommendationIds, timeStrategy });
      return { isSuccess: true, ...response };
    } catch (error) {
      return { isSuccess: false, error: error?.response?.data?.message || "Не вдалося підібрати час." };
    }
  };


  const acceptBatchMutation = useMutation({
    mutationFn: (data) => api.post(`/Recommendations/accept-batch`, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['recommendations', userId] });
      queryClient.invalidateQueries({ queryKey: ['userBookings', userId] });
    }
  });

  const acceptRecommendationsBatch = async ({ recommendationIds, timeStrategy }) => {
    try {
      const result = await acceptBatchMutation.mutateAsync({ recommendationIds, timeStrategy });
      return { isSuccess: true, ...result };
    } catch (error) {
      return { isSuccess: false, error: error?.response?.data?.message || "Помилка запису." };
    }
  };

  return {
    groupsList: Object.values(groupedRecommendations),
    hasProcessedRecs: recommendations.some(r => r.status === "booked"),
    previewBatch,
    acceptRecommendationsBatch,
    fetchRecommendations: () => queryClient.invalidateQueries({ queryKey: ['recommendations', userId] }),
    isAccepting: acceptBatchMutation.isPending
  };
};