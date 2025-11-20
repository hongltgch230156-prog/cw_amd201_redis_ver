    // main.js
    import { createApp } from 'vue'
    import { createPinia } from 'pinia';
    import App from './App.vue'
    import axios from 'axios' // Cài đặt: npm install axios

    import Toast from "vue-toastification";
    import "vue-toastification/dist/index.css";

    // Imports Firebase Client SDK
    import { initializeApp } from 'firebase/app';
    import { getAuth } from 'firebase/auth';

    const firebaseConfig = {
        apiKey: "AIzaSyARxndQyv5TIZ8xxxxes9cqBOOqoqFn4Hw",
        authDomain: "projectamd-c2a48.firebaseapp.com",
        projectId: "projectamd-c2a48",
        storageBucket: "projectamd-c2a48.firebasestorage.app",
        messagingSenderId: "221158769757",
        appId: "1:221158769757:web:db3149d7ecc53a3e5579b4",
        measurementId: "G-CNQMP401LR"
    };
    // Khởi tạo Firebase App và Auth
    const firebaseApp = initializeApp(firebaseConfig);
    const auth = getAuth(firebaseApp);

    // Tạo Axios instance riêng cho Identity Service
    const identityApi = axios.create({
        baseURL: import.meta.env.VITE_IDENTITY_URL // Endpoint của AuthController
    });
    // Cấu hình Axios base URL
    //axios.defaults.baseURL = 'https://localhost:3333/api'; 
    // Lưu ý: Cần xử lý lỗi CORS nếu gặp (từ phía backend ASP.NET Core)

    const app = createApp(App);

    app.provide('auth', auth);
    app.provide('identityApi', identityApi);

    const pinia = createPinia();
    app.use(pinia);

    app.use(Toast, {
        position: "top-right",
        timeout: 3000,
        closeOnClick: true,
        pauseOnHover: true,
        hideProgressBar: false,
        draggable: true,
        draggablePercent: 0.6,
    });


    app.mount('#app');