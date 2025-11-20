<script setup>
import { defineProps, ref } from 'vue';
import { getQrCodeUrl, deleteUrlById } from '../services/api.js';
import { useAuthStore } from '../stores/auth';
import { useToast } from "vue-toastification";

const toast = useToast();
const authStore = useAuthStore();

const props = defineProps({
  urls: {
    type: Array,
    default: () => []
  },
  isLoggedIn: {
    type: Boolean,
    default: false
  }
});

// State để quản lý Modal QR code
const qrCodeImageUrl = ref(null);
const showQrModal = ref(false);
const qrCodeLoading = ref(false); 
const qrCodeError = ref(null); 

// Hàm mở Modal và gọi API lấy QR code
const openQrModal = async (shortUrl) => {
  qrCodeImageUrl.value = null;
  qrCodeError.value = null;
  showQrModal.value = true;
  qrCodeLoading.value = true;
  
  // Trích xuất short code
  const shortCodeMatch = shortUrl.match(/\/api\/url\/([a-zA-Z0-9]+)$/i);
  if (!shortCodeMatch) {
    qrCodeError.value = 'No valid Shortcode found';
    qrCodeLoading.value = false;
    return;
  }
  const shortCode = shortCodeMatch[1];
  
  try {
    // Sửa: Gọi hàm API đã đóng gói, không cần $axios
    const imageUrl = await getQrCodeUrl(shortCode);
    qrCodeImageUrl.value = imageUrl;
  } catch (err) {
    console.error('Error getting QR code:', err);
    qrCodeError.value = 'Unable to generate QR Code. Connection error or invalid code.';
  } finally {
    qrCodeLoading.value = false;
  }
};

// Giữ nguyên logic Copy và Delete
const copyToClipboard = (text) => {
  // Lưu ý: Sử dụng alert() và confirm() chỉ mang tính chất minh họa đơn giản
  navigator.clipboard.writeText(text).then(() => {
    toast.info('Copied to clipboard: ' + text);
  }).catch(err => {
    console.error('Copy error:', err);
    toast.error('Cannot be copied. Please try again.');
  });
};


// --- Delete / Xóa ---
const emit = defineEmits(['delete-item']);

// Khi khởi tạo urls, thêm default cho showDeleteOptions
props.urls.forEach(url => {
  if (url.showDeleteOptions === undefined) url.showDeleteOptions = false;
});


const deleteTemporary = (urlItem, index) => {
  props.urls.splice(index, 1);
  toast.success('Temporarily removed from list');
};

const deletePermanent = async (urlItem, index) => {
  const shortCodeMatch = urlItem.shortUrl.match(/\/([a-zA-Z0-9]+)$/);
  if (!shortCodeMatch) {
    toast.error('Invalid Shortcode. Cannot delete.');
    return;
  }
  const shortCode = shortCodeMatch[1];

  try {
    const success = await deleteUrlById(shortCode, authStore.idToken);
    if (success) {
      props.urls.splice(index, 1);
      toast.success('URL permanently removed');
    } else {
      toast.error('Failed to delete URL. Please try again.');
    }
  } catch (err) {
    console.error(err);
    toast.error('Error while deleting URL: ' + (err.message || 'unidentified error'));
  }
};


</script>

