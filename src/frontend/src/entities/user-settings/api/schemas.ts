import { z } from "zod";

export const appTrackingSettingsDtoSchema = z.object({
  iconDirectory: z.string(),
  metadataStaleThresholdMinutes: z.number().int(),
  activeUsageSessionAutoSaveIntervalSeconds: z.number().int(),
  minValidUsageSessionDurationSeconds: z.number().int(),
  usageSessionMergeToleranceSeconds: z.number().int(),
  usageSessionOptimizationIntervalMinutes: z.number().int(),
});

export type AppTrackingSettingsDto = z.infer<typeof appTrackingSettingsDtoSchema>;

export const websiteTrackingSettingsDtoSchema = z.object({
  iconDirectory: z.string(),
  activeUsageSessionAutoSaveIntervalSeconds: z.number().int(),
  minValidUsageSessionDurationSeconds: z.number().int(),
  usageSessionMergeToleranceSeconds: z.number().int(),
  usageSessionOptimizationIntervalMinutes: z.number().int(),
});

export type WebsiteTrackingSettingsDto = z.infer<typeof websiteTrackingSettingsDtoSchema>;

export const idleDetectionSettingsDtoSchema = z.object({
  isEnabled: z.boolean(),
  inactivityThresholdSeconds: z.number().int(),
  pollingIntervalSeconds: z.number().int(),
});

export type IdleDetectionSettingsDto = z.infer<typeof idleDetectionSettingsDtoSchema>;

export const timeBoundarySettingsDtoSchema = z.object({
  dayCutoffHour: z.number().int(),
});

export type TimeBoundarySettingsDto = z.infer<typeof timeBoundarySettingsDtoSchema>;

export const regionalSettingsDtoSchema = z.object({
  timeZoneId: z.string(),
});

export type RegionalSettingsDto = z.infer<typeof regionalSettingsDtoSchema>;

export const userSettingsDtoSchema = z.object({
  appTracking: appTrackingSettingsDtoSchema,
  websiteTracking: websiteTrackingSettingsDtoSchema,
  idleDetection: idleDetectionSettingsDtoSchema,
  timeBoundary: timeBoundarySettingsDtoSchema,
  regional: regionalSettingsDtoSchema,
});

export type UserSettingsDto = z.infer<typeof userSettingsDtoSchema>;

// api 参数类型
export const patchUserSettingsParamsSchema = userSettingsDtoSchema.partial();

export type PatchUserSettingsParams = z.infer<typeof patchUserSettingsParamsSchema>;
