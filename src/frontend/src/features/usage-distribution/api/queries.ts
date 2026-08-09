import { queryOptions } from "@tanstack/react-query";
import type {
  GetAppCategoryUsageDistributionParams,
  GetAppUsageDistributionParams,
  GetWebsiteCategoryUsageDistributionParams,
  GetWebsiteUsageDistributionParams,
} from "./schemas";
import {
  getAppCategoryUsageDistribution,
  getAppUsageDistribution,
  getWebsiteCategoryUsageDistribution,
  getWebsiteUsageDistribution,
} from "./requests";

export const appUsageDistributionQueryOptions = (params: GetAppUsageDistributionParams) => {
  return queryOptions({
    queryKey: ["app-usage-distribution", params],
    queryFn: () => getAppUsageDistribution(params),
  });
};

export const appCategoryUsageDistributionQueryOptions = (
  params: GetAppCategoryUsageDistributionParams,
) => {
  return queryOptions({
    queryKey: ["app-category-usage-distribution", params],
    queryFn: () => getAppCategoryUsageDistribution(params),
  });
};

export const websiteUsageDistributionQueryOptions = (params: GetWebsiteUsageDistributionParams) => {
  return queryOptions({
    queryKey: ["website-usage-distribution", params],
    queryFn: () => getWebsiteUsageDistribution(params),
  });
};

export const websiteCategoryUsageDistributionQueryOptions = (
  params: GetWebsiteCategoryUsageDistributionParams,
) => {
  return queryOptions({
    queryKey: ["website-category-usage-distribution", params],
    queryFn: () => getWebsiteCategoryUsageDistribution(params),
  });
};
