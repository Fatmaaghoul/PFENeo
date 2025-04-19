<template>
  <div class="container">
    <!-- Loading State -->
    <div v-if="isProcessing" class="loading-bar">
      <div class="spinner"></div>
      <p>⏳ {{ processingMessage }}</p>
    </div>

    <!-- Main Content -->
    <div v-else>
      <!-- Document Header -->
      <div class="document-header">
        <div class="header-top">
          <h1>{{ document.name }}</h1>
          <div class="mode-buttons">
            <button @click="toggleEditMode" class="mode-btn" :class="{ active: isEditMode }">
              <i class="bi bi-pencil"></i> Edit
            </button>
            <button @click="extractImages" class="mode-btn" :disabled="document.imagesExtracted"
              :class="{ active: false, disabled: document.imagesExtracted }">
              <i class="bi bi-image"></i> Extract
            </button>
            <button @click="analyzeDocument" class="mode-btn" 
              :disabled="!document.imagesExtracted || document.isAnalyzed"
              :class="{ active: false, disabled: !document.imagesExtracted || document.isAnalyzed }">
              <i class="bi bi-magic"></i> Analyze
            </button>
          </div>
        </div>
        <div class="metadata">
          <div class="metadata-item">
            <i class="bi bi-upload"></i>
            <span>Uploaded: {{ formatDate(document.uploadDate) }}</span>
          </div>
          <div class="metadata-item">
            <i class="bi bi-hdd"></i>
            <span>Status: 
              <span :class="['status-badge', document.imagesExtracted ? 'extracted' : 'pending']">
                {{ document.imagesExtracted ? 'Images Extracted' : 'Images Pending' }}
              </span>
              <span :class="['status-badge', document.isAnalyzed ? 'analyzed' : 'pending-analysis']">
                {{ document.isAnalyzed ? 'Analyzed' : 'Pending Analysis' }}
              </span>
            </span>
          </div>
        </div>
      </div>

      <!-- Stats Cards -->
      <div v-if="document.isAnalyzed" class="stats-container">
        <div class="stat-card">
          <h3>Total Images</h3>
          <div class="stat-value">{{ document.analysisSummary?.totalImages || 0 }}</div>
        </div>
        <div class="stat-card">
          <h3>Objects Detected</h3>
          <div class="stat-value">{{ document.analysisSummary?.totalObjectsDetected || 0 }}</div>
        </div>
        <div class="stat-card">
          <h3>Processing Time</h3>
          <div class="stat-value">{{ document.analysisSummary?.processingTimeSec || 0 }}s</div>
        </div>
      </div>

      <div class="divider"></div>

      <!-- Document Content Layout -->
      <div class="content-layout">
        <!-- Left Side: Document Viewer -->
        <div class="document-viewer">
          <div class="section-title">
            <i class="bi bi-file-earmark-text"></i>
            <h2>Document Preview</h2>
          </div>
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
      </div>

      <!-- Edit Document Popup -->
      <div v-if="isEditMode" class="edit-popup-overlay" @click.self="toggleEditMode">
        <div class="edit-popup">
          <div class="popup-header">
            <h3>Edit Document Information</h3>
            <button @click="toggleEditMode" class="close-popup-btn">
              <i class="bi bi-x-lg"></i>
            </button>
          </div>
          
          <div class="popup-content">
            <div class="form-group">
              <label>Document Name:</label>
              <input v-model="document.name" class="form-input" />
            </div>

            <div class="form-group">
              <label>Description:</label>
              <textarea 
                v-model="document.description" 
                class="form-textarea"
                placeholder="Add a description for this document"></textarea>
            </div>

            <div class="popup-buttons">
              <button @click="updateDocument" class="update-btn">
                <i class="bi bi-save"></i> Save Changes
              </button>
              <button @click="toggleEditMode" class="cancel-btn">
                Cancel
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Extracted Text Section -->
      <div v-if="!isProcessing && document.text" class="content-section">
        <div class="section-title">
          <i class="bi bi-file-text"></i>
          <h2>Extracted Text</h2>
          <button @click="copyText" class="copy-btn">
            <i class="bi bi-clipboard"></i> Copy Text
          </button>
        </div>
        <div class="text-content">
          <pre>{{ document.text }}</pre>
        </div>
      </div>
      <div v-else-if="!isProcessing && document.imagesExtracted" class="content-section">
        <button @click="extractText" class="action-btn">
          <i class="bi bi-file-earmark-text"></i> Extract Text
        </button>
      </div>

      <!-- Extracted Images Section -->
      <div v-if="document.images && document.images.length > 0" class="content-section">
        <div class="section-title">
          <i class="bi bi-images"></i>
          <h2>Extracted Images</h2>
        </div>
        
        <div class="images-container">
          <div class="image-detail" v-for="(img, index) in document.images" :key="index">
            <div class="image-wrapper">
              <img :src="img.fileUrl" alt="Document Image" class="detail-image" />
              <div class="image-actions">
                <button class="zoom-btn" @click="showFullImage(img.fileUrl)">
                  <i class="bi bi-zoom-in"></i>
                </button>
              </div>
            </div>
            <div class="description-wrapper">
              <div class="image-description">
                <h3>Image {{ index + 1 }}</h3>
                <p v-if="img.description">
                  {{ img.showFullDescription ? img.description : (img.description.length > 150 ? img.description.slice(0, 150) + '...' : img.description) }}
                  <button 
                    v-if="img.description.length > 150"
                    @click="img.showFullDescription = !img.showFullDescription" 
                    class="toggle-description-btn">
                    {{ img.showFullDescription ? 'Show less' : 'Show more' }}
                  </button>
                </p>
                
                <!-- Tags for detected objects -->
                <div v-if="document.isAnalyzed && document.analysisSummary.mainObjects" class="detected-objects">
                  <h4>Main Objects Detected:</h4>
                  <div class="object-tags">
                    <span class="object-tag" v-for="(obj, objIndex) in document.analysisSummary.mainObjects" :key="objIndex">
                      {{ obj }}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>
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
const isEditMode = ref(false)

