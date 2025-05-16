<template>
  <div class="auth-container">
    <div class="auth-card">
      <div class="auth-content">
        <!-- Partie formulaire -->
        <div class="auth-form-container">
          <h2>Réinitialiser le mot de passe</h2>
          <p class="subtitle">Veuillez définir un nouveau mot de passe pour votre compte.</p>

          <form @submit.prevent="resetPassword">
            <div class="form-group">
              <label>Nouveau mot de passe</label>
              <div class="password-input">
                <input 
                  v-model="newPassword" 
                  :type="showPassword ? 'text' : 'password'"
                  placeholder="Entrez votre nouveau mot de passe"
                  class="form-input"
                  required
                  minlength="8"
                />
                <button 
                  type="button" 
                  class="toggle-password"
                  @click="showPassword = !showPassword"
                  aria-label="Toggle password visibility"
                >
                  <i class="bi" :class="showPassword ? 'bi-eye-slash' : 'bi-eye'"></i>
                </button>
              </div>
              <p class="password-hint">Minimum 8 caractères</p>
            </div>

            <div class="form-group">
              <label>Confirmer le mot de passe</label>
              <div class="password-input">
                <input 
                  v-model="confirmPassword" 
                  :type="showConfirmPassword ? 'text' : 'password'"
                  placeholder="Confirmez votre nouveau mot de passe"
                  class="form-input"
                  required
                  minlength="8"
                />
                <button 
                  type="button" 
                  class="toggle-password"
                  @click="showConfirmPassword = !showConfirmPassword"
                  aria-label="Toggle password visibility"
                >
                  <i class="bi" :class="showConfirmPassword ? 'bi-eye-slash' : 'bi-eye'"></i>
                </button>
              </div>
            </div>

            <button type="submit" class="submit-btn" :disabled="loading || !formValid">
              {{ loading ? 'En cours...' : 'Réinitialiser le mot de passe' }}
            </button>
          </form>

          <p v-if="message" :class="['message', status]">{{ message }}</p>
          
          <div class="back-to-login">
            <router-link to="/login">Retour à la connexion</router-link>
          </div>
        </div>

        <!-- Partie illustration -->
        <div class="illustration">
          <img src="@/assets/reset-password-illustration.png" alt="Réinitialisation de mot de passe" />
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';

export default {
  setup() {
    const newPassword = ref('');
    const confirmPassword = ref('');
    const message = ref('');
    const status = ref('');
    const loading = ref(false);
    const showPassword = ref(false);
    const showConfirmPassword = ref(false);
    const route = useRoute();
    const router = useRouter();
    const token = ref('');
    const email = ref('');

    const formValid = computed(() => {
      return newPassword.value.length >= 8 && 
             confirmPassword.value.length >= 8 &&
             newPassword.value === confirmPassword.value;
    });

const isValidToken = (token) => {
  return token && token.length > 30 && token.includes('.');
};

   onMounted(async () => {
      token.value = cleanToken(route.query.token);
      email.value = route.query.email || '';
      
      if (!token.value || !email.value) {
        showError("Lien de réinitialisation incomplet");
        return;
      }

      await validateToken();
    });


    const validateToken = async () => {
      try {
        const response = await axios.get(
          'https://localhost:7036/api/auth/validate-reset-token',
          {
            params: {
              token: encodeURIComponent(token.value), // Nettoyage avant envoi
              email: email.value
            }
          }
        );
        
        if (!response.data?.valid) {
          showError(response.data?.message || "Lien expiré ou invalide");
          return false;
        }
        return true;
      } catch (error) {
        showError("Erreur de vérification du token");
        return false;
      }
    };

    const resetPassword = async () => {
      if (!(await validateToken())) return;

      if (newPassword.value.length < 8) {
        showError("Le mot de passe doit contenir au moins 8 caractères");
        return;
      }

      loading.value = true;
      
      try {
        const response = await axios.post(
          'https://localhost:7036/api/auth/reset-password',
          {
            email: email.value,
            token: encodeURIComponent(token.value),
            newPassword: newPassword.value
          },
          {
            headers: { 'Content-Type': 'application/json' }
          }
        );

        if (response.data.success) {
          showSuccess("Mot de passe réinitialisé avec succès!");
          setTimeout(() => router.push('/login'), 2000);
        } else {
          showError(response.data.message || "Échec de la réinitialisation");
        }
      } catch (error) {
        showError(error.response?.data?.message || "Erreur serveur");
      } finally {
        loading.value = false;
      }
    };

    const showError = (msg) => {
      message.value = msg;
      status.value = "error";
    };

    const showSuccess = (msg) => {
      message.value = msg;
      status.value = "success";
    };
    return { 
      newPassword,
      confirmPassword,
      message,
      status,
      loading,
      showPassword,
      showConfirmPassword,
      formValid,
      resetPassword
    };
  }
};
</script>

