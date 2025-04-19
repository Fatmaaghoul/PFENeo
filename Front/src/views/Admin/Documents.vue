<template>
  <div class="admin-container">
    <div class="card">
      <div class="card-content">
        <div class="header">
          <h2 class="title">Document Management</h2>
          <div class="search-container">
            <div class="search-box">
              <input 
                type="text" 
                class="search-input" 
                placeholder="Search documents..." 
                v-model="searchQuery"
              />
              <button class="search-button">
                <i class="icon-search">🔍</i>
              </button>
            </div>
          </div>
        </div>

        <!-- Loading State -->
        <div v-if="loading" class="loading">
          <div class="spinner"></div>
          <p class="loading-text">Loading documents...</p>
        </div>

        <!-- Documents Table -->
        <div v-else class="table-wrapper">
          <table class="documents-table">
            <thead>
              <tr>
                <th>Document</th>
                <th>User</th>
                <th>Description</th>
                <th>Upload Date</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="document in filteredDocuments" :key="document.id">
                <td>
                  <div class="document-name">
                    <span class="icon-pdf">📄</span>
                    <span>{{ document.name }}</span>
                  </div>
                </td>
                <td>
                  <div v-if="document.user" class="user-info">
                    {{ document.user.userName || document.user.email }}
                  </div>
                  <div v-else class="loading-info">Loading user...</div>
                </td>
                <td>
                  <div class="description">
                    <p>{{ document.description || 'No description' }}</p>
                  </div>
                </td>
                <td>{{ formatDate(document.uploadDate) }}</td>
                <td>
                  <div class="actions">
                    <button class="action-button view" @click="viewDocument(document)" title="View">
                      <span class="icon">👁️</span>
                    </button>
                    <button class="action-button edit" @click="editDocument(document)" title="Edit">
                      <span class="icon">✏️</span>
                    </button>
                    <button class="action-button delete" @click="deleteDocument(document)" title="Delete">
                      <span class="icon">🗑️</span>
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="filteredDocuments.length === 0">
                <td colspan="5" class="empty-state">
                  <div class="empty-message">
                    <span class="icon-empty">📭</span>
                    <span>No documents found</span>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Document Modal -->
    <div v-if="selectedDocument" class="modal" :class="{ 'modal-active': selectedDocument }">
      <div class="modal-content">
        <div class="modal-header">
          <h3>{{ isEditing ? 'Edit Document' : 'Document Details' }}</h3>
          <button class="close-button" @click="closeModal">×</button>
        </div>
        <div class="modal-body">
          <div class="document-header">
            <div class="document-title">
              <span class="icon-pdf">📄</span>
              <h4>{{ selectedDocument.name }}</h4>
            </div>
            <small class="upload-date">Uploaded on {{ formatDate(selectedDocument.UploadDate) }}</small>
          </div>
          
          <div class="form-group">
            <label>Document Name</label>
            <input 
              type="text" 
              v-model="selectedDocument.name"
              :readonly="!isEditing"
              class="input"
            />
          </div>
          
          <div v-if="selectedDocument.user" class="form-group">
            <label>Document Owner</label>
            <div class="user-details">
              <p><strong>Username:</strong> {{ selectedDocument.user.userName }}</p>
              <p><strong>Email:</strong> {{ selectedDocument.user.email }}</p>
            </div>
          </div>
          
          <div class="form-group">
            <label>Description</label>
            <textarea 
              v-model="selectedDocument.description" 
              rows="3"
              :readonly="!isEditing"
              class="textarea"
            ></textarea>
          </div>
          
          <div class="modal-actions">
            <div class="edit-actions">
              <button v-if="!isEditing" class="button secondary" @click="isEditing = true">
                <span class="icon">✏️</span>
                Edit Document
              </button>
              <template v-else>
                <button class="button primary" @click="saveDocumentChanges">
                  <span class="icon">✓</span>
                  Save Changes
                </button>
                <button class="button secondary" @click="cancelEdit">
                  <span class="icon">×</span>
                  Cancel
                </button>
              </template>
            </div>
            <button class="button primary" @click="downloadDocument(selectedDocument)">
              <span class="icon">⬇️</span>
              Download Document
            </button>
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

