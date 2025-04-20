<template>
  <div class="documents-container">
    <!-- Search and Filter Bar -->
    <div class="search-container">
      <div class="search-input">
        <i class="bi bi-search"></i>
        <input 
          type="text" 
          v-model="searchQuery" 
          placeholder="Rechercher des documents..." 
          @input="filterDocuments"
        />
        <button v-if="searchQuery" @click="clearSearch" class="clear-btn">
          <i class="bi bi-x"></i>
        </button>
      </div>
      <div class="filter-buttons">
        <button 
          :class="['filter-btn', currentFilter === 'all' ? 'active' : '']"
          @click="setFilter('all')"
        >
          <i class="bi bi-files"></i>
          Tous
        </button>
        <button 
          :class="['filter-btn', currentFilter === 'traiter' ? 'active' : '']"
          @click="setFilter('traiter')"
        >
          <i class="bi bi-check-circle"></i>
          Traité
        </button>
        <button 
          :class="['filter-btn', currentFilter === 'non-traiter' ? 'active' : '']"
          @click="setFilter('non-traiter')"
        >
          <i class="bi bi-x-circle"></i>
          Non Traité
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading-container">
      <div class="spinner"></div>
      <p>Chargement des documents...</p>
    </div>

    <!-- Documents Grid -->
    <div v-else class="documents-grid">
      <!-- Add Document Card -->
      <div class="document-card add-document-card " @click="showAddModal = true">
        <div class="document-content add-document-card-content">
          <div class="document-icon">
            <i class="bi bi-plus-circle add-icon"></i>
          </div>
          <div class="document-info">
            <h3 class="document-title ">Ajouter un nouveau document</h3>
          </div>
        </div>
      </div>

      <!-- Document Cards -->
      <div v-for="doc in filteredDocuments" :key="doc.id" class="document-card">
        <!-- Status Badge -->
        <span :class="['status-badge', doc.istraiter ? 'traiter' : 'non-traiter']">
          <i :class="doc.istraiter ? 'bi bi-check-circle' : 'bi bi-x-circle'"></i>
        </span>
        <div class="document-content">
          <div class="document-icon">
            <i class="bi bi-file-earmark-text"></i>
          </div>
          <div class="document-info">
            <h3 class="document-title">{{ doc.name }}</h3>
            <p class="document-description" :title="doc.description">{{ doc.description }}</p>
            <p class="document-date">{{ formatDate(doc.uploadDate) }}</p>
          </div>
        </div>
        <div class="document-actions">
          <button class="action-btn view-btn" @click="navigateToContentDocument(doc)">
            <i class="bi bi-eye"></i>
          </button>
          <button class="action-btn download-btn" @click="downloadDocument(doc)">
            <i class="bi bi-download"></i>
          </button>
          <button class="action-btn delete-btn" @click="deleteDocument(doc.id)">
            <i class="bi bi-trash"></i>
          </button>
        </div>
      </div>
    </div>

    <!-- Add Document Modal -->
    <div v-if="showAddModal" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>Ajouter un nouveau document</h2>
          <button class="close-btn" @click="closeModal">
            <i class="bi bi-x"></i>
          </button>
        </div>
        
        <div class="modal-body">
          <!-- Document File Upload (Always visible) -->
          <div class="form-group">
            <label>Fichier PDF</label>
            <div 
              class="file-upload" 
              @dragover.prevent="handleDragOver"
              @dragleave.prevent="handleDragLeave"
              @drop.prevent="handleDrop"
              :class="{ 'dragging': isDragging }"
              @click="triggerFileInput"
            >
              <input 
                type="file" 
                @change="handleFileChange"
                class="file-input"
                accept=".pdf"
                ref="fileInput"
              />
              <div class="upload-placeholder" v-if="!selectedFile">
                <i class="bi bi-cloud-upload"></i>
                <p>Faites glisser votre fichier PDF ici ou cliquez pour parcourir</p>
                <button class="browse-btn">Parcourir les fichiers</button>
              </div>
              <div class="file-preview" v-else>
                <span class="file-name">{{ selectedFile.name }}</span>
                <button class="remove-file" @click.stop="removeFile">×</button>
              </div>
            </div>
          </div>

          <!-- Document Name and Description (Only visible after file selection) -->
          <div v-if="selectedFile" class="form-fields">
            <div class="form-group">
              <label>Nom du document</label>
              <input 
                type="text" 
                v-model="newDocument.name"
                class="form-input"
                placeholder="Entrez le nom du document"
              />
            </div>
            
            <div class="form-group">
              <label>Description</label>
              <textarea 
                v-model="newDocument.description"
                class="form-textarea"
                placeholder="Entrez la description du document"
                rows="3"
              ></textarea>
            </div>
          </div>
        </div>
        
        <div class="modal-footer">
          <button class="cancel-btn" @click="closeModal">Annuler</button>
          <button 
            class="submit-btn" 
            @click="uploadDocument"
            :disabled="!isFormValid || uploading"
          >
            <span v-if="uploading" class="spinner"></span>
            <span v-else>Uploader le document</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import Cookies from 'js-cookie';

