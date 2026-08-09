import { dateOnlySchema } from "@/shared/lib/date-only";
import { z } from "zod";

export const appUsageItemDtoSchema = z.object({
  startTime: z.coerce.date(),
  durationSeconds: z.number(),
});
export type AppUsageItemDto = z.infer<typeof appUsageItemDtoSchema>;

export const appCategoryUsageItemDtoSchema = z.object({
  startTime: z.coerce.date(),
  durationSeconds: z.number(),
});
export type AppCategoryUsageItemDto = z.infer<typeof appCategoryUsageItemDtoSchema>;

export const websiteUsageItemDtoSchema = z.object({
  startTime: z.coerce.date(),
  durationSeconds: z.number(),
});
export type WebsiteUsageItemDto = z.infer<typeof websiteUsageItemDtoSchema>;

export const websiteCategoryUsageItemDtoSchema = z.object({
  startTime: z.coerce.date(),
  durationSeconds: z.number(),
});
export type WebsiteCategoryUsageItemDto = z.infer<typeof websiteCategoryUsageItemDtoSchema>;

// api 参数类型
export const getAppUsageParamsSchema = z.object({
  granularity: z.enum(["hour", "day"]),
  startDate: dateOnlySchema,
  endDate: dateOnlySchema,
  includedIds: z.array(z.string()).optional(),
  excludedIds: z.array(z.string()).optional(),
});
export type GetAppUsageParams = z.infer<typeof getAppUsageParamsSchema>;

export const getAppCategoryUsageParamsSchema = z.object({
  granularity: z.enum(["hour", "day"]),
  startDate: dateOnlySchema,
  endDate: dateOnlySchema,
  includedIds: z.array(z.string()).optional(),
  excludedIds: z.array(z.string()).optional(),
});
export type GetAppCategoryUsageParams = z.infer<typeof getAppCategoryUsageParamsSchema>;

export const getWebsiteUsageParamsSchema = z.object({
  granularity: z.enum(["hour", "day"]),
  startDate: dateOnlySchema,
  endDate: dateOnlySchema,
  includedIds: z.array(z.string()).optional(),
  excludedIds: z.array(z.string()).optional(),
});
export type GetWebsiteUsageParams = z.infer<typeof getWebsiteUsageParamsSchema>;

export const getWebsiteCategoryUsageParamsSchema = z.object({
  granularity: z.enum(["hour", "day"]),
  startDate: dateOnlySchema,
  endDate: dateOnlySchema,
  includedIds: z.array(z.string()).optional(),
  excludedIds: z.array(z.string()).optional(),
});
export type GetWebsiteCategoryUsageParams = z.infer<typeof getWebsiteCategoryUsageParamsSchema>;
