<template>
  <div class="container mt-5">
    <div class="row justify-content-center">
      <div class="col-md-8 col-lg-6">
        <div class="card shadow-lg border-0">
          <div class="card-header bg-gradient-primary text-white text-center py-4">
            <div class="avatar mx-auto mb-3">
              <img 
                v-if="user.avatarUrl" 
                :src="user.avatarUrl" 
                alt="Avatar" 
                class="avatar-img"
                @error="handleAvatarError"
              />
              <i v-else class="bi bi-person-circle avatar-icon"></i>
            </div>
            <h2 class="card-title mb-0">
              <i class="bi bi-person-circle me-2"></i>Mon Profil
            </h2>
          </div>
          <div class="card-body p-4">
            <!-- Mode Consultation -->
            <div v-if="!isEditing" class="profile-info" key="view-mode">
              <div class="mb-4">
                <label class="form-label fw-bold">
                  <i class="bi bi-person me-2"></i>Nom d'utilisateur
                </label>
                <p class="form-text">{{ user.userName || 'Non défini' }}</p>
              </div>
              <div class="mb-4">
                <label class="form-label fw-bold">
                  <i class="bi bi-envelope me-2"></i>Email
                </label>
                <p class="form-text">{{ user.email || 'Non défini' }}</p>
              </div>
              <div class="mb-4">
                <label class="form-label fw-bold">
                  <i class="bi bi-telephone me-2"></i>Numéro de téléphone
                </label>
                <p class="form-text">{{ user.phoneNumber || 'Non défini' }}</p>
              </div>
              <div class="text-center mt-4">
                <button class="btn btn-primary btn-gradient" @click="startEditing">
                  <i class="bi bi-pencil me-2"></i>Modifier
                </button>
              </div>
            </div>

            <!-- Mode Édition -->
            <div v-else class="profile-info" key="edit-mode">
              <form @submit.prevent="submitForm">
                <div class="mb-4">
                  <label class="form-label fw-bold">
                    <i class="bi bi-person me-2"></i>Nom d'utilisateur
                  </label>
                  <input 
                    v-model="editUser.userName" 
                    type="text" 
                    class="form-control" 
                    required 
                    placeholder="Entrez votre nom d'utilisateur"
                  />
                </div>
                <div class="mb-4">
                  <label class="form-label fw-bold">
                    <i class="bi bi-envelope me-2"></i>Email
                  </label>
                  <input 
                    v-model="editUser.email" 
                    type="email" 
                    class="form-control" 
                    disabled 
                    title="L'email ne peut pas être modifié"
                  />
                </div>
                <div class="mb-4">
                  <label class="form-label fw-bold">
                    <i class="bi bi-telephone me-2"></i>Numéro de téléphone
                  </label>
                  <input 
                    v-model="editUser.phoneNumber" 
                    type="tel" 
                    class="form-control" 
                    placeholder="Entrez votre numéro de téléphone"
                  />
                </div>
                <div class="mb-4">
                  <label class="form-label fw-bold">
                    <i class="bi bi-lock me-2"></i>Mot de passe actuel
                  </label>
                  <div class="input-group">
                    <input 
                      v-model="editUser.currentPassword" 
                      :type="showCurrentPassword ? 'text' : 'password'" 
                      class="form-control" 
                      required 
                      placeholder="Entrez votre mot de passe actuel"
                    />
                    <button 
                      type="button" 
                      class="btn btn-outline-secondary" 
                      @click="toggleShowCurrentPassword" 
                      title="Afficher/Masquer le mot de passe"
                    >
                      <i :class="showCurrentPassword ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                    </button>
                  </div>
                </div>
                <div class="mb-4">
                  <label class="form-label fw-bold">
                    <i class="bi bi-lock me-2"></i>Nouveau mot de passe
                  </label>
                  <div class="input-group">
                    <input 
                      v-model="editUser.newPassword" 
                      :type="showNewPassword ? 'text' : 'password'" 
                      class="form-control" 
                      placeholder="Entrez un nouveau mot de passe (facultatif)"
                    />
                    <button 
                      type="button" 
                      class="btn btn-outline-secondary" 
                      @click="toggleShowNewPassword" 
                      title="Afficher/Masquer le mot de passe"
                    >
                      <i :class="showNewPassword ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                    </button>
                  </div>
                </div>
                <div v-if="errorMessage" class="alert alert-danger animate__animated animate__fadeIn" role="alert">
                  {{ errorMessage }}
                </div>
                <div v-if="successMessage" class="alert alert-success animate__animated animate__fadeIn" role="alert">
                  {{ successMessage }}
                </div>
                <div class="text-center mt-4">
                  <button type="button" class="btn btn-secondary me-2" @click="cancelEditing">
                    Annuler
                  </button>
                  <button type="submit" class="btn btn-primary btn-gradient">
                    <i class="bi bi-save me-2"></i>Enregistrer
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import { ref, onMounted } from 'vue';
import Cookies from 'js-cookie';

