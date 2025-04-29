<!-- Preview.vue -->
<template>
    <div class="document-viewer">
      <!-- <h2>Aperçu du document</h2> -->
      <div class="document-container">
        <div v-if="document.fileUrl" class="document-frame">
          <iframe :src="document.fileUrl" frameborder="0" width="100%" height="100%"></iframe>
        </div>
        <div v-else class="no-document">
          <i class="bi bi-file-earmark-text"></i>
          <p>Aucun aperçu disponible</p>
        </div>
      </div>
    </div>
  </template>
  
  <script setup>
  defineProps({
    document: {
      type: Object,
      required: true
    }
  })
  </script>
  
  <style scoped>
  .document-viewer {
    flex: 1;
    min-width: 0;
    min-height: 1000px;
  }
  
  .document-viewer h2 {
    margin-top: 0;
    margin-bottom: 15px;
    color: #333;
    font-size: 1.5rem;
  }
  
  .document-container {
    background: #fefefe;
    border-radius: 12px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    height: 600px;
    overflow: hidden;
  }
  
  .document-frame {
    width: 100%;
    height: 100%;
  }
  
  .no-document {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 100%;
    color: #6c757d;
  }
  
  .no-document i {
    font-size: 3rem;
    margin-bottom: 15px;
  }
  
  @media (max-width: 992px) {
    .document-container {
      height: 500px;
    }
  }
  
  @media (max-width: 768px) {
    .document-container {
      height: 400px;
    }
  }
  </style>