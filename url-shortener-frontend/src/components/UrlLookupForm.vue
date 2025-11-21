<script setup>
import { ref } from 'vue';
import axios from 'axios';

// [MỚI] Thêm emit cho logging
const emit = defineEmits(['url-found', 'request-logged']); 

const shortUrl = ref('');
const isLoading = ref(false);
const errorMessage = ref('');
const successMessage = ref('');

// Cấu hình đường dẫn Backend (API Gateway)
// Chúng ta sẽ gọi API Info riêng biệt (Không phải API Redirect)
const API_URL = import.meta.env.VITE_URL_SHORTEN_URL; // Ví dụ: http://localhost:8083/api/Url

const lookupOriginalUrl = async () => {
  errorMessage.value = '';
  successMessage.value = '';
  
  if (!shortUrl.value) {
    errorMessage.value = 'Please enter short URL or code';
    return;
  }
  
  let shortCode = shortUrl.value.trim();
  const codeRegex = /([a-zA-Z0-9]+)$/i; // Regex đơn giản để lấy code cuối cùng
  
  const match = shortUrl.value.match(codeRegex); 
  if (match && match[1]) {
    shortCode = match[1];
  } else if (!/^[a-zA-Z0-9]+$/.test(shortCode)) {
    errorMessage.value = 'Invalid code. Please check again.';
    return;
  }

  const fullUrl = `${API_URL}/info/${shortCode}`; 
  
  isLoading.value = true;
  const startTime = performance.now(); // Bắt đầu bấm giờ

  try {
    // 1. GỌI AJAX để lấy UrlResult (chứa OriginalUrl và Source: Redis/Database)
    // Lưu ý: Chúng ta gọi thẳng API Redirect, nhưng vì nó trả về UrlResult (JSON) nên Axions sẽ bắt được.
    const response = await axios.get(fullUrl);
    
    const endTime = performance.now(); // Kết thúc bấm giờ
    const duration = Math.round(endTime - startTime);
    
    const result = response.data; // Đây là UrlResult { OriginalUrl, Source }

    if (!result || !result.originalUrl) {
      errorMessage.value = 'Link not found.';
      return;
    }

    // 2. Ghi LOG cho Dashboard
    emit('request-logged', {
      time: new Date().toLocaleTimeString(),
      duration: duration,
      source: result.source || 'Database', // Lấy nguồn từ BE trả về
    });

    // history kq
    const redirectUrl = `${API_URL}/${shortCode}`;
    emit('url-found', {
      id: shortCode,
      shortUrl: redirectUrl,
      originalUrl: result.originalUrl
    });
    
    successMessage.value = `Original URL found via ${result.source}!`;

  } catch (error) {
    console.error('URL search error:', error);
    errorMessage.value = error.response?.status === 404 
                         ? 'Short URL not found in system.'
                         : error.message || 'Network Error.';
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
      placeholder="Enter short code or full URL..."
      class="lookup-input"
      required
    />

    <button
      type="submit"
      :disabled="isLoading"
      class="lookup-btn"
    >
      <span v-if="isLoading">Searching...</span>
      <!-- Đổi nút để phản ánh việc tra cứu lấy dữ liệu -->
      <span v-else>Find Original URL</span> 
    </button>
    
    <p v-if="errorMessage" class="msg-error">{{ errorMessage }}</p>
    <p v-if="successMessage" class="msg-success">{{ successMessage }}</p>
  </form>
</template>

<style scoped>
/* ... (Style giữ nguyên) ... */
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
  cursor: pointer;
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