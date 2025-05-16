<template>
  <div class="images-section">
    <div class="images-grid" v-if="enrichedImages.length > 0">
      <div class="image-card" v-for="img in enrichedImages" :key="img.id">
        <div class="image-card-content">
          <div class="image-container">
            <img :src="img.fileUrl" alt="Image du document" />
            <button class="zoom-btn" @click="$emit('show-full-image', img.fileUrl)">
              <i class="bi bi-zoom-in"></i>
            </button>
          </div>
          <div class="objects-list">
            <table v-if="img.objects && img.objects.length > 0">
              <thead>
                <tr>
                  <th>Sélectionner</th>
                  <th>Nom</th>
                  <th>Occurrences (Texte)</th>
                  <th>Occurrences (Image)</th>
                  <th>Pourcentage</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="obj in img.objects" :key="obj.id">
                  <td>
                    <input
                      type="checkbox"
                      :value="obj.id"
                      v-model="selectedObjects[img.id]"
                      @change="updateSelectedObjects(img.id)"
                    />
                  </td>
                  <td>{{ obj.name || 'Non spécifié' }}</td>
                  <td>{{ obj.occurenceText }}</td>
                  <td>{{ obj.occurenceImage }}</td>
                  <td>{{ obj.pourcentage ? obj.pourcentage.toFixed(2) + '%' : '0%' }}</td>
                </tr>
              </tbody>
            </table>
            <p v-else>Aucun objet détecté pour cette image.</p>
            <div class="describe-btn-container">
              <button
                class="describe-btn"
                :disabled="!hasSelectedObjects(img.id) || documentStore.isAnalysing"
                @click="describeObjects(img.id)"
              >
                <span v-if="documentStore.isAnalysing && timers[img.id] > 0" class="spinner"></span>
                <span>Décrire</span>
              </button>
              <button
                v-if="img.descriptions && img.descriptions.length > 0"
                class="toggle-descriptions-btn"
                @click="toggleDescriptions(img.id)"
              >
                {{ showDescriptions[img.id] ? 'Ne pas voir' : 'Afficher les descriptions' }}
              </button>
            </div>
          </div>
        </div>
        <div
          class="descriptions-section"
          v-if="showDescriptions[img.id] && img.descriptions && img.descriptions.length > 0"
        >
          <div class="descriptions-header">
            <h3>Descriptions</h3>
          </div>
          <div
            class="description-item"
            v-for="desc in displayedDescriptions(img.descriptions, img.id)"
            :key="desc.id"
          >
            <button class="delete-btn" @click="deleteDescription(img.id, desc.id)">
              <i class="bi bi-trash"></i>
            </button>
            <p class="description-timestamp">
              <em>Date : {{ new Date(desc.created).toLocaleString() }}</em>
            </p>
            <p class="description-objects">
              <strong>Objets décrits :</strong> {{ desc.objects.map(obj => obj.name).join(', ') }}
            </p>
            <p class="description-text">{{ desc.text }}</p>
          </div>
        </div>
      </div>
    </div>
    <p v-else class="no-images">Aucune image trouvée pour ce document.</p>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import axios from 'axios';
import { useDocumentStore } from '@/Store/analysis.js';

const props = defineProps({
  images: {
    type: Array,
    required: true,
  },
  clearSelectionsAfterSuccess: {
    type: Boolean,
    default: true,
  },
});

const emit = defineEmits(['show-full-image']);

const documentStore = useDocumentStore();

const enrichedImages = ref(props.images.map(img => ({ ...img, objects: [], descriptions: [] })));
const selectedObjects = ref({});
const showDescriptions = ref({});
const timers = ref({});
const timerIntervals = ref({});

enrichedImages.value.forEach(img => {
  selectedObjects.value[img.id] = [];
  showDescriptions.value[img.id] = false;
  timers.value[img.id] = 0;
});

const fetchObjects = async () => {
  for (let i = 0; i < enrichedImages.value.length; i++) {
    const img = enrichedImages.value[i];
    try {
      const response = await axios.get(`/api/images/objects/${img.id}`);
      enrichedImages.value[i].objects = response.data;
      selectedObjects.value[img.id] = enrichedImages.value[i].objects
        .filter(obj => obj.pourcentage > 0)
        .map(obj => obj.id);
    } catch (error) {
      console.error(`Erreur lors de la récupération des objets pour l'image ${img.id}:`, error);
      enrichedImages.value[i].objects = [];
      selectedObjects.value[img.id] = [];
    }
  }
};

