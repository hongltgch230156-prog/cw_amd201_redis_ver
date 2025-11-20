<script setup>
import { ref, inject } from 'vue';
import { useToast } from "vue-toastification";

const toast = useToast();

// Lấy các service đã provide
const identityApi = inject('identityApi');
const emit = defineEmits(['close', 'success', 'error']);

const fullName = ref('');
const email = ref('');
const password = ref('');
const dob = ref(''); // YYYY-MM-DD string
const isLoading = ref(false);
const localError = ref('');

// Format DateOfBirth cho backend C#
// C# thường nhận format "yyyy-MM-ddTHH:mm:ss"
function formatDateForBackend(dateStr) {
  if (!dateStr) return "1900-01-01T00:00:00"; // Mặc định an toàn
  const d = new Date(dateStr);
  const yyyy = d.getFullYear();
  const mm = String(d.getMonth() + 1).padStart(2, '0');
  const dd = String(d.getDate()).padStart(2, '0');
  return `${yyyy}-${mm}-${dd}T00:00:00`;
}

const handleRegister = async () => {
  localError.value = '';

  if (!email.value || !password.value || !fullName.value) {
    localError.value = 'Please fill in your Name, Email and Password';
    return;
  }

  isLoading.value = true;

  try {
    const payload = {
      fullName: fullName.value,
      email: email.value,
      password: password.value,
      dateOfBirth: formatDateForBackend(dob.value)
    };

    const response = await identityApi.post('/register', payload);

    toast.success('Registration successful! You can log in now');
    emit('success');

  } catch (error) {
    if (error.response) {
      const backendMessage = error.response.data?.message || error.response.data?.error;
      localError.value = backendMessage || 'Registration failed. Please try again.';
    } else {
      localError.value = 'Network error or unknown error';
    }
  } finally {
    isLoading.value = false;
  }
};

</script>

<template>
  <div class="modal-overlay">
    <div class="modal-content animate-modal">
      <h3 class="modal-title">Register an Account</h3>

      <form @submit.prevent="handleRegister" class="modal-form">
        <input v-model="fullName" type="text" placeholder="Họ và Tên" required/>
        <input v-model="email" type="email" placeholder="Email" required/>
        <input v-model="password" type="password" placeholder="Mật khẩu (tối thiểu 6 ký tự)" required/>
        <input v-model="dob" type="date" placeholder="Ngày sinh (Tùy chọn)"/>

        <button type="submit" :disabled="isLoading">
          <span v-if="isLoading">Registering...</span>
          <span v-else>Register</span>
        </button>
      </form>

      <p v-if="localError" class="error-text">{{ localError }}</p>

      <button @click="emit('close')" class="close-btn">Exit</button>
    </div>
  </div>
</template>

<style scoped>
/* Nền mờ */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  backdrop-filter: blur(2px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 999;
}

/* Modal background */
.modal-content {
  width: 100%;
  max-width: 420px;
  background: white;
  padding: 32px;
  border-radius: 14px;
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.15);
  text-align: center;
}

/* Animation fade + scale */
.animate-modal {
  animation: fadeScale 0.28s ease-out;
}
@keyframes fadeScale {
  from { opacity: 0; transform: scale(0.85); }
  to   { opacity: 1; transform: scale(1); }
}

/* Title */
.modal-title {
  font-size: 26px;
  font-weight: bold;
  margin-bottom: 20px;
  color: #333;
}

/* Form styling */
.modal-form input {
  width: 100%;
  padding: 12px;
  margin-bottom: 14px;
  border: 1px solid #ccc;
  border-radius: 8px;
  font-size: 16px;
  transition: border 0.2s;
}

.modal-form input:focus {
  border-color: #42b983;
  outline: none;
  box-shadow: 0 0 3px rgba(66,185,131,0.5);
}

/* Button */
.modal-form button {
  width: 100%;
  padding: 12px;
  background-color: #42b983;
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: bold;
  cursor: pointer;
  transition: background 0.2s;
}
.modal-form button:hover {
  background-color: #369f6f;
}
.modal-form button:disabled {
  background-color: #9cd8bc;
  cursor: not-allowed;
}

/* Error text */
.error-text {
  color: #e63946;
  font-size: 14px;
  margin-top: 10px;
}

/* Close button */
.close-btn {
  margin-top: 14px;
  font-size: 14px;
  color: #555;
  background: none;
  border: none;
  cursor: pointer;
}
.close-btn:hover {
  color: #222;
}
</style>