export default {
  name: 'Profile',
  setup() {
    const user = ref({});
    const editUser = ref({
      userName: '',
      email: '',
      phoneNumber: '',
      currentPassword: '',
      newPassword: ''
    });
    const isEditing = ref(false);
    const errorMessage = ref('');
    const successMessage = ref('');
    const showCurrentPassword = ref(false);
    const showNewPassword = ref(false);

    // Load user data
    onMounted(async () => {
      try {
        const token = Cookies.get('token');
        const response = await axios.get('/api/profile', {
          headers: { Authorization: `Bearer ${token}` }
        });
        user.value = response.data.data;
        editUser.value = { ...response.data.data, currentPassword: '', newPassword: '' };
      } catch (error) {
        console.error('Erreur lors du chargement du profil:', error);
        errorMessage.value = 'Erreur lors du chargement du profil. Veuillez réessayer.';
      }
    });

    // Start editing mode
    const startEditing = () => {
      isEditing.value = true;
    };

    // Cancel editing
    const cancelEditing = () => {
      isEditing.value = false;
      editUser.value = { ...user.value, currentPassword: '', newPassword: '' };
      errorMessage.value = '';
      successMessage.value = '';
    };

    // Handle avatar image error
    const handleAvatarError = (event) => {
      event.target.style.display = 'none';
      event.target.outerHTML = '<i class="bi bi-person-circle avatar-icon"></i>';
    };

    // Toggle password visibility
    const toggleShowCurrentPassword = () => {
      showCurrentPassword.value = !showCurrentPassword.value;
    };
    const toggleShowNewPassword = () => {
      showNewPassword.value = !showNewPassword.value;
    };

    // Submit form
    const submitForm = async () => {
      try {
        const token = Cookies.get('token');
        const response = await axios.put('/api/profile', editUser.value, {
          headers: { Authorization: `Bearer ${token}` }
        });
        successMessage.value = 'Profil mis à jour avec succès !';
        errorMessage.value = '';
        user.value = response.data.data;
        isEditing.value = false;
        setTimeout(() => (successMessage.value = ''), 3000);
      } catch (error) {
        console.error('Erreur lors de la mise à jour:', error);
        errorMessage.value = error.response?.data?.message || 'Erreur lors de la mise à jour du profil.';
      }
    };

    return {
      user,
      editUser,
      isEditing,
      errorMessage,
      successMessage,
      showCurrentPassword,
      showNewPassword,
      toggleShowCurrentPassword,
      toggleShowNewPassword,
      startEditing,
      cancelEditing,
      submitForm,
      handleAvatarError
    };
  }
};
</script>

<style scoped>
.container {
  padding-top: 2rem;
}

.card {
  border-radius: 12px;
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.card:hover {
  transform: translateY(-5px);
  box-shadow: 0 12px 24px rgba(0, 0, 0, 0.15) !important;
}

.card-header {
  background: linear-gradient(135deg, #0d6efd, #6610f2);
  border-radius: 12px 12px 0 0;
  padding: 2rem 1.5rem;
}

.avatar {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f8f9fa;
  border: 2px solid #fff;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}

.avatar-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.avatar-icon {
  font-size: 3rem;
  color: #6c757d;
}

.card-title {
  font-size: 1.75rem;
  font-weight: 600;
  color: #fff;
}

.card-body {
  background: #fff;
  border-radius: 0 0 12px 12px;
}

.profile-info {
  animation: fadeIn 0.3s ease;
}

.form-label {
  color: #2c3e50;
  font-size: 1rem;
  margin-bottom: 0.5rem;
}

.form-text {
  color: #6c757d;
  font-size: 1rem;
  margin: 0;
}

.form-control {
  border-radius: 8px;
  padding: 0.75rem 1rem;
  font-size: 1rem;
  transition: all 0.3s ease;
}

.form-control:focus {
  border-color: #0d6efd;
  box-shadow: 0 0 0 3px rgba(13, 110, 253, 0.1);
}

.form-control:disabled {
  background: #e9ecef;
  cursor: not-allowed;
  opacity: 0.7;
}

.input-group .btn-outline-secondary {
  border-radius: 0 8px 8px 0;
  padding: 0.5rem 1rem;
}

.btn-gradient {
  background: linear-gradient(135deg, #0d6efd, #6610f2);
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  font-size: 1rem;
  border-radius: 8px;
  transition: all 0.3s ease;
}

.btn-gradient:hover {
  background: linear-gradient(135deg, #0b5ed7, #520dc2);
  transform: translateY(-2px);
}

.btn-secondary {
  background: #6c757d;
  border: none;
  color: white;
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  transition: all 0.3s ease;
}

.btn-secondary:hover {
  background: #5c636a;
  transform: translateY(-2px);
}

.alert {
  border-radius: 8px;
  padding: 1rem;
  font-size: 0.9rem;
  margin-bottom: 1.5rem;
}

.alert-danger {
  background: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
}

.alert-success {
  background: #d4edda;
  color: #155724;
  border: 1px solid #c3e6cb;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (max-width: 576px) {
  .card-header {
    padding: 1.5rem 1rem;
  }
  .avatar {
    width: 60px;
    height: 60px;
  }
  .avatar-icon {
    font-size: 2.5rem;
  }
  .card-title {
    font-size: 1.5rem;
  }
  .card-body {
    padding: 1.5rem;
  }
  .btn-gradient,
  .btn-secondary {
    width: 100%;
    margin-bottom: 0.5rem;
  }
  .text-center .btn-secondary {
    margin-right: 0;
  }
}
</style>