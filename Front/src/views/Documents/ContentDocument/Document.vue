<template>
  <div class="container">
    <!-- Message pendant l'extraction -->
    <div v-if="isExtracting" class="loading-bar">
      <div class="spinner"></div>
      <p>⏳ Extraction en cours, veuillez patienter...</p>
    </div>

    <!-- Message pendant l'analyse
    <div v-if="documentStore.isAnalysing" class="loading-bar">
      <div class="spinner"></div>
      <p>⏳ Analyse en cours, veuillez patienter...</p>
    </div> -->

    <!-- Disposition du contenu principal -->
    <div v-else class="content-layout">
      <!-- Moitié gauche : boutons et contenu dynamique -->
      <div class="left-content">
        <!-- Barre de boutons horizontale en haut -->
        <div class="button-bar">
          <button
            class="content-btn"
            :class="{ active: activeSection === 'images' }"
            @click="activeSection = 'images'"
          >
            Images
          </button>
          <button
            class="content-btn"
            :class="{ active: activeSection === 'information' }"
            @click="activeSection = 'information'"
          >
            Information
          </button>
          
          <button
            class="content-btn"
            :class="{ active: activeSection === 'text' }"
            @click="activeSection = 'text'"
          >
            Texte
          </button>

          <!-- Bouton Analyser avec icône tournante -->
          <button
            class="analyse-btn"
            @click="analyseDocument"
            :disabled="documentStore.isAnalysing"
          >
            <i class="bi bi-gear" :class="{ 'spin-icon': documentStore.isAnalysing }"></i> Analyser
          </button>
        </div>

        <!-- Zone de contenu dynamique -->
        <div class="dynamic-content">
          <Information
            v-if="activeSection === 'information'"
            :document="document"
            @update-document="updateDocument"
            @analyse-document="analyseDocument"
          />
          <Images
            v-if="activeSection === 'images' && !isExtracting && !documentStore.isAnalysing"
            :images="images"
            @show-full-image="showFullImage"
          />
          <Text
            v-if="activeSection === 'text' && !isExtracting && !documentStore.isAnalysing && document.text"
            :text="document.text"
            @copy-text="copyText"
          />
        </div>
      </div>

      <!-- Aperçu à droite -->
      <Preview :document="document" />
    </div>

    <!-- Modal pour l'image -->
    <div v-if="selectedImage" class="image-modal" @click="closeModal">
      <div class="modal-content" @click.stop>
        <button class="close-btn" @click="closeModal">
          <i class="bi bi-x-lg"></i>
        </button>
        <img :src="selectedImage" alt="Image en taille réelle" />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import axios from 'axios'
import { useDocumentStore } from '@/Store/analysis'
import Preview from './Preview.vue'
import Information from './Information.vue'
import Images from './Images.vue'
import Text from './Text.vue'

const route = useRoute()
const documentId = route.params.id
const selectedImage = ref(null)
const isCopied = ref(false)
const documentStore = useDocumentStore()
const activeSection = ref('images') // Section active par défaut

const document = ref({
  name: '',
  description: '',
  isExtracted: false,
  isAnalysed: false,
  uploadDate: null,
  fileUrl: null,
  text: null
})
const images = ref([])
const isExtracting = ref(false)

const fetchDocument = async () => {
  try {
    const res = await axios.get(`api/documents/${documentId}`)
    document.value = res.data
    if (!document.value.isExtracted) {
      await extractDocumentContent()
    } else {
      await fetchImages()
    }
  } catch (error) {
    console.error('Erreur lors de la récupération du document :', error)
    alert('❌ Erreur lors de la récupération du document')
  }
}

const fetchImages = async () => {
  try {
    const res = await axios.get(`api/images/${documentId}`)
    images.value = res.data
  } catch (error) {
    console.error('Erreur lors de la récupération des images :', error)
  }
}

const extractDocumentContent = async () => {
  isExtracting.value = true
  try {
    if (!document.value.isExtracted) {
      await axios.post(`api/documents/extract/${documentId}`)
      document.value.isExtracted = true
      const res = await axios.get(`api/documents/${documentId}`)
      document.value = res.data
      await fetchImages()
    }
  } catch (error) {
    console.error('Erreur lors de l\'extraction du contenu :', error)
    alert('❌ Erreur lors de l\'extraction du contenu')
  } finally {
    isExtracting.value = false
  }
}

const updateDocument = async (updatedData) => {
  try {
    await axios.put(`api/documents/${documentId}`, updatedData)
    document.value.name = updatedData.name
    document.value.description = updatedData.description
    alert('Document mis à jour avec succès ✅')
  } catch (error) {
    console.error('Erreur lors de la mise à jour du document :', error)
    alert('❌ Échec de la mise à jour')
  }
}

const analyseDocument = async () => {
  try {
    documentStore.startAnalysing()
    await axios.post(`api/documents/describe/${documentId}`)
    const res = await axios.get(`api/documents/${documentId}`)
    document.value = res.data
    await fetchImages() // Rafraîchir les descriptions des images
    alert('Document analysé avec succès ✅')
  } catch (error) {
    console.error('Erreur lors de l\'analyse du document :', error)
    alert(`❌ Échec de l'analyse : ${error.response?.data?.message || error.message}`)
  } finally {
    documentStore.stopAnalysing()
  }
}

