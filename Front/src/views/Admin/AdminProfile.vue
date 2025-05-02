<template>
  <div class="admin-profile-container">
    <!-- Header with navigation -->
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h1 class="h3 mb-0">Profil d'administrateur</h1>
      <nav aria-label="breadcrumb">
        <ol class="breadcrumb mb-0">
          <li class="breadcrumb-item active">Profil</li>
        </ol>
      </nav>
    </div>

    <!-- Profile section -->
    <div class="card shadow-sm mb-4 profile-header-card">
      <div class="card-body">
        <div class="d-flex align-items-center mb-4">
          <!-- Avatar avec la première lettre de l'email -->
          <div class="avatar-circle bg-primary text-white me-3">
            {{ user?.email?.charAt(0).toUpperCase() }}
          </div>
          <div>
            <h2 class="h4 mb-1">{{ user?.email }}</h2>
            <p class="text-muted mb-0">{{ formattedRoles }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Personal Information -->
    <div class="card shadow-sm profile-info-card">
      <div class="card-body">
        <div class="d-flex justify-content-between align-items-center mb-4">
          <h3 class="h5 mb-0">Informations personnelles</h3>
          <button class="btn btn-outline-primary btn-sm rounded-pill" @click="toggleEditMode">
            <i class="bi bi-pencil me-2"></i>{{ isEditing ? 'Annuler' : 'Modifier' }}
          </button>
        </div>

        <div class="row g-4">
          <div class="col-md-6">
            <div class="mb-3">
              <label class="form-label text-muted small">Nom</label>
              <input 
                v-model="editUser.userName" 
                :readonly="!isEditing" 
                :class="isEditing ? 'form-control' : 'form-control-plaintext'" 
              />
            </div>
          </div>
        
          <div class="col-md-6">
            <div class="mb-3">
              <label class="form-label text-muted small">Email</label>
              <input 
                v-model="editUser.email" 
                :readonly="!isEditing" 
                :class="isEditing ? 'form-control' : 'form-control-plaintext'" 
              />
            </div>
          </div>
          <div class="col-md-6">
            <div class="mb-3">
              <label class="form-label text-muted small">Téléphone</label>
              <input 
                v-model="editUser.phoneNumber" 
                :readonly="!isEditing" 
                :class="isEditing ? 'form-control' : 'form-control-plaintext'" 
              />
            </div>
          </div>

          <!-- Mode Édition : Ajout des champs de mot de passe -->
          <div v-if="isEditing" class="col-12">
            <div class="mb-3">
              <label class="form-label text-muted small">Mot de passe actuel</label>
              <div class="input-group">
                <input 
                  v-model="editUser.currentPassword" 
                  :type="showCurrentPassword ? 'text' : 'password'" 
                  class="form-control" 
                  required 
                />
                <button 
                  type="button" 
                  class="btn btn-outline-secondary" 
                  @click="toggleShowCurrentPassword"
                >
                  <i :class="showCurrentPassword ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                </button>
              </div>
            </div>
          </div>

          <div v-if="isEditing" class="col-12">
            <div class="mb-3">
              <label class="form-label text-muted small">Nouveau mot de passe</label>
              <div class="input-group">
                <input 
                  v-model="editUser.newPassword" 
                  :type="showNewPassword ? 'text' : 'password'" 
                  class="form-control" 
                />
                <button 
                  type="button" 
                  class="btn btn-outline-secondary" 
                  @click="toggleShowNewPassword"
                >
                  <i :class="showNewPassword ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                </button>
              </div>
            </div>
          </div>

          <!-- Bouton de sauvegarde en mode édition -->
          <div v-if="isEditing" class="col-12">
            <button class="btn btn-primary" @click="saveProfile">
              <i class="bi bi-save me-2"></i>Enregistrer
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { computed, ref, onMounted } from 'vue';
import { useStore } from 'vuex';
import axios from 'axios';
import Cookies from 'js-cookie';

export default {
  name: 'AdminProfile',
  setup() {
    const store = useStore();
    const user = computed(() => store.state.user);
    const isEditing = ref(false);
    const showCurrentPassword = ref(false);
    const showNewPassword = ref(false);

    // Données éditables
    const editUser = ref({
      userName: '',
      email: '',
      phoneNumber: '',
      currentPassword: '',
      newPassword: ''
    });

    // Formater les rôles pour l'affichage
    const formattedRoles = computed(() => {
      return user.value?.roles?.join(', ') || '';
    });

    // Basculer entre le mode édition et consultation
    const toggleEditMode = () => {
      isEditing.value = !isEditing.value;
      if (!isEditing.value) {
        // Réinitialiser editUser avec les données actuelles de l'utilisateur
        editUser.value = {
          userName: user.value?.userName || '',
          email: user.value?.email || '',
          phoneNumber: user.value?.phoneNumber || '',
          currentPassword: '',
          newPassword: ''
        };
      }
    };

    // Basculer l'affichage du mot de passe actuel
    const toggleShowCurrentPassword = () => {
      showCurrentPassword.value = !showCurrentPassword.value;
    };

    // Basculer l'affichage du nouveau mot de passe
    const toggleShowNewPassword = () => {
      showNewPassword.value = !showNewPassword.value;
    };

    // Sauvegarder les modifications
    const saveProfile = async () => {
      try {
        const token = Cookies.get('token');
        const response = await axios.put("/api/profile", editUser.value, {
          headers: { Authorization: `Bearer ${token}` }
        });

        // Mettre à jour les données dans le store
        store.commit('SET_USER', response.data.data);
        isEditing.value = false;
        alert('Profile updated successfully!');
      } catch (error) {
        console.error("Error updating profile:", error);
        alert(error.response?.data?.message || "Failed to update profile.");
      }
    };

    // Charger les données utilisateur au montage du composant
    onMounted(async () => {
      try {
        const token = Cookies.get('token');
        const response = await axios.get("/api/profile", {
          headers: { Authorization: `Bearer ${token}` }
        });

        // Mettre à jour les données dans le store
        store.commit('SET_USER', response.data.data);

        // Initialiser editUser avec les données de l'utilisateur
        editUser.value = {
          userName: response.data.data.userName || '',
          email: response.data.data.email || '',
          phoneNumber: response.data.data.phoneNumber || '',
          currentPassword: '',
          newPassword: ''
        };
      } catch (error) {
        console.error("Error loading profile:", error);
        alert("Failed to load profile data.");
      }
    });

    return {
      user,
      editUser,
      isEditing,
      showCurrentPassword,
      showNewPassword,
      formattedRoles,
      toggleEditMode,
      toggleShowCurrentPassword,
      toggleShowNewPassword,
      saveProfile
    };
  }
};
</script>

<style scoped>
.admin-profile-container {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.profile-header-card {
  background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
  border: none;
}

.profile-info-card {
  border: none;
}

.avatar-circle {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.75rem;
  font-weight: bold;
  background: linear-gradient(135deg, #4e73df 0%, #224abe 100%);
}

.form-control-plaintext {
  font-size: 1rem;
  color: #333;
  padding: 0.375rem 0;
  background-color: transparent;
  border: none;
  border-bottom: 1px solid #eee;
}

.btn-outline-secondary {
  width: 40px;
  height: 40px;
  padding: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  border-color: #dee2e6;
}

.btn-outline-secondary:hover {
  background-color: #f8f9fa;
}

.breadcrumb {
  background: transparent;
  padding: 0;
}

.card {
  border-radius: 10px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
  margin-bottom: 1.5rem;
}

.btn-outline-primary {
  border-radius: 20px;
  padding: 0.5rem 1.25rem;
  border-width: 2px;
  font-weight: 500;
}

.btn-outline-primary:hover {
  background-color: rgba(78, 115, 223, 0.1);
}

.btn-primary {
  border-radius: 6px;
  padding: 0.75rem 1.5rem;
  font-weight: 500;
  background: linear-gradient(135deg, #4e73df 0%, #224abe 100%);
  border: none;
}

.text-muted {
  color: #6c757d !important;
}

.input-group {
  border-radius: 6px;
  overflow: hidden;
}

.form-control {
  border-radius: 6px;
  padding: 0.75rem 1rem;
  border: 1px solid #dee2e6;
  transition: all 0.2s;
}

.form-control:focus {
  border-color: #4e73df;
  box-shadow: 0 0 0 0.2rem rgba(78, 115, 223, 0.25);
}

@media (max-width: 768px) {
  .admin-profile-container {
    padding: 1rem;
  }
  
  .avatar-circle {
    width: 50px;
    height: 50px;
    font-size: 1.5rem;
  }
}
</style>