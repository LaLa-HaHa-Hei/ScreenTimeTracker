import { dateOnlySchema } from "@/shared/lib/date-only";
import { z } from "zod";

export const appUsageTimelineItemDtoSchema = z.object({
  id: z.string(),
  name: z.string(),
  color: z.string(),
  startTime: z.coerce.date(),
  endTime: z.coerce.date(),
});
export type AppUsageTimelineItemDto = z.infer<typeof appUsageTimelineItemDtoSchema>;

export const appCategoryUsageTimelineItemDtoSchema = z.object({
  id: z.string(),
  name: z.string(),
  color: z.string(),
  startTime: z.coerce.date(),
  endTime: z.coerce.date(),
});
export type AppCategoryUsageTimelineItemDto = z.infer<typeof appCategoryUsageTimelineItemDtoSchema>;

export const websiteUsageTimelineItemDtoSchema = z.object({
  id: z.string(),
  name: z.string(),
  color: z.string(),
  startTime: z.coerce.date(),
  endTime: z.coerce.date(),
});
export type WebsiteUsageTimelineItemDto = z.infer<typeof websiteUsageTimelineItemDtoSchema>;

export const websiteCategoryUsageTimelineItemDtoSchema = z.object({
  id: z.string(),
  name: z.string(),
  color: z.string(),
  startTime: z.coerce.date(),
  endTime: z.coerce.date(),
});
export type WebsiteCategoryUsageTimelineItemDto = z.infer<
  typeof websiteCategoryUsageTimelineItemDtoSchema
>;

// api 参数类型
export const getAppUsageTimelineParamsSchema = z.object({
  startDate: dateOnlySchema,
  endDate: dateOnlySchema,
  timeZoneId: z.string(),
  includedIds: z.array(z.string()).optional(),
  excludedIds: z.array(z.string()).optional(),
});
export type GetAppUsageTimelineParams = z.infer<typeof getAppUsageTimelineParamsSchema>;

export const getAppCategoryUsageTimelineParamsSchema = z.object({
  startDate: dateOnlySchema,
  endDate: dateOnlySchema,
  timeZoneId: z.string(),
  includedIds: z.array(z.string()).optional(),
  excludedIds: z.array(z.string()).optional(),
});
export type GetAppCategoryUsageTimelineParams = z.infer<
  typeof getAppCategoryUsageTimelineParamsSchema
>;

export const getWebsiteUsageTimelineParamsSchema = z.object({
  startDate: dateOnlySchema,
  endDate: dateOnlySchema,
  timeZoneId: z.string(),
  includedIds: z.array(z.string()).optional(),
  excludedIds: z.array(z.string()).optional(),
});
export type GetWebsiteUsageTimelineParams = z.infer<typeof getWebsiteUsageTimelineParamsSchema>;

export const getWebsiteCategoryUsageTimelineParamsSchema = z.object({
  startDate: dateOnlySchema,
  endDate: dateOnlySchema,
  timeZoneId: z.string(),
  includedIds: z.array(z.string()).optional(),
  excludedIds: z.array(z.string()).optional(),
});
export type GetWebsiteCategoryUsageTimelineParams = z.infer<
  typeof getWebsiteCategoryUsageTimelineParamsSchema
>;
