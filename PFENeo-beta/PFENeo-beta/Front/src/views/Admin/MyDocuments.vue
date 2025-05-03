<template>
  <div class="admin-container">
    <div class="card">
      <div class="card-content">
        <div class="header">
          <h2 class="title">Mes Documents</h2>
          <div class="search-container">
            <div class="search-box">
              <input 
                type="text" 
                class="search-input" 
                placeholder="Rechercher des documents..." 
                v-model="searchQuery" 
              />
              <button class="search-button">
                <i class="bi bi-search"></i>
              </button>
            </div>
          </div>
          <button @click="showAddModal = true" class="button primary">
            <i class="bi bi-plus-lg"></i> Ajouter un document
          </button>
        </div>

        <!-- État de chargement -->
        <div v-if="loading" class="loading">
          <div class="spinner"></div>
          <p class="loading-text">Chargement des documents...</p>
        </div>

        <!-- Tableau des documents -->
        <div v-else class="table-wrapper">
          <table class="documents-table">
            <thead>
              <tr>
                <th>Document</th>
                <th>Description</th>
                <th>Date de téléversement</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="document in filteredDocuments" :key="document.id">
                <td>
                  <div class="document-name" @click="viewDocument(document)">
                    📄
                    <span>{{ document.name }}</span>
                  </div>
                </td>
                <td>
                  <div class="description">
                    <p>{{ document.description || 'Aucune description' }}</p>
                  </div>
                </td>
                <td>{{ formatDate(document.uploadDate) }}</td>
                <td>
                  <div class="action-buttons">
                    <button class="btn-view" @click="downloadDocument(document)" title="Télécharger">
                      <i class="bi bi-download"></i>
                    </button>
                    <button class="btn-delete" @click="deleteDocument(document.id)" title="Supprimer">
                      <i class="bi bi-trash"></i>
                    </button>
                    <button class="btn-view" @click="viewDocument(document)" title="Voir">
  <i class="bi bi-eye"></i>
</button>
                  </div>
                </td>
              </tr>
              <tr v-if="filteredDocuments.length === 0">
                <td colspan="4" class="empty-state">
                  <div class="empty-message">
                    <i class="bi bi-folder-plus"></i>
                    <span>Aucun document trouvé</span>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Modal Ajouter un document -->
    <div v-if="showAddModal" class="modal-overlay" @click="closeAddModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>Ajouter un nouveau document</h2>
          <button class="close-btn" @click="closeAddModal">❌</button>
        </div>
        
        <div class="modal-body">
          <div v-if="serverErrors.length" class="server-errors">
            <p v-for="(error, index) in serverErrors" :key="index" class="error-message">{{ error }}</p>
          </div>

          <div class="form-group">
            <label>Nom du document</label>
            <input 
              type="text" 
              v-model="newDocument.name"
              class="form-input"
              placeholder="Entrez le nom du document"
              :class="{ 'error': (formSubmitted && !newDocument.name) || serverErrors.some(e => e.toLowerCase().includes('name')) }"
            />
            <span v-if="formSubmitted && !newDocument.name" class="error-message">Le nom est requis</span>
          </div>
          
          <div class="form-group">
            <label>Description</label>
            <textarea 
              v-model="newDocument.description"
              class="form-textarea"
              placeholder="Entrez la description du document"
              rows="3"
              :class="{ 'error': (formSubmitted && !newDocument.description) || serverErrors.some(e => e.toLowerCase().includes('description')) }"
            ></textarea>
            <span v-if="formSubmitted && !newDocument.description" class="error-message">La description est requise</span>
          </div>

          <div class="form-group">
            <label>Fichier du document (PDF)</label>
            <div 
              class="file-upload" 
              @dragover.prevent="handleDragOver"
              @dragleave.prevent="handleDragLeave"
              @drop.prevent="handleDrop"
              :class="{ 'dragging': isDragging, 'error': (formSubmitted && !selectedFile) || serverErrors.some(e => e.toLowerCase().includes('file')) }"
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
                ⬆️
                <p>Glissez-déposez votre fichier PDF ici ou cliquez pour parcourir</p>
                <button class="browse-btn">Parcourir les fichiers</button>
              </div>
              <div class="file-preview" v-else>
                <span class="file-name">{{ selectedFile.name }}</span>
                <button class="remove-file" @click.stop="removeFile">×</button>
              </div>
            </div>
            <span v-if="formSubmitted && !selectedFile" class="error-message">Le fichier PDF est requis</span>
          </div>
        </div>
        
        <div class="modal-footer">
          <button class="cancel-btn" @click="closeAddModal">Annuler</button>
          <button 
            class="submit-btn" 
            @click="uploadDocument"
            :disabled="!isFormValid || uploading"
          >
            <span v-if="uploading" class="spinner"></span>
            <span v-else>Téléverser le document</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

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
.btn-view {
  background: #e3f2fd;
  color: #1976d2;
}

