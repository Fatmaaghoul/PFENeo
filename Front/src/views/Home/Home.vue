<template>
  <div class="home-container">
    <!-- Hero Section -->
    <section class="hero-section">
      <div class="hero-content">
        <h1>Plateforme de gestion de documents intelligente</h1>
        <p>Gérez, analysez et extrayez des informations de vos documents avec notre technologie d'IA avancée. Simplifiez votre flux de travail documentaire dès aujourd'hui.</p>
        <div class="button-container">
          <button class="get-started-btn" @click="handleGetStarted">Commencer</button>
        </div>
      </div>
    </section>

    <!-- Features Section -->
    <section class="features-section">
      <div class="section-header">
        <h2>Fonctionnalités principales</h2>
        <p>Découvrez comment notre plateforme peut transformer votre gestion documentaire</p>
      </div>
      
      <div class="features-grid">
        <div class="feature-card">
          <div class="feature-icon">
            <i class="bi bi-file-earmark-text"></i>
          </div>
          <h3>Extraction intelligente</h3>
          <p>Notre IA extrait automatiquement le contenu et les images de vos documents pour une analyse approfondie.</p>
        </div>
        
        <div class="feature-card">
          <div class="feature-icon">
            <i class="bi bi-search"></i>
          </div>
          <h3>Recherche avancée</h3>
          <p>Trouvez rapidement les informations dont vous avez besoin grâce à notre moteur de recherche puissant.</p>
        </div>
        
        <div class="feature-card">
          <div class="feature-icon">
            <i class="bi bi-shield-check"></i>
          </div>
          <h3>Sécurité renforcée</h3>
          <p>Vos documents sont protégés par des mesures de sécurité de pointe pour garantir la confidentialité.</p>
        </div>
      </div>
    </section>

    <!-- How It Works Section -->
    <section class="how-it-works-section">
      <div class="section-header">
        <h2>Comment ça fonctionne</h2>
        <p>Un processus simple en trois étapes pour gérer vos documents efficacement</p>
      </div>
      
      <div class="steps-container">
        <div class="step-card">
          <div class="step-number">1</div>
          <h3>Téléchargez</h3>
          <p>Importez vos documents PDF dans notre plateforme sécurisée.</p>
        </div>
        
        <div class="step-card">
          <div class="step-number">2</div>
          <h3>Analysez</h3>
          <p>Notre IA extrait automatiquement le contenu et les images.</p>
        </div>
        
        <div class="step-card">
          <div class="step-number">3</div>
          <h3>Gérez</h3>
          <p>Accédez, recherchez et partagez vos documents facilement.</p>
        </div>
      </div>
    </section>

    <!-- Stats Section -->
    <section class="stats-section">
      <div class="stats-container">
        <div class="stat-card">
          <div class="stat-number">1000+</div>
          <div class="stat-label">Documents traités</div>
        </div>
        
        <div class="stat-card">
          <div class="stat-number">98%</div>
          <div class="stat-label">Précision d'extraction</div>
        </div>
        
        <div class="stat-card">
          <div class="stat-number">24/7</div>
          <div class="stat-label">Disponibilité</div>
        </div>
      </div>
    </section>

    <!-- CTA Section -->
    <section class="cta-section">
      <div class="cta-content">
        <h2>Prêt à simplifier votre gestion documentaire ?</h2>
        <p>Rejoignez notre plateforme dès aujourd'hui et découvrez la puissance de l'IA pour vos documents.</p>
        <button class="cta-btn" @click="handleGetStarted">Commencer maintenant</button>
      </div>
    </section>
  </div>
</template>

<script>
export default {
  name: 'Home',
  methods: {
    handleGetStarted() {
      // Accède à l'état authentification depuis Vuex
      const authenticated = this.$store.state.authenticated;
      console.log('Bouton cliqué');
      console.log('Utilisateur authentifié:', authenticated); // Vérification de l'authentification

      if (authenticated) {
        this.$router.push('/document'); // Si authentifié, rediriger vers /document
      } else {
        this.$router.push('/login'); // Sinon, rediriger vers /login
      }
    }
  }
}
</script>

<style scoped>
.home-container {
  width: 100%;
  overflow-x: hidden;
}

/* Hero Section */
.hero-section {
  background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%);
  color: white;
  padding: 80px 20px;
  text-align: center;
}

.hero-content {
  max-width: 800px;
  margin: 0 auto;
}

