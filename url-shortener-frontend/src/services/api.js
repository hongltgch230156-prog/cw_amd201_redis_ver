import axios from 'axios';

// ====================== Biến môi trường ======================
const URL_SHORTEN_SERVICE = import.meta.env.VITE_URL_SHORTEN_URL; // http://service_url_shorten:8080/api/Url
const CRUD_SERVICE        = import.meta.env.VITE_CRUD_URL;        // http://service_crud:8080/api/url
const IDENTITY_SERVICE    = import.meta.env.VITE_IDENTITY_URL;    // http://service_identity:8080/api/Auth

//const API_BASE_URL = 'https://localhost:3333/api/Url';
console.log("URL_SHORTEN_SERVICE:", URL_SHORTEN_SERVICE);

// 1. Tạo instance Axios đặc biệt để BẮT Redirect (maxRedirects: 0)
const redirectCatchingApi = axios.create({
    baseURL: URL_SHORTEN_SERVICE,
    maxRedirects: 0 
});

// 2. Tạo instance Axios tiêu chuẩn cho các lệnh POST, GET bình thường
const standardApi = axios.create({
    baseURL: URL_SHORTEN_SERVICE,
});

/**
 * Gọi API rút gọn URL (Dài -> Ngắn).
 * @param {string} originalUrl - URL dài cần rút gọn.
 * @param {string | null} customShortCode - Mã tùy chỉnh (chỉ dành cho user logged-in).
 */
export const shortenUrl = async (originalUrl, customShortCode = null) => {
    // Sửa: Gửi cả originalUrl và CustomShortCode
    // (Giả định Backend đã cập nhật CreateShortUrlCommand để nhận trường này)
    const payload = { 
        originalUrl: originalUrl,
        customShortCode: customShortCode 
    };

    const fullApiUrl = `${URL_SHORTEN_SERVICE}/shorten`;
    const response = await axios.post(fullApiUrl, payload);
    return response.data;
};

//Lấy URL gốc từ mã ngắn (Ngắn -> Dài)
export const getUrlDetails = async (shortCode) => {
  try {
    // Hàm này dùng redirectCatchingApi.get() nên chúng ta sẽ chuyển nó sang gọi tuyệt đối luôn để đồng bộ
    const fullApiUrl = `${URL_SHORTEN_SERVICE}/${shortCode}`;
    const response = await axios.get(fullApiUrl, {
        maxRedirects: 0 
    }); 

    // Backend trả về dạng:
    // { originalUrl: "https://google.com" }
    return {
      Url: `${URL_SHORTEN_SERVICE}/${shortCode}`,
      originalUrl: response.data.originalUrl
    };

  } catch (error) {
    if (error.response && error.response.status === 404) {
      throw new Error('Mã ngắn không tồn tại.');
    }
    throw new Error(error.message || 'Lỗi kết nối không xác định.');
  }
};


/**
 * Lấy mã QR code dưới dạng Blob và tạo Object URL.
 * (Giữ nguyên)
 */
export const getQrCodeUrl = async (shortCode) => {
    const fullApiUrl = `${URL_SHORTEN_SERVICE}/qr/${shortCode}`;
    const response = await axios.get(fullApiUrl, { 
        responseType: 'blob' 
    });
    return URL.createObjectURL(response.data); 
};

export async function deleteUrlById(id, token) {
  try {
    const res = await axios.delete(`${CRUD_SERVICE}/${id}`, {
      headers: {
        Authorization: `Bearer ${token}`
      }
    });

    // Xóa thành công nếu status là 200 hoặc 204
    return res.status === 200 || res.status === 204;
  } catch (err) {
    console.error('Delete API error:', err);
    return false;
  }
}

// export const deleteAllUrls = async () => {
//   try {
//     const response = await axios.delete('https://localhost:2222/api/Url', {
//       headers: { Authorization: `Bearer ${authStore.idToken}` }
//     });

//     if (response.status === 200) {
//       props.urls.splice(0); // Xóa toàn bộ URL trong danh sách frontend
//       alert('Đã xóa tất cả URL.');
//     } else {
//       alert('Xóa tất cả thất bại.');
//     }
//   } catch (err) {
//     console.error('Lỗi xóa tất cả URL:', err);
//     alert('Lỗi khi xóa tất cả URL.');
//   } finally {
//     showDeleteAllModal.value = false;
//   }
// };