const copyText = () => {
  navigator.clipboard.writeText(document.value.text).then(() => {
    isCopied.value = true
    setTimeout(() => {
      isCopied.value = false
    }, 2000)
  })
}

const showFullImage = (imageUrl) => {
  selectedImage.value = imageUrl
}

const closeModal = () => {
  selectedImage.value = null
}

onMounted(() => {
  fetchDocument()
})
</script>

<style scoped>
.container {
  max-width: 100%;
  margin-top: 42px;
  padding: 10px;
  font-family: Arial, sans-serif;
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.loading-bar {
  background-color: #fff3cd;
  color: #856404;
  padding: 20px;
  margin-bottom: 20px;
  border: 1px solid #ffeeba;
  border-radius: 8px;
  font-weight: bold;
  text-align: center;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 15px;
}

.spinner {
  width: 24px;
  height: 24px;
  border: 3px solid rgba(0, 0, 0, 0.1);
  border-radius: 50%;
  border-top-color: #856404;
  animation: spin 1s ease-in-out infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.content-layout {
  display: flex;
  flex: 1;
  gap: 0;
}

.left-content {
  margin-top: 50px;
  width: 50%;
  display: flex;
  flex-direction: column;
}

.button-bar {
  z-index: 99;
  position: fixed;
  display: flex;
  gap: 20px; /* Espacement similaire à l'image */
  padding: 10px;
  background: none; /* Supprimer le fond */
  border-bottom: none; /* Supprimer la bordure */
  width: calc(50% - 20px); /* Aligner avec .left-content (50% - padding) */
}

.content-btn {
  background: none; /* Pas de fond */
  border: none; /* Pas de bordure */
  padding: 5px; /* Padding minimal */
  cursor: pointer;
  position: relative; /* Pour la barre active */
  font-size: 16px; /* Taille de texte ajustée */
  color: #5f6368; /* Gris comme dans l'image */
  transition: color 0.3s;
}

.content-btn.active::after {
  content: '';
  position: absolute;
  bottom: -2px;
  left: 0;
  width: 100%;
  height: 2px; /* Épaisseur de la barre */
  background-color: #1a73e8; /* Bleu comme dans l'image */
}

.content-btn:hover {
  color: #1a73e8; /* Bleu au survol */
}

.analyse-btn {
  margin-left: auto; /* Pousse le bouton à droite */
  background-color: #1bc0c8; /* Cyan pour être remarquable */
  color: white;
  padding: 8px 16px; /* Ajusté pour correspondre à la barre */
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 16px;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: background-color 0.3s;
}

.analyse-btn:hover {
  background-color: #17a2b8;
}

.analyse-btn:disabled {
  background-color: #6c757d;
  cursor: not-allowed;
}

.spin-icon {
  animation: spin 1s ease-in-out infinite; /* Applique l'animation de rotation */
}

.dynamic-content {
  margin-top: 50px;
  flex: 1;
  padding: 20px;
  overflow-y: auto;
}

.preview {
  position: fixed;
  right: 0;
  top: 0;
  width: 50%;
  height: 100vh;
  margin-right: 0;
  background-color: #fff;
  border-left: 1px solid #ddd;
  overflow-y: auto;
  padding: 20px;
  box-sizing: border-box;
}

.image-modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.9);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  padding: 20px;
}

.modal-content {
  position: relative;
  max-width: 90%;
  max-height: 90vh;
  background-color: transparent;
}

.modal-content img {
  max-width: 100%;
  max-height: 90vh;
  object-fit: contain;
  border-radius: 8px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
}

.close-btn {
  position: absolute;
  top: -40px;
  right: -40px;
  background-color: white;
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

.close-btn:hover {
  transform: scale(1.1);
  background-color: #f8f9fa;
}

.close-btn i {
  font-size: 1.2rem;
  color: #333;
}

@media (max-width: 992px) {
  .content-layout {
    flex-direction: column;
  }

  .left-content {
    width: 100%;
  }

  .button-bar {
    flex-wrap: wrap;
    justify-content: center;
    width: 100%; /* Ajuster pour petits écrans */
  }

  .analyse-btn {
    margin-left: 0; /* Centrer sur petits écrans */
    margin-top: 10px; /* Espacement vertical */
  }

  .dynamic-content {
    padding: 15px;
  }

  .preview {
    position: static;
    width: 100%;
    height: 100%;
    border-left: none;
    border-top: 1px solid #ddd;
  }
}

@media (max-width: 768px) {
  .container {
    padding: 15px;
  }

  .button-bar {
    flex-direction: row; /* Garder les boutons en ligne */
    justify-content: center;
    gap: 15px;
  }

  .content-btn {
    padding: 5px;
  }

  .modal-content {
    max-width: 95%;
  }

  .close-btn {
    top: -30px;
    right: 0;
  }
}
</style>