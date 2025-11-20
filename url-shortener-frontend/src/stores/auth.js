// src/stores/auth.js
import { defineStore } from 'pinia';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    isLoggedIn: false,
    user: null,
    idToken: null, // lưu token để dùng cho API
  }),
  actions: {
    login(user, token) {
      this.isLoggedIn = true;
      this.user = user;
      this.idToken = token; // lưu token
    },
    logout() {
      this.isLoggedIn = false;
      this.user = null;
      this.idToken = null;
    }
  }
});