export default {
  name: 'ListDocument',
  data() {
    return {
      documents: [],
      loading: true,
      searchQuery: '',
      showAddModal: false,
      isDragging: false,
      selectedFile: null,
      uploading: false,
      currentFilter: 'all',
      newDocument: {
        name: '',
        description: ''
      }
    };
  },
  computed: {
    filteredDocuments() {
      let filtered = this.documents;
      
      // Apply search filter
      if (this.searchQuery) {
        const query = this.searchQuery.toLowerCase();
        filtered = filtered.filter(doc => {
          const name = doc.name ? doc.name.toLowerCase() : '';
          const description = doc.description ? doc.description.toLowerCase() : '';
          return name.includes(query) || description.includes(query);
        });
      }
      
      // Apply status filter
      if (this.currentFilter === 'traiter') {
        filtered = filtered.filter(doc => doc.istraiter);
      } else if (this.currentFilter === 'non-traiter') {
        filtered = filtered.filter(doc => !doc.istraiter);
      }
      
      return filtered;
    },
    isFormValid() {
      return this.newDocument.name.trim() !== '' && 
             this.newDocument.description.trim() !== '' && 
             this.selectedFile;
    }
  },
  methods: {
    setFilter(filter) {
      this.currentFilter = filter;
    },
    async fetchDocuments() {
      this.loading = true;
      try {
        const token = Cookies.get('token');
        const response = await axios.get('api/documents', {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });
        this.documents = response.data;
      } catch (error) {
        console.error('Erreur lors du chargement des documents:', error);
        alert('Erreur lors du chargement des documents. Veuillez réessayer.');
      } finally {
        this.loading = false;
      }
    },
    formatDate(dateString) {
      const date = new Date(dateString);
      return date.toLocaleDateString('fr-FR', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
      });
    },
    filterDocuments() {
      // Triggered by search input; filtering handled by filteredDocuments
    },
    clearSearch() {
      this.searchQuery = '';
    },
    handleDragOver() {
      this.isDragging = true;
    },
    handleDragLeave() {
      this.isDragging = false;
    },
    handleDrop(event) {
      this.isDragging = false;
      const file = event.dataTransfer.files[0];
      if (file) {
        this.handleFile(file);
      }
    },
    handleFileChange(event) {
      const file = event.target.files[0];
      if (file) {
        this.handleFile(file);
      }
    },
    handleFile(file) {
      if (file.type !== 'application/pdf') {
        alert('Seuls les fichiers PDF sont autorisés.');
        return;
      }
      this.selectedFile = file;
      this.newDocument.name = file.name.replace(/\.[^/.]+$/, "");
    },
    removeFile() {
      this.selectedFile = null;
      this.newDocument.name = '';
      this.newDocument.description = '';
      if (this.$refs.fileInput) {
        this.$refs.fileInput.value = '';
      }
    },
    closeModal() {
      this.showAddModal = false;
      this.resetForm();
    },
    resetForm() {
      this.newDocument = {
        name: '',
        description: ''
      };
      this.selectedFile = null;
      if (this.$refs.fileInput) {
        this.$refs.fileInput.value = '';
      }
      this.isDragging = false;
    },
    triggerFileInput() {
      this.$refs.fileInput.click();
    },
    async uploadDocument() {
      if (!this.isFormValid) return;

      this.uploading = true;

      try {
        const token = Cookies.get('token');
        if (!token) {
          throw new Error('Jeton d’authentification non trouvé. Veuillez vous reconnecter.');
        }

        const formData = new FormData();
        if (this.selectedFile) {
          formData.append('file', this.selectedFile);
        }
        formData.append('description', this.newDocument.description.trim());
        formData.append('name', this.newDocument.name.trim());

        const response = await axios.post('api/documents/add', formData, {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'multipart/form-data',
            'Accept': 'application/json'
          }
        });

        this.showSuccessAlert('Document uploadé avec succès !');
        this.documents.unshift(response.data);
        this.closeModal();
      } catch (error) {
        if (error.response && error.response.data) {
          const errorData = error.response.data;
          if (errorData.errors) {
            const errorMessages = Object.entries(errorData.errors)
              .map(([field, messages]) => `${field}: ${messages.join(', ')}`)
              .join('\n');
            this.showErrorAlert(`Erreurs de validation:\n${errorMessages}`);
          } else {
            this.showErrorAlert(`Erreur lors de l’upload: ${errorData.title || errorData}`);
          }
        } else if (error.message) {
          this.showErrorAlert(error.message);
        } else {
          this.showErrorAlert('Erreur lors de l’upload du document. Veuillez réessayer.');
        }
      } finally {
        this.uploading = false;
      }
    },
    showSuccessAlert(message) {
      const alertDiv = document.createElement('div');
      alertDiv.className = 'custom-alert success';
      alertDiv.innerHTML = `
        <i class="bi bi-check-circle"></i>
        <span>${message}</span>
      `;
      document.body.appendChild(alertDiv);
      setTimeout(() => {
        alertDiv.classList.add('show');
        setTimeout(() => {
          alertDiv.classList.remove('show');
          setTimeout(() => {
            document.body.removeChild(alertDiv);
          }, 300);
        }, 3000);
      }, 100);
    },
    showErrorAlert(message) {
      const alertDiv = document.createElement('div');
      alertDiv.className = 'custom-alert error';
      alertDiv.innerHTML = `
        <i class="bi bi-exclamation-circle"></i>
        <span>${message}</span>
      `;
      document.body.appendChild(alertDiv);
      setTimeout(() => {
        alertDiv.classList.add('show');
        setTimeout(() => {
          alertDiv.classList.remove('show');
          setTimeout(() => {
            document.body.removeChild(alertDiv);
          }, 300);
        }, 3000);
      }, 100);
    },
    navigateToContentDocument(doc) {
      this.$router.push({ name: 'ContentDocument', params: { id: doc.id } });
    },
    async downloadDocument(doc) {
      try {
        const token = Cookies.get('token');
        const response = await axios({
          url: doc.fileUrl,
          method: 'GET',
          responseType: 'blob',
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });

        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', doc.name);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
      } catch (error) {
        console.error('Erreur lors du téléchargement:', error);
        alert('Erreur lors du téléchargement du document. Veuillez réessayer.');
      }
    },
    async deleteDocument(documentId) {
      if (!confirm("Voulez-vous vraiment supprimer ce document ?")) return;
      try {
        const token = Cookies.get('token');
        await axios.delete(`api/documents/${documentId}`, {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });
        this.documents = this.documents.filter(doc => doc.id !== documentId);
        this.showSuccessAlert('Document supprimé avec succès !');
      } catch (error) {
        console.error('Erreur lors de la suppression:', error);
        alert('Erreur lors de la suppression du document. Veuillez réessayer.');
      }
    }
  },
  mounted() {
    this.fetchDocuments();
  }
};
</script>

