<!-- DocumentCard.vue -->
<template>
    <div class="document-card">
      <span :class="['status-badge', document.isAnalysed ? 'traiter' : 'non-traiter']">
        <i :class="document.isAnalysed ? 'bi bi-check-circle' : 'bi bi-x-circle'"></i>
      </span>
      <div class="document-content">
        <div class="document-icon">
          <i class="bi bi-file-earmark-text"></i>
        </div>
        <div class="document-info">
          <h3 class="document-title">{{ document.name }}</h3>
          <p class="document-description" :title="document.description">{{ document.description }}</p>
          <p class="document-date">{{ formatDate(document.uploadDate) }}</p>
        </div>
      </div>
      <div class="document-actions">
        <button class="action-btn view-btn" @click="$emit('navigate-to-content', document)">
          <i class="bi bi-eye"></i>
        </button>
        <button class="action-btn download-btn" @click="$emit('download-document', document)">
          <i class="bi bi-download"></i>
        </button>
        <button class="action-btn delete-btn" @click="$emit('delete-document', document.id)">
          <i class="bi bi-trash"></i>
        </button>
      </div>
    </div>
  </template>
  
  <script>
  export default {
    name: 'DocumentCard',
    props: {
      document: {
        type: Object,
        required: true
      }
    },
    emits: ['navigate-to-content', 'download-document', 'delete-document'],
    methods: {
      formatDate(dateString) {
        const date = new Date(dateString)
        return date.toLocaleDateString('fr-FR', {
          year: 'numeric',
          month: 'short',
          day: 'numeric'
        })
      }
    }
  }
  </script>
  
  <style scoped>
  .document-card {
    position: relative;
    background: white;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
    overflow: hidden;
    transition: all 0.3s ease;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
  }
  
  .document-card:hover {
    transform: translateY(-5px);
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
  }
  
  .document-content {
    padding: 1.5rem;
    display: flex;
    gap: 1rem;
  }
  
  .document-icon {
    font-size: 2rem;
    display: flex;
    align-items: center;
    justify-content: center;
  }
  
  .document-info {
    flex: 1;
  }
  
  .document-title {
    margin: 0 0 0.5rem 0;
    font-size: 1.1rem;
    font-weight: 600;
    color: #2c3e50;
    word-break: break-word;
  }
  
  .document-description {
    margin: 0 0 0.75rem 0;
    font-size: 0.9rem;
    color: #6c757d;
    display: -webkit-box;
    -webkit-box-orient: vertical;
    overflow: hidden;
    text-overflow: ellipsis;
    max-height: 2.2em;
    line-height: 1.35;
    position: relative;
  }
  
  .document-description::after {
    content: '';
    position: absolute;
    bottom: 0;
    right: 0;
    width: 40px;
    height: 1.35em;
    background: linear-gradient(to right, transparent, white);
    pointer-events: none;
  }
  
  .document-date {
    font-size: 0.8rem;
    color: #adb5bd;
    margin: 0;
  }
  
  .document-actions {
    display: flex;
    border-top: 1px solid #f0f0f0;
    padding: 0.75rem;
    gap: 0.5rem;
  }
  
  .action-btn {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 0.5rem;
    border: none;
    border-radius: 6px;
    background: none;
    color: #6c757d;
    cursor: pointer;
    transition: all 0.2s ease;
  }
  
  .action-btn:hover {
    background-color: #f8f9fa;
  }
  
  .view-btn:hover {
    color: #0d6efd;
  }
  
  .download-btn:hover {
    color: #198754;
  }
  
  .delete-btn:hover {
    color: #dc3545;
  }
  
  .status-badge {
    position: absolute;
    top: 1rem;
    right: 1rem;
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.25rem 0.75rem;
    border-radius: 20px;
    font-size: 0.875rem;
    font-weight: 500;
    z-index: 1;
  }
  
  .status-badge.traiter {
    background-color: #d4edda;
    color: #155724;
  }
  
  .status-badge.non-traiter {
    background-color: #f8d7da;
    color: #721c24;
  }
  
  .status-badge i {
    font-size: 1rem;
  }
  </style>7