// Fetch all documents with their users
const fetchDocuments = async () => {
  loading.value = true
  try {
    const token = Cookies.get('token')
    const response = await axios.get('api/admin/documents', {
      headers: { Authorization: `Bearer ${token}` }
    })
    // Fetch user for each document
    const documentsWithUsers = await Promise.all(
      response.data.map(async (doc) => {
        const user = await fetchDocumentUser(doc.id)
        return { ...doc, user }
      })
    )
    documents.value = documentsWithUsers
  } catch (error) {
    console.error('Error fetching documents:', error)
    alert('Failed to load documents. Please try again.')
  } finally {
    loading.value = false
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
    console.error('Error fetching document user:', error)
    return null
  }
}

// View document details
const viewDocument = async (document) => {
  isEditing.value = false
  selectedDocument.value = { ...document }
  const modal = new bootstrap.Modal(document.getElementById('documentModal'))
  modal.show()
}

// Edit document
const editDocument = (document) => {
  isEditing.value = true
  documentBeforeEdit.value = { ...document }
  selectedDocument.value = { ...document }
  const modal = new bootstrap.Modal(document.getElementById('documentModal'))
  modal.show()
}

// Save document changes
const saveDocumentChanges = async () => {
  try {
    const token = Cookies.get('token')
    const response = await axios.put(`api/admin/documents/${selectedDocument.value.id}`, 
      { 
        name: selectedDocument.value.name,
        description: selectedDocument.value.description
      },
      { headers: { Authorization: `Bearer ${token}` } }
    )
    
    if (response.data) {
      // Update the document in the list
      const index = documents.value.findIndex(d => d.id === selectedDocument.value.id)
      if (index !== -1) {
        documents.value[index] = { 
          ...documents.value[index], 
          ...response.data,
          user: documents.value[index].user // Keep the user information
        }
      }
      alert('Document updated successfully!')
      isEditing.value = false
    }
  } catch (error) {
    console.error('Error updating document:', error)
    alert('Failed to update document. Please try again.')
  }
}

// Cancel edit
const cancelEdit = () => {
  if (documentBeforeEdit.value) {
    selectedDocument.value = { ...documentBeforeEdit.value }
  }
  isEditing.value = false
}

// Delete document
const deleteDocument = async (document) => {
  if (confirm(`Are you sure you want to delete "${document.name}"?`)) {
    try {
      const token = Cookies.get('token')
      await axios.delete(`api/admin/documents/${document.id}`, {
        headers: { Authorization: `Bearer ${token}` }
      })
      alert('Document deleted successfully!')
      await fetchDocuments()
    } catch (error) {
      console.error('Error deleting document:', error)
      alert('Failed to delete document. Please try again.')
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
    console.error('Error downloading document:', error)
    alert('Failed to download document. Please try again.')
  }
}

// Format date
const formatDate = (date) => {
  if (!date) return 'N/A'
  const uploadDate = new Date(date)
  return uploadDate.toLocaleDateString('en-US', {
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

const closeModal = () => {
  selectedDocument.value = null;
  isEditing.value = false;
}

onMounted(() => {
  fetchDocuments()
})
</script>

<style scoped>
.admin-container {
  padding: 2rem;
  max-width: 1200px;
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
}

.title {
  margin: 0;
  font-size: 1.5rem;
  color: #2c3e50;
}

.search-container {
  flex: 0 0 300px;
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
  overflow-x: auto;
}

.documents-table {
  width: 100%;
  border-collapse: collapse;
}

.documents-table th,
.documents-table td {
  padding: 1rem;
  text-align: left;
  border-bottom: 1px solid #eee;
}

.documents-table th {
  font-weight: 600;
  color: #2c3e50;
  background: #f8f9fa;
}

.document-name {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.description {
  max-width: 300px;
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
}

.action-button {
  padding: 0.4rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  background: none;
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

/* Modal Styles */
.modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  opacity: 0;
  visibility: hidden;
  transition: all 0.3s;
}

.modal-active {
  opacity: 1;
  visibility: visible;
}

.modal-content {
  background: white;
  border-radius: 8px;
  width: 90%;
  max-width: 800px;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 1.5rem;
  border-bottom: 1px solid #eee;
}

.close-button {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  padding: 0.5rem;
}

.modal-body {
  padding: 1.5rem;
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

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.input, .textarea {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
}

.input:read-only, .textarea:read-only {
  background: #f8f9fa;
}

.textarea {
  resize: vertical;
  min-height: 100px;
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
  margin-top: 2rem;
}

.edit-actions {
  display: flex;
  gap: 0.5rem;
}

.button {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1rem;
}

.button.primary {
  background: #3498db;
  color: white;
}

.button.secondary {
  background: #f8f9fa;
  color: #2c3e50;
}

.button:hover {
  opacity: 0.9;
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