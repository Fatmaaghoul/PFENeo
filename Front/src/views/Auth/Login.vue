<template>  
  <div class="auth-container">
    <div class="auth-card">
      <div class="auth-content">
        <!-- Partie formulaire -->
        <div class="auth-form-container">
          <div class="logo-section">
            <img src="@/assets/logo.png" alt="Your Logo" class="logo" />
          </div>
          <h2>Login</h2>
          <p class="subtitle">Sign in to access your account</p>

          <!-- Messages d'erreur et de succès -->
          <div v-if="errorMessage" class="error-message">{{ errorMessage }}</div>
          <div v-if="successMessage" class="success-message">{{ successMessage }}</div>

          <form @submit.prevent="submit">
            <div class="form-group">
              <input 
                v-model="data.email" 
                type="email" 
                placeholder="Email"
                class="form-input"
                required 
              />
            </div>
            <div class="form-group">
              <div class="password-input">
                <input 
                  v-model="data.password" 
                  :type="showPassword ? 'text' : 'password'"
                  placeholder="Password"
                  class="form-input"
                  required 
                />
                <button 
                  type="button" 
                  class="toggle-password"
                  @click="togglePasswordVisibility"
                >
                  <i class="bi" :class="showPassword ? 'bi-eye-slash' : 'bi-eye'"></i>
                </button>
              </div>
            </div>

            <div class="form-options">
              <label class="remember-me">
                <input type="checkbox" v-model="rememberMe">
                <span>Remember me</span>
              </label>
              <router-link to="/forgot-password" class="forgot-link">Forgot Password?</router-link>
            </div>

            <button type="submit" class="submit-btn">Login</button>
          </form>

          <p class="signup-text">
            Don't have an account? 
         
            <router-link to="/register" class="signup-link">Sign up</router-link>
          </p>
         
        </div>

        <!-- Partie illustration -->
        <div class="illustration">
          <img src="@/assets/login-illustration.png" alt="Login" />
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useStore } from 'vuex';
import { jwtDecode } from 'jwt-decode'; // Importation corrigée
import Cookies from 'js-cookie'; // Import de js-cookie

export default {
  name: "Login",
  setup() {
    const data = reactive({ email: "", password: "" });
    const store = useStore();
    const router = useRouter();
    const errorMessage = ref("");
    const successMessage = ref("");
    const rememberMe = ref(false);
    const showPassword = ref(false);

    const togglePasswordVisibility = () => {
      showPassword.value = !showPassword.value;
    };

    const submit = async () => {
      try {
        const response = await axios.post("/api/auth/login", data, {
          headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
          }
        });

        console.log("Full API response:", response);

        // Vérifiez si le token est présent dans la réponse
        const token = response.data.data?.token || response.data.token;
        if (token) {
          // Stocker le token dans un cookie
          Cookies.set('token', token, { expires: rememberMe.value ? 7 : null }); // Expire dans 7 jours si "Remember me" est coché

          // Décoder le token JWT pour obtenir les informations utilisateur
          const userData = jwtDecode(token); // Utilisation de jwtDecode
          console.log("Decoded user data:", userData);

          // Stocker les données utilisateur et l'état d'authentification
          store.commit("SET_AUTH", { 
            authenticated: true, 
            user: {
              email: userData.email,
              role: userData['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'User'
            }
          });

          // Rediriger en fonction du rôle de l'utilisateur
          if (userData['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] === 'Admin') {
            router.push("/admin/dashboard");
          } else {
            router.push("/document");
          }

          successMessage.value = "Connexion réussie !";
          setTimeout(() => successMessage.value = "", 3000);
        } else {
          throw new Error("No token received from server");
        }
      } catch (error) {
        console.error("Login error details:", error);
        errorMessage.value = error.response?.data?.message || "Erreur de connexion. Veuillez réessayer.";
        setTimeout(() => errorMessage.value = "", 5000);
      }
    };

    const loginWithGoogle = () => {
      window.location.href = "/api/auth/google";
    };

    return { data, submit, rememberMe, showPassword, togglePasswordVisibility, errorMessage, successMessage, loginWithGoogle };
  },
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

.logo-section {
  text-align: center;
  margin-bottom: 1.5rem;
}

.logo {
  height: 40px;
  width: auto;
}

h2 {
  font-size: 1.75rem;
  font-weight: 600;
  color: #1e293b;
  text-align: center;
  margin-bottom: 0.5rem;
}

.subtitle {
  color: #64748b;
  text-align: center;
  margin-bottom: 2rem;
  font-size: 0.95rem;
}

.form-group {
  margin-bottom: 1.25rem;
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
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.remember-me {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #64748b;
  font-size: 0.9rem;
}

.forgot-link {
  color: #4f46e5;
  text-decoration: none;
  font-size: 0.9rem;
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
}

.submit-btn:hover {
  background-color: #4338ca;
  transform: translateY(-1px);
}


.social-divider {
  text-align: center;
  margin: 1.5rem 0;
  position: relative;
  color: #64748b;
  font-size: 0.9rem;
}

.social-divider::before,
.social-divider::after {
  content: '';
  position: absolute;
  top: 50%;
  width: 45%;
  height: 1px;
  background-color: #e2e8f0;
}

.social-divider::before { left: 0; }
.social-divider::after { right: 0; }

.social-buttons {
  display: flex;
  justify-content: center;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.social-btn {
  width: 42px;
  height: 42px;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
  background: white;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.social-btn:hover {
  background-color: #f8fafc;
  transform: translateY(-1px);
}

.signup-text {
  text-align: center;
  color: #64748b;
  font-size: 0.95rem;
  margin: 0;
}

.signup-link {
  color: #4f46e5;
  text-decoration: none;
  font-weight: 500;
}

.illustration {
  flex: 1;
  background-color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
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
}
</style>