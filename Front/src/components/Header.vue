<template>
  <div v-if="!isAdminRoute" style="margin-bottom: 80px;">
    <nav class="navbar navbar-expand-lg navbar-light bg-white shadow-sm fixed-top w-100">
      <div class="container">
        <RouterLink to="/" class="navbar-brand d-flex align-items-center">
          <i class="bi bi-file-earmark-text-fill text-primary me-2 fs-4"></i>
          <span class="fw-bold text-dark fs-4">DocVision</span>
        </RouterLink>

        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav" aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
          <span class="navbar-toggler-icon"></span>
        </button>

        <div class="collapse navbar-collapse" id="navbarNav">
          <ul class="navbar-nav me-auto mb-2 mb-lg-0">
            <li class="nav-item">
              <RouterLink to="/" class="nav-link">
                <i class="bi bi-house-door me-1"></i> Home
              </RouterLink>
            </li>
            <li class="nav-item">
              <RouterLink to="/about" class="nav-link">
                <i class="bi bi-info-circle me-1"></i> About
              </RouterLink>
            </li>
            <li v-if="authenticated" class="nav-item">
              <RouterLink to="/document" class="nav-link">
                <i class="bi bi-files me-1"></i> Documents
              </RouterLink>
            </li>
            <li v-if="authenticated && user?.role === 'Admin'" class="nav-item">
              <RouterLink to="/admin/dashboard" class="nav-link text-danger fw-bold">
                <i class="bi bi-speedometer2 me-1"></i>
                Admin Dashboard
              </RouterLink>
            </li>
          </ul>

          <div class="d-flex align-items-center">
            <div v-if="authenticated" class="user-menu">
              <div class="user-avatar" @click="toggleUserMenu">
                {{ user?.email?.[0]?.toUpperCase() || 'U' }}
              </div>
              <div v-if="isUserMenuOpen" class="user-dropdown">
                <div class="user-info">
                  <div class="user-email">{{ user?.email }}</div>
                </div>
                <div class="dropdown-divider"></div>
                <RouterLink to="/profile" class="dropdown-item">
                  <i class="bi bi-person-circle me-2"></i> Profile
                </RouterLink>
                <button class="dropdown-item" @click="logout">
                  <i class="bi bi-box-arrow-right me-2"></i> Logout
                </button>
              </div>
            </div>
            <div v-else>
              <RouterLink to="/login" class="btn btn-primary">
                <i class="bi bi-box-arrow-in-right me-2"></i> Login
              </RouterLink>
            </div>
          </div>
        </div>
      </div>
    </nav>
    <div ></div>
  </div>
</template>

<script setup>
import { computed, ref, onMounted, onUnmounted } from 'vue';
import { useStore } from 'vuex';
import { useRouter, useRoute } from 'vue-router';

const store = useStore();
const router = useRouter();
const route = useRoute();
const isUserMenuOpen = ref(false);

// Récupération des données du store Vuex
const authenticated = computed(() => store.state.authenticated);
const user = computed(() => store.state.user);

// Vérifier si on est sur une route admin
const isAdminRoute = computed(() => {
  return route.path.startsWith('/admin');
});

// Fonction de déconnexion
const logout = async () => {
  await store.dispatch('logout');
  router.push('/login');
  isUserMenuOpen.value = false;
};

// Toggle le menu utilisateur
const toggleUserMenu = () => {
  isUserMenuOpen.value = !isUserMenuOpen.value;
};

// Fermer le menu si on clique en dehors
const handleClickOutside = (event) => {
  if (isUserMenuOpen.value && !event.target.closest('.user-menu')) {
    isUserMenuOpen.value = false;
  }
};

onMounted(() => {
  document.addEventListener('click', handleClickOutside);
});

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside);
});
</script>

<style scoped>
.navbar {
  z-index: 1000;
  padding: 1rem 0;
  transition: all 0.3s ease;
}

.navbar-brand {
  font-size: 1.5rem;
  transition: all 0.3s ease;
}

.navbar-brand:hover {
  transform: translateY(-2px);
}

.nav-link {
  font-size: 1.1rem;
  padding: 0.5rem 1rem !important;
  transition: all 0.3s ease;
  border-radius: 8px;
}

.nav-link:hover {
  background-color: rgba(13, 110, 253, 0.1);
  transform: translateY(-2px);
}

.nav-link.router-link-active {
  color: #0d6efd !important;
  font-weight: 500;
}

.btn {
  padding: 0.5rem 1.25rem;
  font-size: 1rem;
  transition: all 0.3s ease;
  border-radius: 8px;
}

.btn:hover {
  transform: translateY(-2px);
}

.btn-outline-primary {
  border-width: 2px;
}

.btn-outline-danger {
  border-width: 2px;
}

.user-menu {
  position: relative;
}

.user-avatar {
  width: 40px;
  height: 40px;
  background-color: #0d6efd;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.user-avatar:hover {
  transform: scale(1.1);
  background-color: #0b5ed7;
}

.user-dropdown {
  position: absolute;
  top: 100%;
  right: 0;
  background: white;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  min-width: 200px;
  margin-top: 0.5rem;
  z-index: 1001;
  animation: slideDown 0.2s ease;
}

.user-info {
  padding: 1rem;
  border-bottom: 1px solid #e9ecef;
}

.user-email {
  font-size: 0.9rem;
  color: #6c757d;
  word-break: break-all;
}

.dropdown-divider {
  height: 1px;
  background-color: #e9ecef;
  margin: 0.5rem 0;
}

.dropdown-item {
  display: flex;
  align-items: center;
  padding: 0.75rem 1rem;
  color: #2c3e50;
  text-decoration: none;
  transition: all 0.3s ease;
  cursor: pointer;
  border: none;
  background: none;
  width: 100%;
  text-align: left;
}

.dropdown-item:hover {
  background-color: #f8f9fa;
  color: #0d6efd;
}

.dropdown-item i {
  font-size: 1.1rem;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (max-width: 991.98px) {
  .navbar-collapse {
    background: white;
    padding: 1rem;
    border-radius: 8px;
    margin-top: 1rem;
    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  }

  .user-dropdown {
    position: static;
    box-shadow: none;
    margin-top: 0.5rem;
  }
}
</style>
