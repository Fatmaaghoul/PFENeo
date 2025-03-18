<template>
  <div class="add-document-container">
    <div class="card shadow-sm drop-zone" @dragover.prevent="dragOver" @dragleave="dragLeave" @drop.prevent="handleDrop"
      :class="{ 'dragging': isDragging }">
      <div class="card-body">
        <div class="upload-content">
          <div class="upload-icon-container">
            <i class="bi bi-cloud-upload upload-icon"></i>
          </div>

          <h5 class="upload-title">Ajouter un document</h5>
          <p class="upload-subtitle">Glissez-déposez votre fichier PDF ici ou</p>

          <form @submit.prevent="uploadDocument" class="upload-form">
            <div class="form-group">
              <label for="file-upload" class="upload-label">
                <i class="bi bi-folder-plus me-2"></i>
                Sélectionner un fichier
              </label>

              <input id="file-upload" type="file" class="form-control d-none" @change="handleFileUpload"
                accept=".pdf" />

              <div v-if="file" class="selected-file">
                <div class="file-info">
                  <i class="bi bi-file-pdf text-danger"></i>
                  <span class="file-name">{{ file.name }}</span>
                </div>
                <button type="button" class="btn-remove" @click="removeFile">
                  <i class="bi bi-x-lg"></i>
                </button>
              </div>
            </div>

            <button type="submit" class="btn btn-primary upload-btn" :disabled="!file">
              <i class="bi bi-cloud-upload me-2"></i>
              Add document
              <span v-if="loading" class="spinner-border spinner-border-sm ms-2" role="status">
                <span class="visually-hidden">Loading...</span>
              </span>
            </button>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from "axios";
import { mapState, useStore } from "vuex";
import SideBare from "./SideBare.vue";
import { ref, onMounted } from 'vue';

import Cookies from 'js-cookie';



export default {
  components: {
    SideBare,
  },
  created() {
    this.$store.dispatch("checkUserStatus");
  },
  data() {
    return {
      file: null,
      isDragging: false,
      loading: false,
    };
  },
  methods: {
    handleFileUpload(event) {
      const selectedFile = event.target.files[0];
      this.validateAndSetFile(selectedFile);
    },
    validateAndSetFile(file) {
      if (file) {
        if (file.type !== "application/pdf") {
          this.showError("Seuls les fichiers PDF sont autorisés.");
          return;
        }
        this.file = file;
      }
    },
    dragOver() {
      this.isDragging = true;
    },
    dragLeave() {
      this.isDragging = false;
    },
    handleDrop(event) {
      this.isDragging = false;
      const files = event.dataTransfer.files;
      if (files.length > 0) {
        this.validateAndSetFile(files[0]);
      }
    },
    removeFile() {
      this.file = null;
      document.getElementById("file-upload").value = "";
    },
    showError(message) {
      alert("❌ " + message);
      this.file = null;
      document.getElementById("file-upload").value = "";
    },
    async uploadDocument() {
      if (!this.file) {
        this.showError("Veuillez sélectionner un fichier PDF.");
        return;
      }

      this.loading = true;
      const formData = new FormData();
      formData.append("file", this.file);

      try {
        const token = Cookies.get('token');
        const response = await axios.post(`api/documents/add`, formData,
          {
            headers: { Authorization: `Bearer ${token}` }
          }
        );
        console.log("✅ Document uploadé :", response.data);
        alert("✅ Document PDF uploadé avec succès !");
        this.removeFile();
        this.$emit("document-uploaded");
      } catch (error) {
        console.error("❌ Erreur lors de l'upload", error);
        this.showError(error.response?.data || "Échec de l'upload du document.");
      } finally {
        this.loading = false;
      }
    },
  },
};
</script>

<style scoped>
.add-document-container {
  padding: 2rem;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.drop-zone {
  width: 100%;
  max-width: 600px;
  border: 2px dashed #dee2e6;
  border-radius: 16px;
  transition: all 0.3s ease;
  background-color: #ffffff;
  cursor: pointer;
  overflow: hidden;
}

.drop-zone.dragging {
  border-color: #0d6efd;
  background-color: #f8f9fa;
  transform: scale(1.02);
  box-shadow: 0 0 20px rgba(13, 110, 253, 0.1);
}

.upload-content {
  padding: 2rem;
  text-align: center;
}

.upload-icon-container {
  width: 80px;
  height: 80px;
  margin: 0 auto 1.5rem;
  background-color: #f8f9fa;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s ease;
}

.drop-zone.dragging .upload-icon-container {
  background-color: #e7f1ff;
  transform: scale(1.1);
}

.upload-icon {
  font-size: 2.5rem;
  color: #0d6efd;
}

.upload-title {
  font-size: 1.5rem;
  font-weight: 600;
  color: #2c3e50;
  margin-bottom: 0.5rem;
}

.upload-subtitle {
  color: #6c757d;
  margin-bottom: 1.5rem;
}

.upload-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.upload-label {
  display: inline-block;
  padding: 0.75rem 1.5rem;
  background-color: #f8f9fa;
  border: 1px solid #dee2e6;
  border-radius: 8px;
  color: #0d6efd;
  cursor: pointer;
  transition: all 0.3s ease;
}

.upload-label:hover {
  background-color: #e7f1ff;
  border-color: #0d6efd;
}

.selected-file {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.75rem 1rem;
  background-color: #f8f9fa;
  border-radius: 8px;
  margin-top: 1rem;
}

.file-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.file-name {
  color: #2c3e50;
  font-size: 0.9rem;
}

.btn-remove {
  background: none;
  border: none;
  color: #dc3545;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 50%;
  transition: all 0.3s ease;
}

.btn-remove:hover {
  background-color: #fee2e2;
}

.upload-btn {
  padding: 0.75rem 1.5rem;
  font-weight: 500;
  border-radius: 8px;
  transition: all 0.3s ease;
}

.upload-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.upload-btn:not(:disabled):hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(13, 110, 253, 0.2);
}

@media (max-width: 768px) {
  .add-document-container {
    padding: 1rem;
  }

  .upload-content {
    padding: 1.5rem;
  }

  .upload-icon {
    font-size: 2rem;
  }

  .upload-title {
    font-size: 1.25rem;
  }
}
</style>