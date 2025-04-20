<template>
  <div class="admin-container">
    <div class="card">
      <div class="card-content">
        <div class="header">
          <h2 class="title">Utilisateurs</h2>
          <div class="search-container">
            <div class="search-box">
              <input type="text" class="search-input" placeholder="Rechercher des utilisateurs..." v-model="searchQuery" />
              <button class="search-button">
                🔍
              </button>
            </div>
          </div>
          <button @click="showAddModal = true" class="button-add">
            ➕ Nouvel utilisateur
          </button>
        </div>

        <!-- État de chargement -->
        <div v-if="loading" class="loading">
          <div class="spinner"></div>
          <p class="loading-text">Chargement des utilisateurs...</p>
        </div>

        <!-- Tableau des utilisateurs -->
        <div v-else class="table-wrapper">
          <table class="users-table">
            <thead>
              <tr>
                <th>Nom</th>
                <th>Email</th>
                <th>Rôle</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="user in filteredUsers" :key="user.id">
                <td>
                  <div class="user-name">
                    👤
                    <span>{{ user.username }}</span>
                  </div>
                </td>
                <td>{{ user.email }}</td>
                <td>
                  <span :class="[
                    'badge',
                    user.roles.includes('Admin') ? 'bg-danger' : 'bg-primary'
                  ]">
                    {{ user.roles.join(', ') }}
                  </span>
                </td>
                <td>
                  <div class="actions">
                    <button class="action-button edit" @click="editUser(user)" title="Modifier">
                      ✏️
                    </button>
                    <button v-if="!user.roles.includes('Admin')" class="action-button delete" @click="deleteUser(user.id)" title="Supprimer">
                      🗑️
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="filteredUsers.length === 0">
                <td colspan="4" class="empty-state">
                  <div class="empty-message">
                    👥
                    <span>Aucun utilisateur trouvé</span>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Modal Ajouter/Modifier un utilisateur -->
    <div v-if="showAddModal || showEditModal" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>{{ showEditModal ? 'Modifier l\'utilisateur' : 'Nouvel utilisateur' }}</h2>
          <button class="close-btn" @click="closeModal">
            ❌
          </button>
        </div>
        
        <div class="modal-body">
          <div v-if="serverErrors.length" class="server-errors">
            <p v-for="(error, index) in serverErrors" :key="index" class="error-message">{{ error }}</p>
          </div>

          <div class="form-group">
            <label>Nom d'utilisateur</label>
            <input 
              v-model="formData.username"
              type="text"
              class="form-input"
              placeholder="Entrez le nom d'utilisateur"
              :class="{ 'error': (formSubmitted && !formData.username) || serverErrors.some(e => e.toLowerCase().includes('username')) }"
            />
            <span v-if="formSubmitted && !formData.username" class="error-message">Le nom est requis</span>
          </div>

          <div class="form-group">
            <label>Email</label>
            <input 
              v-model="formData.email"
              type="email"
              class="form-input"
              placeholder="Entrez l'email"
              :disabled="showEditModal"
              :class="{ 
                'error': (formSubmitted && !formData.email) || serverErrors.some(e => e.toLowerCase().includes('email')),
                'disabled': showEditModal
              }"
            />
            <span v-if="formSubmitted && !formData.email" class="error-message">L'email est requis</span>
          </div>

          <div v-if="showAddModal" class="form-group">
            <label>Mot de passe</label>
            <input 
              v-model="formData.password"
              type="password"
              class="form-input"
              placeholder="Entrez le mot de passe"
              :class="{ 'error': (formSubmitted && !formData.password) || serverErrors.some(e => e.toLowerCase().includes('password')) }"
            />
            <span v-if="formSubmitted && !formData.password" class="error-message">Le mot de passe est requis</span>
          </div>
        </div>
        
        <div class="modal-footer">
          <button class="cancel-btn" @click="closeModal">Annuler</button>
          <button 
            class="submit-btn" 
            @click="submitForm"
            :disabled="!isFormValid"
          >
            {{ showEditModal ? 'Modifier' : 'Créer' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useStore } from 'vuex';
import axios from 'axios';
import Cookies from 'js-cookie';

const router = useRouter();
const store = useStore();
const users = ref([]);
const loading = ref(true);
const searchQuery = ref('');
const showAddModal = ref(false);
const showEditModal = ref(false);
const formData = ref({
  username: '',
  email: '',
  password: ''
});
const editingUserId = ref(null);
const formSubmitted = ref(false);
const serverErrors = ref([]);

// Computed property to filter users
const filteredUsers = computed(() => {
  if (!searchQuery.value) return users.value;
  
  const query = searchQuery.value.toLowerCase();
  return users.value.filter(user => 
    user.username.toLowerCase().includes(query) || 
    user.email.toLowerCase().includes(query)
  );
});

// Computed property for form validation
const isFormValid = computed(() => {
  if (showEditModal.value) {
    return formData.value.username.trim();
  }
  return formData.value.username.trim() && 
         formData.value.email.trim() && 
         formData.value.password.trim();
});

// Check admin access
const checkAdminAccess = () => {
  const token = Cookies.get('token');
  if (!token) {
    console.error("Aucun jeton trouvé");
    store.dispatch('logout');
    router.push('/login');
    return false;
  }

  const tokenParts = token.split('.');
  const payload = JSON.parse(atob(tokenParts[1]));
  const userRole = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

  if (userRole !== 'Admin') {
    console.error("L'utilisateur n'est pas administrateur");
    alert("Accès non autorisé. Cette page est réservée aux administrateurs.");
    router.push('/Document');
    return false;
  }

  return true;
};

// Fetch users
const fetchUsers = async () => {
  if (!checkAdminAccess()) {
    loading.value = false;
    return;
  }
  
  loading.value = true;
  try {
    const token = Cookies.get('token');
    const response = await axios.get('/api/users/all', {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Accept': '*/*',
        'Content-Type': 'application/json'
      }
    });
    
    if (Array.isArray(response.data)) {
      users.value = response.data;
    } else {
      console.error("Les données de réponse ne sont pas un tableau:", response.data);
      users.value = [];
    }
  } catch (error) {
    console.error("Détails de l'erreur:", error);
    handleError(error);
  } finally {
    loading.value = false;
  }
};

// Handle errors
const handleError = (error) => {
  let errorMessage = "Erreur lors de la récupération des utilisateurs.";
  if (error.response?.status === 401) {
    errorMessage = "Vous n'êtes pas autorisé à accéder à cette page. Veuillez vous reconnecter.";
    store.dispatch('logout');
    router.push('/login');
  } else if (error.response?.status === 403) {
    errorMessage = "Accès refusé. Cette page est réservée aux administrateurs.";
    router.push('/Document');
  } else if (error.response?.status === 404) {
    errorMessage = `L'endpoint n'a pas été trouvé. URL: ${error.config?.baseURL}${error.config?.url}\nVeuillez vérifier que le serveur backend est en cours d'exécution.`;
  } else if (error.response?.data?.message) {
    errorMessage = error.response.data.message;
  }
  
  alert(errorMessage);
  users.value = [];
};

// Refresh users
const refreshUsers = () => {
  fetchUsers();
};

// Delete user
const deleteUser = async (userId) => {
  if (!confirm("Voulez-vous vraiment supprimer cet utilisateur ? Cette action est irréversible.")) return;

  try {
    const token = Cookies.get('token');
    await axios.delete(`/api/users/delete/${userId}`, {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });
    users.value = users.value.filter(user => user.id !== userId);
    alert("Utilisateur supprimé avec succès !");
  } catch (error) {
    console.error("Détails de l'erreur:", error);
    alert("Une erreur est survenue lors de la suppression de l'utilisateur.");
  }
};

// Edit user
const editUser = (user) => {
  editingUserId.value = user.id;
  formData.value = {
    username: user.username,
    email: user.email
  };
  showEditModal.value = true;
  serverErrors.value = [];
  formSubmitted.value = false;
};

// Close modal
const closeModal = () => {
  showAddModal.value = false;
  showEditModal.value = false;
  editingUserId.value = null;
  formData.value = {
    username: '',
    email: '',
    password: ''
  };
  serverErrors.value = [];
  formSubmitted.value = false;
};

// Submit form
const submitForm = async () => {
  formSubmitted.value = true;
  serverErrors.value = [];

  if (!isFormValid.value) {
    alert("Veuillez remplir tous les champs requis.");
    return;
  }

  try {
    const token = Cookies.get('token');
    if (!token) {
      console.error("Aucun jeton trouvé");
      store.dispatch('logout');
      router.push('/login');
      return;
    }

    if (showEditModal.value) {
      await axios.put(`/api/users/edit/${editingUserId.value}`, {
        username: formData.value.username.trim()
      }, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Accept': '*/*',
          'Content-Type': 'application/json'
        }
      });
      alert("Utilisateur modifié avec succès !");
    } else {
      const userData = {
        username: formData.value.username.trim(),
        email: formData.value.email.trim(),
        password: formData.value.password.trim()
      };

      await axios.post('/api/users/add', userData, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Accept': '*/*',
          'Content-Type': 'application/json'
        }
      });
      alert("Utilisateur créé avec succès !");
    }
    
    closeModal();
    refreshUsers();
  } catch (error) {
    console.error("Détails de l'erreur:", error);
    if (error.response?.data) {
      const errors = error.response.data;
      if (errors.errors) {
        serverErrors.value = Object.entries(errors.errors)
          .map(([field, messages]) => `${field}: ${messages.join(', ')}`);
      } else if (errors.message) {
        serverErrors.value = [errors.message];
      } else {
        serverErrors.value = ["Une erreur inattendue s'est produite."];
      }
    } else {
      serverErrors.value = ["Échec de l'enregistrement de l'utilisateur. Aucune réponse du serveur."];
    }
  }
};

onMounted(fetchUsers);
</script>

<style scoped>
.admin-container {
  padding: 2rem;
  max-width: 100%;
  margin: 0 auto;
}

.card {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.card-content {
  padding: 1.5rem;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  flex-wrap: wrap;
  gap: 1rem;
}

.title {
  margin: 0;
  font-size: 1.5rem;
  color: #2c3e50;
}

.search-container {
  flex: 1 1 300px;
  max-width: 400px;
}

.search-box {
  display: flex;
  border: 1px solid #ddd;
  border-radius: 4px;
  overflow: hidden;
}

.search-input {
  flex: 1;
  padding: 0.5rem;
  border: none;
  outline: none;
}

.search-button {
  padding: 0.5rem 1rem;
  background: none;
  border: none;
  border-left: 1px solid #ddd;
  cursor: pointer;
}

.search-button:hover {
  background: #f5f5f5;
}

.button-add {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 6px;
  background: linear-gradient(45deg, #3498db, #2980b9);
  color: white;
  font-weight: 600;
  font-size: 1rem;
  cursor: pointer;
  transition: background 0.3s ease, transform 0.2s ease;
}

.button-add:hover {
  background: linear-gradient(45deg, #3aa8f2, #3498db);
  transform: translateY(-2px);
}

.button-add:active {
  transform: translateY(0);
}

.loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 3rem;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 3px solid #f3f3f3;
  border-top: 3px solid #3498db;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.loading-text {
  margin-top: 1rem;
  color: #666;
}

.table-wrapper {
  width: 100%;
  overflow-x: hidden;
}

.users-table {
  width: 100%;
  border-collapse: collapse;
  table-layout: auto;
}

.users-table th,
.users-table td {
  padding: 0.8rem;
  text-align: left;
  border-bottom: 1px solid #eee;
  vertical-align: middle;
  word-break: break-word;
}

.users-table th {
  font-weight: 600;
  color: #2c3e50;
  background: #f8f9fa;
}

.users-table td {
  max-width: 200px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.user-name {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.badge {
  padding: 0.5em 0.75em;
  font-weight: 500;
  border-radius: 4px;
}

.bg-danger {
  background-color: #e74c3c;
  color: white;
}

.bg-primary {
  background-color: #3498db;
  color: white;
}

.actions {
  display: flex;
  gap: 0.5rem;
  flex-wrap: nowrap;
}

.action-button {
  padding: 0.4rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  background: none;
  font-size: 1rem;
}

.action-button:hover {
  background: #f5f5f5;
}

.action-button.edit {
  color: #f39c12;
}

.action-button.delete {
  color: #e74c3c;
}

.empty-state {
  text-align: center;
  padding: 3rem;
}

.empty-message {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  color: #666;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  border-radius: 8px;
  width: 90%;
  max-width: 600px;
  max-height: 90vh;
  overflow-y: auto;
  position: relative;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 1.5rem;
  border-bottom: 1px solid #eee;
}

.modal-header h2 {
  margin: 0;
  font-size: 1.5rem;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
}

.modal-body {
  padding: 1.5rem;
}

.modal-footer {
  padding: 1rem 1.5rem;
  border-top: 1px solid #eee;
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.form-input {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
}

.form-input.error {
  border-color: #e74c3c;
}

.form-input.disabled {
  background: #f5f5f5;
  cursor: not-allowed;
}

.error-message {
  color: #e74c3c;
  font-size: 0.85rem;
  margin-top: 0.25rem;
  display: block;
}

.cancel-btn,
.submit-btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1rem;
}

.cancel-btn {
  background: #f8f9fa;
  color: #2c3e50;
}

.submit-btn {
  background: #3498db;
  color: white;
}

.submit-btn:disabled {
  background: #95a5a6;
  cursor: not-allowed;
}

.server-errors {
  background: #ffe6e6;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1rem;
}

.server-errors .error-message {
  margin: 0.25rem 0;
}

@media (max-width: 768px) {
  .admin-container {
    padding: 1rem;
  }
  
  .header {
    flex-direction: column;
    gap: 1rem;
  }
  
  .search-container {
    width: 100%;
    max-width: none;
  }
  
  .users-table th,
  .users-table td {
    padding: 0.5rem;
    font-size: 0.9rem;
  }
  
  .modal-content {
    width: 95%;
  }
  
  .button-add {
    padding: 0.6rem 1.2rem;
    font-size: 0.9rem;
  }
}
</style>