<style scoped>
.documents-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

.search-container {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 2rem;
  flex-wrap: wrap;
}

.search-input {
  position: relative;
  flex: 1;
  min-width: 250px;
  max-width: 500px;
}

.search-input input {
  width: 100%;
  padding: 0.75rem 1rem 0.75rem 2.5rem;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-size: 1rem;
  transition: all 0.3s ease;
}

.search-input input:focus {
  outline: none;
  border-color: #0d6efd;
  box-shadow: 0 0 0 3px rgba(13, 110, 253, 0.1);
}

.search-input i {
  position: absolute;
  left: 1rem;
  top: 50%;
  transform: translateY(-50%);
  color: #6c757d;
}

.clear-btn {
  position: absolute;
  right: 1rem;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: #6c757d;
  cursor: pointer;
  font-size: 1.1rem;
}

.filter-buttons {
  display: flex;
  gap: 0.5rem;
}

.filter-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border: 1px solid #ddd;
  border-radius: 8px;
  background: white;
  color: #666;
  font-size: 0.9rem;
  cursor: pointer;
  transition: all 0.3s ease;
}

.filter-btn i {
  font-size: 1rem;
}

.filter-btn:hover {
  background: #f8f9fa;
  border-color: #0d6efd;
  color: #0d6efd;
}

