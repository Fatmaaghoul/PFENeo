<template>
  <div class="dashboard-container">
    <!-- Cartes de statistiques -->
    <div class="stats-grid">
      <div class="stat-card">
        <div class="stat-icon users">
          <span class="icon">👥</span>
        </div>
        <div class="stat-content">
          <h3 class="stat-value">{{ stats.totalUsers }}</h3>
          <p class="stat-label">Utilisateurs totaux</p>
        </div>
        <div class="stat-trend positive">
          <span class="trend-icon">↑</span>
          <span class="trend-value">+{{ stats.userGrowth }}%</span>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon documents">
          <span class="icon">📄</span>
        </div>
        <div class="stat-content">
          <h3 class="stat-value">{{ stats.totalDocuments }}</h3>
          <p class="stat-label">Documents totaux</p>
        </div>
        <div class="stat-trend positive">
          <span class="trend-icon">↑</span>
          <span class="trend-value">+{{ stats.documentGrowth }}%</span>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon storage">
          <span class="icon">💾</span>
        </div>
        <div class="stat-content">
          <h3 class="stat-value">{{ formatStorage(stats.totalStorage) }}</h3>
          <p class="stat-label">Stockage utilisé</p>
        </div>
        <div class="stat-trend neutral">
          <span class="trend-icon">→</span>
          <span class="trend-value">{{ stats.storageGrowth }}%</span>
        </div>
      </div>
    </div>

    <!-- Section des graphiques -->
    <div class="charts-grid">
      <div class="chart-card">
        <div class="chart-header">
          <h3>Activité des documents</h3>
        </div>
        <div class="chart-content">
          <div class="chart-container">
            <div class="y-axis">
              <div v-for="label in yAxisLabels" :key="label" class="y-axis-label">
                {{ label }}
              </div>
            </div>
            <div class="chart-placeholder">
              <div class="chart-bars">
                <div 
                  v-for="(item, index) in chartData" 
                  :key="index" 
                  class="chart-bar" 
                  :style="{ height: `${(item.count / maxCount) * 100}%` }"
                >
                  <span class="bar-value">{{ item.count }}</span>
                </div>
              </div>
              <div class="x-axis">
                <div 
                  v-for="(item, index) in chartData" 
                  :key="index" 
                  class="x-axis-label"
                >
                  {{ item.label }}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="chart-card">
        <div class="chart-header">
          <h3>Utilisation du stockage</h3>
          <div class="chart-legend">
            <span class="legend-item">
              <span class="legend-color" style="background: #3498db"></span>
              Utilisé
            </span>
            <span class="legend-item">
              <span class="legend-color" style="background: #e0e0e0"></span>
              Libre
            </span>
          </div>
        </div>
        <div class="chart-content">
          <div class="pie-chart-placeholder">
            <div class="pie-chart">
              <div class="pie-segment" :style="{ 
                '--percentage': `${storagePercentage}%`,
                '--color': '#3498db'
              }"></div>
            </div>
            <div class="pie-chart-center">
              <span class="pie-value">{{ storagePercentage }}%</span>
              <span class="pie-label">Utilisé</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Activité récente -->
    <div class="activity-card">
      <div class="activity-header">
        <h3>Activité récente</h3>
        <button class="view-all-btn" @click="toggleShowAll">
          {{ showAllActivities ? 'Voir moins' : 'Voir tout' }}
        </button>
      </div>
      <div class="activity-list">
        <div v-for="activity in recentActivities" :key="activity.id" class="activity-item">
          <div class="activity-icon" :class="activity.type">
            <span class="icon">{{ getActivityIcon(activity.type) }}</span>
          </div>
          <div class="activity-details">
            <p class="activity-text">{{ activity.description }}</p>
            <span class="activity-time">{{ formatTime(activity.timestamp) }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'

const apiBaseUrl = 'https://localhost:7036'

// État réactif
const stats = ref({
  totalUsers: 0,
  userGrowth: 0,
  totalDocuments: 0,
  documentGrowth: 0,
  totalStorage: 0,
  maxStorage: 10,
  storageGrowth: 0
})

const chartData = ref([])
const allActivities = ref([]) // Stocke toutes les activités
const showAllActivities = ref(false) // Contrôle l'affichage complet ou limité
const maxCount = ref(0)

// Propriété calculée pour les activités affichées
const recentActivities = computed(() => {
  const activities = allActivities.value
    .filter(activity => activity.timestamp && !isNaN(new Date(activity.timestamp).getTime()))
    .sort((a, b) => new Date(b.timestamp) - new Date(a.timestamp))
  
  return showAllActivities.value ? activities : activities.slice(0, 4)
})

// Labels de l'axe Y calculés
const yAxisLabels = computed(() => {
  if (maxCount.value === 0) return []
  const step = Math.ceil(maxCount.value / 5) || 1
  return Array.from({ length: 6 }, (_, i) => Math.round(i * step)).reverse()
})

// Propriété calculée pour le pourcentage du stockage
const storagePercentage = computed(() => {
  if (stats.value.maxStorage === 0 || stats.value.totalStorage < 0) return 0
  return Math.round((stats.value.totalStorage / stats.value.maxStorage) * 100)
})

// Fonctions utilitaires
const formatStorage = (value) => {
  return `${value} Go`
}

const formatTime = (timestamp) => {
  const now = new Date()
  const diff = now - new Date(timestamp)
  const minutes = Math.floor(diff / 1000 / 60)
  
  if (minutes < 60) {
    return `il y a ${minutes} minute${minutes > 1 ? 's' : ''}`
  } else if (minutes < 1440) {
    const hours = Math.floor(minutes / 60)
    return `il y a ${hours} heure${hours > 1 ? 's' : ''}`
  } else {
    const days = Math.floor(minutes / 1440)
    return `il y a ${days} jour${days > 1 ? 's' : ''}`
  }
}

const getActivityIcon = (type) => {
  const icons = {
    upload: '⬆️',
    edit: '✏️',
    delete: '🗑️',
    share: '📤'
  }
  return icons[type] || '📌'
}

// Basculer entre afficher toutes les activités ou seulement 4
const toggleShowAll = () => {
  showAllActivities.value = !showAllActivities.value
}

// Appels API
const fetchUsers = async () => {
  try {
    const response = await axios.get(`${apiBaseUrl}/api/users/all`)
    const users = response.data
    stats.value.totalUsers = users.length
    stats.value.userGrowth = Math.round(Math.random() * 20)
  } catch (error) {
    console.error('Erreur lors de la récupération des utilisateurs :', error)
  }
}

const fetchCloudinaryStorage = async () => {
  try {
    console.log('Appel à l\'API .NET pour les données de stockage Cloudinary...')
    const response = await axios.get(`${apiBaseUrl}/api/admin/documents/storageCloud`, {
      timeout: 10000
    })
    console.log('Réponse de l\'API .NET:', response.data)
    if (typeof response.data.totalStorage !== 'number' || typeof response.data.maxStorage !== 'number') {
      throw new Error('Données de stockage invalides reçues')
    }
    stats.value.totalStorage = response.data.totalStorage
    stats.value.maxStorage = response.data.maxStorage > 0 ? response.data.maxStorage : 10
    stats.value.storageGrowth = Math.round(Math.random() * 5)
  } catch (error) {
    console.error('Erreur lors de la récupération des données de stockage :', error)
    if (error.response) {
      console.error('Réponse du serveur:', error.response.status, error.response.data)
    }
    stats.value.totalStorage = 0
    stats.value.maxStorage = 10
  }
}

const fetchDocuments = async () => {
  try {
    const response = await axios.get(`${apiBaseUrl}/api/admin/documents`)
    const documents = response.data
    stats.value.totalDocuments = documents.length
    stats.value.documentGrowth = Math.round(Math.random() * 10)
  } catch (error) {
    console.error('Erreur lors de la récupération des documents :', error)
  }
}

const fetchRecentActivities = async () => {
  try {
    const response = await axios.get(`${apiBaseUrl}/api/admin/documents`)
    const documents = response.data
    
    // Transformer les données des documents en format d'activité
    allActivities.value = await Promise.all(
      documents
        .filter(doc => doc.uploadDate && !isNaN(new Date(doc.uploadDate).getTime()))
        .map(async (doc) => {
          try {
            const userResponse = await axios.get(`${apiBaseUrl}/api/admin/documents/${doc.id}/user`)
            const user = userResponse.data
            return {
              id: doc.id,
              type: 'upload',
              description: `${user.userName || 'Utilisateur'} a ajoutée le document "${doc.name}"`,
              timestamp: doc.uploadDate
            }
          } catch (error) {
            console.error(`Erreur lors de la récupération de l'utilisateur pour le document ${doc.id} :`, error)
            return {
              id: doc.id,
              type: 'upload',
              description: `Utilisateur a téléversé le document "${doc.name}"`,
              timestamp: doc.uploadDate
            }
          }
        })
    )
  } catch (error) {
    console.error('Erreur lors de la récupération des activités :', error)
  }
}

const fetchChartData = async () => {
  try {
    const response = await axios.get(`${apiBaseUrl}/api/admin/documents`)
    const documents = response.data
    
    const now = new Date()
    const bins = Array(6).fill().map((_, i) => ({
      startDate: new Date(now.getTime() - (29 - i * 5) * 24 * 60 * 60 * 1000),
      endDate: new Date(now.getTime() - (24 - i * 5) * 24 * 60 * 60 * 1000),
      count: 0,
      label: ''
    }))
    
    bins.forEach(bin => {
      bin.label = bin.startDate.toLocaleDateString('fr-FR', { month: 'short', day: 'numeric' })
    })
    
    documents.forEach(doc => {
      if (!doc.uploadDate) return
      const docDate = new Date(doc.uploadDate)
      if (isNaN(docDate.getTime())) return
      
      for (const bin of bins) {
        if (docDate >= bin.startDate && docDate < bin.endDate) {
          bin.count++
          break
        }
      }
    })
    
    maxCount.value = Math.max(...bins.map(bin => bin.count), 1)
    chartData.value = bins
  } catch (error) {
    console.error('Erreur lors de la récupération des données du graphique :', error)
  }
}

// Récupération initiale des données
onMounted(async () => {
  axios.interceptors.request.use(config => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  })
  
  await Promise.all([
    fetchUsers(),
    fetchDocuments(),
    fetchCloudinaryStorage(),
    fetchRecentActivities(),
    fetchChartData()
  ])
})
</script>

