import { z } from "zod";

export const websiteSchema = z.object({
  id: z.string(),
  name: z.string(),
  color: z.string(),
  host: z.string(),
  allowMetadataAutoRefresh: z.boolean(),
  metadataLastRefreshedAt: z.date(),
  websiteCategoryId: z.string(),
  iconPath: z.string().nullable(),
  iconPathLastUpdatedAt: z.date(),
  isSystem: z.boolean(),
});
export type Website = z.infer<typeof websiteSchema>;
