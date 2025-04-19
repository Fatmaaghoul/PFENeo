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
    const userId = route.query.userId || route.query.email || '';

    const message = ref("Confirmation en cours...");
    const title = ref("🔄 Confirmation...");
    const success = ref(false);

    onMounted(async () => {
      console.log("Token:", token, "UserId/Email:", userId); // Debug

      // Vérifie si nous sommes déjà sur la page de réponse API (avec une réponse JSON)
      if (document.contentType === 'application/json') {
        try {
          // Essayons de parser le JSON de la page
          const jsonContent = JSON.parse(document.body.textContent);
          message.value = jsonContent.message || "E-mail confirmé avec succès !";
          title.value = jsonContent.success ? "✅ Succès" : "❌ Erreur";
          success.value = jsonContent.success || false;
          return;
        } catch (e) {
          console.error("Erreur lors du parsing JSON:", e);
        }
      }

      // Si nous ne sommes pas sur une page de réponse JSON ou si le parsing a échoué
      if (!token || !userId) {
        message.value = "Lien invalide.";
        title.value = "❌ Erreur";
        return;
      }

      try {
        // Détermine quel paramètre utiliser (userId ou email) en fonction de ce qui est disponible
        const params = {};
        if (route.query.userId) {
          params.userId = userId;
        } else {
          params.email = userId;
        }
        params.token = token;

        const response = await axios.get('https://localhost:7036/api/auth/confirm-email', {
          params: params
        });

        message.value = response.data.message;
        title.value = "✅ Succès";
        success.value = true;
      } catch (error) {
        console.error("Erreur Axios:", error); // Debug
        if (error.code === "ERR_NETWORK") {
          message.value = "Impossible de se connecter au serveur. Veuillez réessayer plus tard.";
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