<template>
    <div class="images-section">
      <h2>🖼️ Images extraites</h2>
      <div class="images-grid" v-if="images.length > 0">
        <div class="image-card" v-for="img in images" :key="img.id">
          <div class="image-card-content">
            <div class="image-container">
              <img :src="img.fileUrl" alt="Image du document" />
              <button class="zoom-btn" @click="$emit('show-full-image', img.fileUrl)">
                <i class="bi bi-zoom-in"></i>
              </button>
            </div>
            <div class="image-description">
              <p>{{ img.description ? img.description : 'Aucune description disponible' }}</p>
            </div>
          </div>
        </div>
      </div>
      <p v-else class="no-images">Aucune image trouvée pour ce document.</p>
    </div>
  </template>
  
  <script setup>
  defineProps({
    images: {
      type: Array,
      required: true
    }
  })
  
  defineEmits(['show-full-image'])
  </script>
  
  <style scoped>
  .images-section {
    margin-top: 40px;
  }
  
  .images-section h2 {
    margin-bottom: 20px;
    color: #333;
    font-size: 1.5rem;
  }
  
  .images-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(45%, 1fr)); /* Cartes occupent ~moitié de l'écran */
    gap: 20px;
  }
  
  .image-card {
    background: #fefefe;
    border-radius: 12px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    overflow: hidden;
    transition: transform 0.3s, box-shadow 0.3s;
  }
  
  .image-card:hover {
    transform: translateY(-5px);
    box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
  }
  
  .image-card-content {
    display: flex;
    align-items: stretch;
    gap: 15px;
  }
  
  .image-container {
    flex: 0 0 250px; /* Taille de l'image */
    height: 250px; /* Hauteur correspondante */
    overflow: hidden;
    position: relative;
  }
  
  .image-container img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    transition: transform 0.3s;
  }
  
  .image-card:hover .image-container img {
    transform: scale(1.05);
  }
  
  .image-description {
    flex: 1;
    padding: 15px;
    background: #f8f9fa;
    display: flex;
    flex-direction: column;
    justify-content: center;
  }
  
  .image-description p {
    margin: 0;
    font-size: 14px;
    color: #333;
    line-height: 1.5;
    word-break: break-word; /* Gérer les longs mots */
  }
  
  .no-images {
    text-align: center;
    padding: 30px;
    background: #f8f9fa;
    border-radius: 8px;
    color: #6c757d;
  }
  
  .zoom-btn {
    position: absolute;
    top: 10px;
    right: 10px;
    background-color: rgba(255, 255, 255, 0.9);
    border: none;
    border-radius: 50%;
    width: 36px;
    height: 36px;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: all 0.3s ease;
    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.2);
  }
  
  .zoom-btn:hover {
    background-color: white;
    transform: scale(1.1);
  }
  
  .zoom-btn i {
    font-size: 1.2rem;
    color: #333;
  }
  
  @media (max-width: 1024px) {
    .images-grid {
      grid-template-columns: 1fr; /* Une seule carte par ligne sur écrans moyens/petits */
    }
  
    .image-card-content {
      flex-direction: column;
    }
  
    .image-container {
      flex: none;
      width: 100%;
      height: 300px; /* Ajuster la hauteur pour les écrans plus petits */
    }
  
    .image-description {
      width: 100%;
    }
  }
  
  @media (max-width: 768px) {
    .image-container {
      height: 250px;
    }
  }
  </style>