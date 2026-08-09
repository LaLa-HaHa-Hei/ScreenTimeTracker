import { queryOptions } from "@tanstack/react-query";
import type {
  GetAppCategoryUsageParams,
  GetAppUsageParams,
  GetWebsiteCategoryUsageParams,
  GetWebsiteUsageParams,
} from "./schemas";
import {
  getAppCategoryUsage,
  getAppUsage,
  getWebsiteCategoryUsage,
  getWebsiteUsage,
} from "./requests";

export const appUsageQueryOptions = (params: GetAppUsageParams) => {
  return queryOptions({
    queryKey: ["app-usage", params],
    queryFn: () => getAppUsage(params),
  });
};

export const appCategoryUsageQueryOptions = (params: GetAppCategoryUsageParams) => {
  return queryOptions({
    queryKey: ["app-category-usage", params],
    queryFn: () => getAppCategoryUsage(params),
  });
};

export const websiteUsageQueryOptions = (params: GetWebsiteUsageParams) => {
  return queryOptions({
    queryKey: ["website-usage", params],
    queryFn: () => getWebsiteUsage(params),
  });
};

export const websiteCategoryUsageQueryOptions = (params: GetWebsiteCategoryUsageParams) => {
  return queryOptions({
    queryKey: ["website-category-usage", params],
    queryFn: () => getWebsiteCategoryUsage(params),
  });
};
