<template>
  <div class="admin-container">
    <div class="card">
      <div class="card-content">
        <div class="header">
          <h2 class="title">Documents</h2>
          <div class="search-container">
            <div class="search-box">
              <input type="text" class="search-input" placeholder="Rechercher des documents..." v-model="searchQuery" />
              <button class="search-button">
                🔍
              </button>
            </div>
          </div>
          <button @click="showAddModal = true" class="button primary">
            ➕ Ajouter un document
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
                <th>Utilisateur</th>
                <th>Description</th>
                <th>Date de téléversement</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="document in filteredDocuments" :key="document.id">
                <td>
                  <div class="document-name">
                    📄
                    <span>{{ document.name }}</span>
                  </div>
                </td>
                <td>
                  <div v-if="document.user" class="user-info">
                    {{ document.user.userName || document.user.email }}
                  </div>
                  <div v-else class="loading-info">Chargement de l'utilisateur...</div>
                </td>
                <td>
                  <div class="description">
                    <p>{{ document.description || 'Aucune description' }}</p>
                  </div>
                </td>
                <td>{{ formatDate(document.uploadDate) }}</td>
                <td>
                  <div class="actions">
                    <button class="action-button view" @click="viewDocument(document)" title="Voir">
                      👁️
                    </button>
                    <button class="action-button edit" @click="editDocument(document)" title="Modifier">
                      ✏️
                    </button>
                    <button class="action-button delete" @click="deleteDocument(document)" title="Supprimer">
                      🗑️
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="filteredDocuments.length === 0">
                <td colspan="5" class="empty-state">
                  <div class="empty-message">
                    📂
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
          <button class="close-btn" @click="closeAddModal">
            ❌
          </button>
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
            <label>Sélectionner un utilisateur</label>
            <select 
              v-model="newDocument.userId"
              class="form-input"
              :class="{ 'error': (formSubmitted && !newDocument.userId) || serverErrors.some(e => e.toLowerCase().includes('userid')) }"
            >
              <option value="" disabled>Sélectionnez un utilisateur</option>
              <option v-for="user in nonAdminUsers" :key="user.id" :value="user.id">
                {{ user.userName || user.email }}
              </option>
            </select>
            <span v-if="formSubmitted && !newDocument.userId" class="error-message">L'utilisateur est requis</span>
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

    <!-- Modal Modifier/Voir le document -->
    <div v-if="selectedDocument" class="modal-overlay" @click="closeDocumentModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>{{ isEditing ? 'Modifier le document' : 'Détails du document' }}</h2>
          <button class="close-btn" @click="closeDocumentModal">
            ❌
          </button>
        </div>
        
        <div class="modal-body">
          <div v-if="serverErrors.length && isEditing" class="server-errors">
            <p v-for="(error, index) in serverErrors" :key="index" class="error-message">{{ error }}</p>
          </div>

          <div class="document-header">
            <h3 class="document-title">
              📄
              <span v-if="!isEditing">{{ selectedDocument.name }}</span>
              <input 
                v-else 
                v-model="selectedDocument.name" 
                class="input"
                placeholder="Nom du document"
                :class="{ 'error': (formSubmitted && !selectedDocument.name) || serverErrors.some(e => e.toLowerCase().includes('name')) }"
              />
            </h3>
          </div>

          <div class="form-group">
            <label>Description</label>
            <p v-if="!isEditing">{{ selectedDocument.description || 'Aucune description' }}</p>
            <textarea 
              v-else
              v-model="selectedDocument.description"
              class="textarea"
              placeholder="Entrez la description"
              :class="{ 'error': (formSubmitted && !selectedDocument.description) || serverErrors.some(e => e.toLowerCase().includes('description')) }"
            ></textarea>
          </div>

          <div class="form-group" v-if="isEditing">
            <label>Sélectionner un utilisateur</label>
            <select 
              v-model="selectedDocument.userId"
              class="form-input"
              :class="{ 'error': (formSubmitted && !selectedDocument.userId) || serverErrors.some(e => e.toLowerCase().includes('userid')) }"
            >
              <option value="" disabled>Sélectionnez un utilisateur</option>
              <option v-for="user in nonAdminUsers" :key="user.id" :value="user.id">
                {{ user.userName || user.email }}
              </option>
            </select>
            <span v-if="formSubmitted && !selectedDocument.userId" class="error-message">L'utilisateur est requis</span>
          </div>

          <div class="form-group">
            <label>Date de téléversement</label>
            <p>{{ formatDate(selectedDocument.uploadDate) }}</p>
          </div>

          <div v-if="selectedDocument.user" class="form-group">
            <label>Téléversé par</label>
            <div class="user-details">
              <p>{{ selectedDocument.user.userName || selectedDocument.user.email }}</p>
            </div>
          </div>
        </div>
        
        <div class="modal-footer">
          <div class="modal-actions">
            <button 
              v-if="!isEditing"
              class="button secondary"
              @click="downloadDocument(selectedDocument)"
            >
              Télécharger
            </button>
            
            <div v-if="isEditing" class="edit-actions">
              <button 
                class="button secondary" 
                @click="cancelEdit"
              >
                Annuler
              </button>
              <button 
                class="button primary"
                @click="saveDocumentChanges"
              >
                Enregistrer les modifications
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import Cookies from 'js-cookie'