<template>
  <div class="url-list-wrapper">

    <!-- Khi chưa có URL -->
    <div v-if="urls.length === 0" class="empty-box">
      There are no URLs in the list yet
    </div>
    
    <!-- Danh sách URL -->
    <div 
      v-for="(url, index) in urls" 
      :key="index" 
      class="url-card"
    >
      <p class="url-original"><strong>Original URL:</strong> {{ url.originalUrl }}</p>
      
      <!-- Hiển thị link ngắn -->
      <a 
        :href="url.shortUrl" 
        target="_blank" 
        class="url-short"
      >
        {{ url.shortUrl }}
      </a>
      
      <!-- Thời gian rút gọn -->
      <p class="url-date">{{ url.createdAt }}</p>

      <!-- Các nút hành động -->
      <div class="action-buttons">
        <button class="btn-green" @click="copyToClipboard(url.shortUrl)">Copy</button>
        <button class="btn-blue" @click="openQrModal(url.shortUrl)">QR</button>

        <!-- Nút Xóa/Hiển thị 2 lựa chọn -->
        <button v-if="props.isLoggedIn" class="btn-red" @click="url.showDeleteOptions = true">Delete</button>

          <!-- Modal Xóa -->
          <div v-if="url.showDeleteOptions" class="modal-overlay">
            <div class="modal-content">
              <h3 class="modal-title">Delete URL</h3>
              <p>How do you want to delete this URL?</p>
              <div style="display:flex;gap:10px;justify-content:center;margin-top:15px;">
                <button class="btn-orange" @click="deleteTemporary(url, index)">Temporary deletion</button>
                <button class="btn-red" @click="deletePermanent(url, index)">Permanently delete</button>
                <button class="btn-gray" @click="url.showDeleteOptions = false">Cancel</button>
              </div>
            </div>
          </div>

        <!-- Guest vẫn xóa tạm thời ngay -->
        <button v-if="!props.isLoggedIn" class="btn-red" @click="deleteTemporary(url, index)">Delete</button>
      </div>
    </div>
    
    <!-- Modal Hiển Thị QR Code (Đã nâng cấp) -->
    <div v-if="showQrModal" class="modal-overlay">
      <div class="modal-content">
        <!-- Đóng modal và reset trạng thái -->
        <button 
          @click="showQrModal = false; qrCodeImageUrl = null; qrCodeError = null;" 
          class="modal-close"
        >
          &times;
        </button>
        <h3 class="modal-title">QR code</h3>
        
        <!-- Hiển thị Loading -->
        <div v-if="qrCodeLoading" class="modal-loading"> 
          Loading QR Code...
        </div>
        
        <!-- Hiển thị Lỗi -->
        <div v-else-if="qrCodeError" class="modal-error">
          {{ qrCodeError }}
        </div>

        <!-- Hiển thị QR Code thành công -->
        <div v-else-if="qrCodeImageUrl" class="modal-body">
          <img :src="qrCodeImageUrl" alt="QR Code" class="qr-image"/>
          <p class="qr-note">Scan to access the original URL</p>
        </div>
        
        <div v-else class="modal-body">
            QR Code cannot be displayed
        </div>

      </div>
    </div>
  </div>
</template>

<style scoped>
/* WRAPPER */
.url-list-wrapper {
  width: 100%;
  max-width: 600px;
  margin: auto;
}

/* EMPTY BOX */
.empty-box {
  background: white;
  padding: 22px;
  border-radius: 12px;
  text-align: center;
  color: #666;
  border: 1px solid #ddd;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
}

/* URL CARD */
.url-card {
  background: white;
  padding: 18px;
  border-radius: 12px;
  box-shadow: 0 3px 10px rgba(0,0,0,0.1);
  border: 1px solid #e8e8e8;
  margin-bottom: 15px;
}

.url-original {
  color: #666;
  font-size: 14px;
  margin-bottom: 6px;
  word-break: break-all;
}

.url-short {
  color: #42b983;
  font-size: 17px;
  font-weight: 600;
  word-break: break-all;
}

.url-date {
  margin-top: 4px;
  color: #9ca3af;
  font-size: 13px;
}

/* BUTTON GROUP */
.action-buttons {
  margin-top: 14px;
  display: flex;
  gap: 8px;
}

button {
  padding: 7px 14px;
  font-size: 14px;
  border-radius: 8px;
  cursor: pointer;
  transition: 0.25s;
  border: none;
}

.btn-green {
  background: #e7f8ef;
  color: #42b983;
}
.btn-green:hover {
  background: #d8f5e6;
}

.btn-blue {
  background: #e6f7fb;
  color: #0284c7;
}
.btn-blue:hover {
  background: #d8f1f8;
}

.btn-red {
  background: #fde7e7;
  color: #e63946;
}
.btn-red:hover {
  background: #fbd1d1;
}

/* MODAL */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.55);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 50;
}

.modal-content {
  background: white;
  padding: 28px;
  border-radius: 16px;
  width: 320px;
  position: relative;
  text-align: center;
  box-shadow: 0 8px 28px rgba(0,0,0,0.2);
}

.modal-close {
  position: absolute;
  top: 10px;
  right: 14px;
  font-size: 28px;
  background: none;
  border: none;
  color: #777;
}
.modal-close:hover {
  color: #333;
}

.modal-title {
  font-size: 20px;
  font-weight: 700;
  margin-bottom: 18px;
}

.modal-loading,
.modal-error {
  color: #666;
  font-size: 15px;
  padding: 20px;
}

.qr-image {
  width: 180px;
  height: 180px;
  border: 1px solid #ccc;
  padding: 4px;
  border-radius: 8px;
}

.qr-note {
  margin-top: 10px;
  font-size: 14px;
  color: #666;
}

.btn-orange {
  background: #facc15;
  color: white;
}
.btn-orange:hover {
  background: #eab308;
}

.btn-gray {
  background: #d1d5db;
  color: #111;
}
.btn-gray:hover {
  background: #9ca3af;
}

</style>
