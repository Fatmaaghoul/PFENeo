<template>
  <div class="upload-container">
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

    <!-- Description Generation Zone -->
    <div class="description-generator" 
         @dragover.prevent="dragOver"
         @dragleave="dragLeave"
         @drop.prevent="handleDrop"
         :class="{ 'dragging': isDragging }">
      <div class="generator-header">
        <h3>Generate Description</h3>
        <button v-if="selectedDocument" 
                class="btn btn-outline-danger btn-sm" 
                @click="clearSelection">
          <i class="bi bi-trash"></i> Clear
        </button>
      </div>
      <div v-if="selectedDocument" class="selected-document">
        <div class="document-icon">
          <i class="bi bi-file-pdf text-danger"></i>
        </div>
        <div class="document-info">
          <h5>{{ selectedDocument.name }}</h5>
          <p class="text-muted">Drag a document here or select one from the list</p>
        </div>
      </div>
      <div v-else class="drop-zone">
        <i class="bi bi-cloud-upload"></i>
        <p>Drag a document here or select one from the list</p>
      </div>
      <div v-if="generatedDescription" class="generated-description">
        <h5>Generated Description:</h5>
        <p>{{ generatedDescription }}</p>
        <div class="description-actions">
          <button class="btn btn-success" @click="applyDescription">
            <i class="bi bi-check-lg"></i> Apply
          </button>
          <button class="btn btn-outline-secondary" @click="regenerateDescription">
            <i class="bi bi-arrow-clockwise"></i> Regenerate
          </button>
        </div>
      </div>
    </div>

    <!-- Documents List -->
    <div class="documents-list">
      <h3>Documents Without Description</h3>
      <div v-if="loading" class="loading-container">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
      </div>
      
      <div v-else-if="filteredDocuments.length > 0" class="documents-grid">
        <div v-for="doc in filteredDocuments" 
             :key="doc.id" 
             class="document-card"
             draggable="true"
             @dragstart="startDrag($event, doc)">
          <div class="document-content">
            <div class="document-icon">
              <i class="bi bi-file-pdf text-danger"></i>
            </div>
            <div class="document-info">
              <h5 class="document-title">{{ doc.name }}</h5>
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
            <button @click="selectDocument(doc)" class="action-btn select-btn" title="Select">
              <i class="bi bi-check-circle"></i>
            </button>
            <button @click="downloadFile(doc)" class="action-btn download-btn" title="Download">
              <i class="bi bi-download"></i>
            </button>
          </div>
        </div>
      </div>

      <div v-else class="empty-state">
        <div class="empty-icon">
          <i class="bi bi-check-circle-fill text-success"></i>
        </div>
        <h3>All Documents Have Descriptions</h3>
        <p>No documents to process</p>
      </div>
    </div>
  </div>
</template>

<script>
import axios from "axios";
import { mapState } from "vuex";

export default {
  name: "UploadDocument",
  data() {
    return {
      loading: true,
      documentsWithoutDescription: [],
      searchQuery: "",
      generatedDescription: "",
      selectedDocument: null,
      isDragging: false
    };
  },
  computed: {
    ...mapState({
      authenticated: (state) => state.authenticated,
      user: (state) => state.user,
    }),
    filteredDocuments() {
      if (!this.searchQuery) return this.documentsWithoutDescription;
      const searchTerm = this.searchQuery.toLowerCase();
      return this.documentsWithoutDescription.filter(doc => 
        doc.name.toLowerCase().includes(searchTerm)
      );
    }
  },
  methods: {
    async fetchDocumentsWithoutDescription() {
      try {
        const response = await axios.get(`api/documents`);
        this.documentsWithoutDescription = response.data.filter(doc => !doc.description);
      } catch (error) {
        console.error("Error fetching documents", error);
      } finally {
        this.loading = false;
      }
    },
    async generateDescription() {
      if (!this.selectedDocument) {
        alert("Please select a document first");
        return;
      }
      try {
        // Implement description generation logic here
        this.generatedDescription = "Generated description for " + this.selectedDocument.name;
      } catch (error) {
        console.error("Error generating description", error);
        alert("Failed to generate description");
      }
    },
    async applyDescription() {
      if (!this.selectedDocument || !this.generatedDescription) {
        alert("Please select a document and generate a description");
        return;
      }
      try {
        await axios.put(`api/documents/${this.selectedDocument.id}`, {
          description: this.generatedDescription
        });
        this.fetchDocumentsWithoutDescription();
        this.selectedDocument = null;
        this.generatedDescription = "";
      } catch (error) {
        console.error("Error updating description", error);
        alert("Failed to update description");
      }
    },
    selectDocument(doc) {
      this.selectedDocument = doc;
      this.generatedDescription = "";
    },
    clearSelection() {
      this.selectedDocument = null;
      this.generatedDescription = "";
    },
    clearSearch() {
      this.searchQuery = "";
    },
    regenerateDescription() {
      this.generateDescription();
    },
    formatFileSize(bytes) {
      if (bytes === 0) return '0 Bytes';
      const k = 1024;
      const sizes = ['Bytes', 'KB', 'MB', 'GB'];
      const i = Math.floor(Math.log(bytes) / Math.log(k));
      return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    },
    startDrag(event, doc) {
      event.dataTransfer.setData('documentId', doc.id);
    },
    dragOver(event) {
      this.isDragging = true;
    },
    dragLeave(event) {
      this.isDragging = false;
    },
    handleDrop(event) {
      this.isDragging = false;
      const documentId = event.dataTransfer.getData('documentId');
      const doc = this.documentsWithoutDescription.find(d => d.id === documentId);
      if (doc) {
        this.selectDocument(doc);
      }
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
    this.fetchDocumentsWithoutDescription();
  },
};
</script>

<style scoped>
.upload-container {
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

.generator-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
  margin-bottom: 1.5rem;
}

.description-generator {
  background: white;
  border-radius: 12px;
  padding: 2rem;
  margin-bottom: 2rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  border: 2px dashed #dee2e6;
  transition: all 0.3s ease;
  min-height: 200px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

.description-generator.dragging {
  border-color: #0d6efd;
  background-color: #f8f9fa;
}

.drop-zone {
  text-align: center;
  color: #6c757d;
}

.drop-zone i {
  font-size: 3rem;
  margin-bottom: 1rem;
  color: #0d6efd;
}

.selected-document {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 8px;
  width: 100%;
}

.generated-description {
  margin-top: 2rem;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 8px;
  border: 1px solid #e9ecef;
  width: 100%;
}

.generated-description h5 {
  color: #2c3e50;
  margin-bottom: 0.5rem;
}

.description-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1rem;
}

.documents-list h3 {
  margin-bottom: 1.5rem;
  color: #2c3e50;
}

.loading-container {
  display: flex;
  justify-content: center;
  padding: 2rem;
}

.documents-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

.document-card {
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  transition: all 0.3s ease;
  overflow: hidden;
  border: 1px solid #e9ecef;
  cursor: move;
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

.select-btn:hover {
  color: #198754;
}

.download-btn:hover {
  color: #0d6efd;
}

.empty-state {
  text-align: center;
  padding: 3rem;
  color: #6c757d;
}

.empty-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
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
  .upload-container {
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
