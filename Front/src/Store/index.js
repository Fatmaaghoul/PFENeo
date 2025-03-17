import { createStore } from 'vuex';
import axios from 'axios';
import Cookies from 'js-cookie'; // Importez js-cookie

export default createStore({
  state: {
    authenticated: false,
    user: null
  },
  mutations: {
    SET_AUTH(state, { authenticated, user }) {
      state.authenticated = authenticated;
      state.user = user;
    },
    LOGOUT(state) {
      state.authenticated = false;
      state.user = null;
    }
  },
  actions: {
    login({ commit }, user) {
      commit('SET_AUTH', { authenticated: true, user });
    },
    logout({ commit }) {
      Cookies.remove('token'); // Supprime le token du cookie
      commit('LOGOUT');
    },
    checkUserStatus({ commit }) {
      const token = Cookies.get('token'); // Récupère le token depuis les cookies
      if (token) {
        try {
          // Décoder le token JWT
          const base64Url = token.split('.')[1];
          const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
          const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
          }).join(''));
          const userData = JSON.parse(jsonPayload);

          // Vérifiez si le rôle est présent dans le token
          const userRole = userData['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'User';

          commit('SET_AUTH', { 
            authenticated: true, 
            user: {
              email: userData.email,
              role: userRole
            }
          });
        } catch (error) {
          console.error('Error decoding token:', error);
          Cookies.remove('token'); // Supprime le token du cookie en cas d'erreur
          commit('LOGOUT');
        }
      } else {
        commit('SET_AUTH', { authenticated: false, user: null });
      }
    },
    async fetchUser({ commit }) {
      try {
        const token = Cookies.get('token'); // Récupère le token depuis les cookies
        const response = await axios.get("/api/profile", {
          headers: { Authorization: `Bearer ${token}` }
        });

        console.log("API response:", response.data); // Inspectez la réponse

        // Utilise SET_AUTH pour mettre à jour l'état
        commit("SET_AUTH", { 
          authenticated: true, 
          user: {
            ...response.data.data, // Inclut toutes les données de l'API
            role: response.data.data.role || 'User' // Ajoute le rôle si nécessaire
          }
        });
      } catch (error) {
        console.error("Erreur lors du chargement de l'utilisateur :", error);
        throw error;
      }
    }
  },
  getters: {
    isAuthenticated(state) {
      return state.authenticated;
    },
    user(state) {
      return state.user;
    }
  }
});