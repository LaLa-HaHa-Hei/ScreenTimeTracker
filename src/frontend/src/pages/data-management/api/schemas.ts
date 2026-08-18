import z from "zod";
import { dateOnlySchema } from "@/shared/lib/date-only";

export const deleteUsageDataParamsSchema = z.object({
  startDate: dateOnlySchema,
  endDate: dateOnlySchema,
  timeZoneId: z.string,
});
export type DeleteUsageDataParams = z.infer<typeof deleteUsageDataParamsSchema>;

export const importDataDtoSchema = z.object({
  newApps: z.number(),
  newAppCategories: z.number(),
  newWebsites: z.number(),
  newWebsiteCategories: z.number(),
  importedAppUsageSessions: z.number(),
  skippedAppUsageSessions: z.number(),
  importedWebsiteUsageSessions: z.number(),
  skippedWebsiteUsageSessions: z.number(),
});
export type ImportDataDto = z.infer<typeof importDataDtoSchema>;