.btn-view:hover {
  background: #bbdefb;
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
  color: #1976d2;
  border-left: 1px solid #ddd;
  cursor: pointer;
}

.search-button:hover {
  background: #f5f5f5;
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

.documents-table {
  width: 100%;
  border-collapse: collapse;
  table-layout: auto;
}

.documents-table th,
.documents-table td {
  padding: 0.8rem;
  text-align: left;
  border-bottom: 1px solid #eee;
  vertical-align: middle;
  word-break: break-word;
}

.documents-table th {
  font-weight: 600;
  color: #2c3e50;
  background: #f8f9fa;
}

.documents-table td {
  max-width: 200px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.document-name {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.description {
  max-width: 250px;
}

.description p {
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
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

.form-input,
.form-textarea {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
}

.form-input.error,
.form-textarea.error,
.file-upload.error {
  border-color: #e74c3c;
}

.error-message {
  color: #e74c3c;
  font-size: 0.85rem;
  margin-top: 0.25rem;
  display: block;
}

.form-textarea {
  resize: vertical;
  min-height: 100px;
}

.file-upload {
  border: 2px dashed #ddd;
  border-radius: 4px;
  padding: 1.5rem;
  text-align: center;
  cursor: pointer;
  transition: all 0.3s;
}

.file-upload.dragging {
  border-color: #3498db;
  background: #f8f9fa;
}

.file-input {
  display: none;
}

.upload-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
}

.upload-placeholder i {
  font-size: 2rem;
  color: #666;
}

.browse-btn {
  margin-top: 0.5rem;
  padding: 0.5rem 1rem;
  background: #f8f9fa;
  border: 1px solid #ddd;
  border-radius: 4px;
  cursor: pointer;
}

.browse-btn:hover {
  background: #e8ecef;
}

.file-preview {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.5rem;
  background: #f8f9fa;
  border-radius: 4px;
}

.remove-file {
  background: none;
  border: none;
  font-size: 1.2rem;
  cursor: pointer;
  color: #e74c3c;
}

.action-buttons {
  display: flex;
  gap: 0.5rem;
}

.action-buttons button {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-view {
  background: #fff8e1;
  color: #ffa000;
}

.btn-view:hover {
  background: #ffecb3;
}

.btn-delete {
  background: #ffebee;
  color: #d32f2f;
}

.btn-delete:hover {
  background: #ffcdd2;
}

.cancel-btn,
.submit-btn,
.button {
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

.submit-btn,
.button.primary {
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
  .document-name {
  cursor: pointer;
}

.document-name:hover {
  text-decoration: underline;
  color: #1976d2;
}
  
  .search-container {
    width: 100%;
    max-width: none;
  }
  
  .documents-table th,
  .documents-table td {
    padding: 0.5rem;
    font-size: 0.9rem;
  }
  
  .description {
    max-width: 150px;
  }
  
  .modal-content {
    width: 95%;
  }
  
  .modal-actions {
    flex-direction: column;
    gap: 1rem;
  }
  
  .button {
    width: 100%;
    justify-content: center;
  }
}
</style>

<script>
import axios from 'axios'
import Cookies from 'js-cookie'

export default {
  name: 'MyDocuments',
  data() {
    return {
      documents: [],
      loading: true,
      searchQuery: '',
      showAddModal: false,
      newDocument: {
        name: '',
        description: ''
      },
      selectedFile: null,
      isDragging: false,
      uploading: false,
      formSubmitted: false,
      serverErrors: []
    }
  },
  computed: {
    filteredDocuments() {
      if (!this.searchQuery) return this.documents
      
      const query = this.searchQuery.toLowerCase()
      return this.documents.filter(doc => 
        doc.name.toLowerCase().includes(query) || 
        (doc.description && doc.description.toLowerCase().includes(query))
   ) },
    isFormValid() {
      return (
        this.newDocument.name.trim() &&
        this.newDocument.description.trim() &&
        this.selectedFile
      )
    }
  },
  methods: {
    async fetchMyDocuments() {
      this.loading = true
      try {
        const token = Cookies.get('token')
        const response = await axios.get('api/documents', {
          headers: { Authorization: `Bearer ${token}` }
        })
        this.documents = response.data
      } catch (error) {
        console.error('Erreur lors du chargement des documents:', error)
        this.showAlert('error', 'Erreur lors du chargement des documents')
      } finally {
        this.loading = false
      }
    },
    formatDate(dateString) {
      if (!dateString) return 'N/A'
      const date = new Date(dateString)
      return date.toLocaleDateString('fr-FR', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      })
    },
    async downloadDocument(doc) {
  try {
    const token = Cookies.get('token');
    const fileUrl = `api/documents/${doc.id}/file`; // Adaptez selon votre API
    
    // Option 1: Ouverture dans un nouvel onglet
    window.open(fileUrl, '_blank');
    
    // Option 2: Téléchargement forcé
    const link = document.createElement('a');
    link.href = fileUrl;
    link.download = `${doc.name}.pdf`;
    link.target = '_blank';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
  } catch (error) {
    console.error('Erreur:', error);
    this.showAlert('error', 'Échec du téléchargement');
  }
},
    async deleteDocument(documentId) {
      if (!confirm('Confirmez la suppression de ce document ?')) return
      
      try {
        const token = Cookies.get('token')
        await axios.delete(`api/documents/${documentId}`, {
          headers: { Authorization: `Bearer ${token}` }
        })
        
        this.documents = this.documents.filter(doc => doc.id !== documentId)
        this.showAlert('success', 'Document supprimé avec succès')
      } catch (error) {
        console.error('Erreur lors de la suppression:', error)
        this.showAlert('error', 'Échec de la suppression')
      }
    },
    triggerFileInput() {
      this.$refs.fileInput.click()
    },
    handleDragOver() {
      this.isDragging = true
    },
    handleDragLeave() {
      this.isDragging = false
    },
    handleDrop(e) {
      this.isDragging = false
      const file = e.dataTransfer.files[0]
      if (file && file.type === 'application/pdf') {
        this.selectedFile = file
      } else {
        alert('Veuillez sélectionner un fichier PDF valide')
      }
    },
    handleFileChange(e) {
      const file = e.target.files[0]
      if (file && file.type === 'application/pdf') {
        this.selectedFile = file
      } else {
        alert('Veuillez sélectionner un fichier PDF valide')
      }
    },
    removeFile() {
      this.selectedFile = null
      this.$refs.fileInput.value = ''
    },
    closeAddModal() {
      this.showAddModal = false
      this.newDocument = { name: '', description: '' }
      this.selectedFile = null
      this.isDragging = false
      this.formSubmitted = false
      this.serverErrors = []
    },
    async uploadDocument() {
      this.formSubmitted = true
      this.serverErrors = []
      
      if (!this.isFormValid) {
        alert('Veuillez remplir tous les champs obligatoires')
        return
      }

      this.uploading = true
      try {
        const token = Cookies.get('token')
        const formData = new FormData()
        formData.append('file', this.selectedFile)
        formData.append('name', this.newDocument.name)
        formData.append('description', this.newDocument.description)

        const response = await axios.post('api/documents/add', formData, {
          headers: {
            Authorization: `Bearer ${token}`,
            'Content-Type': 'multipart/form-data'
          }
        })

        this.documents.unshift(response.data)
        this.showAlert('success', 'Document ajouté avec succès')
        this.closeAddModal()
      } catch (error) {
        console.error('Erreur lors de l\'upload:', error)
        if (error.response?.data?.errors) {
          this.serverErrors = Object.values(error.response.data.errors).flat()
        } else {
          this.serverErrors = [error.response?.data?.message || 'Erreur lors de l\'upload']
        }
      } finally {
        this.uploading = false
      }
    },
    viewDocument(document) {
  this.$router.push({ name: 'ConsultDocument', params: { id: document.id } });
},
    showAlert(type, message) {
      const alertDiv = document.createElement('div')
      alertDiv.className = `custom-alert ${type}`
      alertDiv.innerHTML = `
        <i class="bi ${type === 'success' ? 'bi-check-circle' : 'bi-exclamation-circle'}"></i>
        <span>${message}</span>
      `
      document.body.appendChild(alertDiv)
      
      setTimeout(() => {
        alertDiv.classList.add('show')
        setTimeout(() => {
          alertDiv.classList.remove('show')
          setTimeout(() => document.body.removeChild(alertDiv), 300)
        }, 3000)
      }, 100)
    }
  },
  mounted() {
    this.fetchMyDocuments()
  }
}
</script>
  