const document = ref({
  id: '',
  name: '',
  description: '',
  uploadDate: null,
  fileUrl: null,
  text: '',
  userId: '',
  isAnalyzed: false,
  analysisDate: null,
  imagesExtracted: false,
  imagesExtractedDate: null,
  images: [],
  analysisSummary: {
    totalImages: 0,
    totalObjectsDetected: 0,
    mainObjects: [],
    processingTimeSec: 0,
    analysisModel: ''
  }
})

const isProcessing = ref(false)
const processingMessage = ref('Processing...')

const toggleEditMode = () => {
  isEditMode.value = !isEditMode.value
}

const fetchDocument = async () => {
  try {
    isProcessing.value = true
    const response = await axios.get(`/api/documents/${documentId}`)
    document.value = response.data
    
    // Initialize showFullDescription for each image
    if (response.data.images) {
      document.value.images = response.data.images.map(img => ({
        ...img,
        showFullDescription: false
      }))
    }
  } catch (error) {
    console.error('Error fetching document:', error)
  } finally {
    isProcessing.value = false
  }
}

const extractImages = async () => {
  isProcessing.value = true
  processingMessage.value = 'Extracting images from document...'
  
  try {
    await axios.post(`/api/images/${documentId}/extract`)
    await fetchDocument()
  } catch (error) {
    console.error('Error during image extraction:', error)
  } finally {
    isProcessing.value = false
  }
}

const analyzeDocument = async () => {
  if (!document.value.imagesExtracted) {
    alert('Please extract images first')
    return
  }

  isProcessing.value = true
  processingMessage.value = 'Analyzing document content...'
  
  try {
    await axios.post(`/api/documents/${documentId}/analyze`)
    await fetchDocument()
  } catch (error) {
    console.error('Error during document analysis:', error)
  } finally {
    isProcessing.value = false
  }
}

const extractText = async () => {
  isProcessing.value = true
  processingMessage.value = 'Extracting text from document...'

  try {
    await axios.post(`/api/documents/${documentId}/extract-text`)
    await fetchDocument()
  } catch (error) {
    console.error('Error extracting text:', error)
  } finally {
    isProcessing.value = false
  }
}

const copyText = async () => {
  try {
    await navigator.clipboard.writeText(document.value.text)
    alert('Text copied to clipboard! ✅')
  } catch (err) {
    alert('❌ Failed to copy text')
  }
}

const updateDocument = async () => {
  try {
    await axios.put(`/api/documents/${documentId}`, {
      name: document.value.name,
      description: document.value.description
    })
    alert('Document updated successfully ✅')
    isEditMode.value = false
  } catch (error) {
    console.error('Error updating document:', error)
    alert('❌ Update failed: ' + error.response?.data?.message || error.message)
  }
}

