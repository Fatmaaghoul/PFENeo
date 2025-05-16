<template>
  <div class="text-tab">
    <div class="text-section">
      <h3>Texte extrait</h3>
      <textarea readonly :value="document.text"></textarea>
      <button @click="copyText" class="copy-btn">
        <i class="bi bi-copy"></i> Copier le texte
      </button>
      <div v-if="showTextCopied" class="notification">Texte copié !</div>
    </div>

    <div class="resume-section">
      <div class="resume-header">
        <h3>Résumé du document</h3>
        <button 
          @click="$emit('generate-resumer')" 
          :disabled="isGeneratingResumer"
          class="generate-btn"
        >
          <template v-if="isGeneratingResumer">
            <i class="bi bi-arrow-clockwise spin-icon"></i> Génération...
          </template>
          <template v-else>
            <i class="bi bi-arrow-clockwise"></i>
            {{ document.resumer ? 'Régénérer' : 'Générer' }}
          </template>
        </button>
      </div>
      <div class="resume-content">
        <div v-if="isGeneratingResumer" class="loading-container">
          <div class="loading-spinner"></div>
          <p>Génération du résumé en cours...</p>
        </div>
        <template v-else>
          <div v-if="document.resumer">
            <p class="resume-text">{{ document.resumer }}</p>
            <button @click="copyResume" class="copy-resume-btn">
              <i class="bi bi-copy"></i> Copier le résumé
            </button>
            <div v-if="showResumeCopied" class="notification">Résumé copié !</div>
          </div>
          <p v-else class="no-resume">Aucun résumé disponible</p>
        </template>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  props: {
    document: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      isGeneratingResumer: false,
      showTextCopied: false,
      showResumeCopied: false
    };
  },
  methods: {
    copyText() {
      navigator.clipboard.writeText(this.document.text);
      this.showTextCopied = true;
      setTimeout(() => {
        this.showTextCopied = false;
      }, 2000);
      this.$emit('copy-text');
    },
    copyResume() {
      navigator.clipboard.writeText(this.document.resumer);
      this.showResumeCopied = true;
      setTimeout(() => {
        this.showResumeCopied = false;
      }, 2000);
    }
  }
};
</script>

<style scoped>
.text-tab {
  font-family: 'Segoe UI', Roboto, 'Helvetica Neue', sans-serif;
  max-width: 900px;
  margin: 0 auto;
  padding: 20px;
}

.text-section textarea {
  width: 100%;
  min-height: 300px;
  padding: 15px;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  resize: vertical;
  font-size: 14px;
  line-height: 1.5;
  background: #fafafa;
  transition: border 0.3s ease;
}

.text-section textarea:focus {
  outline: none;
  border-color: #2196F3;
}

.copy-btn {
  margin-top: 12px;
  background: #ffffff;
  border: 1px solid #e0e0e0;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  transition: all 0.2s ease;
}

.copy-btn:hover {
  background: #f5f5f5;
  border-color: #d0d0d0;
}

.copy-btn i {
  font-size: 14px;
}

.resume-section {
  margin-top: 30px;
}

.resume-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.generate-btn {
  background: #2196F3;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  transition: all 0.2s ease;
}

.generate-btn:hover:not(:disabled) {
  background: #0d8bf2;
}

.generate-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.resume-content {
  padding: 20px;
  background: #f8f9fa;
  border-radius: 8px;
  border: 1px solid #e9ecef;
  min-height: 150px;
  position: relative;
}

.no-resume {
  color: #666;
  font-style: italic;
  margin: 0;
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 20px 0;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 3px solid rgba(33, 150, 243, 0.2);
  border-radius: 50%;
  border-top-color: #2196F3;
  animation: spin 1s ease-in-out infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.spin-icon {
  animation: spin 1s linear infinite;
}

.resume-text {
  margin-bottom: 15px;
  line-height: 1.6;
  white-space: pre-wrap;
}

.copy-resume-btn {
  background: #ffffff;
  border: 1px solid #e0e0e0;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  transition: all 0.2s ease;
}

.copy-resume-btn:hover {
  background: #f5f5f5;
  border-color: #d0d0d0;
}

.notification {
  margin-top: 8px;
  padding: 6px 12px;
  background: #4CAF50;
  color: white;
  border-radius: 4px;
  font-size: 13px;
  animation: fadeInOut 2s ease-in-out;
  opacity: 0;
}

@keyframes fadeInOut {
  0% { opacity: 0; }
  20% { opacity: 1; }
  80% { opacity: 1; }
  100% { opacity: 0; }
}
</style>