const router = useRouter()
const documents = ref([])
const loading = ref(true)
const searchQuery = ref('')
const selectedDocument = ref(null)
const isEditing = ref(false)
const documentBeforeEdit = ref(null)
const showAddModal = ref(false)
const newDocument = ref({
  name: '',
  description: '',
  userId: ''
})
const selectedFile = ref(null)
const isDragging = ref(false)
const uploading = ref(false)
const users = ref([])
const formSubmitted = ref(false)
const serverErrors = ref([])

// Computed property to filter non-admin users
const nonAdminUsers = computed(() => {
  return users.value.filter(user => 
    !user.roles?.some(r => r.toLowerCase() === 'admin')
  )
})

// Fetch all documents with their users
const fetchDocuments = async () => {
  loading.value = true
  try {
    const token = Cookies.get('token')
    const response = await axios.get('api/admin/documents', {
      headers: { Authorization: `Bearer ${token}` }
    })
    const documentsWithUsers = await Promise.all(
      response.data.map(async (doc) => {
        const user = await fetchDocumentUser(doc.id)
        return { ...doc, user }
      })
    )
    documents.value = documentsWithUsers
  } catch (error) {
    console.error('Erreur lors de la récupération des documents:', error)
    alert('Échec du chargement des documents. Veuillez réessayer.')
  } finally {
    loading.value = false
  }
}

// Fetch all users
const fetchUsers = async () => {
  try {
    const token = Cookies.get('token')
    const response = await axios.get('api/users/all', {
      headers: { Authorization: `Bearer ${token}` }
    })
    users.value = response.data
    console.log('Utilisateurs récupérés:', users.value)
  } catch (error) {
    console.error('Erreur lors de la récupération des utilisateurs:', error)
    alert('Échec du chargement des utilisateurs. Veuillez réessayer.')
  }
}

// Fetch document user
const fetchDocumentUser = async (documentId) => {
  try {
    const token = Cookies.get('token')
    const response = await axios.get(`api/admin/documents/${documentId}/user`, {
      headers: { Authorization: `Bearer ${token}` }
    })
    return response.data
  } catch (error) {
    console.error('Erreur lors de la récupération de l\'utilisateur du document:', error)
    return null
  }
}

// View document (redirect to fileUrl)
const viewDocument = (document) => {
  if (document.fileUrl) {
    window.open(document.fileUrl, '_blank')
  } else {
    alert('Lien du document non disponible.')
  }
}

// Edit document
const editDocument = (document) => {
  isEditing.value = true
  documentBeforeEdit.value = { ...document }
  selectedDocument.value = { ...document, userId: document.user?.id || '' }
  serverErrors.value = []
  formSubmitted.value = false
}

