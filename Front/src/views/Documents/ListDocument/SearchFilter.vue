<!-- SearchFilter.vue -->
<template>
    <div class="search-container">
      <div class="search-input">
        <i class="bi bi-search"></i>
        <input
          type="text"
          :value="searchQuery"
          placeholder="Rechercher des documents..."
          @input="$emit('update:search-query', $event.target.value)"
        />
        <button v-if="searchQuery" @click="$emit('clear-search')" class="clear-btn">
          <i class="bi bi-x"></i>
        </button>
      </div>
      <div class="filter-buttons">
        <button
          :class="['filter-btn', currentFilter === 'all' ? 'active' : '']"
          @click="$emit('update:filter', 'all')"
        >
          <i class="bi bi-files"></i>
          Tous
        </button>
        <button
          :class="['filter-btn', currentFilter === 'traiter' ? 'active' : '']"
          @click="$emit('update:filter', 'traiter')"
        >
          <i class="bi bi-check-circle"></i>
          Traité
        </button>
        <button
          :class="['filter-btn', currentFilter === 'non-traiter' ? 'active' : '']"
          @click="$emit('update:filter', 'non-traiter')"
        >
          <i class="bi bi-x-circle"></i>
          Non Traité
        </button>
      </div>
    </div>
  </template>
  
  <script>
  export default {
    name: 'SearchFilter',
    props: {
      searchQuery: {
        type: String,
        required: true
      },
      currentFilter: {
        type: String,
        required: true
      }
    },
    emits: ['update:search-query', 'update:filter', 'clear-search']
  }
  </script>
  
  <style scoped>
  .search-container {
    display: flex;
    align-items: center;
    gap: 1rem;
    margin-bottom: 2rem;
    flex-wrap: wrap;
  }
  
  .search-input {
    position: relative;
    flex: 1;
    min-width: 250px;
    max-width: 500px;
  }
  
  .search-input input {
    width: 100%;
    padding: 0.75rem 1rem 0.75rem 2.5rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 1rem;
    transition: all 0.3s ease;
  }
  
  .search-input input:focus {
    outline: none;
    border-color: #0d6efd;
    box-shadow: 0 0 0 3px rgba(13, 110, 253, 0.1);
  }
  
  .search-input i {
    position: absolute;
    left: 1rem;
    top: 50%;
    transform: translateY(-50%);
    color: #6c757d;
  }
  
  .clear-btn {
    position: absolute;
    right: 1rem;
    top: 50%;
    transform: translateY(-50%);
    background: none;
    border: none;
    color: #6c757d;
    cursor: pointer;
    font-size: 1.1rem;
  }
  
  .filter-buttons {
    display: flex;
    gap: 0.5rem;
  }
  
  .filter-btn {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    background: white;
    color: #666;
    font-size: 0.9rem;
    cursor: pointer;
    transition: all 0.3s ease;
  }
  
  .filter-btn i {
    font-size: 1rem;
  }
  
  .filter-btn:hover {
    background: #f8f9fa;
    border-color: #0d6efd;
    color: #0d6efd;
  }
  
  .filter-btn.active {
    background: #0d6efd;
    border-color: #0d6efd;
    color: white;
  }
  
  @media (max-width: 768px) {
    .search-container {
      flex-direction: column;
      align-items: stretch;
    }
  
    .search-input {
      max-width: none;
    }
  
    .filter-buttons {
      justify-content: center;
      flex-wrap: wrap;
    }
  }
  </style>