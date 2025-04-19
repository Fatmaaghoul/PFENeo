/**
 * Formate une date en format lisible
 * @param {string|Date} date - La date à formater
 * @returns {string} La date formatée
 */
export const formatDate = (date) => {
  if (!date) return 'Non disponible'
  
  const d = new Date(date)
  return d.toLocaleDateString('fr-FR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

/**
 * Formate un temps en secondes en format lisible
 * @param {number} seconds - Le temps en secondes
 * @returns {string} Le temps formaté
 */
export const formatTime = (seconds) => {
  if (!seconds) return '0s'
  
  if (seconds < 60) {
    return `${seconds}s`
  }
  
  const minutes = Math.floor(seconds / 60)
  const remainingSeconds = seconds % 60
  
  if (minutes < 60) {
    return `${minutes}m ${remainingSeconds}s`
  }
  
  const hours = Math.floor(minutes / 60)
  const remainingMinutes = minutes % 60
  
  return `${hours}h ${remainingMinutes}m ${remainingSeconds}s`
}

/**
 * Formate une taille de fichier en format lisible
 * @param {number} bytes - La taille en bytes
 * @returns {string} La taille formatée
 */
export const formatFileSize = (bytes) => {
  if (!bytes) return '0 B'
  
  const units = ['B', 'KB', 'MB', 'GB', 'TB']
  let size = bytes
  let unitIndex = 0
  
  while (size >= 1024 && unitIndex < units.length - 1) {
    size /= 1024
    unitIndex++
  }
  
  return `${size.toFixed(1)} ${units[unitIndex]}`
} 