<style scoped>
.dashboard-container {
  padding: 2rem;
  max-width: 1400px;
  margin: 0 auto;
}

/* Grille des statistiques */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.stat-card {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  display: flex;
  align-items: center;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
  position: relative;
  overflow: hidden;
}

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 1rem;
}

.stat-icon .icon {
  font-size: 1.5rem;
}

.stat-icon.users { background: rgba(52, 152, 219, 0.1); }
.stat-icon.documents { background: rgba(46, 204, 113, 0.1); }
.stat-icon.storage { background: rgba(155, 89, 182, 0.1); }

.stat-content {
  flex: 1;
}

.stat-value {
  font-size: 1.5rem;
  font-weight: 600;
  margin: 0;
  color: #2c3e50;
}

.stat-label {
  margin: 0;
  color: #666;
  font-size: 0.875rem;
}

.stat-trend {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  font-size: 0.875rem;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
}

.stat-trend.positive {
  color: #27ae60;
  background: rgba(39, 174, 96, 0.1);
}

.stat-trend.negative {
  color: #e74c3c;
  background: rgba(231, 76, 60, 0.1);
}

.stat-trend.neutral {
  color: #7f8c8d;
  background: rgba(127, 140, 141, 0.1);
}

/* Grille des graphiques */
.charts-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.chart-card {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.chart-header h3 {
  margin: 0;
  font-size: 1.25rem;
  color: #2c3e50;
}

.chart-legend {
  display: flex;
  gap: 1rem;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  color: #666;
}

.legend-color {
  width: 12px;
  height: 12px;
  border-radius: 2px;
}

.chart-content {
  height: 300px;
  position: relative;
}

.chart-container {
  display: flex;
  height: 100%;
  padding: 1rem 0;
}

.y-axis {
  width: 50px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  align-items: flex-end;
  margin-right: 10px;
  font-size: 0.75rem;
  color: #666;
}

.y-axis-label {
  text-align: right;
}

.chart-placeholder {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
  position: relative;
}

.chart-bars {
  display: flex;
  align-items: flex-end;
  gap: 8px;
  height: calc(100% - 30px);
  width: 100%;
}

.chart-bar {
  flex: 1;
  background: #3498db;
  border-radius: 4px;
  transition: height 0.3s ease;
  position: relative;
}

.bar-value {
  position: absolute;
  top: -20px;
  width: 100%;
  text-align: center;
  font-size: 0.75rem;
  color: #2c3e50;
}

.x-axis {
  display: flex;
  justify-content: space-between;
  margin-top: 10px;
  font-size: 0.75rem;
  color: #666;
}

.x-axis-label {
  flex: 1;
  text-align: center;
}

.pie-chart-placeholder {
  position: relative;
  width: 200px;
  height: 200px;
  margin: 0 auto;
}

.pie-chart {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  background: #e0e0e0;
  position: relative;
  overflow: hidden;
}

.pie-segment {
  position: absolute;
  width: 100%;
  height: 100%;
  background: conic-gradient(
    var(--color) 0% var(--percentage),
    transparent var(--percentage) 100%
  );
}

.pie-chart-center {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  text-align: center;
}

.pie-value {
  font-size: 1.5rem;
  font-weight: 600;
  color: #2c3e50;
  display: block;
}

.pie-label {
  font-size: 0.875rem;
  color: #666;
}

/* Carte d'activité */
.activity-card {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.activity-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.activity-header h3 {
  margin: 0;
  font-size: 1.25rem;
  color: #2c3e50;
}

.view-all-btn {
  padding: 0.5rem 1rem;
  border: none;
  background: none;
  color: #3498db;
  cursor: pointer;
  font-size: 0.875rem;
}

.activity-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.activity-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  border-radius: 8px;
  background: #f8f9fa;
}

.activity-icon {
  width: 40px;
  height: 40px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.activity-icon.upload { background: rgba(52, 152, 219, 0.1); }
.activity-icon.edit { background: rgba(46, 204, 113, 0.1); }
.activity-icon.delete { background: rgba(231, 76, 60, 0.1); }
.activity-icon.share { background: rgba(241, 196, 15, 0.1); }

.activity-details {
  flex: 1;
}

.activity-text {
  margin: 0;
  color: #2c3e50;
}

.activity-time {
  font-size: 0.875rem;
  color: #666;
}

@media (max-width: 768px) {
  .dashboard-container {
    padding: 1rem;
  }

  .charts-grid {
    grid-template-columns: 1fr;
  }

  .chart-content {
    height: 250px;
  }

  .activity-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }

  .x-axis-label {
    font-size: 0.65rem;
  }

  .y-axis {
    width: 40px;
    font-size: 0.65rem;
  }
}
</style>