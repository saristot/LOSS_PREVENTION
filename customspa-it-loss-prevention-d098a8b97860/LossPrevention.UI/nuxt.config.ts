import { defineNuxtConfig } from "nuxt";

export default defineNuxtConfig({
  ssr: true, // Enable Server-Side Rendering
  buildModules: ["@pinia/nuxt", "@nuxtjs/vuetify"],
  css: ["vuetify/styles"],
  vuetify: {
    theme: {
      themes: {
        light: {
          primary: "#1976D2",
          secondary: "#424242",
          accent: "#82B1FF",
          error: "#FF5252",
          info: "#2196F3",
          success: "#4CAF50",
          warning: "#FB8C00",
        },
      },
    },
  },
});
