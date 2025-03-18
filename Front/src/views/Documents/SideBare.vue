<template>
  <div class="sidebar-container">
    <!-- Sidebar -->
    <div class="sidebar" :class="{ 'collapsed': isCollapsed }">
      <div class="sidebar-header">
       
        <button class="btn-toggle" @click="toggleSidebar">
          <i class="bi" :class="isCollapsed ? 'bi-chevron-right' : 'bi-chevron-left'"></i>
        </button>
      </div>

      <div class="sidebar-content">
        <ul class="nav-list">
          <li>
            <RouterLink to="/document/List"
              :class="{
                'active': isActive('/document/List'),
                'collapsed': isCollapsed
              }"
              class="nav-item"
              @click="setActive('/document/List')">
              <i class="bi bi-list-ul"></i>
              <span>List</span>
            </RouterLink>
          </li>

          <li>
            <RouterLink to="/document/Upload"
              :class="{
                'active': isActive('/document/Upload'),
                'collapsed': isCollapsed
              }"
              class="nav-item"
              @click="setActive('/document/Upload')">
              <i class="bi bi-upload"></i>
              <span>Generate Description</span>
            </RouterLink>
          </li>

          <li>
            <RouterLink to="/document/Add"
              :class="{
                'active': isActive('/document/Add'),
                'collapsed': isCollapsed
              }"
              class="nav-item"
              @click="setActive('/document/Add')">
              <i class="bi bi-plus-lg"></i>
              <span>Add</span>
            </RouterLink>
          </li>
        </ul>
      </div>
    </div>

    <!-- Main Content -->
    <div class="main-content" :class="{ 'expanded': isCollapsed }">
      <div class="content-wrapper">
        <router-view v-slot="{ Component }">
          <component :is="Component" />
        </router-view>
      </div>
    </div>
  </div>
</template>

<script>
import { mapState } from 'vuex';

export default {
  name: 'SidebarLayout',
  data() {
    return {
      isCollapsed: false,
    }
  },
  computed: {
    ...mapState({
      authenticated: (state) => state.authenticated,
      user: (state) => state.user,
    }),
    currentRoute() {
      return this.$route.path;
    }
  },
  methods: {
    setActive(route) {
      // La route sera automatiquement mise à jour par le router
    },
    isActive(route) {
      return this.currentRoute.toLowerCase() === route.toLowerCase();
    },
    toggleSidebar() {
      this.isCollapsed = !this.isCollapsed;
    },
    checkScreenSize() {
      if (window.innerWidth < 768) {
        this.isCollapsed = true;
      }
    }
  },
  watch: {
    $route: {
      immediate: true,
      handler(to) {
        // Met à jour l'état actif quand la route change
        this.$nextTick(() => {
          const links = document.querySelectorAll('.nav-item');
          links.forEach(link => {
            if (link.getAttribute('href') === to.path) {
              link.classList.add('active');
            } else {
              link.classList.remove('active');
            }
          });
        });
      }
    }
  },
  mounted() {
    this.checkScreenSize();
    window.addEventListener('resize', this.checkScreenSize);
  },
  beforeUnmount() {
    window.removeEventListener('resize', this.checkScreenSize);
  }
}
</script>

<style scoped>
.sidebar-container {
  display: flex;
  min-height: calc(100vh - 80px);
  
}

.sidebar {
  width: 250px;
  background: white;
  box-shadow: 0 0 15px rgba(0, 0, 0, 0.05);
  transition: all 0.3s ease;
  position: fixed;
  height: calc(100vh - 80px);
  z-index: 900;
}

.sidebar.collapsed {
  width: 70px;
}

.sidebar-header {
  padding: 1.5rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid #e0e0e0;
}

.sidebar-header h5 {
  color: #2c3e50;
  font-weight: 600;
  margin: 0;
}

.btn-toggle {
  background: none;
  border: none;
  color: #6c757d;
  cursor: pointer;
  padding: 0.5rem;
  border-radius: 50%;
  transition: all 0.3s ease;
}

.btn-toggle:hover {
  background: rgba(0, 0, 0, 0.05);
  color: #2c3e50;
}

.sidebar-content {
  padding: 0;
}

.nav-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.nav-item {
  display: flex;
  align-items: center;
  padding: 0.75rem 1.5rem;
  color: #6c757d;
  text-decoration: none;
  transition: all 0.3s ease;
  gap: 1rem;
  border-radius: 8px;
  margin: 0.25rem 1rem;
}

.nav-item i {
  font-size: 1.2rem;
  min-width: 24px;
  text-align: center;
}

.nav-item:hover {
  background: rgba(13, 110, 253, 0.1);
  color: #0d6efd;
  transform: translateX(5px);
}

.nav-item.active {
  background: #0d6efd;
  color: white;
}

.nav-item.collapsed {
  padding: 0.75rem;
  justify-content: center;
}

.nav-item.collapsed span {
  display: none;
}

.main-content {
  flex: 1;
  margin-left: 200px;
  padding: 2rem;
  transition: all 0.3s ease;
  background: #f8f9fa;
}

.main-content.expanded {
  margin-left: 70px;
}

.content-wrapper {
  background: white;
  border-radius: 8px;
  box-shadow: 0 0 15px rgba(0, 0, 0, 0.05);
  padding: 2rem;
  min-height: calc(100vh - 120px);
}

@media (max-width: 768px) {
  .sidebar {
    transform: translateX(-100%);
  }

  .sidebar.collapsed {
    transform: translateX(0);
  }

  .main-content {
    margin-left: 0;
  }

  .main-content.expanded {
    margin-left: 0;
  }
}

@media (max-width: 576px) {
  .main-content {
    padding: 1rem;
  }
  
  .content-wrapper {
    padding: 1rem;
  }
}
</style>

