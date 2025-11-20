<script setup>
import { ref, onMounted } from 'vue';
import { useAuthStore } from './stores/auth';
import { onAuthStateChanged, signOut, getAuth } from 'firebase/auth'; // Imports Firebase
import ShortenForm from './components/ShortenForm.vue';
import UrlLookupForm from './components/UrlLookupForm.vue';
import UrlList from './components/UrlList.vue';
import LoginModal from './components/LoginModal.vue';       
import RegisterModal from './components/RegisterModal.vue'; 
import { useToast } from "vue-toastification";

const toast = useToast();

// Lấy Auth service từ main.js
const authStore = useAuthStore();
const auth = getAuth();

// ------------------------------------------------------------------
// QUẢN LÝ TRẠNG THÁI XÁC THỰC
// ------------------------------------------------------------------
const isLoggedIn = ref(false);
const userDisplayName = ref('Guest');
const userEmail = ref(null);
const showLoginModal = ref(false);
const showRegisterModal = ref(false);

// Trạng thái URL
const activeTab = ref('shorten'); 
const urlList = ref([]); 

// Hàm thêm URL mới vào danh sách (Sử dụng chung cho Shorten và Lookup)
const addToList = (newUrl) => {
  urlList.value.unshift({ 
    ...newUrl, 
    createdAt: new Date().toLocaleString('vi-VN') 
  });
};

// Hàm Logout
const handleLogout = async () => {
    try {
        await signOut(auth); 
        isLoggedIn.value = false;
        userDisplayName.value = 'Guest';
        userEmail.value = null;
        toast.success('Logged out successfully!');
    } catch (error) {
        console.error('Error when logging out:', error);
        toast.error('Logout failed! Please try again.');
    }
}

// Cập nhật trạng thái người dùng khi có thay đổi (Login/Logout)
const updateAuthState = (user) => {
    if (user) {
        // 1. Lấy phần đầu của email (hoặc dùng 'User' nếu email không có)
        const emailPart = user.email ? user.email.split('@')[0] : 'User';
        
        // 2. Ưu tiên DisplayName, nếu không có thì dùng phần đầu email
        const name = user.displayName || emailPart;

        isLoggedIn.value = true;
        userDisplayName.value = name;
        userEmail.value = user.email;
    } else {
        isLoggedIn.value = false;
        userDisplayName.value = '';
        userEmail.value = null;
    }
};

// Lắng nghe trạng thái Firebase ngay khi component được mount
onMounted(() => {
    onAuthStateChanged(auth, async (user) => {
      if (user) {
        const token = await user.getIdToken(); // JWT
        authStore.login(user, token);

        isLoggedIn.value = true;
        updateAuthState(user);
      } else {
        authStore.logout();
        isLoggedIn.value = false;
        updateAuthState(null);
      }
    });
});


</script>
// ------------------------------------------------------------------

<template>
  <div id="app" class="page-wrapper">
    <!-- Header -->
    <header class="navbar">
      
      <!-- Navbar bên phải -->
      <div class="navbar-left">
        <!-- Hiển thị Chào mừng + Tên User nếu đã đăng nhập -->
        <strong v-if="isLoggedIn"> Hi, {{ userDisplayName }} 👋</strong>
        <span v-else>Welcome to URL Shortener ✨</span>
      </div>

      <nav class="navbar-right">
        <button class="nav-btn">About</button>
        <button class="nav-btn" @click="showLoginModal = true">Login</button>
        <button class="nav-btn" @click="showRegisterModal = true">Register</button>
        <button class="nav-btn" @click="handleLogout">Logout</button>
<!--
        <template v-if="!isLoggedIn">
          <button class="nav-btn" @click="showLoginModal = true">Login</button>
          <button class="nav-btn" @click="showRegisterModal = true">Register</button>
        </template>

        <template v-else>
          <button class="nav-btn" @click="handleLogout">Logout</button>
        </template>
