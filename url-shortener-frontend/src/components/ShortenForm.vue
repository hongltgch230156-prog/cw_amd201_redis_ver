<script setup>
import { ref, computed } from 'vue';
import axios from 'axios';
import { useAuthStore } from '../stores/auth';
import { shortenUrl } from '../services/api.js'; 

const auth = useAuthStore();
const isLoggedIn = computed(() => auth.isLoggedIn);

const emit = defineEmits(['url-shortened']);

const originalUrl = ref('');
const customShortcode = ref('');
const errorMessage = ref('');
const isLoading = ref(false);

const props = defineProps({
  isLoggedIn: Boolean
});


// Regex shortcode: chỉ chữ + số, tối đa 6 ký tự
const shortcodeRegex = /^[A-Za-z0-9]{1,6}$/;

const callShortenUrlApi = async () => {
  errorMessage.value = '';

  if (!originalUrl.value) {
    errorMessage.value = 'Please enter a long URL.';
    return;
  }

  if (!isLoggedIn.value && customShortcode.value) {
    errorMessage.value = 'You must be logged in to use custom shortcode!';
    return;
  }

  if (customShortcode.value && !shortcodeRegex.test(customShortcode.value)) {
    errorMessage.value = 'Shortcode consists of only letters + numbers and maximum 6 characters';
    return;
  }

  try {
    isLoading.value = true;

    let shortUrl, inputOriginalUrl;

    if (isLoggedIn.value && customShortcode.value) {
      // User nhập shortcode → CRUD backend (port 2222)
      const body = {
        originalUrl: originalUrl.value,
        shortCode: customShortcode.value
      };

      console.log('Token sent to backend:', auth.idToken);
      const result = await axios.post(
        'https://localhost:2222/api/Url',
        body,
        {
          headers: { Authorization: `Bearer ${auth.idToken}` }
        }
      );

      shortUrl = `https://localhost:3333/api/Url/${result.data.shortCode}`;
      inputOriginalUrl = originalUrl.value;

    } else {
      // 2️⃣ Không nhập shortcode → dùng API tự động (port 3333)
      const result = await shortenUrl(originalUrl.value);
      shortUrl = result.url;           // backend trả về URL đầy đủ
      inputOriginalUrl = result.originalUrl;
    }

    emit('url-shortened', {
      id: shortUrl.split('/').pop(),
      shortUrl,
      originalUrl: inputOriginalUrl
    });

    // Reset form
    originalUrl.value = '';
    customShortcode.value = '';

  } catch (err) {
    console.error(err);
    errorMessage.value =
      err.response?.data?.message || 'Unable to shorten URL. Please try again.';
  } finally {
    isLoading.value = false;
  }
};
</script>

<template>
  <form @submit.prevent="callShortenUrlApi" class="shorten-form">
    <input
      v-model="originalUrl"
      type="url"
      placeholder="Enter long URL..."
      class="input-field"
      required
    />

    <div v-if="isLoggedIn" class="shortcode-box">
      <label>Custom Alias(maximum 6 characters):</label>
      <input
        v-model="customShortcode"
        type="text"
        maxlength="6"
        placeholder="Enter Shortcode (optional)"
        class="input-field"
      />
    </div>

    <button
      type="submit"
      :disabled="isLoading"
      class="main-btn">
      <span v-if="isLoading">Shortening...</span>
      <span v-else>Shorten</span>
    </button>

    <p v-if="errorMessage" class="error-text">{{ errorMessage }}</p>
  </form>
</template>

<style scoped>
/* FORM WRAPPER */
.shorten-form {
  width: 100%;
  max-width: 600px;
  margin: auto;
  background: white;
  padding: 24px;
  border-radius: 14px;
  box-shadow: 0 4px 14px rgba(0,0,0,0.08);
}


/* INPUT FIELD */
.input-field {
  width: 100%;
  padding: 14px;
  border-radius: 10px;
  border: 1px solid #d1d5db;
  font-size: 16px;
  outline: none;
  box-sizing: border-box;
  transition: 0.2s ease;
}

.input-field:focus {
  border-color: #42b983;
  box-shadow: 0 0 4px rgba(66, 185, 131, 0.45);
}

/* SHORTCODE WRAPPER */
.shortcode-box {
  text-align: left;
  margin-bottom: 14px;
}

.shortcode-box label {
  font-size: 14px;
  font-weight: 600;
  margin-bottom: 6px;
  display: block;
}

/* BUTTON */
.main-btn {
  width: 100%;
  margin-top: 12px;
  padding: 14px;
  background: #42b983;
  color: white;
  font-size: 16px;
  font-weight: 600;
  border-radius: 10px;
  transition: 0.2s;
}

.main-btn:hover {
  background-color: #369f6f;
}

.main-btn:disabled {
  background-color: #9cd8bc;
  cursor: not-allowed;
}

/* ERROR */
.error-text {
  color: #e63946;
  font-size: 14px;
  margin-top: 10px;
}
</style>
