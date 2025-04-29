<template>
  <div class="container">
    <div class="analyze-box">
      <h1>Analyse de Document</h1>
      
      <!-- Section Documents disponibles -->
      <div class="documents-section">
        <h3>Documents disponibles</h3>
        <div class="documents-list">
          <div v-for="doc in userDocuments" 
               :key="doc.id" 
               class="document-item"
               :class="{ 'selected': selectedDocument?.id === doc.id }"
               @click="selectDocument(doc)">
            <i class="bi bi-file-pdf"></i>
            <span class="document-name">{{ doc.name }}</span>
            <span class="document-date">{{ formatDate(doc.uploadDate) }}</span>
            <span class="select-circle"></span>
          </div>
        </div>
      </div>

      <!-- Options d'analyse -->
      <div v-if="selectedDocument" class="analysis-options">
        <div class="options-grid">
          <!-- Solutions de Reconnaissance -->
          <div class="option-section">
            <h3>Solution d'analyse</h3>
            <div class="radio-group">
              <label class="radio-item" :class="{ 'active-option': selectedModel === 'LLAVA' }">
                <input type="radio" v-model="selectedModel" value="LLAVA">
                <span>LLAVA</span>
                <span v-if="selectedModel === 'LLAVA'" class="active-badge">Actif</span>
              </label>
              <label class="radio-item" :class="{ 'active-option': selectedModel === 'LLaMA Vision' }">
                <input type="radio" v-model="selectedModel" value="LLaMA Vision">
                <span>LLaMA Vision</span>
                <span v-if="selectedModel === 'LLaMA Vision'" class="active-badge">Actif</span>
              </label>
            </div>
          </div>

          <!-- Paramètres d'Analyse -->
          <div class="option-section">
            <h3>Paramètres d'Analyse</h3>
            <div class="select-group">
              <label>Niveau de détail</label>
              <select class="select-input" disabled>
                <option selected>Détaillé (fixe)</option>
                <option>Basique</option>
                <option>Avancé</option>
              </select>

              <label>Langue de description</label>
              <select class="select-input" disabled>
                <option selected>Français (fixe)</option>
                <option>English</option>
                <option>Español</option>
              </select>
            </div>
          </div>
        </div>

        <button 
          @click="startAnalysis" 
          :disabled="isAnalyzing || !selectedDocument"
          class="analyze-btn"
        >
          <span v-if="isAnalyzing" class="spinner"></span>
          {{ isAnalyzing ? 'Analyse en cours...' : 'Démarrer l\'analyse' }}
        </button>

        <div class="analysis-info">
          <i class="bi bi-info-circle"></i>
          <p>L'analyse utilise actuellement {{ selectedModel }} avec description détaillée en français.</p>
        </div>
      </div>

      <!-- Résultats -->
      <div v-if="analysisResults" class="results-section">
        <h2>Résultats de l'analyse</h2>
        
        <div class="summary-section">
          <div class="summary-card">
            <h3>Résumé</h3>
            <div class="summary-grid">
              <div class="summary-item">
                <span class="summary-label">Modèle utilisé</span>
                <span class="summary-value">{{ analysisResults.analysisModel }}</span>
              </div>
              <div class="summary-item">
                <span class="summary-label">Images analysées</span>
                <span class="summary-value">{{ analysisResults.imagesAnalyzed }}</span>
              </div>
              <div class="summary-item">
                <span class="summary-label">Objets détectés</span>
                <span class="summary-value">{{ analysisResults.totalObjectsDetected }}</span>
              </div>
              <div class="summary-item">
                <span class="summary-label">Temps de traitement</span>
                <span class="summary-value">{{ analysisResults.processingTimeSec }} sec</span>
              </div>
            </div>
          </div>
        </div>

        <div class="images-results">
          <h3>Images analysées</h3>
          <div v-for="(image, index) in analysisResults.images" :key="index" class="image-result">
            <div class="image-container">
              <h4>Image {{ index + 1 }}</h4>
              <div class="image-comparison">
                <div class="image-wrapper">
                  <h5>Originale</h5>
                  <img :src="image.original_image" alt="Image originale" class="result-image">
                </div>
                <div class="image-wrapper">
                  <h5>Analyse</h5>
                  <img :src="image.annotated_image" alt="Image annotée" class="result-image">
                </div>
              </div>
              <div class="image-description">
                <h5>Description détaillée</h5>
                <p>{{ image.llava_description }}</p>
              </div>
              <div v-if="image.detected_objects && image.detected_objects.length" class="detected-objects">
                <h5>Objets détectés</h5>
                <div class="objects-list">
                  <span v-for="(obj, idx) in image.detected_objects" :key="idx" class="object-tag">
                    {{ obj.label }} ({{ (obj.confidence * 100).toFixed(1) }}%)
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Messages d'erreur -->
      <div v-if="error" class="error-message">
        <i class="bi bi-exclamation-triangle"></i>
        {{ error }}
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import axios from 'axios';
import { useDocumentStore } from '../../Store/analysis';
import { computed } from 'vue';

