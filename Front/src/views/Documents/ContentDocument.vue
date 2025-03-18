<template>
    <div>
      <br><br><br><br><br><br>
  
      <!-- Afficher les images associées au document -->
      <div v-if="images.length > 0" class="space-y-6 mt-8 ml-10 w-50">
        <div v-for="(image, index) in images" :key="index" class=" image-container">
          <hr>
            <img :src="image.fileUrl" :alt="'Image ' + (index + 1)" class="mt-10 document-image"/>
        </div>
      </div>
      <div v-else>
        <p>Aucune image disponible pour ce document.</p>
      </div>
    </div>
  </template>
  
  
  
  <script>
  import axios from 'axios';
  export default {
    props: ['id'],
  
    data() {
      return {
        documentId: this.id || this.$route.params.id, 
        images: [],  
        loading: true, 
      };
    },
  
    methods: {
      async fetchDocuments() {
        try {
          const response = await axios.get(`api/images/${this.documentId}`);
          this.images = response.data; 
          console.log(this.images);
        } catch (error) {
          console.error("Erreur lors de la récupération des images", error);
        } finally {
          this.loading = false;
        }
      },
    },
  
    mounted() {
      this.fetchDocuments();  
     
    },
  };
  </script>
  