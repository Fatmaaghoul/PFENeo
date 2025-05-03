<template>
    <div class="consult-container">
      <!-- Header avec bouton retour -->
      <div class="header">
        <button @click="$router.go(-1)" class="back-button">
          <i class="bi bi-arrow-left"></i> Retour
        </button>
        <h1>{{ document.name }}</h1>
        <div class="document-status">
          <span :class="['status-badge', document.isExtracted ? 'extracted' : 'pending']">
            {{ document.isExtracted ? 'Extrait' : 'Non extrait' }}
          </span>
          <span :class="['status-badge', document.isAnalysed ? 'analysed' : 'not-analysed']">
            {{ document.isAnalysed ? 'Analysé' : 'Non analysé' }}
          </span>
        </div>
      </div>
  
      <!-- Barre d'onglets -->
      <div class="tabs">
        <button 
          v-for="tab in tabs" 
          :key="tab.id"
          @click="activeTab = tab.id"
          :class="{ 'active': activeTab === tab.id }"
        >
          <i :class="tab.icon"></i> {{ tab.label }}
        </button>
      </div>
  
      <!-- Contenu des onglets -->
      <div class="tab-content">
        <!-- Onglet Consultation -->
        <div v-if="activeTab === 'consult'" class="consult-tab">
          <div class="document-preview">
            <iframe 
              v-if="document.fileUrl"
              :src="document.fileUrl"
              frameborder="0"
              width="100%"
              height="600px"
            ></iframe>
            <div v-else class="no-preview">
              <i class="bi bi-file-earmark-text"></i>
              <p>Aucun aperçu disponible</p>
            </div>
          </div>
        </div>
  
  <!-- Onglet Édition -->
  <div v-if="activeTab === 'edit'" class="edit-tab" :key="'edit-tab-' + isEditing">    <!-- Mode lecture -->
      <!-- Mode lecture -->
      <div v-if="!isEditing" class="edit-mode">  <!-- Utilisez la même classe edit-mode -->
  <div class="form-group">
    <label>Nom du document</label>
    <p class="form-input-static">{{ document.name }}</p>  <!-- Remplacez input par p -->
  </div>
  
  <div class="form-group">
    <label>Description</label>
    <pre class="form-textarea-static">{{ document.description || 'Aucune description' }}</pre>
  </div>
  
  <div class="form-group">
    <label>Date de téléversement</label>
    <p class="form-input-static">{{ formatDate(document.uploadDate) }}</p>
  </div>
  
  <div class="action-buttons">
    <button @click="enableEditing" class="edit-btn">
      <i class="bi bi-pencil"></i> Modifier
    </button>
  </div>
</div>
  
  <!-- Mode édition -->
  <div v-else class="edit-mode">
      <div class="form-group">
        <label>Nom du document</label>
        <input v-model="editForm.name" type="text" class="form-input">
      </div>
      
      <div class="form-group">
        <label>Description</label>
        <textarea v-model="editForm.description" class="form-textarea" rows="5"></textarea>
      </div>
      
      <div class="form-group">
        <label>Date de téléversement</label>
        <p class="info-text">{{ formatDate(document.uploadDate) }}</p>
      </div>
      
      <div class="action-buttons">
        <button @click="cancelEditing" class="cancel-btn">Annuler</button>
        <button @click="updateDocument" class="save-btn">Enregistrer</button>
      </div>
    </div>
  </div>
      
<!-- Onglet Extraction -->
<div v-if="activeTab === 'extract'" class="extract-tab">
  <!-- Section Analyse -->
  <div class="analysis-section">
    <button @click="analyzeDocument" class="analyze-btn" :disabled="isAnalyzing">
      <i class="bi bi-gear" :class="{ 'spin': isAnalyzing }"></i>
      {{ isAnalyzing ? 'Analyse en cours...' : 'Analyser le document' }}
    </button>
    
    <div v-if="analysisResults" class="analysis-results">
      <h3>Résultats de l'analyse :</h3>
      <pre>{{ analysisResults }}</pre>
    </div>
  </div>

  <!-- Section Images -->
  <div class="content-section">
    <h3>Images extraites ({{ images.length }})</h3>
    <div class="images-grid">
      <div v-for="(image, index) in images" :key="index" class="image-card">
        <img :src="image.fileUrl" :alt="'Image ' + (index + 1)">
        <p v-if="image.description">{{ image.description }}</p>
        <p v-else class="no-description">Aucune description</p>
      </div>
    </div>
  </div>
  
  <!-- Section Texte -->
  <div class="content-section">
    <h3>Texte extrait</h3>
    <div class="text-container">
      <textarea readonly class="form-textarea">{{ document.text }}</textarea>
      <button @click="copyText" class="copy-btn">
        <i class="bi bi-copy"></i> Copier tout le texte
      </button>
    </div>
  </div>