// Initialisation du store et du routeur
const analysisStore = useDocumentStore();
const router = useRouter();

// Variables réactives locales
const userDocuments = ref([]);
const selectedDocument = ref(null);
const selectedModel = ref('LLAVA'); // Modèle par défaut
const analysisResults = ref(null);
const error = ref(null);

// Accéder à isAnalyzing depuis le store
const isAnalyzing = computed(() => analysisStore.isAnalyzing);

// Contrôleur pour annuler la requête
let abortController = null;

// Charger les documents de l'utilisateur
onMounted(async () => {
  try {
    const response = await axios.get('api/documents');
    userDocuments.value = response.data;
  } catch (err) {
    showError('Erreur lors du chargement des documents');
    console.error('Erreur:', err);
  }
});

const selectDocument = (doc) => {
  selectedDocument.value = doc;
  analysisResults.value = null;
  error.value = null;
};

const startAnalysis = async () => {
  if (!selectedDocument.value) return;

  // Initialiser AbortController
  abortController = new AbortController();

  // Mettre à jour l'état global via le store
  analysisStore.setAnalyzing(true);
  error.value = null;

  try {
    // Déterminer l'endpoint en fonction du modèle sélectionné
    const endpoint = selectedModel.value === 'LLaMA Vision' 
      ? `api/documents/describe/${selectedDocument.value.id}`
      : `api/documents/${selectedDocument.value.id}/analyze`;

    const response = await axios.post(
      endpoint,
      { model: selectedModel.value }, // Envoyer le modèle sélectionné au backend
      {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
          'Content-Type': 'application/json'
        },
        signal: abortController.signal // Ajouter le signal pour annulation
      }
    );

    if (response.data.status !== 'success') {
      throw new Error(response.data.message || 'Réponse inattendue du serveur');
    }

    analysisResults.value = {
      status: response.data.status,
      analysisModel: response.data.analysisModel || selectedModel.value,
      imagesAnalyzed: response.data.images?.length || 0,
      totalObjectsDetected: response.data.images?.reduce((acc, img) => acc + (img.detected_objects?.length || 0), 0) || 0,
      processingTimeSec: response.data.stats?.processingTimeSec || 0,
      images: response.data.images || []
    };

  } catch (err) {
    if (err.name === 'AbortError') {
      console.log('Requête annulée lors de la navigation');
    } else {
      const errorMessage = err.response?.data?.message || 
                          err.message || 
                          "Erreur inconnue lors de l'analyse";
      console.error('Détails de l\'erreur:', err.response?.data || err);
      showError(`Erreur: ${errorMessage}`);
    }
  } finally {
    // Réinitialiser l'état global uniquement si la requête est terminée
    analysisStore.setAnalyzing(false);
    abortController = null;
  }
};

const showError = (message) => {
  error.value = message;
  setTimeout(() => error.value = null, 5000);
};

const formatDate = (dateString) => {
  if (!dateString) return '';
  return new Date(dateString).toLocaleDateString('fr-FR', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  });
};

// Annuler la requête lors de la navigation ou du démontage du composant
onUnmounted(() => {
  if (abortController) {
    abortController.abort();
  }
});

// Écouter les changements de route pour annuler la requête
router.beforeEach(() => {
  if (abortController) {
    abortController.abort();
  }
});
</script>

<style scoped>
.container {
  padding: 2rem;
  max-width: 1200px;
  margin: 80px auto 0;
}

.analyze-box {
  background: white;
  border-radius: 10px;
  padding: 2rem;
  box-shadow: 0 0 20px rgba(0, 0, 0, 0.1);
}

h1 {
  margin-bottom: 2rem;
  color: #333;
  font-size: 1.8rem;
  font-weight: 600;
}

h2 {
  margin: 2rem 0 1.5rem;
  color: #333;
  font-size: 1.5rem;
  font-weight: 600;
}

h3 {
  color: #333;
  margin-bottom: 1rem;
  font-size: 1.2rem;
  font-weight: 500;
}

h4, h5 {
  color: #444;
  margin: 0.5rem 0;
}

/* Documents list */
.documents-section {
  background: #f9f9f9;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 1.5rem;
  margin-bottom: 2rem;
}

.documents-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.document-item {
  display: flex;
  align-items: center;
  padding: 0.75rem 1rem;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
}

.document-item:hover {
  background-color: #f0f0f0;
}

.document-item.selected {
  background-color: #e6f2ff;
  border-left: 4px solid #007bff;
}

