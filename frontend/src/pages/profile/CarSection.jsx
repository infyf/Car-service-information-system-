import { Car, Save, X } from "lucide-react";
import { useState } from "react";
import api from "../../api/api";

const BACKEND_URL = "https://localhost:7064";

const getImageSrc = (url) => {
  if (!url) return null;
  if (url.startsWith("http")) return url;
  return `${BACKEND_URL}${url}`;
};

export default function CarSection({ profile, isEditingCar, setIsEditingCar, isSavingCar, carData, setCarData, handleCarSave, handleCarCancel, userId }) {
  const hasCar = profile?.carBrand && profile.carBrand !== "Не вказано";
  const [selectedFile, setSelectedFile] = useState(null);
  const [isUploading, setIsUploading] = useState(false);

  const handleSaveWithFile = async () => {
    if (!selectedFile) { handleCarSave(); return; }
    setIsUploading(true);
    try {
      const formData = new FormData();
      formData.append("file", selectedFile);
      const uploadRes = await api.postFormData(`/Profile/${userId}/uploadCar`, formData);
      const updatedCarData = { ...carData, carImageUrl: uploadRes.url };
      await handleCarSave(updatedCarData);
      setSelectedFile(null);
    } catch (error) {
      alert("Помилка завантаження файлу: " + (error.message || "Невідома помилка"));
    } finally { setIsUploading(false); }
  };

  return (
    <div className="rounded-4 bg-white p-4 border shadow-sm">
      <div className="d-flex align-items-center justify-content-between mb-4">
        <h2 className="fw-bold mb-0">Мій автомобіль</h2>
        {!isEditingCar && hasCar && (
        <button onClick={() => setIsEditingCar(true)} className="btn btn-sm btn-link text-decoration-none text-secondary d-flex align-items-center gap-1 hover-orange">
          Змінити
        </button>
      )}
      </div>
      
      {isEditingCar ? (
        <div className="d-flex flex-column gap-3" style={{ maxWidth: '28rem' }}>
          <div>
            <label className="form-label small fw-medium text-secondary">Фото автомобіля:</label>
            <input 
              type="file" 
              accept="image/*" 
              onChange={(e) => setSelectedFile(e.target.files[0])} 
              className="form-control form-control-sm custom-file-input" 
            />
            {selectedFile && <img src={URL.createObjectURL(selectedFile)} alt="Preview" className="mt-2 rounded-3 border object-fit-cover" style={{ height: '6rem', width: '100%' }} />}
          </div>

          {[{ name: "carBrand", label: "Марка" }, { name: "carModel", label: "Модель" }, { name: "carYear", label: "Рік" }, { name: "carEngine", label: "Двигун" }, { name: "carPlate", label: "Номер" }].map(f => (
            <div key={f.name} className="d-flex align-items-center gap-3">
              <label className="small fw-medium text-secondary" style={{ width: '5rem' }}>{f.label}:</label>
              <input 
                name={f.name} 
                value={carData[f.name]} 
                onChange={(e) => setCarData({ ...carData, [e.target.name]: e.target.value })} 
                className="form-control form-control-sm" 
              />
            </div>
          ))}
          
          <div className="d-flex gap-2 mt-2">
            <button onClick={handleSaveWithFile} disabled={isSavingCar || isUploading} className="btn btn-modern-orange text-white d-flex align-items-center gap-1.5 px-4 py-2 fw-semibold border-0">
              <Save size={14} />
              {isUploading ? "Завантаження фото..." : isSavingCar ? "Збереження..." : "Зберегти"}
            </button>
            <button onClick={handleCarCancel} disabled={isSavingCar || isUploading} className="btn btn-light border d-flex align-items-center gap-1.5 px-4 py-2">
              <X size={14} /> Скасувати
            </button>
          </div>
        </div>
      ) : hasCar ? (
        <div className="d-flex flex-column flex-sm-row align-items-start gap-4">
          <div className="rounded-3 overflow-hidden bg-light border d-flex align-items-center justify-content-center flex-shrink-0" style={{ width: '13rem', height: '9rem' }}>
            {profile?.carImageUrl ? (
              <img src={getImageSrc(profile.carImageUrl)} alt="Auto" className="w-100 h-100 object-fit-cover" />
            ) : (
              <Car size={48} className="text-secondary opacity-25" />
            )}
          </div>
          <div className="d-flex flex-column gap-2">
            <p className="fs-5 fw-bold mb-0">{profile.carBrand} {profile.carModel}</p>
            <p className="small text-muted mb-0">{profile.carYear} {profile.carEngine}</p>
            {profile.carPlate !== "Не вказано" && <span className="badge bg-light text-dark border border-secondary fw-bold mt-1 w-fit">{profile.carPlate}</span>}
          </div>
        </div>
      ) : (
        <div className="d-flex flex-column align-items-center justify-content-center py-5 text-secondary">
          <Car size={48} className="mb-3 opacity-25" />
          <p className="small mb-3">Автомобіль не додано</p>
          <button onClick={() => setIsEditingCar(true)} className="btn btn-modern-orange text-white px-4 py-2 fw-semibold border-0">Додати</button>
        </div>
      )}
    </div>
  );
}