import i18n from "i18next";
import { initReactI18next } from "react-i18next";


i18n.use(initReactI18next).init({
  enableSelector:"strict",
  fallbackLng: "en-US",
  lng: "en-US",
  interpolation: {
    escapeValue: false,
  },
});

export default i18n;