// Save document changes
const saveDocumentChanges = async () => {
  formSubmitted.value = true
  serverErrors.value = []

  if (!selectedDocument.value.name.trim() || !selectedDocument.value.description.trim() || !selectedDocument.value.userId) {
    alert('Veuillez remplir tous les champs requis.')
    return
  }

  try {
    const token = Cookies.get('token')
    const response = await axios.put(`api/admin/documents/${selectedDocument.value.id}`, 
      { 
        Name: selectedDocument.value.name.trim(),
        Description: selectedDocument.value.description.trim(),
        UserId: selectedDocument.value.userId
      },
      { headers: { Authorization: `Bearer ${token}` } }
    )
    
    if (response.data) {
      const index = documents.value.findIndex(d => d.id === selectedDocument.value.id)
      if (index !== -1) {
        const updatedUser = await fetchDocumentUser(response.data.id)
        documents.value[index] = { 
          ...response.data, 
          user: updatedUser
        }
      }
      alert('Document modifié avec succès !')
      isEditing.value = false
      closeDocumentModal()
    }
  } catch (error) {
    console.error('Erreur lors de la modification du document:', error)
    if (error.response?.data) {
      const errors = error.response.data
      if (errors.errors) {
        serverErrors.value = Object.entries(errors.errors)
          .map(([field, messages]) => `${field}: ${messages.join(', ')}`)
      } else if (errors.error) {
        serverErrors.value = [errors.error]
      } else {
        serverErrors.value = ['Une erreur inattendue s\'est produite.']
      }
    } else {
      serverErrors.value = ['Échec de la modification du document. Aucune réponse du serveur.']
    }
  }
}

// Cancel edit
const cancelEdit = () => {
  if (documentBeforeEdit.value) {
    selectedDocument.value = { ...documentBeforeEdit.value }
  }
  isEditing.value = false
  serverErrors.value = []
  formSubmitted.value = false
}

// Delete document
const deleteDocument = async (document) => {
  if (confirm(`Voulez-vous vraiment supprimer "${document.name}" ?`)) {
    try {
      const token = Cookies.get('token')
      await axios.delete(`api/admin/documents/${document.id}`, {
        headers: { Authorization: `Bearer ${token}` }
      })
      alert('Document supprimé avec succès !')
      await fetchDocuments()
    } catch (error) {
      console.error('Erreur lors de la suppression du document:', error)
      alert('Échec de la suppression du document. Veuillez réessayer.')
    }
  }
}

// Download document
const downloadDocument = async (document) => {
  try {
    const token = Cookies.get('token')
    const response = await axios.get(`api/documents/${document.id}/download`, {
      headers: { Authorization: `Bearer ${token}` },
      responseType: 'blob'
    })
    
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', document.name)
    document.body.appendChild(link)
    link.click()
    link.remove()
  } catch (error) {
    console.error('Erreur lors du téléchargement du document:', error)
    alert('Échec du téléchargement du document. Veuillez réessayer.')
  }
}

