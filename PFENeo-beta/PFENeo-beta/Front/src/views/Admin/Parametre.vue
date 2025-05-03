<template>
    <div class="settings-container">
      <div class="settings-header">
        <h1><i class="bi bi-gear-fill"></i> Paramètres</h1>
        <p>Personnalisez votre expérience utilisateur</p>
      </div>
  
      <div class="settings-grid">
        <!-- Section Sécurité -->
        <div class="settings-card">
          <div class="card-header">
            <h2><i class="bi bi-shield-lock"></i> Sécurité</h2>
          </div>
          <div class="card-body">
            <div class="setting-item">
              <div class="setting-info">
                <h3>Authentification à deux facteurs</h3>
                <p>Ajoutez une couche de sécurité supplémentaire</p>
              </div>
              <label class="switch">
                <input type="checkbox" v-model="twoFactorEnabled">
                <span class="slider round"></span>
              </label>
            </div>
  
            <div class="setting-item">
              <div class="setting-info">
                <h3>Sessions actives</h3>
                <p>3 appareils connectés</p>
              </div>
              <button class="btn-edit" @click="viewActiveSessions">
                <i class="bi bi-list"></i> Voir
              </button>
            </div>
  
            <div class="setting-item">
              <div class="setting-info">
                <h3>Historique de sécurité</h3>
                <p>Connexions récentes et activités suspectes</p>
              </div>
              <button class="btn-edit" @click="viewSecurityHistory">
                <i class="bi bi-clock-history"></i> Consulter
              </button>
            </div>
          </div>
        </div>
  
        <!-- Section Confidentialité -->
        <div class="settings-card">
          <div class="card-header">
            <h2><i class="bi bi-eye-slash"></i> Confidentialité</h2>
          </div>
          <div class="card-body">
            <div class="setting-item">
              <div class="setting-info">
                <h3>Visibilité des données</h3>
                <p>Contrôlez qui peut voir vos informations</p>
              </div>
              <button class="btn-edit" @click="openPrivacySettings">
                <i class="bi bi-pencil"></i> Configurer
              </button>
            </div>
  
            <div class="setting-item">
              <div class="setting-switch">
                <h3>Partage de données analytiques</h3>
                <p>Aidez-nous à améliorer le service</p>
              </div>
              <label class="switch">
                <input type="checkbox" v-model="shareAnalytics">
                <span class="slider round"></span>
              </label>
            </div>
  
            <div class="setting-item">
              <div class="setting-info">
                <h3>Export de données</h3>
                <p>Téléchargez toutes vos données personnelles</p>
              </div>
              <button class="btn-export" @click="exportData">
                <i class="bi bi-download"></i> Exporter
              </button>
            </div>
          </div>
        </div>
  
        <!-- Section Notifications -->
        <div class="settings-card">
          <div class="card-header">
            <h2><i class="bi bi-bell"></i> Notifications</h2>
          </div>
          <div class="card-body">
            <div class="setting-item">
              <div class="setting-switch">
                <h3>Notifications par email</h3>
                <p>Recevez des emails importants</p>
              </div>
              <label class="switch">
                <input type="checkbox" v-model="emailNotifications">
                <span class="slider round"></span>
              </label>
            </div>
  
            <div class="setting-item">
              <div class="setting-switch">
                <h3>Notifications push</h3>
                <p>Alertes en temps réel</p>
              </div>
              <label class="switch">
                <input type="checkbox" v-model="pushNotifications">
                <span class="slider round"></span>
              </label>
            </div>
  
            <div class="setting-item">
              <div class="setting-info">
                <h3>Préférences avancées</h3>
                <p>Personnalisez chaque type de notification</p>
              </div>
              <button class="btn-edit" @click="openNotificationSettings">
                <i class="bi bi-gear"></i> Personnaliser
              </button>
            </div>
          </div>
        </div>
  
        <!-- Section Application -->
        <div class="settings-card">
          <div class="card-header">
            <h2><i class="bi bi-app-indicator"></i> Application</h2>
          </div>
          <div class="card-body">
            <div class="setting-item">
              <div class="setting-info">
                <h3>Thème</h3>
                <p>Personnalisez l'apparence de l'application</p>
              </div>
              <select v-model="theme" class="theme-select">
                <option value="light">Clair</option>
                <option value="dark">Sombre</option>
                <option value="system">Système</option>
              </select>
            </div>
  
            <div class="setting-item">
              <div class="setting-info">
                <h3>Langue</h3>
                <p>Langue d'affichage</p>
              </div>
              <select v-model="language" class="language-select">
                <option value="fr">Français</option>
                <option value="en">English</option>
                <option value="es">Español</option>
              </select>
            </div>
  
            <div class="setting-item">
              <div class="setting-info">
                <h3>Performances</h3>
                <p>Mode basse consommation</p>
              </div>
              <label class="switch">
                <input type="checkbox" v-model="lowPowerMode">
                <span class="slider round"></span>
              </label>
            </div>
          </div>
        </div>
  
        <!-- Section Support -->
        <div class="settings-card">
          <div class="card-header">
            <h2><i class="bi bi-question-circle"></i> Support</h2>
          </div>
          <div class="card-body">
            <div class="setting-item">
              <div class="setting-info">
                <h3>Aide & FAQ</h3>
                <p>Questions fréquentes et documentation</p>
              </div>
              <button class="btn-link" @click="openHelpCenter">
                <i class="bi bi-book"></i> Accéder
              </button>
            </div>
  
            <div class="setting-item">
              <div class="setting-info">
                <h3>Contacter le support</h3>
                <p>Signalez un problème ou posez une question</p>
              </div>
              <button class="btn-link" @click="contactSupport">
                <i class="bi bi-envelope"></i> Contacter
              </button>
            </div>
  
            <div class="setting-item">
              <div class="setting-info">
                <h3>Conditions d'utilisation</h3>
                <p>Lisez nos termes et conditions</p>
              </div>
              <button class="btn-link" @click="openTerms">
                <i class="bi bi-file-text"></i> Lire
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </template>
  
  <script>
  import { ref } from 'vue';
  
  export default {
    name: 'SettingsPage',
    setup() {
      const twoFactorEnabled = ref(false);
      const shareAnalytics = ref(true);
      const emailNotifications = ref(true);
      const pushNotifications = ref(true);
      const theme = ref('system');
      const language = ref('fr');
      const lowPowerMode = ref(false);
  
      const viewActiveSessions = () => {
        // Logique pour voir les sessions actives
      };
  
      const viewSecurityHistory = () => {
        // Logique pour l'historique de sécurité
      };
  
      const openPrivacySettings = () => {
        // Logique pour les paramètres de confidentialité
      };
  
      const exportData = () => {
        // Logique pour exporter les données
      };
  
      const openNotificationSettings = () => {
        // Logique pour les paramètres de notification
      };
  
      const openHelpCenter = () => {
        // Logique pour le centre d'aide
      };
  
      const contactSupport = () => {
        // Logique pour contacter le support
      };
  
      const openTerms = () => {
        // Logique pour les conditions d'utilisation
      };
  
      return {
        twoFactorEnabled,
        shareAnalytics,
        emailNotifications,
        pushNotifications,
        theme,
        language,
        lowPowerMode,
        viewActiveSessions,
        viewSecurityHistory,
        openPrivacySettings,
        exportData,
        openNotificationSettings,
        openHelpCenter,
        contactSupport,
        openTerms
      };
    }
  };
  </script>
  
  <style scoped>
  .settings-container {
    padding: 2rem;
    max-width: 1200px;
    margin: 0 auto;
  }
  
  .settings-header {
    margin-bottom: 2.5rem;
    text-align: center;
  }
  
  .settings-header h1 {
    font-size: 2.5rem;
    color: #2c3e50;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 1rem;
    margin-bottom: 0.5rem;
  }
  
  .settings-header p {
    color: #7f8c8d;
    font-size: 1.1rem;
  }
  
  .settings-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
    gap: 1.5rem;
  }
  
  .settings-card {
    background: white;
    border-radius: 16px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);
    overflow: hidden;
    transition: transform 0.3s ease, box-shadow 0.3s ease;
  }
  
  .settings-card:hover {
    transform: translateY(-5px);
    box-shadow: 0 8px 30px rgba(0, 0, 0, 0.1);
  }
  
  .card-header {
    padding: 1.25rem 1.5rem;
    background: #f8f9fa;
    border-bottom: 1px solid #eee;
  }
  
  .card-header h2 {
    margin: 0;
    font-size: 1.4rem;
    color: #2c3e50;
    display: flex;
    align-items: center;
    gap: 0.75rem;
  }
  
  .card-body {
    padding: 1.5rem;
  }
  
  .setting-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1rem 0;
    border-bottom: 1px solid #f0f0f0;
  }
  
  .setting-item:last-child {
    border-bottom: none;
  }
  
  .setting-info h3 {
    margin: 0 0 0.25rem;
    font-size: 1.1rem;
    color: #34495e;
  }
  
  .setting-info p {
    margin: 0;
    color: #7f8c8d;
    font-size: 0.9rem;
  }
  
  .btn-edit {
    padding: 0.5rem 1rem;
    background: #3498db;
    color: white;
    border: none;
    border-radius: 8px;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    cursor: pointer;
    transition: background 0.2s;
  }
  
  .btn-edit:hover {
    background: #2980b9;
  }
  
  .btn-export {
    padding: 0.5rem 1rem;
    background: #2ecc71;
    color: white;
    border: none;
    border-radius: 8px;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    cursor: pointer;
    transition: background 0.2s;
  }
  
  .btn-export:hover {
    background: #27ae60;
  }
  
  .btn-link {
    background: none;
    border: none;
    color: #3498db;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem;
  }
  
  .btn-link:hover {
    text-decoration: underline;
  }
  
  .theme-select, .language-select {
    padding: 0.5rem;
    border-radius: 8px;
    border: 1px solid #ddd;
    min-width: 120px;
  }
  
  /* Switch toggle */
  .switch {
    position: relative;
    display: inline-block;
    width: 50px;
    height: 24px;
  }
  
  .switch input {
    opacity: 0;
    width: 0;
    height: 0;
  }
  
  .slider {
    position: absolute;
    cursor: pointer;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: #ccc;
    transition: .4s;
  }
  
  .slider:before {
    position: absolute;
    content: "";
    height: 16px;
    width: 16px;
    left: 4px;
    bottom: 4px;
    background-color: white;
    transition: .4s;
  }
  
  input:checked + .slider {
    background-color: #2ecc71;
  }
  
  input:focus + .slider {
    box-shadow: 0 0 1px #2ecc71;
  }
  
  input:checked + .slider:before {
    transform: translateX(26px);
  }
  
  .slider.round {
    border-radius: 34px;
  }
  
  .slider.round:before {
    border-radius: 50%;
  }
  
  @media (max-width: 768px) {
    .settings-grid {
      grid-template-columns: 1fr;
    }
    
    .setting-item {
      flex-direction: column;
      align-items: flex-start;
      gap: 1rem;
    }
  }
  </style>