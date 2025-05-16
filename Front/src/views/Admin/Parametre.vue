<template>
  <div class="settings-container">
    <div class="settings-header">
      <h1><i class="bi bi-gear-fill"></i> Paramètres</h1>
      <p>Personnalisez votre expérience utilisateur</p>
    </div>

    <div class="settings-grid">
   

      <!-- Section Application -->
      <div class="settings-card">
        <div class="card-header">
          <h2><i class="bi bi-app-indicator"></i> Application</h2>
        </div>
        <div class="card-body">
          <div class="setting-item">
            <div class="setting-info">
              <h3>Thème</h3>
              <p>Personnalisez l'apparence de l'application</p>
            </div>
            <select v-model="theme" class="theme-select">
              <option value="light">Clair</option>
              <option value="dark">Sombre</option>
            </select>
          </div>

          <div class="setting-item">
            <div class="setting-info">
              <h3>Langue</h3>
              <p>Langue d'affichage</p>
            </div>
            <select v-model="language" class="language-select">
              <option value="fr">Français</option>

            </select>
          </div>

        </div>
      </div>

      <!-- Section Modèles IA -->
      <div class="settings-card">
        <div class="card-header">
          <h2><i class="bi bi-robot"></i> Modèles IA</h2>
        </div>
        <div class="card-body">
          <div class="setting-item">
            <div class="setting-info">
              <h3>Modèle actuel</h3>
              <p v-if="currentModel">Nom : {{ currentModel.modelName }} ({{ currentModel.type }})</p>
              <p v-else>Chargement du modèle actuel...</p>
            </div>
          </div>

<div class="setting-item">
  <div class="setting-info">
    <h3>Changer de modèle</h3>
    <p>Sélectionnez un nouveau modèle IA</p>
  </div>
<select 
  v-model="selectedModelValue" 
  class="model-select"
>
  <option disabled value="">Choisir un modèle</option>
  <option
    v-for="model in availableModels"
    :key="model.modelValue"
    :value="model.modelValue"
  >
    {{ model.modelName }} ({{ model.type }})
  </option>
</select>
</div>

<div class="setting-item">
  <button 
  class="btn-edit" 
  @click="updateModel"
  :disabled="!selectedModelValue || isUpdating"
>
  <i class="bi bi-arrow-repeat"></i> 
  {{ isUpdating ? 'Mise à jour...' : 'Mettre à jour' }}
</button>
  <p v-if="modelStatus" class="model-status">{{ modelStatus }}</p>
</div>

          <div v-if="selectedModelInfo" class="model-details">
            <h4>Détails du modèle :</h4>
            <p><strong>Nom :</strong> {{ selectedModelInfo.modelName }}</p>
            <p><strong>Description :</strong> {{ selectedModelInfo.description }}</p>
          </div>
        </div>
      </div>

      <!-- Section Support -->
      <div class="settings-card">
        <div class="card-header">
          <h2><i class="bi bi-question-circle"></i> Support</h2>
        </div>
        <div class="card-body">
          <div class="setting-item">
            <div class="setting-info">
              <h3>Aide & FAQ</h3>
              <p>Questions fréquentes et documentation</p>
            </div>
            <button class="btn-link" @click="openHelpCenter">
              <i class="bi bi-book"></i> Accéder
            </button>
          </div>

          <div class="setting-item">
            <div class="setting-info">
              <h3>Contacter le support</h3>
              <p>Signalez un problème ou posez une question</p>
            </div>
            <button class="btn-link" @click="contactSupport">
              <i class="bi bi-envelope"></i> Contacter
            </button>
          </div>

          <div class="setting-item">
            <div class="setting-info">
              <h3>Conditions d'utilisation</h3>
              <p>Lisez nos termes et conditions</p>
            </div>
            <button class="btn-link" @click="openTerms">
              <i class="bi bi-file-text"></i> Lire
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue';
import axios from 'axios';
import { useThemeStore } from '@/Store/theme';

