<template>
  <div class="documents-container">
    <!-- Search Bar -->
    <div class="search-container">
      <div class="search-input">
        <i class="bi bi-search"></i>
        <input 
          type="text" 
          v-model="searchQuery" 
          placeholder="Search documents..." 
          @input="filterDocuments"
        />
        <button v-if="searchQuery" @click="clearSearch" class="clear-btn">
          <i class="bi bi-x"></i>
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading-container">
      <div class="spinner"></div>
      <p>Loading documents...</p>
    </div>

    <!-- Documents Grid -->
    <div v-else class="documents-grid">
      <!-- Add Document Card -->
      <div class="document-card add-document-card" @click="showAddModal = true">
        <div class="document-content">
          <div class="document-icon">
            <i class="bi bi-plus-circle add-icon"></i>
          </div>
          <div class="document-info">
            <h3 class="document-title">Add New Document</h3>
            <p class="document-description">Click to upload a new document</p>
          </div>
        </div>
      </div>

      <!-- Document Cards -->
      <div v-for="doc in filteredDocuments" :key="doc.id" class="document-card">
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
          <h2>Add New Document</h2>
          <button class="close-btn" @click="closeModal">
            <i class="bi bi-x"></i>
          </button>
        </div>
        
        <div class="modal-body">
          <div class="form-group">
            <label>Document Name</label>
            <input 
              type="text" 
              v-model="newDocument.name"
              class="form-input"
              placeholder="Enter document name"
            />
          </div>
          
          <div class="form-group">
            <label>Description</label>
            <textarea 
              v-model="newDocument.description"
              class="form-textarea"
              placeholder="Enter document description"
              rows="3"
            ></textarea>
          </div>

          <div class="form-group">
            <label>Document File (PDF)</label>
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
                <p>Drag and drop your PDF file here or click to browse</p>
                <button class="browse-btn">Browse Files</button>
              </div>
              <div class="file-preview" v-else>
                <span class="file-name">{{ selectedFile.name }}</span>
                <button class="remove-file" @click.stop="removeFile">×</button>
              </div>
            </div>
          </div>
        </div>
        
        <div class="modal-footer">
          <button class="cancel-btn" @click="closeModal">Cancel</button>
          <button 
            class="submit-btn" 
            @click="uploadDocument"
            :disabled="!isFormValid || uploading"
          >
            <span v-if="uploading" class="spinner"></span>
            <span v-else>Upload Document</span>
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
      newDocument: {
        name: '',
        description: ''
      }
    };
  },
  computed: {
    filteredDocuments() {
      if (!this.searchQuery) return this.documents;
      
      const query = this.searchQuery.toLowerCase();
      return this.documents.filter(doc => 
        doc.name.toLowerCase().includes(query) || 
        doc.description.toLowerCase().includes(query)
      );
    },
    isFormValid() {
      return this.newDocument.name && 
             this.newDocument.description && 
             this.selectedFile;
    }
  },
  methods: {
    async fetchDocuments() {
      this.loading = true;
      try {
        const response = await axios.get('api/documents');
        this.documents = response.data;
      } catch (error) {
        console.error('Error fetching documents:', error);
        alert('Error loading documents. Please try again.');
      } finally {
        this.loading = false;
      }
    },
    formatDate(dateString) {
      const date = new Date(dateString);
      return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
      });
    },
    filterDocuments() {
      // This method is triggered by the @input event on the search field
      // The actual filtering is done in the computed property
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
        alert('Only PDF files are allowed.');
        return;
      }
      this.selectedFile = file;
      if (!this.newDocument.name) {
        this.newDocument.name = file.name.replace(/\.[^/.]+$/, "");
      }
    },
    removeFile() {
      this.selectedFile = null;
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
      
      // Vérifier que la description n'est pas vide
      if (!this.newDocument.description || this.newDocument.description.trim() === '') {
        alert('Description is required.');
        this.uploading = false;
        return;
      }

      try {
        const token = Cookies.get('token');
        if (!token) {
          throw new Error('Authentication token not found. Please log in again.');
        }

        // Créer un nouveau FormData
        const formData = new FormData();
        
        // Ajouter le fichier
        if (this.selectedFile) {
          formData.append('file', this.selectedFile);
        }
        
        // Ajouter la description comme paramètre de requête
        // Le backend attend un paramètre nommé "description"
        formData.append('description', this.newDocument.description);
        
        // Ajouter le nom
        formData.append('name', this.newDocument.name);

        console.log('Uploading document with token:', token.substring(0, 10) + '...');
        console.log('FormData contents:', {
          name: this.newDocument.name,
          description: this.newDocument.description,
          file: this.selectedFile ? this.selectedFile.name : 'No file'
        });

        // Afficher le contenu du FormData pour débogage
        for (let pair of formData.entries()) {
          console.log(pair[0] + ': ' + pair[1]);
        }

        // Essayer une approche différente pour envoyer les données
        // Utiliser URLSearchParams pour les paramètres de requête
        const params = new URLSearchParams();
        params.append('description', this.newDocument.description);
        
        // Envoyer la requête avec les paramètres dans l'URL
        const response = await axios({
          method: 'post',
          url: `api/documents/add?${params.toString()}`,
          data: formData,
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'multipart/form-data'
          }
        });
        
        console.log('Document uploaded successfully:', response.data);
        alert('Document uploaded successfully!');
        
        this.documents.unshift(response.data);
        this.closeModal();
      } catch (error) {
        console.error('Error uploading document:', error);
        if (error.response && error.response.data) {
          alert(`Error uploading document: ${error.response.data}`);
        } else if (error.message) {
          alert(error.message);
        } else {
          alert('Error uploading document. Please try again.');
        }
      } finally {
        this.uploading = false;
      }
    },
    navigateToContentDocument(doc) {
      this.$router.push({ name: 'ContentDocument', params: { id: doc.id } });
    },
    async downloadDocument(doc) {
      try {
        // Vérifier si le document a une URL directe
        if (doc.fileUrl) {
          // Télécharger directement le fichier
          const response = await axios({
            url: doc.fileUrl,
            method: 'GET',
            responseType: 'blob'
          });
          
          // Créer un lien temporaire pour télécharger le fichier
          const url = window.URL.createObjectURL(new Blob([response.data]));
          const link = document.createElement('a');
          link.href = url;
          link.setAttribute('download', doc.name);
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
          window.URL.revokeObjectURL(url);
        } else {
          // Si pas d'URL directe, essayer de construire l'URL
          const baseUrl = 'https://localhost:7036';
          const fileUrl = `${baseUrl}/uploads/documents/${doc.id}/${doc.name}`;
          
          // Télécharger directement le fichier
          const response = await axios({
            url: fileUrl,
            method: 'GET',
            responseType: 'blob'
          });
          
          // Créer un lien temporaire pour télécharger le fichier
          const url = window.URL.createObjectURL(new Blob([response.data]));
          const link = document.createElement('a');
          link.href = url;
          link.setAttribute('download', doc.name);
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
          window.URL.revokeObjectURL(url);
        }
      } catch (error) {
        console.error('Error downloading document:', error);
        alert('Error downloading document. Please try again.');
      }
    },
    async deleteDocument(documentId) {
      if (!confirm("Are you sure you want to delete this document?")) return;
      try {
        await axios.delete(`api/documents/${documentId}`);
        this.fetchDocuments();
      } catch (error) {
        console.error('Error deleting document:', error);
        alert('Error deleting document. Please try again.');
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
  margin-bottom: 2rem;
}

.search-input {
  position: relative;
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

.add-icon {
  color: #0d6efd;
  font-size: 2rem;
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
  -webkit-line-clamp: 2;
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

@media (max-width: 768px) {
  .documents-container {
    padding: 1rem;
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
