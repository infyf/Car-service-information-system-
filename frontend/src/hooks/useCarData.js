import { useState, useEffect } from "react";
import api from "../api/api";

export const useCarData = (userId) => {
  const [carData, setCarData] = useState({
    carBrand: "", carModel: "", carYear: "", carEngine: "", carPlate: ""
  });

  useEffect(() => {
    if (!userId) return;
    
    api.get(`/Profile/${userId}`).then(data => {
      setCarData({
        carBrand: data?.carBrand || "",
        carModel: data?.carModel || "",
        carYear: data?.carYear?.toString() || "",
        carEngine: data?.carEngine || "",
        carPlate: data?.carPlate || ""
      });
    }).catch(err => console.error("Помилка завантаження даних авто:", err));
  }, [userId]);

  return { carData, setCarData };
};