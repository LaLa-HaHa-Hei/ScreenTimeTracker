import { z } from "zod";

export const websiteCategorySchema = z.object({
  id: z.string(),
  name: z.string(),
  color: z.string(),
  iconPath: z.string().nullable(),
  iconPathLastUpdatedAt: z.date(),
  isSystem: z.boolean(),
});
export type WebsiteCategory = z.infer<typeof websiteCategorySchema>;
