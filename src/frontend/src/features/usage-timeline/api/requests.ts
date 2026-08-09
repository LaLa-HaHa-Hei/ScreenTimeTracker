import { apiClient } from "@/shared/api";
import {
  type GetAppUsageTimelineParams,
  type AppUsageTimelineItemDto,
  appUsageTimelineItemDtoSchema,
  type GetAppCategoryUsageTimelineParams,
  type AppCategoryUsageTimelineItemDto,
  appCategoryUsageTimelineItemDtoSchema,
  type GetWebsiteCategoryUsageTimelineParams,
  type GetWebsiteUsageTimelineParams,
  type WebsiteCategoryUsageTimelineItemDto,
  websiteCategoryUsageTimelineItemDtoSchema,
  type WebsiteUsageTimelineItemDto,
  websiteUsageTimelineItemDtoSchema,
} from "./schemas";

export const getAppUsageTimeline = async (
  params: GetAppUsageTimelineParams,
): Promise<AppUsageTimelineItemDto[]> => {
  const { data } = await apiClient.get("/screen-time/usage/apps/timeline", {
    params,
  });
  return data.map((dto: unknown) => {
    const validated = appUsageTimelineItemDtoSchema.parse(dto);
    return {
      id: validated.id,
      name: validated.name,
      color: validated.color,
      startTime: validated.startTime,
      endTime: validated.endTime,
    };
  });
};

export const getAppCategoryUsageTimeline = async (
  params: GetAppCategoryUsageTimelineParams,
): Promise<AppCategoryUsageTimelineItemDto[]> => {
  const { data } = await apiClient.get("/screen-time/usage/app-categories/timeline", {
    params,
  });
  return data.map((dto: unknown) => {
    const validated = appCategoryUsageTimelineItemDtoSchema.parse(dto);
    return {
      id: validated.id,
      name: validated.name,
      color: validated.color,
      startTime: validated.startTime,
      endTime: validated.endTime,
    };
  });
};

export const getWebsiteUsageTimeline = async (
  params: GetWebsiteUsageTimelineParams,
): Promise<WebsiteUsageTimelineItemDto[]> => {
  const { data } = await apiClient.get("/screen-time/usage/websites/timeline", {
    params,
  });
  return data.map((dto: unknown) => {
    const validated = websiteUsageTimelineItemDtoSchema.parse(dto);
    return {
      id: validated.id,
      name: validated.name,
      color: validated.color,
      startTime: validated.startTime,
      endTime: validated.endTime,
    };
  });
};

export const getWebsiteCategoryUsageTimeline = async (
  params: GetWebsiteCategoryUsageTimelineParams,
): Promise<WebsiteCategoryUsageTimelineItemDto[]> => {
  const { data } = await apiClient.get("/screen-time/usage/website-categories/timeline", {
    params,
  });
  return data.map((dto: unknown) => {
    const validated = websiteCategoryUsageTimelineItemDtoSchema.parse(dto);
    return {
      id: validated.id,
      name: validated.name,
      color: validated.color,
      startTime: validated.startTime,
      endTime: validated.endTime,
    };
  });
};