.filter-btn.active {
  background: #0d6efd;
  border-color: #0d6efd;
  color: white;
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem;
  color: #6c757d;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid rgba(0, 0, 0, 0.1);
  border-radius: 50%;
  border-top-color: #0d6efd;
  animation: spin 1s ease-in-out infinite;
  margin-bottom: 1rem;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.documents-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

.document-card {
  position: relative;
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  overflow: hidden;
  transition: all 0.3s ease;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.document-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
}

.add-document-card {
  cursor: pointer;
  border: 2px dashed #dee2e6;
  background-color: #f8f9fa;
}

.add-document-card:hover {
  border-color: #0d6efd;
  background-color: #f0f7ff;
}
.add-document-card-content{
  margin-top: 50px;
}
.add-icon {
  color: #0d6efd;
  font-size: 2rem;
  margin-top: -10px;
}

.document-content {
  padding: 1.5rem;
  display: flex;
  gap: 1rem;
}

.document-icon {
  font-size: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.document-info {
  flex: 1;
}

.document-title {
  margin: 0 0 0.5rem 0;
  font-size: 1.1rem;
  font-weight: 600;
  color: #2c3e50;
  word-break: break-word;
}

.document-description {
  margin: 0 0 0.75rem 0;
  font-size: 0.9rem;
  color: #6c757d;
  display: -webkit-box;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
  max-height: 2.2em;
  line-height: 1.35;
  position: relative;
}

.document-description::after {
  content: '';
  position: absolute;
  bottom: 0;
  right: 0;
  width: 40px;
  height: 1.35em;
  background: linear-gradient(to right, transparent, white);
  pointer-events: none;
}

.document-date {
  font-size: 0.8rem;
  color: #adb5bd;
  margin: 0;
}

.document-actions {
  display: flex;
  border-top: 1px solid #f0f0f0;
  padding: 0.75rem;
  gap: 0.5rem;
}

.action-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0.5rem;
  border: none;
  border-radius: 6px;
  background: none;
  color: #6c757d;
  cursor: pointer;
  transition: all 0.2s ease;
}

.action-btn:hover {
  background-color: #f8f9fa;
}

.view-btn:hover {
  color: #0d6efd;
}

.download-btn:hover {
  color: #198754;
}

.delete-btn:hover {
  color: #dc3545;
}

/* Status Badge */
.status-badge {
  position: absolute;
  top: 1rem;
  right: 1rem;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.875rem;
  font-weight: 500;
  z-index: 1;
}

.status-badge.traiter {
  background-color: #d4edda;
  color: #155724;
}

.status-badge.non-traiter {
  background-color: #f8d7da;
  color: #721c24;
}

.status-badge i {
  font-size: 1rem;
}

/* Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  border-radius: 12px;
  width: 90%;
  max-width: 600px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
  animation: modalFadeIn 0.3s ease;
}

@keyframes modalFadeIn {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1.5rem;
  border-bottom: 1px solid #f0f0f0;
}

.modal-header h2 {
  margin: 0;
  font-size: 1.5rem;
  color: #2c3e50;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.5rem;
  color: #6c757d;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
}

.close-btn:hover {
  background-color: #f8f9fa;
  color: #dc3545;
}

.modal-body {
  padding: 1.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: #2c3e50;
}

.form-input,
.form-textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-size: 1rem;
}

.form-textarea {
  resize: vertical;
  min-height: 100px;
}

.file-upload {
  border: 2px dashed #ddd;
  border-radius: 8px;
  padding: 1.5rem;
  text-align: center;
  cursor: pointer;
  transition: all 0.3s ease;
}

.file-upload.dragging {
  border-color: #0d6efd;
  background-color: #f0f7ff;
}

.upload-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  color: #6c757d;
}

.upload-placeholder i {
  font-size: 2rem;
  color: #0d6efd;
}

.browse-btn {
  margin-top: 0.5rem;
  padding: 0.5rem 1rem;
  background-color: #0d6efd;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
  transition: background-color 0.2s;
}

.browse-btn:hover {
  background-color: #0b5ed7;
}

.file-input {
  display: none;
}

.file-preview {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: #f8f9fa;
  padding: 0.75rem;
  border-radius: 8px;
}

.file-name {
  font-size: 0.875rem;
  color: #2c3e50;
}

.remove-file {
  background: none;
  border: none;
  color: #dc3545;
  cursor: pointer;
  font-size: 1.25rem;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding: 1.5rem;
  border-top: 1px solid #f0f0f0;
}

.submit-btn,
.cancel-btn {
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  font-size: 1rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.submit-btn {
  background: #0d6efd;
  color: white;
  border: none;
}

.submit-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.cancel-btn {
  background: none;
  border: 1px solid #ddd;
  color: #666;
}

/* Form Fields Animation */
.form-fields {
  animation: fadeIn 0.3s ease;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Custom Alert Styles */
.custom-alert {
  position: fixed;
  top: 20px;
  right: 20px;
  padding: 1rem 1.5rem;
  border-radius: 8px;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  transform: translateX(120%);
  transition: transform 0.3s ease;
  z-index: 9999;
}

.custom-alert.show {
  transform: translateX(0);
}

.custom-alert.success {
  background-color: #d4edda;
  color: #155724;
  border: 1px solid #c3e6cb;
}

.custom-alert.error {
  background-color: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
}

.custom-alert i {
  font-size: 1.25rem;
}

.custom-alert span {
  font-size: 0.95rem;
  font-weight: 500;
}

@media (max-width: 768px) {
  .documents-container {
    padding: 1rem;
  }
  
  .search-container {
    flex-direction: column;
    align-items: stretch;
  }
  
  .search-input {
    max-width: none;
  }
  
  .filter-buttons {
    justify-content: center;
    flex-wrap: wrap;
  }
  
  .documents-grid {
    grid-template-columns: 1fr;
  }
  
  .modal-content {
    width: 95%;
  }
  
  .modal-footer {
    flex-direction: column;
  }
  
  .submit-btn,
  .cancel-btn {
    width: 100%;
    justify-content: center;
  }
}
</style>