import { queryOptions } from "@tanstack/react-query";
import {
  getAppUsageTimeline,
  getAppCategoryUsageTimeline,
  getWebsiteCategoryUsageTimeline,
  getWebsiteUsageTimeline,
} from "./requests";
import type {
  GetAppUsageTimelineParams,
  GetAppCategoryUsageTimelineParams,
  GetWebsiteCategoryUsageTimelineParams,
  GetWebsiteUsageTimelineParams,
} from "./schemas";

export const appUsageTimelineQueryOptions = (params: GetAppUsageTimelineParams) => {
  return queryOptions({
    queryKey: ["app-usage-timeline", params],
    queryFn: () => getAppUsageTimeline(params),
  });
};

export const appCategoryUsageTimelineQueryOptions = (params: GetAppCategoryUsageTimelineParams) => {
  return queryOptions({
    queryKey: ["app-category-usage-timeline", params],
    queryFn: () => getAppCategoryUsageTimeline(params),
  });
};

export const websiteUsageTimelineQueryOptions = (params: GetWebsiteUsageTimelineParams) => {
  return queryOptions({
    queryKey: ["website-usage-timeline", params],
    queryFn: () => getWebsiteUsageTimeline(params),
  });
};

export const websiteCategoryUsageTimelineQueryOptions = (
  params: GetWebsiteCategoryUsageTimelineParams,
) => {
  return queryOptions({
    queryKey: ["website-category-usage-timeline", params],
    queryFn: () => getWebsiteCategoryUsageTimeline(params),
  });
};
