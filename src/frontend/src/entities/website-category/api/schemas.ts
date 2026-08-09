import { z } from "zod";
import { websiteCategorySchema } from "../model/schemas";

export const websiteCategoryDtoSchema = z.object({
  id: z.string(),
  name: z.string(),
  color: z.string(),
  iconPath: z.string().nullable(),
  iconPathLastUpdatedAt: z.coerce.date(),
  isSystem: z.boolean(),
});
export type WebsiteCategoryDto = z.infer<typeof websiteCategoryDtoSchema>;

// api 参数类型
export const getWebsiteCategoriesParamsSchema = z.object({
  fields: z.string().optional(),
});
export type GetWebsiteCategoriesParams = z.infer<typeof getWebsiteCategoriesParamsSchema>;

export const createWebsiteCategorySchema = websiteCategorySchema.pick({
  name: true,
  color: true,
  iconPath: true,
});
export type CreateWebsiteCategoryParams = z.infer<typeof createWebsiteCategorySchema>;

export const patchWebsiteCategorySchema = z.object({
  id: z.string(),
  body: websiteCategorySchema.pick({ name: true, color: true, iconPath: true }).partial(),
});
export type PatchWebsiteCategoryParams = z.infer<typeof patchWebsiteCategorySchema>;