const fetchDescriptions = async () => {
  for (let i = 0; i < enrichedImages.value.length; i++) {
    const img = enrichedImages.value[i];
    try {
      const response = await axios.get(`https://localhost:7036/api/images/descriptions/${img.id}`, {
        headers: {
          Accept: '*/*',
        },
      });
      // Normalize \n characters in description text
      enrichedImages.value[i].descriptions = (response.data.descriptions || []).map(desc => ({
        ...desc,
        text: desc.text.replace(/\\n/g, '\n'),
      }));
    } catch (error) {
      console.error(`Erreur lors de la récupération des descriptions pour l'image ${img.id}:`, error);
      enrichedImages.value[i].descriptions = [];
    }
  }
};

const hasSelectedObjects = imageId => {
  return selectedObjects.value[imageId]?.length > 0;
};

const updateSelectedObjects = imageId => {
  if (showDescriptions.value[imageId]) {
    showDescriptions.value[imageId] = false;
    setTimeout(() => {
      showDescriptions.value[imageId] = true;
    }, 0);
  }
};

const toggleDescriptions = imageId => {
  showDescriptions.value[imageId] = !showDescriptions.value[imageId];
};

const displayedDescriptions = (descriptions, imageId) => {
  if (!imageId || !descriptions) return [];

  const selectedObjectIds = selectedObjects.value[imageId] || [];
  const filteredDescriptions = descriptions.filter(desc =>
    desc.objects.some(obj => selectedObjectIds.includes(obj.id))
  );
  console.log(`Image ${imageId}: ${filteredDescriptions.length} filtered descriptions`);
  console.log('Description texts:', filteredDescriptions.map(desc => desc.text));

  return [...filteredDescriptions].sort((a, b) => new Date(b.created) - new Date(a.created));
};

const describeObjects = async imageId => {
  const objectIds = selectedObjects.value[imageId];
  if (!objectIds?.length) {
    alert('Veuillez sélectionner au moins un objet à décrire.');
    return;
  }

  const image = enrichedImages.value.find(img => img.id === imageId);
  const selectedObjectsList = image.objects.filter(obj => objectIds.includes(obj.id));

  if (!confirm(`Voulez-vous analyser : ${selectedObjectsList.map(o => o.name).join(', ')}?`)) {
    return;
  }

  // Setup loading state
  timers.value[imageId] = 0;
  timerIntervals.value[imageId] = setInterval(() => timers.value[imageId]++, 1000);
  documentStore.startAnalysing();
  
  try {
    const response = await axios.post(`/api/images/describe/${imageId}`, {
      ImageUrl: image.fileUrl,
      Objects: selectedObjectsList.map(obj => obj.name)
    });

    console.log('Full response:', response); // Debug log

    // Vérification approfondie de la réponse
    if (!response.data?.Description && !response.data?.description) {
      throw new Error('Réponse invalide - aucune description trouvée');
    }

    // Gestion des variations de casse (Description/description)
    const descriptionText = response.data.Description || response.data.description;
    const modelUsed = response.data.ModelUsed || response.data.model_used;

    // Affichage du succès
    alert(`🗒️ Description générée (${modelUsed}):\n\n${descriptionText.replace(/\\n/g, '\n')}`);

    // Rafraîchissement des données
    await fetchDescriptions();
    showDescriptions.value[imageId] = true;

  } catch (error) {
    console.error('Description error:', error);
    
    // Gestion d'erreur améliorée
    let errorMsg = 'Erreur inconnue';
    if (error.response) {
      errorMsg = error.response.data?.title || 
                error.response.data?.detail || 
                JSON.stringify(error.response.data);
    } else {
      errorMsg = error.message;
    }

    alert(`❌ Échec de la description:\n${errorMsg}`);
  } finally {
    clearInterval(timerIntervals.value[imageId]);
    timers.value[imageId] = 0;
    documentStore.stopAnalysing();
  }
};

const deleteDescription = async (imageId, descriptionId) => {
  if (!confirm('Êtes-vous sûr de vouloir supprimer cette description ?')) {
    return;
  }

  try {
    const response = await axios.delete(`api/description/${descriptionId}`, {
      headers: {
        Accept: '*/*',
      },
    });

    await fetchDescriptions();
    alert(response.data.Message || 'Description supprimée avec succès.');
  } catch (error) {
    console.error(`Erreur lors de la suppression de la description ${descriptionId}:`, error);
    alert(`Erreur: ${error.response?.data || error.message}`);
  }
};

// Cleanup timers on component unmount
onUnmounted(() => {
  Object.keys(timerIntervals.value).forEach(imageId => {
    clearInterval(timerIntervals.value[imageId]);
  });
});

onMounted(async () => {
  await fetchObjects();
  await fetchDescriptions();
});
</script>

<style scoped>
.images-section {
  margin-top: 40px;
}

.images-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(60.75%, 1fr));
  gap: 20px;
}

.image-card {
  background: #fefefe;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  transition: transform 0.3s, box-shadow 0.3s;
  display: flex;
  flex-direction: column;
}

.image-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
}

.image-card-content {
  display: flex;
  align-items: stretch;
  gap: 15px;
}