.hero-section h1 {
  font-size: 3rem;
  font-weight: 700;
  margin-bottom: 20px;
  line-height: 1.2;
}

.hero-section p {
  font-size: 1.25rem;
  margin-bottom: 40px;
  line-height: 1.6;
  opacity: 0.9;
}

.button-container {
  margin-top: 30px;
}

.get-started-btn {
  background-color: white;
  color: #4F46E5;
  padding: 15px 40px;
  border-radius: 9999px;
  border: none;
  font-size: 1.125rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}

.get-started-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 15px -3px rgba(0, 0, 0, 0.2);
}

/* Section Header */
.section-header {
  text-align: center;
  margin-bottom: 60px;
}

.section-header h2 {
  font-size: 2.5rem;
  color: #1F2937;
  margin-bottom: 15px;
}

.section-header p {
  font-size: 1.125rem;
  color: #6B7280;
  max-width: 600px;
  margin: 0 auto;
}

/* Features Section */
.features-section {
  padding: 100px 20px;
  background-color: #F9FAFB;
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 30px;
  max-width: 1200px;
  margin: 0 auto;
}

.feature-card {
  background-color: white;
  border-radius: 12px;
  padding: 30px;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;
  text-align: center;
}

.feature-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.1);
}

.feature-icon {
  font-size: 2.5rem;
  color: #4F46E5;
  margin-bottom: 20px;
}

.feature-card h3 {
  font-size: 1.5rem;
  color: #1F2937;
  margin-bottom: 15px;
}

.feature-card p {
  color: #6B7280;
  line-height: 1.6;
}

/* How It Works Section */
.how-it-works-section {
  padding: 100px 20px;
  background-color: white;
}

.steps-container {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 30px;
  max-width: 1200px;
  margin: 0 auto;
}

.step-card {
  flex: 1;
  min-width: 250px;
  max-width: 350px;
  background-color: #F9FAFB;
  border-radius: 12px;
  padding: 30px;
  text-align: center;
  position: relative;
}

.step-number {
  position: absolute;
  top: -20px;
  left: 50%;
  transform: translateX(-50%);
  width: 40px;
  height: 40px;
  background-color: #4F46E5;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 1.25rem;
}

.step-card h3 {
  font-size: 1.5rem;
  color: #1F2937;
  margin: 20px 0 15px;
}

.step-card p {
  color: #6B7280;
  line-height: 1.6;
}

/* Stats Section */
.stats-section {
  padding: 80px 20px;
  background-color: #4F46E5;
  color: white;
}

.stats-container {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 40px;
  max-width: 1200px;
  margin: 0 auto;
}

.stat-card {
  text-align: center;
  flex: 1;
  min-width: 200px;
}

.stat-number {
  font-size: 3rem;
  font-weight: 700;
  margin-bottom: 10px;
}

.stat-label {
  font-size: 1.125rem;
  opacity: 0.9;
}

/* CTA Section */
.cta-section {
  padding: 100px 20px;
  background-color: #F9FAFB;
  text-align: center;
}

.cta-content {
  max-width: 800px;
  margin: 0 auto;
}

.cta-section h2 {
  font-size: 2.5rem;
  color: #1F2937;
  margin-bottom: 20px;
}

.cta-section p {
  font-size: 1.125rem;
  color: #6B7280;
  margin-bottom: 40px;
}

.cta-btn {
  background-color: #4F46E5;
  color: white;
  padding: 15px 40px;
  border-radius: 9999px;
  border: none;
  font-size: 1.125rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 6px -1px rgba(79, 70, 229, 0.3);
}

.cta-btn:hover {
  background-color: #4338CA;
  transform: translateY(-2px);
  box-shadow: 0 6px 15px -3px rgba(79, 70, 229, 0.4);
}

/* Responsive Design */
@media (max-width: 768px) {
  .hero-section h1 {
    font-size: 2.5rem;
  }
  
  .hero-section p {
    font-size: 1.125rem;
  }
  
  .section-header h2 {
    font-size: 2rem;
  }
  
  .features-grid {
    grid-template-columns: 1fr;
  }
  
  .steps-container {
    flex-direction: column;
    align-items: center;
  }
  
  .step-card {
    width: 100%;
  }
  
  .stats-container {
    flex-direction: column;
    gap: 30px;
  }
  
  .cta-section h2 {
    font-size: 2rem;
  }
}
</style>
  