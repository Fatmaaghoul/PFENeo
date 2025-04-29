<!-- AddDocumentModal.vue -->
<template>
    <div class="modal-overlay" @click="$emit('close')">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>Ajouter un nouveau document</h2>
          <button class="close-btn" @click="$emit('close')">
            <i class="bi bi-x"></i>
          </button>
        </div>
        <div class="modal-body">
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
            <span v-if="!selectedFile && formSubmitted" class="error-message">
              Veuillez sélectionner un fichier PDF.
            </span>
          </div>
          <div v-if="selectedFile" class="form-fields">
            <div class="form-group">
              <label>Nom du document</label>
              <input
                type="text"
                v-model="newDocument.name"
                class="form-input"
                :class="{ 'error': !newDocument.name.trim() && formSubmitted }"
                placeholder="Entrez le nom du document"
              />
              <span v-if="!newDocument.name.trim() && formSubmitted" class="error-message">
                Le nom du document est requis.
              </span>
            </div>
            <div class="form-group">
              <label>Description</label>
              <textarea
                v-model="newDocument.description"
                class="form-textarea"
                :class="{ 'error': !newDocument.description.trim() && formSubmitted }"
                placeholder="Entrez la description du document"
                rows="3"
              ></textarea>
              <span v-if="!newDocument.description.trim() && formSubmitted" class="error-message">
                La description est requise.
              </span>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button class="cancel-btn" @click="$emit('close')">Annuler</button>
          <button
            class="submit-btn"
            @click="uploadDocument"
            :disabled="!isFormValid || uploading"
          >
            <span v-if="uploading" class="spinner"></span>
            <span v-else>Ajouter</span>
          </button>
        </div>
      </div>
    </div>
  </template>
  
  <script>
  export default {
    name: 'AddDocumentModal',
    data() {
      return {
        isDragging: false,
        selectedFile: null,
        uploading: false,
        formSubmitted: false,
        newDocument: {
          name: '',
          description: ''
        }
      }
    },
    computed: {
      isFormValid() {
        return (
          this.newDocument.name.trim() !== '' &&
          this.newDocument.description.trim() !== '' &&
          this.selectedFile
        )
      }
    },
    methods: {
      handleDragOver() {
        this.isDragging = true
      },
      handleDragLeave() {
        this.isDragging = false
      },
      handleDrop(event) {
        this.isDragging = false
        const file = event.dataTransfer.files[0]
        if (file) {
          this.handleFile(file)
        }
      },
      handleFileChange(event) {
        const file = event.target.files[0]
        if (file) {
          this.handleFile(file)
        }
      },
      handleFile(file) {
        if (file.type !== 'application/pdf') {
          alert('Seuls les fichiers PDF sont autorisés.')
          return
        }
        this.selectedFile = file
        this.newDocument.name = file.name.replace(/\.[^/.]+$/, '')
      },
      removeFile() {
        this.selectedFile = null
        this.newDocument.name = ''
        this.newDocument.description = ''
        if (this.$refs.fileInput) {
          this.$refs.fileInput.value = ''
        }
      },
      triggerFileInput() {
        this.$refs.fileInput.click()
      },
      uploadDocument() {
        this.formSubmitted = true
        if (!this.isFormValid) {
          alert('Veuillez remplir tous les champs obligatoires.')
          return
        }
        this.uploading = true
        this.$emit('upload-document', {
          file: this.selectedFile,
          name: this.newDocument.name,
          description: this.newDocument.description
        })
        this.uploading = false
        this.resetForm()
      },
      resetForm() {
        this.newDocument = {
          name: '',
          description: ''
        }
        this.selectedFile = null
        this.formSubmitted = false
        if (this.$refs.fileInput) {
          this.$refs.fileInput.value = ''
        }
        this.isDragging = false
      }
    },
    emits: ['close', 'upload-document']
  }
  </script>
  
  <style scoped>
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
  
  .form-input.error,
  .form-textarea.error {
    border-color: #dc3545;
  }
  
  .form-textarea {
    resize: vertical;
    min-height: 100px;
  }
  
  .error-message {
    color: #dc3545;
    font-size: 0.875rem;
    margin-top: 0.25rem;
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
  
  @media (max-width: 768px) {
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