const formatDate = (dateString) => {
  if (!dateString) return 'Not available'
  const date = new Date(dateString)
  return date.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
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
/* Container */
.container {
  max-width: 1400px;
  margin: 30px auto;
  padding: 20px;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  background-color: #f8f9fa;
  border-radius: 12px;
}

/* Loading State */
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

/* Document Header */
.document-header {
  margin-bottom: 25px;
}

.header-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
  flex-wrap: wrap;
  gap: 15px;
}

.document-header h1 {
  color: #2c3e50;
  font-size: 2rem;
  margin-bottom: 0;
}

.mode-buttons {
  display: flex;
  gap: 10px;
}

.mode-btn {
  padding: 8px 16px;
  border: 1px solid #ddd;
  border-radius: 20px;
  background-color: white;
  color: #555;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 14px;
  transition: all 0.2s;
}

.mode-btn:hover:not(.disabled) {
  background-color: #f0f0f0;
}

.mode-btn.active {
  background-color: #2196F3;
  color: white;
  border-color: #2196F3;
}

.mode-btn.disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.metadata {
  display: flex;
  gap: 20px;
  margin-bottom: 20px;
  color: #7f8c8d;
  font-size: 0.9rem;
}

.metadata-item {
  display: flex;
  align-items: center;
  gap: 5px;
}

/* Stats Cards */
.stats-container {
  display: flex;
  gap: 20px;
  margin-bottom: 30px;
}

.stat-card {
  flex: 1;
  background: white;
  padding: 15px;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
}

.stat-card h3 {
  color: #7f8c8d;
  font-size: 0.9rem;
  margin-bottom: 10px;
}

.stat-value {
  font-size: 1.8rem;
  font-weight: bold;
  color: #2c3e50;
}

/* Status Badge */
.status-badge {
  display: inline-block;
  padding: 5px 10px;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 600;
  margin-right: 8px;
}

.status-badge.extracted {
  background-color: #d4edda;
  color: #155724;
}

.status-badge.pending {
  background-color: #fff3cd;
  color: #856404;
}

.status-badge.analyzed {
  background-color: #cce5ff;
  color: #004085;
}

.status-badge.pending-analysis {
  background-color: #f8d7da;
  color: #721c24;
}

/* Content Layout */
.content-layout {
  display: flex;
  gap: 30px;
  margin-bottom: 30px;
}

.document-viewer {
  flex: 2;
  min-width: 0;
}

