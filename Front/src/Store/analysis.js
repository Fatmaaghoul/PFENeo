import { defineStore } from 'pinia'

export const useDocumentStore = defineStore('document', {
  state: () => ({
    isAnalysing: false
  }),
  actions: {
    startAnalysing() {
      this.isAnalysing = true
    },
    stopAnalysing() {
      this.isAnalysing = false
    }
  }
})