</div>
      </div>
    </div>
  </template>
  
  <script>
  import axios from 'axios';
  import Cookies from 'js-cookie';
  
  export default {
    name: 'ConsultDocument',
    data() {
      return {
        document: {
          id: null,
          name: '',
          description: '',
          fileUrl: '',
          uploadDate: null,
          isExtracted: true,
          isAnalysed: false,
          text: null
        },
          isEditing: false,
          editForm: {
        name: '',
        description: ''
      },

        
        images: [],
        activeTab: 'consult',
        tabs: [
          { id: 'consult', label: 'Consultation', icon: 'bi bi-eye' },
          { id: 'edit', label: 'Édition', icon: 'bi bi-pencil' },
          { id: 'extract', label: 'Extraction', icon: 'bi bi-file-earmark-zip' }
        ],
        editForm: {
          name: '',
          description: ''
        },
        isAnalyzing: false,
        analysisResults: null
      };
    },
    created() {
      this.fetchDocument();
    },
    methods: {
        async fetchDocument() {
  try {
    const token = Cookies.get('token');
    const response = await axios.get(`/api/documents/${this.$route.params.id}`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    this.document = response.data;
    
    // Extraction automatique si pas déjà fait
    if (!this.document.isExtracted) {
      await this.extractContent();
    } else {
      this.fetchImages();
    }
    
  } catch (error) {
    console.error('Error fetching document:', error);
    alert('Erreur lors du chargement du document');
  }
},
      async fetchImages() {
        try {
          const token = Cookies.get('token');
          const response = await axios.get(`/api/images/${this.$route.params.id}`, {
            headers: { Authorization: `Bearer ${token}` }
          });
          this.images = response.data;
        } catch (error) {
          console.error('Error fetching images:', error);
        }
      },
      
      async extractContent() {
        try {
          const token = Cookies.get('token');
          await axios.post(`/api/documents/extract/${this.$route.params.id}`, {}, {
            headers: { Authorization: `Bearer ${token}` }
          });
          this.document.isExtracted = true;
          await this.fetchDocument();
          this.fetchImages();
          alert('Contenu extrait avec succès!');
        } catch (error) {
          console.error('Error extracting content:', error);
          alert('Erreur lors de l\'extraction du contenu');
        }
      },
      
      async analyzeDocument() {
        this.isAnalyzing = true;
        try {
          const token = Cookies.get('token');
          const response = await axios.post(`/api/documents/analyze/${this.$route.params.id}`, {}, {
            headers: { Authorization: `Bearer ${token}` }
          });
          this.analysisResults = response.data;
          this.document.isAnalysed = true;
          await this.fetchDocument();
          alert('Analyse terminée avec succès!');
        } catch (error) {
          console.error('Error analyzing document:', error);
          alert('Erreur lors de l\'analyse du document');
        } finally {
          this.isAnalyzing = false;
        }
      },
      enableEditing() {
  console.log("Activation du mode édition");
  this.isEditing = true;
  // Copier les valeurs actuelles dans le formulaire
  this.editForm = {
    name: this.document.name,
    description: this.document.description
  };
  
  // Forcer la mise à jour du DOM si nécessaire
  this.$nextTick(() => {
    // Focus sur le premier champ pour une meilleure UX
    if (this.$refs.nameInput) {
      this.$refs.nameInput.focus();
    }
  });
},
    
    cancelEditing() {
      console.log("Annulation de l'édition"); // Debug
      this.isEditing = false;
    },
  
      
    async updateDocument() {
      try {
        console.log("Tentative de mise à jour"); // Debug
        const token = Cookies.get('token');
        await axios.put(`/api/documents/${this.$route.params.id}`, this.editForm, {
          headers: { Authorization: `Bearer ${token}` }
        });
        
        // Mise à jour des données locales
        this.document.name = this.editForm.name;
        this.document.description = this.editForm.description;
        
        this.isEditing = false;
        alert('Document mis à jour avec succès!');
      } catch (error) {
        console.error('Erreur lors de la mise à jour:', error);
        alert('Échec de la mise à jour: ' + (error.response?.data?.message || error.message));
      }
    },
      
      resetEditForm() {
        this.editForm = {
          name: this.document.name,
          description: this.document.description
        };
      },
      
      async copyText() {
        try {
          await navigator.clipboard.writeText(this.document.text);
          alert('Texte copié dans le presse-papiers!');
        } catch (error) {
          console.error('Error copying text:', error);
          alert('Erreur lors de la copie du texte');
        }
      },
      
      formatDate(dateString) {
        if (!dateString) return 'N/A';
        const date = new Date(dateString);
        return date.toLocaleDateString('fr-FR', {
          year: 'numeric',
          month: 'long',
          day: 'numeric',
          hour: '2-digit',
          minute: '2-digit'
        });
      }
    }
  };
  </script>
  
  <style scoped>
  .consult-container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 20px;
  }
  
  .header {
    display: flex;
    align-items: center;
    gap: 20px;
    margin-bottom: 30px;
    flex-wrap: wrap;
  }
  
  .back-button {
    background: #f0f0f0;
    border: none;
    padding: 8px 15px;
    border-radius: 5px;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 5px;
    color: #405569;
  }
  
  .back-button:hover {
    background: #e0e0e0;
  }
  
  .document-status {
    display: flex;
    gap: 10px;
    margin-left: auto;
  }
  
  .status-badge {
    padding: 5px 10px;
    border-radius: 20px;
    font-size: 0.8rem;
    font-weight: 500;
  }
  
  .status-badge.extracted {
    background: #e8f5e9;
    color: #2e7d32;
  }
  
  .status-badge.pending {
    background: #fff8e1;
    color: #ff8f00;
  }
  
  .status-badge.analysed {
    background: #e3f2fd;
    color: #1565c0;
  }
  
  .status-badge.not-analysed {
    background: #ffebee;
    color: #c62828;
  }
  
  .tabs {
    display: flex;
    border-bottom: 1px solid #ddd;
    margin-bottom: 20px;
  }
  
  .tabs button {
    padding: 10px 20px;
    background: none;
    border: none;
    border-bottom: 3px solid transparent;
    cursor: pointer;
    font-size: 1rem;
    display: flex;
    align-items: center;
    gap: 8px;
    color: #405569;
  }
  
  .tabs button:hover {
    background: #f5f5f5;
  }
  
  .tabs button.active {
    border-bottom-color: #1976d2;
    color: #1976d2;
    font-weight: 500;
  }
  
  .tab-content {
    background: white;
    border-radius: 8px;
    padding: 20px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  }
  
  .document-preview {
    height: 600px;
    border: 1px solid #eee;
    border-radius: 5px;
    overflow: hidden;
  }
  
  .no-preview {
    height: 100%;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    color: #666;
  }
  
  .no-preview i {
    font-size: 3rem;
    margin-bottom: 15px;
  }
  
  .form-group {
    margin-bottom: 20px;
  }
  

  .form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: 500;
  color: #666;
  font-size: 0.9rem;
}
  