.document-item i {
  margin-right: 0.75rem;
  color: #d32f2f;
  font-size: 1.2rem;
}

.document-name {
  flex-grow: 1;
  font-weight: 500;
}

.document-date {
  font-size: 0.85rem;
  color: #666;
  margin-right: 1rem;
}

.select-circle {
  width: 18px;
  height: 18px;
  border: 2px solid #ddd;
  border-radius: 50%;
  transition: all 0.2s;
}

.document-item.selected .select-circle {
  background-color: #007bff;
  border-color: #007bff;
}

/* Analysis options */
.analysis-options {
  margin-top: 2rem;
}

.options-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2rem;
  margin-bottom: 2rem;
}

@media (max-width: 768px) {
  .options-grid {
    grid-template-columns: 1fr;
  }
}

.option-section {
  background: #f9f9f9;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 1.5rem;
}

/* Radio buttons */
.radio-group {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.radio-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  cursor: pointer;
  padding: 0.5rem;
  border-radius: 4px;
  transition: background 0.2s;
}

.radio-item:hover {
  background: #f0f0f0;
}

.radio-item input {
  accent-color: #007bff;
}

.active-option {
  background-color: #e8f5e9;
  border-left: 3px solid #4caf50;
}

.active-badge {
  background: #4caf50;
  color: white;
  padding: 0.2rem 0.5rem;
  border-radius: 12px;
  font-size: 0.75rem;
  margin-left: 0.5rem;
}

/* Select inputs */
.select-group {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.select-group label {
  font-size: 0.9rem;
  color: #555;
  margin-bottom: 0.25rem;
}

.select-input {
  width: 100%;
  padding: 0.6rem;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 0.95rem;
  background: white;
  transition: border 0.2s;
}

.select-input:disabled {
  background: #f5f5f5;
  cursor: not-allowed;
}

/* Analyze button */
.analyze-btn {
  width: 100%;
  padding: 1rem;
  background: #007bff;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 1.1rem;
  font-weight: 500;
  margin-top: 1rem;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.analyze-btn:hover:not(:disabled) {
  background: #0069d9;
  transform: translateY(-1px);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.analyze-btn:disabled {
  background: #cccccc;
  cursor: not-allowed;
  opacity: 0.7;
}

.spinner {
  border: 2px solid #fff;
  border-top: 2px solid transparent;
  border-radius: 50%;
  width: 16px;
  height: 16px;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.analysis-info {
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;
  margin-top: 1.5rem;
  padding: 1rem;
  background: #e3f2fd;
  border-radius: 6px;
  color: #1976d2;
}

.analysis-info i {
  font-size: 1.2rem;
  flex-shrink: 0;
}

.analysis-info p {
  margin: 0;
  font-size: 0.9rem;
}

/* Results section */
.results-section {
  margin-top: 3rem;
  animation: fadeIn 0.5s ease-out;
}

.summary-section {
  margin-bottom: 2rem;
}

.summary-card {
  background: #f8f9fa;
  border-radius: 8px;
  padding: 1.5rem;
  border: 1px solid #dee2e6;
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
  margin-top: 1rem;
}

.summary-item {
  display: flex;
  flex-direction: column;
}

.summary-label {
  font-size: 0.9rem;
  color: #6c757d;
  margin-bottom: 0.25rem;
}

.summary-value {
  font-size: 1.1rem;
  font-weight: 500;
  color: #333;
}

/* Images results */
.images-results {
  margin-top: 2rem;
}

.image-result {
  margin-bottom: 3rem;
  padding-bottom: 2rem;
  border-bottom: 1px solid #eee;
}

.image-result:last-child {
  border-bottom: none;
}

.image-comparison {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
  margin: 1rem 0;
}

@media (max-width: 768px) {
  .image-comparison {
    grid-template-columns: 1fr;
  }
}

.image-wrapper {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.result-image {
  width: 100%;
  border-radius: 6px;
  border: 1px solid #ddd;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.image-description {
  background: #f8f9fa;
  padding: 1rem;
  border-radius: 6px;
  margin-top: 1rem;
  border: 1px solid #eee;
}

.image-description p {
  margin: 0.5rem 0 0;
  line-height: 1.6;
  color: #333;
}

.detected-objects {
  margin-top: 1rem;
}

.objects-list {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-top: 0.5rem;
}

.object-tag {
  background: #e9ecef;
  padding: 0.3rem 0.75rem;
  border-radius: 50px;
  font-size: 0.85rem;
  color: #495057;
}

/* Error message */
.error-message {
  color: #dc3545;
  padding: 1rem;
  background: #f8d7da;
  border-radius: 6px;
  text-align: center;
  margin-top: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  animation: fadeIn 0.3s ease-out;
}

.error-message i {
  font-size: 1.2rem;
}

/* Animations */
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>