import { z } from "zod";

export const appSchema = z.object({
  id: z.string(),
  name: z.string(),
  color: z.string(),
  processName: z.string(),
  allowMetadataAutoUpdate: z.boolean(),
  metadataLastUpdatedAt: z.date(),
  appCategoryId: z.string(),
  executablePath: z.string().nullable(),
  iconPath: z.string().nullable(),
  iconPathLastUpdatedAt: z.date(),
  isSystem: z.boolean(),
});
export type App = z.infer<typeof appSchema>;
