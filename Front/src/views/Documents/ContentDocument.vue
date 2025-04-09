<template>
  <div class="container">
    <!-- Message during extraction -->
    <div v-if="isExtracting" class="loading-bar">
      <div class="spinner"></div>
      <p>⏳ Extraction in progress, please wait...</p>
    </div>

    <!-- Main Content Layout -->
    <div v-else class="content-layout">
      <!-- Left Side: Document Viewer -->
      <div class="document-viewer">
        <h2>Document Preview</h2>
        <div class="document-container">
          <div v-if="document.fileUrl" class="document-frame">
            <iframe :src="document.fileUrl" frameborder="0" width="100%" height="100%"></iframe>
          </div>
          <div v-else class="no-document">
            <i class="bi bi-file-earmark-text"></i>
            <p>Document preview not available</p>
          </div>
        </div>
      </div>

      <!-- Right Side: Document Information -->
      <div class="document-info">
        <h2>Document Information</h2>
        <div class="info-card">
          <div class="form-group">
            <label>Name:</label>
            <input v-model="document.name" class="form-input" />
          </div>

          <div class="form-group">
            <label>Description:</label>
            <textarea v-model="document.description" class="form-textarea"></textarea>
          </div>

          <div class="form-group">
            <label>Upload Date:</label>
            <p class="info-text">{{ formatDate(document.uploadDate) }}</p>
          </div>

          <div class="form-group">
            <label>Status:</label>
            <p class="info-text">
              <span :class="['status-badge', document.isExtracted ? 'extracted' : 'pending']">
                {{ document.isExtracted ? 'Extracted' : 'Pending Extraction' }}
              </span>
              <span :class="['status-badge', document.isTraiter ? 'traiter' : 'non-traiter']">
                {{ document.isTraiter ? 'Traité' : 'Non Traité' }}
              </span>
            </p>
          </div>

          <button @click="updateDocument" class="update-btn">
            <i class="bi bi-save"></i> Update Document
          </button>
        </div>
      </div>
    </div>

    <!-- Bottom Section: Images -->
    <div class="images-section" v-if="!isExtracting">
      <h2>🖼️ Extracted Images</h2>

      <div class="images-grid" v-if="images.length > 0">
        <div class="image-card" v-for="img in images" :key="img.id">
          <div class="image-container">
            <img :src="img.fileUrl" alt="Document Image" />
            <button class="zoom-btn" @click="showFullImage(img.fileUrl)">
              <i class="bi bi-zoom-in"></i>
            </button>
          </div>
          <div class="image-description">
            <p>{{ img.description ? img.description : 'No description available' }}</p>
          </div>
        </div>
      </div>
      <p v-else class="no-images">No images found for this document.</p>
    </div>
  </div>

  <!-- Image Modal -->
  <div v-if="selectedImage" class="image-modal" @click="closeModal">
    <div class="modal-content" @click.stop>
      <button class="close-btn" @click="closeModal">
        <i class="bi bi-x-lg"></i>
      </button>
      <img :src="selectedImage" alt="Full size image" />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import axios from 'axios'

const route = useRoute()
const documentId = route.params.id
const selectedImage = ref(null)

const document = ref({ 
  name: '', 
  description: '', 
  isExtracted: false,
  isTraiter: false,
  uploadDate: null,
  fileUrl: null
})
const images = ref([])
const isExtracting = ref(false)

const fetchDocument = async () => {
  try {
    const res = await axios.get(`api/documents/${documentId}`)
    document.value = res.data

    // If the document is not yet extracted, start extraction
    if (!document.value.isExtracted) {
      await extractDocumentContent()
    } else {
      await fetchImages() // If already extracted, load images
    }
  } catch (error) {
    console.error('Error fetching document:', error)
  }
}

const fetchImages = async () => {
  try {
    const res = await axios.get(`api/images/${documentId}`)
    images.value = res.data
  } catch (error) {
    console.error('Error fetching images:', error)
  }
}

