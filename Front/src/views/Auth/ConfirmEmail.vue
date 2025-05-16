<template>
  <div class="container">
    <div class="box">
      <h2>{{ title }}</h2>
      <div v-if="isLoading" class="loading">
        <p>Vérification en cours...</p>
        <div class="spinner"></div>
      </div>
      <template v-else>
        <p>{{ message }}</p>
        <router-link v-if="success" to="/login" class="btn">Aller à la page de connexion</router-link>
        <button v-else @click="retryConfirmation" class="btn">Renvoyer l'email de confirmation</button>
      </template>
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
    // Décodage plus robuste du token
    const token = decodeURIComponent(rawToken.replace(/ /g, '+'));
    const userId = route.query.userId || '';

    const message = ref("Confirmation en cours...");
    const title = ref("🔄 Confirmation...");
    const success = ref(false);
    const isLoading = ref(true);

    onMounted(async () => {
      try {
        if (!token || !userId) {
          throw new Error("Lien de confirmation incomplet");
        }

        const response = await axios.get('https://localhost:7036/api/auth/confirm-email', {
          params: { 
            userId: userId,
            token: token 
          },
          timeout: 10000 // 10 secondes timeout
        });

        if (response.data.success) {
          message.value = response.data.message;
          title.value = "✅ Succès";
          success.value = true;
        } else {
          throw new Error(response.data.message || "Échec de la confirmation");
        }
      } catch (error) {
        console.error("Erreur de confirmation:", error);
        
        if (error.code === "ERR_NETWORK" || error.message.includes("timeout")) {
          message.value = "Impossible de se connecter au serveur. Veuillez réessayer plus tard.";
        } else if (error.message.includes("Lien de confirmation incomplet")) {
          message.value = "Le lien de confirmation est incomplet. Veuillez utiliser le lien exact reçu par email.";
        } else {
          message.value = error.response?.data?.message || 
                         error.message || 
                         "Une erreur est survenue lors de la confirmation";
        }
        
        title.value = "❌ Erreur";
      } finally {
        isLoading.value = false;
      }
    });

    return { message, title, success, isLoading };
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
.loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #007bff;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}
</style>