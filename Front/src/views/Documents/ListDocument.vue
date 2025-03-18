<template>
  <div class="list-document-container">
    <!-- Search Bar -->
    <div class="search-container">
      <div class="search-box">
        <i class="bi bi-search search-icon"></i>
        <input 
          type="text" 
          class="form-control search-input" 
          v-model="searchQuery" 
          placeholder="Search for a document..."
        >
        <button class="btn btn-outline-secondary clear-search" @click="clearSearch" v-if="searchQuery">
          <i class="bi bi-x-lg"></i>
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading-container">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
      <p class="loading-text">Loading documents...</p>
    </div>

    <!-- Documents Grid -->
    <div v-else-if="filteredDocuments.length > 0" class="documents-grid">
      <div v-for="(doc) in filteredDocuments" :key="doc.id" class="document-card">
        <div class="document-content">
          <div class="document-icon">
            <i class="bi bi-file-pdf text-danger"></i>
          </div>
          <div class="document-info">
            <h5 class="document-title">{{ doc.name }}</h5>
            <p class="document-description">{{ doc.description || 'No description available' }}</p>
            <div class="document-meta">
              <span class="document-date">
                <i class="bi bi-calendar3"></i>
                {{ new Date(doc.createdAt).toLocaleDateString() }}
              </span>
              <span class="document-size">
                <i class="bi bi-hdd"></i>
                {{ formatFileSize(doc.size) }}
              </span>
            </div>
          </div>
        </div>
        <div class="document-actions">
          <button @click.stop="viewDocument(doc)" class="action-btn view-btn" title="View">
            <i class="bi bi-eye"></i>
          </button>
          <button @click.stop="downloadFile(doc)" class="action-btn download-btn" title="Download">
            <i class="bi bi-download"></i>
          </button>
          <button @click.stop="deleteDocument(doc.id)" class="action-btn delete-btn" title="Delete">
            <i class="bi bi-trash"></i>
          </button>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="empty-state">
      <div class="empty-icon">
        <i class="bi bi-folder-x"></i>
      </div>
      <h3>No documents found</h3>
      <p>{{ searchQuery ? 'Try adjusting your search' : 'Start by adding a new document' }}</p>
      <router-link to="/document/Add" class="btn btn-primary">
        <i class="bi bi-plus-lg me-2"></i>
        Add Document
      </router-link>
    </div>
  </div>
</template>

<script>
import axios from "axios";
import UploadDocument from "./UploadDocument.vue";
import { mapState } from "vuex";
import SideBare from "./SideBare.vue";

export default {
  components: { UploadDocument, SideBare },
  data() {
    return { 
      documents: [], 
      loading: true,
      searchQuery: ''
    };
  },
  computed: {
    ...mapState({
      authenticated: (state) => state.authenticated,
      user: (state) => state.user,
    }),
    filteredDocuments() {
      if (!this.searchQuery) return this.documents;
      const searchTerm = this.searchQuery.toLowerCase();
      return this.documents.filter(doc => 
        doc.name.toLowerCase().includes(searchTerm) ||
        (doc.description && doc.description.toLowerCase().includes(searchTerm))
      );
    }
  },
  methods: {
    async fetchDocuments() {
      try {
        const response = await axios.get(`api/documents`);
        this.documents = response.data;
      } catch (error) {
        console.error("Error fetching documents", error);
      } finally {
        this.loading = false;
      }
    },
    clearSearch() {
      this.searchQuery = '';
    },
    async deleteDocument(documentId) {
      if (!confirm("Are you sure you want to delete this document?")) return;
      try {
        await axios.delete(`api/documents/${documentId}`);
        this.fetchDocuments();
      } catch (error) {
        console.error("Error deleting document", error);
        alert("Failed to delete document.");
      }
    },
    viewDocument(doc) {
      if (!doc || !doc.id) {
        console.error("Invalid document data");
        return;
      }
      try {
        window.open(doc.fileUrl, '_blank');
      } catch (error) {
        console.error("Error viewing document", error);
        alert("Failed to open document");
      }
    },
    formatFileSize(bytes) {
      if (bytes === 0) return '0 Bytes';
      const k = 1024;
      const sizes = ['Bytes', 'KB', 'MB', 'GB'];
      const i = Math.floor(Math.log(bytes) / Math.log(k));
      return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    },
    async downloadFile(doc) {
      try {
        const response = await axios.get(doc.fileUrl, {
          responseType: 'blob'
        });
        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', doc.name);
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
      } catch (error) {
        console.error("Error downloading file", error);
        alert("Failed to download file");
      }
    }
  },
  mounted() {
    this.fetchDocuments();
  },
};
</script>

<style scoped>
.list-document-container {
  padding: 2rem;
  height: 100%;
  overflow-y: auto;
}

.search-container {
  margin-bottom: 2rem;
}

.search-box {
  position: relative;
  max-width: 600px;
  margin: 0 auto;
  display: flex;
  align-items: center;
}

.search-icon {
  position: absolute;
  left: 1rem;
  color: #6c757d;
}

.search-input {
  padding: 0.75rem 1rem 0.75rem 3rem;
  border: 1px solid #dee2e6;
  border-radius: 8px;
  font-size: 1rem;
  transition: all 0.3s ease;
  width: 100%;
}

.search-input:focus {
  outline: none;
  border-color: #0d6efd;
  box-shadow: 0 0 0 3px rgba(13, 110, 253, 0.1);
}

.clear-search {
  position: absolute;
  right: 0.5rem;
  padding: 0.5rem;
  border-radius: 50%;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem;
}

.loading-text {
  margin-top: 1rem;
  color: #6c757d;
}

.documents-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
  padding: 1rem 0;
}

.document-card {
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  transition: all 0.3s ease;
  overflow: hidden;
  border: 1px solid #e9ecef;
}

.document-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.document-content {
  padding: 1.5rem;
}

.document-icon {
  font-size: 2rem;
  margin-bottom: 1rem;
}

.document-info {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.document-title {
  font-size: 1.1rem;
  font-weight: 600;
  color: #2c3e50;
  margin: 0;
}

.document-description {
  font-size: 0.9rem;
  color: #6c757d;
  margin: 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.document-meta {
  display: flex;
  gap: 1rem;
  font-size: 0.8rem;
  color: #6c757d;
  margin-top: 0.5rem;
}

.document-meta i {
  margin-right: 0.25rem;
}

.document-actions {
  display: flex;
  padding: 0.75rem;
  background: #f8f9fa;
  border-top: 1px solid #e9ecef;
  gap: 0.5rem;
}

.action-btn {
  padding: 0.5rem;
  border: none;
  background: none;
  border-radius: 6px;
  color: #6c757d;
  cursor: pointer;
  transition: all 0.3s ease;
}

.action-btn:hover {
  background: #e9ecef;
}

.view-btn:hover {
  color: #0d6efd;
}

.download-btn:hover {
  color: #0d6efd;
}

.delete-btn:hover {
  color: #dc3545;
}

.empty-state {
  text-align: center;
  padding: 3rem;
  color: #6c757d;
}

.empty-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
  color: #dee2e6;
}

.empty-state h3 {
  font-size: 1.5rem;
  margin-bottom: 0.5rem;
  color: #2c3e50;
}

.empty-state p {
  margin-bottom: 1.5rem;
}

@media (max-width: 768px) {
  .list-document-container {
    padding: 1rem;
  }

  .documents-grid {
    grid-template-columns: 1fr;
  }

  .document-card {
    margin-bottom: 1rem;
  }
}
</style>
