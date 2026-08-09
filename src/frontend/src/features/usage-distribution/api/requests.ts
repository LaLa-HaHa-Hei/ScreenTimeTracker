import { apiClient } from "@/shared/api";
import {
  type GetAppUsageDistributionParams,
  type GetAppCategoryUsageDistributionParams,
  appUsageDistributionDtoSchema,
  type AppUsageDistributionDto,
  appCategoryUsageDistributionDtoSchema,
  type AppCategoryUsageDistributionDto,
  websiteCategoryUsageDistributionDtoSchema,
  type WebsiteCategoryUsageDistributionDto,
  type WebsiteUsageDistributionDto,
  websiteUsageDistributionDtoSchema,
  type GetWebsiteCategoryUsageDistributionParams,
  type GetWebsiteUsageDistributionParams,
} from "./schemas";

export const getAppUsageDistribution = async (
  params: GetAppUsageDistributionParams,
): Promise<AppUsageDistributionDto> => {
  const { data } = await apiClient.get("/screen-time/usage/apps/distribution", {
    params,
  });
  return appUsageDistributionDtoSchema.parse(data);
};

export const getAppCategoryUsageDistribution = async (
  params: GetAppCategoryUsageDistributionParams,
): Promise<AppCategoryUsageDistributionDto> => {
  const { data } = await apiClient.get("/screen-time/usage/app-categories/distribution", {
    params,
  });
  return appCategoryUsageDistributionDtoSchema.parse(data);
};

export const getWebsiteUsageDistribution = async (
  params: GetWebsiteUsageDistributionParams,
): Promise<WebsiteUsageDistributionDto> => {
  const { data } = await apiClient.get("/screen-time/usage/websites/distribution", {
    params,
  });
  return websiteUsageDistributionDtoSchema.parse(data);
};

export const getWebsiteCategoryUsageDistribution = async (
  params: GetWebsiteCategoryUsageDistributionParams,
): Promise<WebsiteCategoryUsageDistributionDto> => {
  const { data } = await apiClient.get("/screen-time/usage/website-categories/distribution", {
    params,
  });
  return websiteCategoryUsageDistributionDtoSchema.parse(data);
};
