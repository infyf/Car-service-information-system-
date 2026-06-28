const API_URL = 'https://localhost:7064/api';


const request = async (method, endpoint, data = null, isFormData = false) => {
  const headers = {};
  
  const token = localStorage.getItem('token');
  if (token) headers['Authorization'] = `Bearer ${token}`;
  

  if (!isFormData) headers['Content-Type'] = 'application/json';

  const config = { method, headers };
  if (data) config.body = isFormData ? data : JSON.stringify(data);

  const response = await fetch(`${API_URL}${endpoint}`, config);
  return handleResponse(response, isFormData);
};


async function handleResponse(response, isFormData = false) {
  if (response.status === 204) return null; // Порожня відповідь (наприклад, при успішному DELETE)

  const text = await response.text();
  let data = null;

  if (text) {
    try {
      data = JSON.parse(text);
    } catch (e) {

      if (!response.ok) throw new Error(text || 'Помилка завантаження файлу');
      return text; 
    }
  }

  if (!response.ok) {
    const error = new Error(data?.message || 'Помилка запиту');
    error.data = data; 
    error.status = response.status; 
    throw error;
  }

  return data;
}

const api = {

  get: (endpoint) => request('GET', endpoint),
  post: (endpoint, data) => request('POST', endpoint, data),
  put: (endpoint, data) => request('PUT', endpoint, data),
  patch: (endpoint, data) => request('PATCH', endpoint, data),
  delete: (endpoint) => request('DELETE', endpoint), 


  postFormData: (endpoint, formData) => request('POST', endpoint, formData, true),
  putFormData: (endpoint, formData) => request('PUT', endpoint, formData, true),
};

export default api;