<style scoped>
.auth-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #f8fafc;
  padding: 2rem;
}

.auth-card {
  background: white;
  border-radius: 20px;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 1000px;
  overflow: hidden;
}

.auth-content {
  display: flex;
  align-items: stretch;
}

.auth-form-container {
  flex: 1;
  padding: 2.5rem;
  max-width: 450px;
}

h2 {
  font-size: 1.75rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.5rem;
}

.subtitle {
  color: #64748b;
  margin-bottom: 2rem;
  font-size: 0.95rem;
  line-height: 1.5;
}

.form-group {
  margin-bottom: 1.25rem;
}

label {
  display: block;
  margin-bottom: 0.5rem;
  color: #4b5563;
  font-size: 0.95rem;
  font-weight: 500;
}

.password-hint {
  font-size: 0.75rem;
  color: #64748b;
  margin-top: 0.25rem;
}

.form-input {
  width: 100%;
  padding: 0.875rem 1rem;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  font-size: 0.95rem;
  transition: all 0.2s;
}

.form-input:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

.password-input {
  position: relative;
}

.toggle-password {
  position: absolute;
  right: 1rem;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: #64748b;
  cursor: pointer;
  padding: 0;
  font-size: 1rem;
}

.submit-btn {
  width: 100%;
  padding: 0.875rem;
  background-color: #4f46e5;
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 500;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.2s;
  margin-top: 1rem;
}

.submit-btn:hover:not(:disabled) {
  background-color: #4338ca;
  transform: translateY(-1px);
}

.submit-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
  background-color: #c7d2fe;
}

.message {
  margin-top: 1rem;
  padding: 0.75rem;
  border-radius: 6px;
  font-size: 0.95rem;
  text-align: center;
}

.message.success {
  background-color: #f0fdf4;
  color: #166534;
  border: 1px solid #dcfce7;
}

.message.error {
  background-color: #fef2f2;
  color: #991b1b;
  border: 1px solid #fee2e2;
}

.back-to-login {
  margin-top: 1.5rem;
  text-align: center;
  font-size: 0.9rem;
}

.back-to-login a {
  color: #4f46e5;
  text-decoration: none;
  font-weight: 500;
}

.back-to-login a:hover {
  text-decoration: underline;
}

.illustration {
  flex: 1;
  background-color: #f9fafb;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  border-left: 1px solid #e2e8f0;
}

.illustration img {
  width: 100%;
  height: auto;
  max-width: 400px;
  object-fit: contain;
}

@media (max-width: 1024px) {
  .illustration {
    display: none;
  }

  .auth-form-container {
    max-width: none;
    margin: 0 auto;
  }

  .auth-card {
    max-width: 450px;
  }
}

@media (max-width: 640px) {
  .auth-container {
    padding: 1rem;
  }

  .auth-form-container {
    padding: 1.5rem;
  }

  h2 {
    font-size: 1.5rem;
  }
}
</style>