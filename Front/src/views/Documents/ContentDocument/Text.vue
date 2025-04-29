<!-- Text.vue -->
<template>
    <div class="text-section">
      <h2>📝 Texte</h2>
      <div class="text-container">
        <textarea readonly class="form-textarea" :value="text"></textarea>
        <button @click="$emit('copy-text')" class="copy-btn">
          <i :class="['bi', isCopied ? 'bi-check' : 'bi-copy']"></i> Copier
        </button>
      </div>
    </div>
  </template>
  
  <script setup>
  import { ref } from 'vue'
  
  defineProps({
    text: {
      type: String,
      required: true
    }
  })
  
  defineEmits(['copy-text'])
  
  const isCopied = ref(false)
  </script>
  
  <style scoped>
  .text-section {
    margin-top: 40px;
    width: 60%;
  }
  
  .text-section h2 {
    margin-bottom: 20px;
    color: #333;
    font-size: 1.5rem;
  }
  
  .text-container {
    position: relative;
    background: #fefefe;
    padding: 20px;
    border-radius: 12px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  }
  
  .text-container textarea {
    width: 100%;
    padding: 12px;
    border-radius: 8px;
    border: 1px solid #ddd;
    font-size: 16px;
    resize: none;
    background-color: #f8f9fa;
    cursor: default;
    min-height: 600px;
  }
  
  .copy-btn {
    position: absolute;
    top: 20px;
    right: 20px;
    background-color: #4caf50;
    color: white;
    padding: 8px 16px;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    font-size: 14px;
    display: flex;
    align-items: center;
    gap: 8px;
    transition: background-color 0.3s;
  }
  
  .copy-btn:hover {
    background-color: #45a049;
  }
  </style>