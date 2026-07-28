import { z } from "zod";

export const localSettingsDtoSchema = z.object({
  defaultUIOpenMode: z.enum(["Window", "Browser"]),
  isAutoStartEnabled: z.boolean(),
  isSilentStartEnabled: z.boolean(),
  language: z.enum(["en-US", "zh-CN"]),
});
export type LocalSettingsDto = z.infer<typeof localSettingsDtoSchema>;

// api 参数类型
export const patchLocalSettingsParamsSchema = localSettingsDtoSchema.partial();
export type PatchLocalSettingsParams = z.infer<
  typeof patchLocalSettingsParamsSchema
>;