.image-container {
  flex: 0 0 337.5px;
  height: 337.5px;
  overflow: hidden;
  position: relative;
}

.image-container img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s;
}

.image-card:hover .image-container img {
  transform: scale(1.05);
}

.objects-list {
  flex: 1;
  padding: 15px;
  background: #f8f9fa;
  display: flex;
  flex-direction: column;
}

.objects-list table {
  width: 100%;
  border-collapse: collapse;
}

.objects-list th,
.objects-list td {
  padding: 8px;
  text-align: left;
  border-bottom: 1px solid #dee2e6;
}

.objects-list th {
  background: #e9ecef;
  font-weight: 600;
}

.objects-list td {
  font-size: 0.9rem;
  color: #333;
}

.objects-list p {
  margin: 10px 0;
  color: #6c757d;
  font-style: italic;
}

.no-images {
  text-align: center;
  padding: 30px;
  background: #f8f9fa;
  border-radius: 8px;
  color: #6c757d;
}

.zoom-btn {
  position: absolute;
  top: 10px;
  right: 10px;
  background-color: rgba(255, 255, 255, 0.9);
  border: none;
  border-radius: 50%;
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.2);
}

.zoom-btn:hover {
  background-color: white;
  transform: scale(1.1);
}

.zoom-btn i {
  font-size: 1.2rem;
  color: #333;
}

.describe-btn-container {
  margin-top: 10px;
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  padding-top: 10px;
  align-items: center;
}

.describe-btn {
  background-color: #007bff;
  color: white;
  border: none;
  border-radius: 5px;
  padding: 8px 16px;
  cursor: pointer;
  font-size: 0.9rem;
  transition: background-color 0.3s, transform 0.2s;
  display: flex;
  align-items: center;
  gap: 5px;
}

.describe-btn:hover:not(:disabled) {
  background-color: #0056b3;
  transform: scale(1.05);
}

.describe-btn:disabled {
  background-color: #6c757d;
  cursor: not-allowed;
}

.describe-btn:disabled .spinner {
  animation: spin 1s linear infinite;
}

.toggle-descriptions-btn {
  background-color: #28a745;
  color: white;
  border: none;
  border-radius: 5px;
  padding: 8px 16px;
  cursor: pointer;
  font-size: 0.9rem;
  transition: background-color 0.3s, transform 0.2s;
}

.toggle-descriptions-btn:hover {
  background-color: #218838;
  transform: scale(1.05);
}

.descriptions-section {
  margin-top: 20px;
  padding: 15px;
  border-top: 1px solid #dee2e6;
  width: 100%;
}

.descriptions-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.descriptions-section h3 {
  font-size: 1.2rem;
  color: #333;
  margin: 0;
}

.description-item {
  background: #fff;
  padding: 10px;
  border-radius: 5px;
  margin-bottom: 10px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  position: relative;
}

.delete-btn {
  position: absolute;
  bottom: 10px;
  right: 10px;
  background: none;
  border: none;
  cursor: pointer;
  padding: 5px;
  transition: color 0.3s, transform 0.2s;
}

.delete-btn i {
  font-size: 1.2rem;
  color: #dc3545;
}

.delete-btn:hover i {
  color: #c82333;
  transform: scale(1.1);
}

.description-timestamp {
  font-size: 0.85rem;
  color: #6c757d;
  font-style: italic;
  margin-bottom: 5px;
}

.description-objects {
  font-size: 0.9rem;
  color: #555;
  margin-bottom: 5px;
}

.description-text {
  font-size: 0.95rem;
  color: #333;
  white-space: pre-line;
}

.no-descriptions {
  padding: 15px;
  color: #6c757d;
  font-style: italic;
  text-align: center;
}

.spinner {
  width: 16px;
  height: 16px;
  border: 2px solid #fff;
  border-top: 2px solid #0056b3;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

@media (max-width: 1024px) {
  .images-grid {
    grid-template-columns: 1fr;
  }

  .image-card-content {
    flex-direction: column;
  }

  .image-container {
    flex: none;
    width: 100%;
    height: 405px;
  }

  .objects-list {
    width: 100%;
  }

  .describe-btn-container {
    flex-direction: column;
    gap: 5px;
  }
}

@media (max-width: 768px) {
  .image-container {
    height: 337.5px;
  }

  .objects-list table {
    font-size: 0.85rem;
  }

  .description-text {
    font-size: 0.9rem;
  }

  .description-objects {
    font-size: 0.85rem;
  }

  .description-timestamp {
    font-size: 0.8rem;
  }

  .describe-btn,
  .toggle-descriptions-btn {
    font-size: 0.85rem;
    padding: 6px 12px;
  }

  .describe-btn-container {
    flex-direction: column;
    gap: 5px;
  }

  .delete-btn i {
    font-size: 1rem;
  }
}
</style>
