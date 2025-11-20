<script setup>
import { ref } from 'vue';
import { getAuth, signInWithEmailAndPassword } from 'firebase/auth';
import { useAuthStore } from '../stores/auth';
import { useToast } from "vue-toastification";

const toast = useToast();

const authStore = useAuthStore();       // Pinia store
const authFirebase = getAuth();         // Firebase Auth instance

const emit = defineEmits(['close', 'success', 'error']);

const email = ref('');
const password = ref('');
const isLoading = ref(false);
const localError = ref('');

const handleLogin = async () => {
  localError.value = '';
  if (!email.value || !password.value) {
    localError.value = 'Please fill in your Email and Password';
    return;
  }

  isLoading.value = true;

  try {
    // Đăng nhập Firebase
    const userCredential = await signInWithEmailAndPassword(authFirebase, email.value, password.value);
    const user = userCredential.user;

    // Lấy ID Token để gọi backend nếu cần
    const idToken = await user.getIdToken();

    // Cập nhật trạng thái Pinia store
    authStore.isLoggedIn = true;
    authStore.email = user.email;
    authStore.uid = user.uid;
    authStore.displayName = user.displayName || '';
    authStore.idToken = idToken;
    emit('success', {
      fullName: authStore.displayName || user.email.split('@')[0],
      email: user.email,
      uid: user.uid
    });

    toast.success(`Login successful! Welcome, ${authStore.displayName || user.email.split('@')[0]}!`);
    emit('close');

  } catch (error) {
    console.error('Login Error:', error);
    let message = 'Login failed. Please check your Email and Password again.';

    if (error.code && error.code.startsWith('auth/')) {
      if (error.code === 'auth/wrong-password' || error.code === 'auth/user-not-found') {
        message = 'Email or Password is incorrect';
      } else {
        message = `Firebase error: ${error.code}`;
      }
    } else if (error.response) {
      message = `Backend error: ${error.response.data?.Message || error.response.data?.Error || 'unidentified'}`;
    }

    localError.value = message;
    emit('error', message);
  } finally {
    isLoading.value = false;
  }
};
</script>

<template>
  <div class="modal-overlay">
    <div class="modal-content animate-modal">
      <h3 class="modal-title">Login</h3>

      <form @submit.prevent="handleLogin" class="modal-form">
        <input
          v-model="email"
          type="email"
          placeholder="Email"
          required
        />
        <input
          v-model="password"
          type="password"
          placeholder="Mật khẩu"
          required
        />

        <button
          type="submit"
          :disabled="isLoading"
          class="w-full bg-green-500 text-white font-bold p-3 rounded-lg hover:bg-green-600 transition duration-200 disabled:bg-green-300"
        >
          <span v-if="isLoading">Processing...</span>
          <span v-else>Login</span>
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

/* Khung modal */
.modal-content {
  width: 100%;
  max-width: 420px;
  background: white;
  padding: 32px;
  border-radius: 14px;
  box-shadow: 0 6px 20px rgba(0,0,0,0.15);
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

/* Form */
.modal-form input {
  width: 100%;
  padding: 12px;
  margin-bottom: 14px;
  border: 1px solid #ccc;
  border-radius: 8px;
  box-sizing: border-box;
  font-size: 16px;
  transition: border 0.2s;
}

/* Focus effect */
.modal-form input:focus {
  border-color: #42b983;
  outline: none;
  box-shadow: 0 0 3px rgba(66, 185, 131, 0.5);
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

/* Error */
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