.form-input, .form-textarea {
  width: 100%;
  padding: 12px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
}

.form-textarea {
  min-height: 150px;
  resize: vertical;
}

.action-buttons {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
  margin-top: 20px;
}
.cancel-btn {
  padding: 10px 20px;
  background: #f5f5f5;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  color: #405569;

}

.cancel-btn:hover {
  background: #e0e0e0;
}

.save-btn {
  padding: 10px 20px;
  background: #4caf50;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.save-btn:hover {
  background: #3d8b40;
}
  .primary-btn {
    padding: 10px 20px;
    border: none;
    border-radius: 5px;
    cursor: pointer;
    font-size: 1rem;
    display: flex;
    align-items: center;
    gap: 8px;
  }
  
  
  
  .primary-btn {
    background: #1976d2;
    color: white;
  }
  
   .primary-btn:hover {
    background: #1565c0;
  }
  
.analyze-btn {
  padding: 12px 20px;
  background: #4caf50;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 1rem;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  transition: all 0.2s;
}

.analyze-btn:hover:not(:disabled) {
  background: #3d8b40;
}
.analyze-btn:disabled {
  background: #81c784;
  cursor: not-allowed;
}

  
  .extract-first, .not-extracted {
    text-align: center;
    padding: 40px 20px;
    border: 2px dashed #ddd;
    border-radius: 8px;
  }
  
  .images-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
    gap: 20px;
    margin-top: 20px;
  }
  
  .image-card {
    border: 1px solid #eee;
    border-radius: 5px;
    overflow: hidden;
  }
  
  .image-card img {
    width: 100%;
    height: 150px;
    object-fit: cover;
  }
  
  .image-card p {
    padding: 10px;
    margin: 0;
    font-size: 0.9rem;
  }
  
  .no-description {
    color: #999;
    font-style: italic;
  }
  
  .text-container {
    position: relative;
    margin-top: 20px;
  }
  
  .copy-btn {
    position: absolute;
    top: 10px;
    right: 10px;
    padding: 5px 10px;
    background: #f5f5f5;
    border: 1px solid #ddd;
    border-radius: 4px;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 5px;
  }
  
  .copy-btn:hover {
    background: #e0e0e0;
  }
  
  .analysis-results {
  margin-top: 20px;
  padding: 15px;
  background: white;
  border-radius: 6px;
  border: 1px solid #eee;
}

