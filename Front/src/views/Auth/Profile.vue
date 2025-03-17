<template>
  <div class="container mt-6">
    <div class="row justify-content-center">
      <div class="col-md-8">
        <div class="card shadow">
          <div class="card-body">
            <h2 class="card-title text-center mb-4">
              <i class="bi bi-person-circle me-2"></i>Mon Profil
            </h2>

            <!-- Mode Consultation -->
            <div v-if="!isEditing" class="profile-info">
              <div class="mb-3">
                <label class="form-label fw-bold">
                  <i class="bi bi-person me-2"></i>Nom d'utilisateur
                </label>
                <p>{{ user.userName }}</p>
              </div>

              <div class="mb-3">
                <label class="form-label fw-bold">
                  <i class="bi bi-envelope me-2"></i>Email
                </label>
                <p>{{ user.email }}</p>
              </div>

              <div class="mb-3">
                <label class="form-label fw-bold">
                  <i class="bi bi-telephone me-2"></i>Numéro de téléphone
                </label>
                <p>{{ user.phoneNumber }}</p>
              </div>

              <div class="text-center mt-4">
                <button class="btn btn-primary" @click="startEditing">
                  <i class="bi bi-pencil me-2"></i>Modifier
                </button>
              </div>
            </div>

            <!-- Mode Édition -->
            <div v-else class="profile-info">
              <form @submit.prevent="submitForm">
                <div class="mb-3">
                  <label class="form-label fw-bold">
                    <i class="bi bi-person me-2"></i>Nom d'utilisateur
                  </label>
                  <input v-model="editUser.userName" type="text" class="form-control" required />
                </div>

                <div class="mb-3">
                  <label class="form-label fw-bold">
                    <i class="bi bi-envelope me-2"></i>Email
                  </label>
                  <input v-model="editUser.email" type="email" class="form-control" required />
                </div>

                <div class="mb-3">
                  <label class="form-label fw-bold">
                    <i class="bi bi-telephone me-2"></i>Numéro de téléphone
                  </label>
                  <input v-model="editUser.phoneNumber" type="tel" class="form-control" />
                </div>

            <!-- Champ Mot de passe actuel avec show/hide -->
            <div class="mb-3">
                  <label class="form-label fw-bold">
                    <i class="bi bi-lock me-2"></i>Mot de passe actuel
                  </label>
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

                <!-- Champ Nouveau mot de passe avec show/hide -->
                <div class="mb-3">
                  <label class="form-label fw-bold">
                    <i class="bi bi-lock me-2"></i>Nouveau mot de passe
                  </label>
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

                <div v-if="errorMessage" class="alert alert-danger" role="alert">
                  {{ errorMessage }}
                </div>
                <div v-if="successMessage" class="alert alert-success" role="alert">
                  {{ successMessage }}
                </div>

                <div class="text-center mt-4">
                  <button type="button" class="btn btn-secondary me-2" @click="cancelEditing">
                    Annuler
                  </button>
                  <button type="submit" class="btn btn-primary">
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
import { useStore } from 'vuex';
import Cookies from 'js-cookie';

export default {
  name: "Profile",
  setup() {
    const store = useStore();
    const user = ref({});
    const editUser = ref({
      userName: "",
      email: "",
      phoneNumber: "",
      currentPassword: "",
      newPassword: ""
    });
    const isEditing = ref(false);
    const errorMessage = ref("");
    const successMessage = ref("");

    // Variables pour gérer l'affichage des mots de passe
    const showCurrentPassword = ref(false);
    const showNewPassword = ref(false);

    // Méthodes pour basculer l'affichage des mots de passe
    const toggleShowCurrentPassword = () => {
      showCurrentPassword.value = !showCurrentPassword.value;
    };

    const toggleShowNewPassword = () => {
      showNewPassword.value = !showNewPassword.value;
    };

    // Charger les données utilisateur au montage du composant
    onMounted(async () => {
      try {
        const token = Cookies.get('token');
        const response = await axios.get("/api/profile", {
          headers: { Authorization: `Bearer ${token}` }
        });

        user.value = response.data.data;
        editUser.value = { ...response.data.data, currentPassword: "", newPassword: "" };
      } catch (error) {
        console.error("Erreur lors du chargement du profil :", error);
        errorMessage.value = "Erreur lors du chargement du profil.";
      }
    });

    // Activer le mode édition
    const startEditing = () => {
      isEditing.value = true;
    };

    // Annuler l'édition
    const cancelEditing = () => {
      isEditing.value = false;
      editUser.value = { ...user.value, currentPassword: "", newPassword: "" };
      errorMessage.value = "";
      successMessage.value = "";
    };

    // Soumettre le formulaire de mise à jour
    const submitForm = async () => {
      try {
        const token = Cookies.get('token');
        const response = await axios.put("/api/profile", editUser.value, {
          headers: { Authorization: `Bearer ${token}` }
        });

        successMessage.value = "Profil mis à jour avec succès !";
        errorMessage.value = "";
        user.value = response.data.data;
        isEditing.value = false;

        // Effacer le message de succès après 3 secondes
        setTimeout(() => successMessage.value = "", 3000);
      } catch (error) {
        console.error("Erreur lors de la mise à jour :", error);
        errorMessage.value = error.response?.data?.message || "Erreur lors de la mise à jour du profil.";
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
      submitForm
    };
  },
};
</script>