export default {
  name: 'SettingsPage',
  setup() {
    const themeStore = useThemeStore();
    const theme = computed({
      get: () => themeStore.isDark ? 'dark' : 'light',
      set: (value) => {
        if (value === 'dark' !== themeStore.isDark) {
          themeStore.toggleTheme();
        }
      }
    });

    // États réactifs
    const currentModel = ref(null);
    const availableModels = ref([]);
    const selectedModelValue = ref('');
    const isUpdating = ref(false);
    const modelStatus = ref('');
    const loadingCurrent = ref(false);
    const loadingModels = ref(false);

    // Computed properties
    const selectedModelInfo = computed(() => {
      return availableModels.value.find(model => model.modelValue === selectedModelValue.value);
    });

    const statusClass = computed(() => {
      return {
        'success': modelStatus.value.includes('succès'),
        'error': modelStatus.value.includes('Erreur')
      };
    });

    // Méthodes
    const fetchCurrentModel = async () => {
      try {
        loadingCurrent.value = true;
        const response = await axios.get('/api/models/current-model');
        currentModel.value = response.data;
      } catch (error) {
        console.error('Erreur lors du chargement du modèle actuel:', error);
        modelStatus.value = 'Erreur lors du chargement du modèle actuel';
      } finally {
        loadingCurrent.value = false;
      }
    };

    const fetchAvailableModels = async () => {
      try {
        loadingModels.value = true;
        const response = await axios.get('/api/models/available-models');
        availableModels.value = response.data.map(model => ({
          modelValue: model.id || model.value,
          modelName: model.name,
          type: model.type,
          description: model.description
        }));
      } catch (error) {
        console.error('Erreur lors du chargement des modèles disponibles:', error);
        modelStatus.value = 'Erreur lors du chargement des modèles disponibles';
      } finally {
        loadingModels.value = false;
      }
    };

    const updateModel = async () => {
      if (!selectedModelValue.value) {
        modelStatus.value = "Veuillez sélectionner un modèle";
        return;
      }

      try {
        isUpdating.value = true;
        modelStatus.value = 'Mise à jour en cours...';
        
        const response = await axios.post('/api/models/update-model', {
          modelValue: selectedModelValue.value
        }, {
          headers: {
            'Content-Type': 'application/json'
          }
        });
        
        currentModel.value = response.data;
        modelStatus.value = 'Modèle mis à jour avec succès';
      } catch (error) {
        console.error('Erreur lors de la mise à jour:', error);
        modelStatus.value = error.response?.data?.message || 
                          error.message || 
                          'Erreur lors de la mise à jour';
      } finally {
        isUpdating.value = false;
      }
    };

    // Initialisation
    onMounted(async () => {
      await Promise.all([
        fetchCurrentModel(),
        fetchAvailableModels()
      ]);
    });

    return {
      // États
      currentModel,
      availableModels,
      selectedModelValue,
      isUpdating,
      modelStatus,
      loadingCurrent,
      loadingModels,
      theme,
      
      // Computed
      selectedModelInfo,
      statusClass,
      
      // Méthodes
      updateModel,
      fetchCurrentModel,
      fetchAvailableModels
    };
  }
};
</script>

<style scoped>
.settings-container {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.settings-header {
  margin-bottom: 2.5rem;
  text-align: center;
}

.settings-header h1 {
  font-size: 2.5rem;
  color: var(--text-color);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  margin-bottom: 0.5rem;
}

.settings-header p {
  color: var(--text-secondary);
  font-size: 1.1rem;
}

.settings-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
  gap: 1.5rem;
}

.settings-card {
  background: var(--background-color);
  border-radius: 16px;
  box-shadow: var(--card-shadow);
  overflow: hidden;
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.settings-card:hover {
  transform: translateY(-5px);
  box-shadow: var(--hover-shadow);
}

.card-header {
  padding: 1.25rem 1.5rem;
  background: var(--secondary-background);
  border-bottom: 1px solid var(--border-color);
}

.card-header h2 {
  margin: 0;
  font-size: 1.4rem;
  color: var(--text-color);
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.card-body {
  padding: 1.5rem;
}

.setting-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 0;
  border-bottom: 1px solid var(--border-color);
}

.setting-item:last-child {
  border-bottom: none;
}

.setting-info h3 {
  margin: 0 0 0.25rem;
  font-size: 1.1rem;
  color: var(--text-color);
}

.setting-info p {
  margin: 0;
  color: var(--text-secondary);
  font-size: 0.9rem;
}

.btn-edit {
  padding: 0.5rem 1rem;
  background: var(--primary-color);
  color: white;
  border: none;
  border-radius: 8px;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-edit:hover {
  background: var(--secondary-color);
}

.btn-edit:disabled {
  background: var(--text-secondary);
  cursor: not-allowed;
}

.btn-export {
  padding: 0.5rem 1rem;
  background: #2ecc71;
  color: white;
  border: none;
  border-radius: 8px;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-export:hover {
  background: #27ae60;
}

.btn-link {
  background: none;
  border: none;
  color: #3498db;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem;
}

.btn-link:hover {
  text-decoration: underline;
}

.theme-select, 
.language-select,
.model-select {
  padding: 0.5rem;
  border-radius: 8px;
  border: 1px solid var(--border-color);
  background-color: var(--background-color);
  color: var(--text-color);
  min-width: 120px;
}

.model-select {
  min-width: 200px;
}

.model-select:focus {
  outline: none;
  border-color: var(--primary-color);
  box-shadow: 0 0 0 2px rgba(52, 152, 219, 0.2);
}

.model-status {
  margin-top: 0.5rem;
  font-size: 0.9rem;
  color: var(--secondary-color);
}

.model-details {
  margin-top: 1rem;
  padding: 1rem;
  background-color: var(--secondary-background);
  border-radius: 8px;
}

.model-details h4 {
  margin-top: 0;
  color: var(--text-color);
}

.model-details p {
  margin: 0.5rem 0;
  color: var(--text-color);
}

/* Switch toggle */
.switch {
  position: relative;
  display: inline-block;
  width: 50px;
  height: 24px;
}

.switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #ccc;
  transition: .4s;
}

.slider:before {
  position: absolute;
  content: "";
  height: 16px;
  width: 16px;
  left: 4px;
  bottom: 4px;
  background-color: white;
  transition: .4s;
}

input:checked + .slider {
  background-color: #2ecc71;
}

input:focus + .slider {
  box-shadow: 0 0 1px #2ecc71;
}

input:checked + .slider:before {
  transform: translateX(26px);
}

.slider.round {
  border-radius: 34px;
}

.slider.round:before {
  border-radius: 50%;
}

@media (max-width: 768px) {
  .settings-grid {
    grid-template-columns: 1fr;
  }
  
  .setting-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }
  
  .btn-edit, 
  .btn-export, 
  .btn-link {
    width: 100%;
    justify-content: center;
  }
}
</style>