// Format date
const formatDate = (date) => {
  if (!date) return 'N/A'
  const uploadDate = new Date(date)
  return uploadDate.toLocaleDateString('fr-FR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// Filter documents based on search query
const filteredDocuments = computed(() => {
  if (!searchQuery.value) return documents.value
  
  const query = searchQuery.value.toLowerCase()
  return documents.value.filter(doc => 
    doc.name.toLowerCase().includes(query) || 
    (doc.description && doc.description.toLowerCase().includes(query))
  )
})

// Add Document Modal Logic
const isFormValid = computed(() => {
  return newDocument.value.name.trim() &&
         newDocument.value.description.trim() &&
         selectedFile.value &&
         newDocument.value.userId
})

const triggerFileInput = () => {
  document.querySelector('.file-input').click()
}

const handleDragOver = () => {
  isDragging.value = true
}

const handleDragLeave = () => {
  isDragging.value = false
}

const handleDrop = (event) => {
  isDragging.value = false
  const file = event.dataTransfer.files[0]
  if (file && file.type === 'application/pdf' && file.size <= 10 * 1024 * 1024) {
    selectedFile.value = file
  } else {
    alert('Veuillez téléverser un fichier PDF valide (max 10 Mo).')
  }
}

const handleFileChange = (event) => {
  const file = event.target.files[0]
  if (file && file.type === 'application/pdf' && file.size <= 10 * 1024 * 1024) {
    selectedFile.value = file
  } else {
    alert('Veuillez téléverser un fichier PDF valide (max 10 Mo).')
  }
}

const removeFile = () => {
  selectedFile.value = null
  document.querySelector('.file-input').value = ''
}

const uploadDocument = async () => {
  formSubmitted.value = true
  serverErrors.value = []

  if (!isFormValid.value) {
    console.warn('Échec de la validation du formulaire:', {
      name: newDocument.value.name,
      description: newDocument.value.description,
      userId: newDocument.value.userId,
      file: selectedFile.value
    })
    alert('Veuillez remplir tous les champs requis.')
    return
  }

  uploading.value = true
  try {
    const token = Cookies.get('token')
    const formData = new FormData()

    formData.append('File', selectedFile.value)
    formData.append('Name', newDocument.value.name.trim())
    formData.append('Description', newDocument.value.description.trim())
    formData.append('UserId', newDocument.value.userId)

    console.log('Données du formulaire:', {
      File: selectedFile.value,
      Name: newDocument.value.name,
      Description: newDocument.value.description,
      UserId: newDocument.value.userId
    })

    const response = await axios.post('api/admin/documents', formData, {
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'multipart/form-data'
      }
    })

    console.log('Téléversement réussi:', response.data)
    alert('Document téléversé avec succès !')
    closeAddModal()
    await fetchDocuments()
  } catch (error) {
    console.error('Erreur lors du téléversement du document:', error)
    if (error.response?.data) {
      const errors = error.response.data
      if (errors.errors) {
        serverErrors.value = Object.entries(errors.errors)
          .map(([field, messages]) => `${field}: ${messages.join(', ')}`)
      } else if (errors.error) {
        serverErrors.value = [errors.error]
      } else {
        serverErrors.value = ['Une erreur inattendue s\'est produite.']
      }
    } else {
      serverErrors.value = ['Échec du téléversement du document. Aucune réponse du serveur.']
    }
  } finally {
    uploading.value = false
  }
}

const closeAddModal = () => {
  showAddModal.value = false
  newDocument.value = { name: '', description: '', userId: '' }
  selectedFile.value = null
  isDragging.value = false
  formSubmitted.value = false
  serverErrors.value = []
}

const closeDocumentModal = () => {
  selectedDocument.value = null
  isEditing.value = false
  documentBeforeEdit.value = null
  serverErrors.value = []
  formSubmitted.value = false
}

onMounted(() => {
  fetchDocuments()
  fetchUsers()
})
</script>

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

.actions {
  display: flex;
  gap: 0.5rem;
  flex-wrap: nowrap;
}

.action-button {
  padding: 0.4rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  background: none;
  font-size: 1rem;
}

.action-button:hover {
  background: #f5f5f5;
}

.action-button.view { color: #3498db; }
.action-button.edit { color: #f39c12; }
.action-button.delete { color: #e74c3c; }

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
.form-textarea,
.input,
.textarea {
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

.form-textarea,
.textarea {
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

.cancel-btn,
.submit-btn,
.button {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1rem;
}

.cancel-btn,
.button.secondary {
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

.document-header {
  background: #f8f9fa;
  padding: 1rem;
  border-radius: 8px;
  margin-bottom: 1.5rem;
}

.document-title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}

.user-details {
  background: #f8f9fa;
  padding: 1rem;
  border-radius: 4px;
}

.user-details p {
  margin: 0.5rem 0;
}

.modal-actions {
  display: flex;
  justify-content: space-between;
  width: 100%;
}

.edit-actions {
  display: flex;
  gap: 0.5rem;
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
  
  .edit-actions {
    width: 100%;
  }
  
  .button {
    width: 100%;
    justify-content: center;
  }
}
</style>
