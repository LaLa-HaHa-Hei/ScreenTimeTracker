import "i18next";

import app from "../app/i18n/en-US.json";

import entity_app from "../entity/app/i18n/en-US.json";
import entity_appCategory from "../entity/app-category/i18n/en-US.json";
import entity_website from "../entity/website/i18n/en-US.json";
import entity_websiteCategory from "../entity/website-category/i18n/en-US.json";

import feature_dateFilter from "../features/date-filter/i18n/en-US.json";
import feature_dimensionControl from "../features/dimension-control/i18n/en-US.json";
import feature_usageChart from "../features/usage-chart/i18n/en-US.json";
import feature_usageDistribution from "../features/usage-distribution/i18n/en-US.json";

import page_appManagement from "../pages/app-management/i18n/en-US.json";
import page_appCategoryManagement from "../pages/app-category-management/i18n/en-US.json";
import page_websiteManagement from "../pages/website-management/i18n/en-US.json";
import page_websiteCategoryManagement from "../pages/website-category-management/i18n/en-US.json";
import page_dataManagement from "../pages/data-management/i18n/en-US.json";
import page_settingsManagement from "../pages/settings-management/i18n/en-US.json";
import page_usageDetails from "../pages/usage-details/i18n/en-US.json";
import page_usageSummary from "../pages/usage-summary/i18n/en-US.json";

declare module "i18next" {
  interface CustomTypeOptions {
    enableSelector: "strict";
    resources: {
      app: typeof app;

      entity_app: typeof entity_app;
      entity_appCategory: typeof entity_appCategory;
      entity_website: typeof entity_website;
      entity_websiteCategory: typeof entity_websiteCategory;

      feature_dateFilter: typeof feature_dateFilter;
      feature_dimensionControl: typeof feature_dimensionControl;
      feature_usageChart: typeof feature_usageChart;
      feature_usageDistribution: typeof feature_usageDistribution;

      page_appManagement: typeof page_appManagement;
      page_appCategoryManagement: typeof page_appCategoryManagement;
      page_websiteManagement: typeof page_websiteManagement;
      page_websiteCategoryManagement: typeof page_websiteCategoryManagement;
      page_dataManagement: typeof page_dataManagement;
      page_settingsManagement: typeof page_settingsManagement;
      page_usageDetails: typeof page_usageDetails;
      page_usageSummary: typeof page_usageSummary;
    };
  }
}