.analysis-results pre {
  white-space: pre-wrap;
  font-family: inherit;
  margin: 0;
}
  
.spin {
  animation: spin 1s linear infinite;
}
  /* Styles pour le mode lecture - DEBUT */
.view-mode {
  padding: 25px;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  max-width: 800px;
  margin: 0 auto;
}

.info-group {
  margin-bottom: 25px;
  padding-bottom: 15px;
  border-bottom: 1px solid #f0f0f0;
}

.info-group:last-child {
  border-bottom: none;
}

.info-group label {
  display: block;
  font-weight: 600;
  color: #2c3e50;
  margin-bottom: 8px;
  font-size: 0.95rem;
}

.info-value {
  padding: 12px 15px;
  background: #f8fafc;
  border-radius: 6px;
  margin: 0;
  border: 1px solid #e2e8f0;
  color: #4a5568;
  line-height: 1.5;
}
.extract-tab {
  padding: 20px;
}
.content-section {
  margin-top: 30px;
}

.edit-btn {
    padding: 10px 20px;
  background: #1976d2;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.edit-btn:hover {
  background: #2779bd;
 
}

.edit-btn:active {
  transform: translateY(0);
}

.edit-btn i {
  font-size: 1.1rem;
}

.edit-tab {
  transition: all 0.3s ease;
}

/* Style cohérent pour les titres */
.edit-tab h3 {
  color: #2c3e50;
  margin-bottom: 20px;
  font-size: 1.2rem;
  border-bottom: 1px solid #eee;
  padding-bottom: 10px;
}
/* Styles pour le mode lecture - FIN */

/* Version responsive */
@media (max-width: 768px) {
  .header {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .document-status {
    margin-left: 0;
    width: 100%;
  }
  
  .tabs {
    overflow-x: auto;
  }
  
  .document-preview {
    height: 400px;
  }
  
  .view-mode {
    padding: 15px;
  }
  
  .info-group {
    margin-bottom: 20px;
  }
  
  .edit-btn {
    width: 100%;
    justify-content: center;
  }
}
  
  @keyframes spin {
    from { transform: rotate(0deg); }
    to { transform: rotate(360deg); }
  }
  
  @media (max-width: 768px) {
    .header {
      flex-direction: column;
      align-items: flex-start;
    }
    
    .document-status {
      margin-left: 0;
      width: 100%;
    }
    
    .tabs {
      overflow-x: auto;
    }
    
    .document-preview {
      height: 400px;
    }
  }
  .analysis-section {
  margin-bottom: 30px;
  padding: 20px;
  background: #f8f9fa;
  border-radius: 8px;
}
  </style>
