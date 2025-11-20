<script setup>
import { ref } from 'vue';

const emit = defineEmits(['url-found']); 

const shortUrl = ref('');
const isLoading = ref(false);
const errorMessage = ref('');
const successMessage = ref('');

// Cấu hình đường dẫn Backend của bạn (Port mà backend đang chạy)
const URL_SHORTEN_SERVICE = import.meta.env.VITE_URL_SHORTEN_URL;

const lookupOriginalUrl = () => {
  errorMessage.value = '';
  successMessage.value = '';
  
  if (!shortUrl.value) {
    errorMessage.value = 'Please enter short URL or code';
    return;
  }
  
  // 1. Logic trích xuất Short Code
  let shortCode = shortUrl.value.trim();
  
  // Xử lý nếu user paste cả link dài: https://localhost:3333/api/Url/ABC123
  // Hoặc paste link frontend: http://localhost:8085/ABC123
  if (shortCode.includes('/')) {
    // Lấy phần cuối cùng sau dấu gạch chéo
    const parts = shortCode.split('/');
    shortCode = parts[parts.length - 1];
  }

  // Validate mã (Chỉ chữ và số, tối đa 6 ký tự)
  if (!shortCode || !/^[a-zA-Z0-9]{6}$/.test(shortCode)) {
     // Lưu ý: Tùy regex của bạn, ở trên bạn để 1-6 ký tự thì sửa lại regex
  }
  
  // 2. THAY VÌ GỌI API -> TA MỞ LUÔN LINK
  try {
    isLoading.value = true;

    // Tạo link đầy đủ tới Backend
    const fullTargetUrl = `${URL_SHORTEN_SERVICE}/${shortCode}`;

    // Mở link trong tab mới (Trình duyệt sẽ tự lo phần Redirect, không bị lỗi CORS)
    window.open(fullTargetUrl, '_blank');

    // Thông báo giao diện
    successMessage.value = `Redirecting to destination...`;
    
    // (Tùy chọn) Emit sự kiện nếu cha cần biết
    emit('url-found', {
      shortUrl: fullTargetUrl,
      originalUrl: 'Hidden (Redirected)' // Vì Redirect nên ta không biết link gốc
    });

    // Reset input
    shortUrl.value = ''; 

  } catch (error) {
    console.error(error);
    errorMessage.value = 'Something went wrong.';
  } finally {
    isLoading.value = false;
  }
};
</script>

<template>
  <form @submit.prevent="lookupOriginalUrl" class="lookup-wrapper">
    <input
      v-model="shortUrl"
      type="text"
      placeholder="Enter short code (e.g., Xy7z9A)..."
      class="lookup-input"
      required
    />

    <button
      type="submit"
      :disabled="isLoading"
      class="lookup-btn"
    >
      <span v-if="isLoading">Opening...</span>
      <span v-else>Go to Link</span> 
    </button>
    
    <p v-if="errorMessage" class="msg-error">{{ errorMessage }}</p>
    <p v-if="successMessage" class="msg-success">{{ successMessage }}</p>
  </form>
</template>

<style scoped>
/* ... (Giữ nguyên CSS cũ của bạn) ... */
.lookup-wrapper {
  width: 100%;
  max-width: 600px;
  margin: auto;
  background: white;
  padding: 24px;
  border-radius: 14px;
  box-shadow: 0 4px 14px rgba(0,0,0,0.08);
}
.lookup-input {
  width: 100%;
  padding: 14px;
  border-radius: 10px;
  border: 1px solid #d1d5db;
  font-size: 16px;
  outline: none;
  box-sizing: border-box;
  transition: 0.2s ease;
}
.lookup-input:focus {
  border-color: #42b983;
  box-shadow: 0 0 0 3px rgba(66,185,131,0.25);
}
.lookup-btn {
  width: 100%;
  margin-top: 12px;
  padding: 14px;
  background: #42b983;
  color: white;
  font-size: 16px;
  font-weight: 600;
  border-radius: 10px;
  transition: 0.2s;
  cursor: pointer; /* Thêm cái này cho trải nghiệm tốt hơn */
}
.lookup-btn:hover {
  background: #369d6d;
}
.lookup-btn:disabled {
  background: #8bd8b5;
  cursor: not-allowed;
}
.msg-error {
  margin-top: 10px;
  color: #e63946;
  text-align: center;
  font-size: 14px;
}
.msg-success {
  margin-top: 10px;
  color: #2a9d8f;
  text-align: center;
  font-size: 14px;
}
</style>