-->
      </nav>
    </header>

    <!-- Main Content -->
    <main class="main-center">
      <!-- Shortener/Lookup Box -->
      <div class="form-card">
        <div class="tabs">
          <!-- Tab Dài -> Ngắn -->
          <button 
            @click="activeTab = 'shorten'"
            :class="['tab', 
                     activeTab === 'shorten' ? 'active-tab' : '']"
          >
            Long → Short
          </button>
          
          <!-- Tab Ngắn -> Dài -->
          <button 
            @click="activeTab = 'lookup'"
            :class="['tab', 
                     activeTab === 'lookup' ? 'active-tab' : '']"
          >
            Short → Long
          </button>
        </div>

        <!--<h2 class="title">
            {{ activeTab === 'shorten' ? 'Rút gọn URL' : 'Tra cứu URL Gốc' }}
        </h2>-->
        <h2 class="title">Transform Your Links</h2>

        <!-- Hiển thị ShortenForm (Truyền trạng thái đăng nhập xuống) -->
        <ShortenForm 
          v-if="activeTab === 'shorten'"
          @url-shortened="addToList"
          :isLoggedIn="isLoggedIn"
        />

        <!-- Hiển thị UrlLookupForm -->
        <UrlLookupForm 
          v-if="activeTab === 'lookup'"
          @url-found="addToList"
        />
      </div>
      
      <h2 class="history-title">Your Link History</h2>

      <div class="history-wrapper">
        <UrlList 
        :urls="urlList" 
        :isLoggedIn="isLoggedIn"
        @delete-item="urlList = urlList.filter(u => u.id !== $event)"
      />
      </div>
    </main>

    <!-- Footer -->
    <footer class="footer">
      <© 2025 URL Shortener — All Rights Reserved
    </footer>

    <!-- MODALS -->
    <LoginModal 
        v-if="showLoginModal" 
        @close="showLoginModal = false"
        @success="updateAuthState"
    />

    <RegisterModal 
        v-if="showRegisterModal" 
        @close="showRegisterModal = false" 
        @success="showRegisterModal = false; showLoginModal = true"
    />
  </div>
</template>

<style scoped>
/* PAGE LAYOUT */
.page-wrapper {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: #f8fafc;
}

/* NAVBAR */
.navbar {
  background: linear-gradient(90deg, #42b983, #2f855a);
  color: white;
  padding: 16px 32px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
  border-bottom-left-radius: 12px;
  border-bottom-right-radius: 12px;
}

.nav-btn {
  background: rgba(255,255,255,0.15);
  color: white;
  font-size: 16px;       /* chữ to hơn */
  font-weight: 600;      /* chữ đậm hơn */
  padding: 10px 18px;    /* to hơn */
  border-radius: 8px;
  border: none;
  cursor: pointer;
  transition: all 0.3s ease;
  margin-left: 8px;
  box-shadow: 0 2px 6px rgba(0,0,0,0.1);
}

.nav-btn {
  background: transparent;
  border: none;
  color: white;
  font-size: 15px;
  cursor: pointer;
  padding: 6px 12px;
  border-radius: 6px;
}

.nav-btn:hover {
  background: rgba(255,255,255,0.35);
  transform: translateY(-2px);
  box-shadow: 0 4px 10px rgba(0,0,0,0.2);
}

.navbar-left {
  font-family: 'Inter', 'Roboto', sans-serif;
  font-size: 18px;
  font-weight: 600;        /* semi-bold */
  color: #ffffff;
  letter-spacing: 0.5px;   /* khoảng cách chữ tinh tế */
  text-shadow: 0 1px 2px rgba(0,0,0,0.2); /* nhẹ, không lòe loẹt */
  cursor: default;
  transition: color 0.2s ease;
}

.navbar-left:hover {
  color: #e0f7ef;  /* hover nhẹ thay đổi màu */
}


/* MAIN CONTENT CENTER ALIGN */
.main-center {
  width: 100%;
  max-width: 700px;
  margin: 0 auto;
  padding: 24px;
}

/* FORM CARD */
.form-card {
  background: white;
  border: 2px solid #d4f5e3;
  border-radius: 14px;
  padding: 30px;
  box-shadow: 0 4px 16px rgba(0,0,0,0.1);
  margin-top: 40px;
}

/* TABS */
/* TABS NÂNG CẤP CHUYÊN NGHIỆP */
.tabs {
  display: flex;
  justify-content: center;
  gap: 16px;
  margin-bottom: 20px;
}

.tab {
  padding: 12px 24px;
  font-size: 16px;
  font-weight: 600;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.3s ease;
  background: linear-gradient(135deg, #e0e0e0, #f5f5f5);
  color: #374151;
  box-shadow: 0 2px 6px rgba(0,0,0,0.1);
}

.tab:hover {
  background: linear-gradient(135deg, #42b983, #2f855a);
  color: white;
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(0,0,0,0.25);
}

.active-tab {
  background: linear-gradient(135deg, #42b983, #2f855a);
  color: white;
  box-shadow: 0 6px 16px rgba(0,0,0,0.25);
  transform: translateY(-1px);
}

/* TITLE */
.title {
  text-align: center;
  font-size: 22px;
  margin-bottom: 20px;
  color: #374151;
}

/* HISTORY */
.history-title {
  text-align: center;
  font-size: 20px;
  margin: 40px 0 20px;
  color: #374151;
}

.history-wrapper {
  max-width: 700px;
  margin: 0 auto;
}

/* FOOTER */
.footer {
  background: #42b983;
  color: white;
  text-align: center;
  padding: 14px;
  margin-top: auto;
}
</style>