/* Document Viewer */
.document-container {
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  height: 500px;
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

/* Content Sections */
.content-section {
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
  margin-bottom: 20px;
}

.section-title {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 15px;
}

.section-title h2 {
  color: #34495e;
  font-size: 1.5rem;
  margin: 0;
  flex-grow: 1;
}

/* Action Buttons */
.action-btn {
  background-color: #2196F3;
  color: white;
  padding: 12px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  transition: background-color 0.3s;
}

.action-btn:hover:not(.disabled) {
  background-color: #0b7dda;
}

.action-btn.disabled {
  background-color: #9E9E9E;
  cursor: not-allowed;
}

.copy-btn {
  background-color: #6c757d;
  color: white;
  padding: 8px 15px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 14px;
}

.copy-btn:hover {
  background-color: #5a6268;
}

/* Text Content */
.text-content {
  background: #f5f5f5;
  padding: 15px;
  border-radius: 6px;
  font-family: 'Courier New', Courier, monospace;
  white-space: pre-wrap;
  line-height: 1.5;
  max-height: 300px;
  overflow-y: auto;
}

/* New styles for the image-detail layout */
.images-container {
  display: flex;
  flex-direction: column;
  gap: 30px;
}

.image-detail {
  display: flex;
  gap: 20px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.image-wrapper {
  flex: 1;
  min-width: 0;
  position: relative;
}

.detail-image {
  width: 100%;
  height: 100%;
  object-fit: contain;
  max-height: 400px;
  display: block;
}

.description-wrapper {
  flex: 1;
  min-width: 0;
  padding: 20px;
  display: flex;
  flex-direction: column;
}

.image-description {
  margin-bottom: 15px;
}

.image-description h3 {
  margin-top: 0;
  color: #2c3e50;
}

.image-description p {
  margin: 0 0 15px 0;
  color: #495057;
  line-height: 1.6;
}

.detected-objects {
  margin-top: 20px;
}

.detected-objects h4 {
  margin: 0 0 10px 0;
  color: #2c3e50;
  font-size: 1.1rem;
}

.object-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.object-tag {
  background-color: #e0f7fa;
  color: #006064;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 0.85rem;
  font-weight: 500;
}

.toggle-description-btn {
  background: none;
  border: none;
  color: #2196F3;
  cursor: pointer;
  padding: 0;
  margin-left: 5px;
  font-size: 0.9rem;
}

.toggle-description-btn:hover {
  text-decoration: underline;
}

.image-actions {
  position: absolute;
  top: 10px;
  right: 10px;
  display: flex;
  gap: 8px;
}

.zoom-btn, .compare-btn {
  width: 32px;
  height: 32px;
  background-color: rgba(255, 255, 255, 0.9);
  border: none;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.2);
  transition: transform 0.2s;
}

.zoom-btn:hover, .compare-btn:hover {
  transform: scale(1.1);
}

.compare-view {
  margin-top: 20px;
}

.compare-images {
  display: flex;
  gap: 15px;
}

.compare-image {
  flex: 1;
}

.compare-image p {
  font-size: 12px;
  text-align: center;
  margin: 5px 0;
  color: #666;
}

.compare-image img {
  width: 100%;
  height: 120px;
  object-fit: cover;
  border-radius: 4px;
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
}

.modal-content {
  position: relative;
  max-width: 90%;
  max-height: 90vh;
}

.modal-content img {
  max-width: 100%;
  max-height: 90vh;
  object-fit: contain;
  border-radius: 8px;
}

.close-btn {
  position: absolute;
  top: -40px;
  right: 0;
  background-color: white;
  border: none;
  border-radius: 50%;
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}

/* Divider */
.divider {
  height: 1px;
  background-color: #eaeaea;
  margin: 20px 0;
}

/* Edit Popup Styles */
.edit-popup-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.edit-popup {
  background-color: white;
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
  width: 90%;
  max-width: 500px;
  max-height: 90vh;
  overflow-y: auto;
  animation: popupFadeIn 0.3s ease-out;
}

@keyframes popupFadeIn {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.popup-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid #eee;
}

.popup-header h3 {
  margin: 0;
  color: #2c3e50;
}

.close-popup-btn {
  background: none;
  border: none;
  font-size: 1.2rem;
  cursor: pointer;
  color: #7f8c8d;
  padding: 5px;
}

.close-popup-btn:hover {
  color: #2c3e50;
}

.popup-content {
  padding: 20px;
}

.popup-buttons {
  display: flex;
  gap: 10px;
  margin-top: 20px;
}

.cancel-btn {
  background-color: #f5f5f5;
  color: #555;
  padding: 12px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 16px;
  flex: 1;
  transition: background-color 0.3s;
}

.cancel-btn:hover {
  background-color: #e0e0e0;
}

.update-btn {
  background-color: #4CAF50;
  color: white;
  padding: 12px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 16px;
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  transition: background-color 0.3s;
}

.update-btn:hover {
  background-color: #3e8e41;
}

.form-group {
  margin-bottom: 15px;
}

.form-group label {
  display: block;
  margin-bottom: 5px;
  font-weight: 500;
  color: #495057;
}

.form-input {
  width: 100%;
  padding: 10px;
  border: 1px solid #ced4da;
  border-radius: 4px;
  font-size: 14px;
}

.form-textarea {
  width: 100%;
  padding: 10px;
  border: 1px solid #ced4da;
  border-radius: 4px;
  font-size: 14px;
  min-height: 100px;
  resize: vertical;
}

/* Responsive Design */
@media (max-width: 992px) {
  .content-layout {
    flex-direction: column;
  }
  
  .document-container {
    height: 400px;
  }
  
  .stats-container {
    flex-direction: column;
  }
}

@media (max-width: 768px) {
  .container {
    padding: 15px;
  }
  
  .metadata {
    flex-wrap: wrap;
  }
  
  .modal-content .close-btn {
    top: -30px;
    right: 0;
  }
  
  .header-top {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .mode-buttons {
    width: 100%;
    justify-content: space-between;
  }
  
  .mode-btn {
    flex: 1;
    text-align: center;
  }
  
  .edit-popup {
    width: 95%;
  }

  .image-detail {
    flex-direction: column;
  }
  
  .image-wrapper, .description-wrapper {
    flex: none;
    width: 100%;
  }
  
  .detail-image {
    max-height: 300px;
  }

  .compare-images {
    flex-direction: column;
  }
}
</style>