<template>
  <div class="text-section">
    <!-- <h2>📝 Texte</h2> -->
    <div class="text-container">
      <textarea readonly class="form-textarea" :value="text" style="color: black;"></textarea>
      <button @click="copyText" class="copy-btn">
        <i :class="['bi', isCopied ? 'bi-check-circle' : 'bi-copy']"></i> Copier
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

// Définir les props et accéder à 'text'
const props = defineProps({
  text: {
    type: String,
    required: true
  }
})

const isCopied = ref(false)

const copyText = async () => {
  if (!props.text) {
    alert('❌ Aucun texte à copier')
    return
  }

  try {
    await navigator.clipboard.writeText(props.text)
    isCopied.value = true
    setTimeout(() => {
      isCopied.value = false
    }, 2000) // Revenir à l'icône initiale après 2 secondes
  } catch (error) {
    console.error('Erreur lors de la copie :', error)
    alert('❌ Échec de la copie du texte. Vérifiez les permissions du presse-papiers.')
  }
}
</script>

<style scoped>
.text-section {
  margin-top: 40px;
  width: 90%;
}

.text-section h2 {
  margin-bottom: 20px;
  color: #333;
  font-size: 1.5rem;
}

.text-container {
  position: relative;
  background: #fefefe;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.text-container textarea {
  width: 100%;
  padding: 20px;
  border-radius: 8px;
  border: 1px solid #ddd;
  font-size: 21.6px;
  resize: none;
  background-color: #ffffff;
  cursor: default;
  min-height: 600px;
}

.copy-btn {
  position: absolute;
  top: 20px;
  right: 20px;
  background-color: #73adeb;
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
  background-color: #3c8fe8;
}
</style>