const extractDocumentContent = async () => {
  isExtracting.value = true
  try {
    // Call the extraction API only if isExtracted is false
    if (!document.value.isExtracted) {
      await axios.post(`api/documents/extract/${documentId}`)

      // Update locally after extraction
      document.value.isExtracted = true

      // Load images once extraction is complete
      await fetchImages()
    }
  } catch (error) {
    console.error('Error during content extraction:', error)
  } finally {
    isExtracting.value = false
  }
}

const updateDocument = async () => {
  try {
    await axios.put(`api/documents/${documentId}`, {
      name: document.value.name,
      description: document.value.description
    })
    alert('Document updated successfully ✅')
  } catch (error) {
    console.error('Error updating document:', error)
    alert('❌ Update failed')
  }
}

const formatDate = (dateString) => {
  if (!dateString) return 'Not available';
  const date = new Date(dateString);
  return date.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
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
  max-width: 1400px;
  margin: 30px auto;
  padding: 20px;
  font-family: Arial, sans-serif;
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
  to { transform: rotate(360deg); }
}

/* Main Content Layout */
.content-layout {
  display: flex;
  gap: 30px;
  margin-bottom: 40px;
}

/* Document Viewer */
.document-viewer {
  flex: 1;
  min-width: 0;
}

.document-viewer h2 {
  margin-top: 0;
  margin-bottom: 15px;
  color: #333;
  font-size: 1.5rem;
}

.document-container {
  background: #fefefe;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  height: 600px;
  overflow: hidden;
}

.document-frame {
  width: 100%;
  height: 100%;
}

.no-document {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: #6c757d;
}

.no-document i {
  font-size: 3rem;
  margin-bottom: 15px;
}

/* Document Information */
.document-info {
  flex: 1;
  min-width: 0;
}

.document-info h2 {
  margin-top: 0;
  margin-bottom: 15px;
  color: #333;
  font-size: 1.5rem;
}

.info-card {
  background: #fefefe;
  padding: 20px;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: 600;
  color: #333;
}

.form-input,
.form-textarea {
  width: 100%;
  padding: 12px;
  border-radius: 8px;
  border: 1px solid #ddd;
  font-size: 16px;
  transition: border-color 0.3s;
}

.form-input:focus,
.form-textarea:focus {
  outline: none;
  border-color: #4CAF50;
}

.form-textarea {
  resize: vertical;
  min-height: 100px;
}

.info-text {
  margin: 0;
  padding: 12px;
  background-color: #f8f9fa;
  border-radius: 8px;
  color: #333;
}

.status-badge {
  display: inline-block;
  padding: 5px 10px;
  border-radius: 20px;
  font-size: 14px;
  font-weight: 600;
}

.status-badge.extracted {
  background-color: #d4edda;
  color: #155724;
}

.status-badge.pending {
  background-color: #fff3cd;
  color: #856404;
}

.status-badge.traiter {
  background-color: #cce5ff;
  color: #004085;
  margin-left: 10px;
}

.status-badge.non-traiter {
  background-color: #f8d7da;
  color: #721c24;
  margin-left: 10px;
}

.update-btn {
  background-color: #4CAF50;
  color: white;
  padding: 12px 20px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 16px;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: background-color 0.3s;
}

.update-btn:hover {
  background-color: #45a049;
}

/* Images Section */
.images-section {
  margin-top: 40px;
}

.images-section h2 {
  margin-bottom: 20px;
  color: #333;
  font-size: 1.5rem;
}

.images-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.image-card {
  background: #fefefe;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  transition: transform 0.3s, box-shadow 0.3s;
}

.image-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
}

.image-container {
  height: 200px;
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

.image-description {
  padding: 15px;
  background: #f8f9fa;
}

.image-description p {
  margin: 0;
  font-size: 14px;
  color: #333;
  line-height: 1.5;
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

/* Responsive Design */
@media (max-width: 992px) {
  .content-layout {
    flex-direction: column;
  }
  
  .document-container {
    height: 500px;
  }
}

@media (max-width: 768px) {
  .container {
    padding: 15px;
  }
  
  .images-grid {
    grid-template-columns: 1fr;
  }
  
  .document-container {
    height: 400px;
  }
}

/* Image Modal */
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

@media (max-width: 768px) {
  .modal-content {
    max-width: 95%;
  }
  
  .close-btn {
    top: -30px;
    right: 0;
  }
}
</style>
