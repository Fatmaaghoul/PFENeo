<script>
import Header from '@/components/Header.vue'
import Footer from '@/components/Footer.vue'
import { useThemeStore } from '@/Store/theme'
import { onMounted } from 'vue'

export default {
  name: 'App',
  components: {
    Header,
    Footer
  },
  setup() {
    const themeStore = useThemeStore()

    onMounted(() => {
      themeStore.initTheme()
    })
  }
}
</script>

<template>
  <div class="app-wrapper">
    <Header />
    <main class="main-content">
      <router-view></router-view>
    </main>
    <!-- <Footer /> -->
  </div>
</template>

<style>
:root {
  --primary-color: #4CAF50;
  --background-color: #ffffff;
  --text-color: #333333;
  --secondary-background: #f5f5f5;
  --border-color: #e0e0e0;
}

.dark-theme {
  --primary-color: #66bb6a;
  --background-color: #121212;
  --text-color: #ffffff;
  --secondary-background: #1e1e1e;
  --border-color: #333333;
}

body {
  background-color: var(--background-color);
  color: var(--text-color);
  transition: background-color 0.3s, color 0.3s;
}

.app-wrapper {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background-color: var(--background-color);
}

.main-content {
  flex: 1;
}
</style>
