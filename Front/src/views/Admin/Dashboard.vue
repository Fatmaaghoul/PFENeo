<template>
  <div class="dashboard-container">
    <!-- Stats Cards -->
    <div class="stats-grid">
      <div class="stat-card">
        <div class="stat-icon users">
          <span class="icon">👥</span>
        </div>
        <div class="stat-content">
          <h3 class="stat-value">{{ stats.totalUsers }}</h3>
          <p class="stat-label">Total Users</p>
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
          <p class="stat-label">Total Documents</p>
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
          <p class="stat-label">Storage Used</p>
        </div>
        <div class="stat-trend neutral">
          <span class="trend-icon">→</span>
          <span class="trend-value">{{ stats.storageGrowth }}%</span>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon activity">
          <span class="icon">📊</span>
        </div>
        <div class="stat-content">
          <h3 class="stat-value">{{ stats.activeUsers }}</h3>
          <p class="stat-label">Active Users</p>
        </div>
        <div class="stat-trend positive">
          <span class="trend-icon">↑</span>
          <span class="trend-value">+{{ stats.activityGrowth }}%</span>
        </div>
      </div>
    </div>

    <!-- Charts Section -->
    <div class="charts-grid">
      <div class="chart-card">
        <div class="chart-header">
          <h3>Document Activity</h3>
          <div class="chart-actions">
            <button class="chart-action-btn">Week</button>
            <button class="chart-action-btn active">Month</button>
            <button class="chart-action-btn">Year</button>
          </div>
        </div>
        <div class="chart-content">
          <!-- Placeholder for chart -->
          <div class="chart-placeholder">
            <div class="chart-bars">
              <div v-for="(value, index) in chartData" :key="index" 
                   class="chart-bar" 
                   :style="{ height: `${value}%` }">
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="chart-card">
        <div class="chart-header">
          <h3>Storage Usage</h3>
          <div class="chart-legend">
            <span class="legend-item">
              <span class="legend-color" style="background: #3498db"></span>
              Used
            </span>
            <span class="legend-item">
              <span class="legend-color" style="background: #e0e0e0"></span>
              Free
            </span>
          </div>
        </div>
        <div class="chart-content">
          <!-- Placeholder for pie chart -->
          <div class="pie-chart-placeholder">
            <div class="pie-chart">
              <div class="pie-segment" :style="{ 
                '--percentage': `${(stats.totalStorage / stats.maxStorage) * 100}%`,
                '--color': '#3498db'
              }"></div>
            </div>
            <div class="pie-chart-center">
              <span class="pie-value">{{ Math.round((stats.totalStorage / stats.maxStorage) * 100) }}%</span>
              <span class="pie-label">Used</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Recent Activity -->
    <div class="activity-card">
      <div class="activity-header">
        <h3>Recent Activity</h3>
        <button class="view-all-btn">View All</button>
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
import { ref, onMounted } from 'vue'

// Mock data for demonstration
const stats = ref({
  totalUsers: 1234,
  userGrowth: 12,
  totalDocuments: 5678,
  documentGrowth: 8,
  totalStorage: 256,
  maxStorage: 1024,
  storageGrowth: 5,
  activeUsers: 789,
  activityGrowth: 15
})

const chartData = ref([65, 45, 75, 50, 85, 60, 70, 55, 80, 65, 75, 60])

const recentActivities = ref([
  {
    id: 1,
    type: 'upload',
    description: 'John Doe uploaded a new document',
    timestamp: new Date(Date.now() - 1000 * 60 * 5) // 5 minutes ago
  },
  {
    id: 2,
    type: 'edit',
    description: 'Sarah Smith edited document "Project Plan"',
    timestamp: new Date(Date.now() - 1000 * 60 * 15) // 15 minutes ago
  },
  {
    id: 3,
    type: 'delete',
    description: 'Mike Johnson deleted a document',
    timestamp: new Date(Date.now() - 1000 * 60 * 30) // 30 minutes ago
  },
  {
    id: 4,
    type: 'share',
    description: 'Emily Brown shared a document with the team',
    timestamp: new Date(Date.now() - 1000 * 60 * 60) // 1 hour ago
  }
])

const formatStorage = (value) => {
  return `${value} GB`
}

const formatTime = (timestamp) => {
  const now = new Date()
  const diff = now - timestamp
  const minutes = Math.floor(diff / 1000 / 60)
  
  if (minutes < 60) {
    return `${minutes} minutes ago`
  } else if (minutes < 1440) {
    return `${Math.floor(minutes / 60)} hours ago`
  } else {
    return `${Math.floor(minutes / 1440)} days ago`
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

onMounted(() => {
  // Here you would typically fetch real data from your API
  // For now we're using mock data
})
</script>

<style scoped>
.dashboard-container {
  padding: 2rem;
  max-width: 1400px;
  margin: 0 auto;
}

/* Stats Grid */
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
.stat-icon.activity { background: rgba(241, 196, 15, 0.1); }

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

/* Charts Grid */
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

.chart-actions {
  display: flex;
  gap: 0.5rem;
}

.chart-action-btn {
  padding: 0.25rem 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  background: none;
  cursor: pointer;
  font-size: 0.875rem;
  color: #666;
}

.chart-action-btn.active {
  background: #3498db;
  color: white;
  border-color: #3498db;
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

.chart-placeholder {
  height: 100%;
  display: flex;
  align-items: flex-end;
  gap: 8px;
  padding: 1rem 0;
}

.chart-bars {
  display: flex;
  align-items: flex-end;
  gap: 8px;
  height: 100%;
  width: 100%;
}

.chart-bar {
  flex: 1;
  background: #3498db;
  border-radius: 4px;
  transition: height 0.3s ease;
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

/* Activity Card */
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
}
</style> 