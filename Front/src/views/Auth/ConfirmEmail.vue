<template>
  <div class="container">
    <div class="box">
      <h2>{{ title }}</h2>
      <p>{{ message }}</p>
      <router-link v-if="success" to="/login" class="btn">Aller à la page de connexion</router-link>
    </div>
  </div>
</template>

<script>
import { useRoute } from 'vue-router';
import { ref, onMounted } from 'vue';
import axios from 'axios';

export default {
  setup() {
    const route = useRoute();
    const rawToken = route.query.token || '';
    const token = decodeURIComponent(rawToken).replace(/ /g, '+');
    const email = route.query.email || '';

    const message = ref("Confirmation en cours...");
    const title = ref("🔄 Confirmation...");
    const success = ref(false);

    onMounted(async () => {
      console.log("Token:", token, "Email:", email); // Debug

      if (!token || !email) {
        message.value = "Lien invalide.";
        title.value = "❌ Erreur";
        return;
      }

      try {
        const response = await axios.get('https://localhost:7036/api/auth/confirm-email', {
          params: { token, email }
        });

        message.value = response.data.message;
        title.value = "✅ Succès";
        success.value = true;
      } catch (error) {
        console.error("Erreur Axios:", error); // Debug
        if (error.code === "ERR_NETWORK") {
          message.value = "Unable to connect to the server. Please try again later.";
        } else {
          message.value = error.response?.data?.message || error.response?.data || "Erreur lors de la confirmation.";
        }
        title.value = "❌ Erreur";
        success.value = false;
      }
    });

    return { message, title, success };
  }
};
</script>

<style scoped>
.container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  background-color: #f8f9fa;
}
.box {
  background: white;
  padding: 2rem;
  border-radius: 10px;
  text-align: center;
  box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
}
.btn {
  margin-top: 1rem;
  padding: 0.8rem 1.5rem;
  background-color: #007bff;
  color: white;
  border-radius: 5px;
  text-decoration: none;
}
.btn:hover {
  background-color: #0056b3;
}
</style>