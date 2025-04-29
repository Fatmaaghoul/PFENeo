<template>
  <div class="container">
    <!-- Message pendant l'extraction -->
    <div v-if="isExtracting" class="loading-bar">
      <div class="spinner"></div>
      <p>⏳ Extraction en cours, veuillez patienter...</p>
    </div>

    <!-- Message pendant l'analyse -->
    <div v-if="documentStore.isAnalysing" class="loading-bar">
      <div class="spinner"></div>
      <p>⏳ Analyse en cours, veuillez patienter...</p>
    </div>

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
const activeSection = ref('information') // Section active par défaut

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
  margin: 0;
  padding: 20px;
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
  width: 50%;
  display: flex;
  flex-direction: column;
}

.button-bar {
  display: flex;
  gap: 10px;
  padding: 10px;
  background-color: #fff;
  border-bottom: 1px solid #ddd;
}

.content-btn {
  padding: 8px 16px;
  background: #fff;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 14px;
  color: #333;
  cursor: pointer;
  transition: background-color 0.2s, border-color 0.2s;
}

.content-btn:hover {
  background-color: #f8f9fa;
  border-color: #bbb;
}

.content-btn.active {
  background-color: #4caf50;
  border-color: #4caf50;
  color: white;
}

.dynamic-content {
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
  }

  .dynamic-content {
    padding: 15px;
  }

  .preview {
    position: static;
    width: 100%;
    height: auto;
    border-left: none;
    border-top: 1px solid #ddd;
  }
}

@media (max-width: 768px) {
  .container {
    padding: 15px;
  }

  .button-bar {
    flex-direction: column;
    align-items: center;
  }

  .content-btn {
    width: 100%;
    margin-bottom: 10px;
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