import { z } from "zod";
import { websiteSchema } from "../model/schemas";

export const websiteDtoSchema = z.object({
  id: z.string(),
  name: z.string(),
  color: z.string(),
  host: z.string(),
  allowMetadataAutoRefresh: z.boolean(),
  metadataLastRefreshedAt: z.coerce.date(),
  websiteCategoryId: z.string(),
  iconPath: z.string().nullable(),
  iconPathLastUpdatedAt: z.coerce.date(),
  isSystem: z.boolean(),
});
export type WebsiteDto = z.infer<typeof websiteDtoSchema>;

// api 参数类型
export const getWebsitesParamsSchema = z.object({
  fields: z.string().optional(),
});
export type GetWebsitesParams = z.infer<typeof getWebsitesParamsSchema>;

export const patchWebsiteSchema = z.object({
  id: z.string(),
  body: websiteSchema
    .pick({
      name: true,
      color: true,
      allowMetadataAutoRefresh: true,
      websiteCategoryId: true,
      iconPath: true,
    })
    .partial(),
});
export type PatchWebsiteParams = z.infer<typeof patchWebsiteSchema>;
