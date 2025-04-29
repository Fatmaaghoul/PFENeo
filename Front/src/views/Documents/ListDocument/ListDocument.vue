<!-- ListDocument.vue -->
<template>
    <div class="documents-container">
      <SearchFilter
        v-model:search-query="searchQuery"
        :current-filter="currentFilter"
        @update:filter="setFilter"
        @clear-search="clearSearch"
      />
      <div v-if="loading" class="loading-container">
        <div class="spinner"></div>
        <p>Chargement des documents...</p>
      </div>
      <DocumentGrid
        v-else
        :documents="filteredDocuments"
        @navigate-to-content="navigateToContentDocument"
        @download-document="downloadDocument"
        @delete-document="deleteDocument"
        @show-add-modal="showAddModal = true"
      />
      <AddDocumentModal
        v-if="showAddModal"
        @close="closeModal"
        @upload-document="uploadDocument"
      />
    </div>
  </template>
  
  <script>
  import axios from 'axios'
  import Cookies from 'js-cookie'
  import SearchFilter from './SearchFilter.vue'
  import DocumentGrid from './DocumentGrid.vue'
  import AddDocumentModal from './AddDocumentModal.vue'
  
  export default {
    name: 'ListDocument',
    components: {
      SearchFilter,
      DocumentGrid,
      AddDocumentModal
    },
    data() {
      return {
        documents: [],
        loading: true,
        searchQuery: '',
        showAddModal: false,
        currentFilter: 'all'
      }
    },
    computed: {
      filteredDocuments() {
        let filtered = this.documents
  
        if (this.searchQuery) {
          const query = this.searchQuery.toLowerCase()
          filtered = filtered.filter((doc) => {
            const name = doc.name ? doc.name.toLowerCase() : ''
            const description = doc.description ? doc.description.toLowerCase() : ''
            return name.includes(query) || description.includes(query)
          })
        }
  
        if (this.currentFilter === 'traiter') {
          filtered = filtered.filter((doc) => doc.isAnalysed)
        } else if (this.currentFilter === 'non-traiter') {
          filtered = filtered.filter((doc) => !doc.isAnalysed)
        }
  
        return filtered
      }
    },
    methods: {
      setFilter(filter) {
        this.currentFilter = filter
      },
      async fetchDocuments() {
        this.loading = true
        try {
          const token = Cookies.get('token')
          const response = await axios.get('api/documents', {
            headers: {
              Authorization: `Bearer ${token}`
            }
          })
          this.documents = response.data
        } catch (error) {
          console.error('Erreur lors du chargement des documents:', error)
          this.showErrorAlert('Erreur lors du chargement des documents. Veuillez réessayer.')
        } finally {
          this.loading = false
        }
      },
      clearSearch() {
        this.searchQuery = ''
      },
      showSuccessAlert(message) {
        const alertDiv = document.createElement('div')
        alertDiv.className = 'custom-alert success'
        alertDiv.innerHTML = `
          <i class="bi bi-check-circle"></i>
          <span>${message}</span>
        `
        document.body.appendChild(alertDiv)
        setTimeout(() => {
          alertDiv.classList.add('show')
          setTimeout(() => {
            alertDiv.classList.remove('show')
            setTimeout(() => {
              document.body.removeChild(alertDiv)
            }, 300)
          }, 3000)
        }, 100)
      },
      showErrorAlert(message) {
        const alertDiv = document.createElement('div')
        alertDiv.className = 'custom-alert error'
        alertDiv.innerHTML = `
          <i class="bi bi-exclamation-circle"></i>
          <span>${message}</span>
        `
        document.body.appendChild(alertDiv)
        setTimeout(() => {
          alertDiv.classList.add('show')
          setTimeout(() => {
            alertDiv.classList.remove('show')
            setTimeout(() => {
              document.body.removeChild(alertDiv)
            }, 300)
          }, 3000)
        }, 100)
      },
      navigateToContentDocument(doc) {
        this.$router.push({ name: 'ContentDocument', params: { id: doc.id } })
      },
      async downloadDocument(doc) {
        try {
          const token = Cookies.get('token')
          const response = await axios({
            url: doc.fileUrl,
            method: 'GET',
            responseType: 'blob',
            headers: {
              Authorization: `Bearer ${token}`
            }
          })
  
          const url = window.URL.createObjectURL(new Blob([response.data]))
          const link = document.createElement('a')
          link.href = url
          link.setAttribute('download', doc.name)
          document.body.appendChild(link)
          link.click()
          document.body.removeChild(link)
          window.URL.revokeObjectURL(url)
        } catch (error) {
          console.error('Erreur lors du téléchargement:', error)
          this.showErrorAlert('Erreur lors du téléchargement du document. Veuillez réessayer.')
        }
      },
      async deleteDocument(documentId) {
        if (!confirm('Voulez-vous vraiment supprimer ce document ?')) return
        try {
          const token = Cookies.get('token')
          await axios.delete(`api/documents/${documentId}`, {
            headers: {
              Authorization: `Bearer ${token}`
            }
          })
          this.documents = this.documents.filter((doc) => doc.id !== documentId)
          this.showSuccessAlert('Document supprimé avec succès !')
        } catch (error) {
          console.error('Erreur lors de la suppression:', error)
          this.showErrorAlert('Erreur lors de la suppression du document. Veuillez réessayer.')
        }
      },
      closeModal() {
        this.showAddModal = false
      },
      async uploadDocument({ file, name, description }) {
        try {
          const token = Cookies.get('token')
          if (!token) {
            throw new Error('Jeton d’authentification non trouvé. Veuillez vous reconnecter.')
          }
  
          const formData = new FormData()
          if (file) {
            formData.append('file', file)
          }
          formData.append('description', description.trim() || 'No description')
          formData.append('name', name.trim())
  
          const response = await axios.post('api/documents/add', formData, {
            headers: {
              Authorization: `Bearer ${token}`,
              'Content-Type': 'multipart/form-data',
              Accept: 'application/json'
            }
          })
  
          this.showSuccessAlert('Document uploadé avec succès !')
          this.documents.unshift(response.data)
          this.closeModal()
        } catch (error) {
          let errorMessage = 'Erreur lors de l’upload du document. Veuillez réessayer.'
          if (error.response && error.response.data) {
            const errorData = error.response.data
            if (errorData.errors) {
              errorMessage = Object.entries(errorData.errors)
                .map(([field, messages]) => `${field}: ${messages.join(', ')}`)
                .join('\n')
            } else if (errorData.title) {
              errorMessage = errorData.title
            } else if (typeof errorData === 'string') {
              errorMessage = errorData
            }
          } else if (error.message) {
            errorMessage = error.message
          }
          this.showErrorAlert(errorMessage)
        }
      }
    },
    mounted() {
      this.fetchDocuments()
    }
  }
  </script>
  
  <style scoped>
  .documents-container {
    
    max-width: 1200px;
    margin: 0 auto;
    margin-top: 80px;
    padding: 2rem;
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
    to {
      transform: rotate(360deg);
    }
  }
  
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
  }
  </style>