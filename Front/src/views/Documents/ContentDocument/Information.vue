<template>
    <div class="document-info">
      <h2>Informations du document</h2>
      <div class="info-card">
        <div class="form-group">
          <label>Nom :</label>
          <p class="info-text">{{ document.name || 'Non spécifié' }}</p>
        </div>
  
        <div class="form-group">
          <label>Description :</label>
          <p class="info-text">{{ document.description || 'Aucune description' }}</p>
        </div>
  
        <div class="form-group">
          <label>Date de création :</label>
          <p class="info-text">{{ formatDate(document.uploadDate) }}</p>
        </div>
  
        <div class="form-group">
          <label>Statut :</label>
          <p class="info-text">
            <span :class="['status-badge', document.isExtracted ? 'extracted' : 'pending']">
              {{ document.isExtracted ? 'Extrait' : 'En attente d\'extraction' }}
            </span>
            <span :class="['status-badge', document.isAnalysed ? 'traiter' : 'non-traiter']">
              {{ document.isAnalysed ? 'Traité' : 'Non traité' }}
            </span>
          </p>
        </div>
  
        <div class="button-group">
          <button @click="openUpdateModal" class="update-btn">
            <i class="bi bi-save"></i> Mettre à jour
          </button>
          <button
            @click="$emit('analyse-document')"
            class="analyse-btn"
            :disabled="documentStore.isAnalysing"
          >
            <i class="bi bi-gear"></i> Analyser
          </button>
        </div>
      </div>
  
      <!-- Modal pour mise à jour -->
      <div v-if="showUpdateModal" class="modal-overlay" @click="closeUpdateModal">
        <div class="modal-content" @click.stop>
          <div class="modal-header">
            <h2>Modifier les informations</h2>
            <button class="close-btn" @click="closeUpdateModal">
              <i class="bi bi-x"></i>
            </button>
          </div>
          <div class="modal-body">
            <div class="form-group">
              <label>Nom :</label>
              <input v-model="tempDocument.name" class="form-input" placeholder="Entrez le nom" />
              <span v-if="formSubmitted && !tempDocument.name.trim()" class="error-message">
                Le nom est requis.
              </span>
            </div>
            <div class="form-group">
              <label>Description :</label>
              <textarea
                v-model="tempDocument.description"
                class="form-textarea"
                placeholder="Entrez la description"
              ></textarea>
              <span v-if="formSubmitted && !tempDocument.description.trim()" class="error-message">
                La description est requise.
              </span>
            </div>
          </div>
          <div class="modal-footer">
            <button class="cancel-btn" @click="closeUpdateModal">Annuler</button>
            <button class="submit-btn" @click="submitUpdate">Enregistrer</button>
          </div>
        </div>
      </div>
    </div>
  </template>
  
  <script setup>
  import { ref } from 'vue'
  import { useDocumentStore } from '@/Store/analysis'
  
  const props = defineProps({
    document: {
      type: Object,
      required: true
    }
  })
  
  const emit = defineEmits(['update-document', 'analyse-document'])
  
  const documentStore = useDocumentStore()
  const showUpdateModal = ref(false)
  const formSubmitted = ref(false)
  const tempDocument = ref({
    name: '',
    description: ''
  })
  
  const formatDate = (dateString) => {
    if (!dateString) return 'Non disponible'
    const date = new Date(dateString)
    return date.toLocaleDateString('fr-FR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  }
  
  const openUpdateModal = () => {
    tempDocument.value = {
      name: props.document.name || '',
      description: props.document.description || ''
    }
    showUpdateModal.value = true
    formSubmitted.value = false
  }
  
  const closeUpdateModal = () => {
    showUpdateModal.value = false
    tempDocument.value = { name: '', description: '' }
    formSubmitted.value = false
  }
  
  const submitUpdate = () => {
    formSubmitted.value = true
    if (!tempDocument.value.name.trim() || !tempDocument.value.description.trim()) {
      return
    }
    emit('update-document', {
      name: tempDocument.value.name,
      description: tempDocument.value.description
    })
    closeUpdateModal()
  }
  </script>
  
  <style scoped>
  .document-info {
    flex: 1;
    min-width: 0;
  }
  
  .document-info h2 {
    margin-top: 0;
    margin-bottom: 15px;
    color: #333;
    font-size: 1.5rem;
  }
  
  .info-card {
    background: #fefefe;
    padding: 20px;
    border-radius: 12px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  }
  
  .form-group {
    margin-bottom: 20px;
  }
  
  .form-group label {
    display: block;
    margin-bottom: 8px;
    font-weight: 600;
    color: #333;
  }
  
  .form-input,
  .form-textarea {
    width: 100%;
    padding: 12px;
    border-radius: 8px;
    border: 1px solid #ddd;
    font-size: 16px;
    transition: border-color 0.3s;
  }
  
  .form-input:focus,
  .form-textarea:focus {
    outline: none;
    border-color: #4caf50;
  }
  
  .form-textarea {
    resize: vertical;
    min-height: 100px;
  }
  
  .info-text {
    margin: 0;
    padding: 12px;
    background-color: #f8f9fa;
    border-radius: 8px;
    color: #333;
    word-break: break-word;
  }
  
  .status-badge {
    display: inline-block;
    padding: 5px 10px;
    border-radius: 20px;
    font-size: 14px;
    font-weight: 600;
  }
  
  .status-badge.extracted {
    background-color: #d4edda;
    color: #155724;
  }
  
  .status-badge.pending {
    background-color: #fff3cd;
    color: #856404;
  }
  
  .status-badge.traiter {
    background-color: #cce5ff;
    color: #004085;
    margin-left: 10px;
  }
  
  .status-badge.non-traiter {
    background-color: #f8d7da;
    color: #721c24;
    margin-left: 10px;
  }
  
  .button-group {
    display: flex;
    justify-content: space-between;
    gap: 10px;
  }
  
  .update-btn {
    background-color: #4caf50;
    color: white;
    padding: 12px 20px;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    font-size: 16px;
    display: flex;
    align-items: center;
    gap: 8px;
    transition: background-color 0.3s;
  }
  
  .update-btn:hover {
    background-color: #45a049;
  }
  
  .analyse-btn {
    background-color: #1bc0c8;
    color: white;
    padding: 12px 20px;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    font-size: 16px;
    display: flex;
    align-items: center;
    gap: 8px;
  }
  
  .analyse-btn:hover {
    background-color: #17a2b8;
  }
  
  .analyse-btn:disabled {
    background-color: #6c757d;
    cursor: not-allowed;
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
    max-width: 500px;
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
  
  .modal-footer {
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
    padding: 1.5rem;
    border-top: 1px solid #f0f0f0;
  }
  
  .cancel-btn {
    background: none;
    border: 1px solid #ddd;
    color: #666;
    padding: 0.75rem 1.5rem;
    border-radius: 8px;
    font-size: 1rem;
    cursor: pointer;
  }
  
  .submit-btn {
    background: #4caf50;
    color: white;
    border: none;
    padding: 0.75rem 1.5rem;
    border-radius: 8px;
    font-size: 1rem;
    cursor: pointer;
  }
  
  .submit-btn:hover {
    background-color: #45a049;
  }
  
  .error-message {
    color: #dc3545;
    font-size: 0.875rem;
    margin-top: 0.25rem;
  }
  </style>