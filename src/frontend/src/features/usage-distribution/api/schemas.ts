import { dateOnlySchema } from "@/shared/lib/date-only";
import { z } from "zod";

export const appUsageDistributionItemDtoSchema = z.object({
  id: z.string(),
  name: z.string(),
  color: z.string(),
  iconPath: z.string().nullable(),
  iconPathLastUpdatedAt: z.coerce.date(),
  durationSeconds: z.number(),
});
export type AppUsageDistributionItemDto = z.infer<typeof appUsageDistributionItemDtoSchema>;

export const appUsageDistributionDtoSchema = z.object({
  items: appUsageDistributionItemDtoSchema.array(),
  totalCount: z.number(),
  totalDurationSeconds: z.number(),
  othersCount: z.number(),
  othersDurationSeconds: z.number(),
});
export type AppUsageDistributionDto = z.infer<typeof appUsageDistributionDtoSchema>;

export const appCategoryUsageDistributionItemDtoSchema = z.object({
  id: z.string(),
  name: z.string(),
  color: z.string(),
  iconPath: z.string().nullable(),
  iconPathLastUpdatedAt: z.coerce.date(),
  durationSeconds: z.number(),
});
export type AppCategoryUsageDistributionItemDto = z.infer<
  typeof appCategoryUsageDistributionItemDtoSchema
>;

export const appCategoryUsageDistributionDtoSchema = z.object({
  items: appCategoryUsageDistributionItemDtoSchema.array(),
  totalCount: z.number(),
  totalDurationSeconds: z.number(),
  othersCount: z.number(),
  othersDurationSeconds: z.number(),
});
export type AppCategoryUsageDistributionDto = z.infer<typeof appCategoryUsageDistributionDtoSchema>;

export const websiteUsageDistributionItemDtoSchema = z.object({
  id: z.string(),
  name: z.string(),
  color: z.string(),
  iconPath: z.string().nullable(),
  iconPathLastUpdatedAt: z.coerce.date(),
  durationSeconds: z.number(),
});
export type WebsiteUsageDistributionItemDto = z.infer<typeof websiteUsageDistributionItemDtoSchema>;

export const websiteUsageDistributionDtoSchema = z.object({
  items: websiteUsageDistributionItemDtoSchema.array(),
  totalCount: z.number(),
  totalDurationSeconds: z.number(),
  othersCount: z.number(),
  othersDurationSeconds: z.number(),
});
export type WebsiteUsageDistributionDto = z.infer<typeof websiteUsageDistributionDtoSchema>;

export const websiteCategoryUsageDistributionItemDtoSchema = z.object({
  id: z.string(),
  name: z.string(),
  color: z.string(),
  iconPath: z.string().nullable(),
  iconPathLastUpdatedAt: z.coerce.date(),
  durationSeconds: z.number(),
});
export type WebsiteCategoryUsageDistributionItemDto = z.infer<
  typeof websiteCategoryUsageDistributionItemDtoSchema
>;

export const websiteCategoryUsageDistributionDtoSchema = z.object({
  items: websiteCategoryUsageDistributionItemDtoSchema.array(),
  totalCount: z.number(),
  totalDurationSeconds: z.number(),
  othersCount: z.number(),
  othersDurationSeconds: z.number(),
});
export type WebsiteCategoryUsageDistributionDto = z.infer<
  typeof websiteCategoryUsageDistributionDtoSchema
>;
// api 参数类型
export const getAppUsageDistributionParamsSchema = z.object({
  startDate: dateOnlySchema,
  endDate: dateOnlySchema,
  topN: z.number().optional(),
  excludedIds: z.array(z.string()).optional(),
});
export type GetAppUsageDistributionParams = z.infer<typeof getAppUsageDistributionParamsSchema>;

export const getAppCategoryUsageDistributionParamsSchema = z.object({
  startDate: dateOnlySchema,
  endDate: dateOnlySchema,
  topN: z.number().optional(),
  excludedIds: z.array(z.string()).optional(),
});
export type GetAppCategoryUsageDistributionParams = z.infer<
  typeof getAppCategoryUsageDistributionParamsSchema
>;

export const getWebsiteUsageDistributionParamsSchema = z.object({
  startDate: dateOnlySchema,
  endDate: dateOnlySchema,
  topN: z.number().optional(),
  excludedIds: z.array(z.string()).optional(),
});
export type GetWebsiteUsageDistributionParams = z.infer<
  typeof getWebsiteUsageDistributionParamsSchema
>;

export const getWebsiteCategoryUsageDistributionParamsSchema = z.object({
  startDate: dateOnlySchema,
  endDate: dateOnlySchema,
  topN: z.number().optional(),
  excludedIds: z.array(z.string()).optional(),
});
export type GetWebsiteCategoryUsageDistributionParams = z.infer<
  typeof getWebsiteCategoryUsageDistributionParamsSchema
>;
