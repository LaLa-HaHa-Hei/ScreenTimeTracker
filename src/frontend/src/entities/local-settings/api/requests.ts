import { apiClient } from "@/shared/api";
import type { PatchLocalSettingsParams, LocalSettingsDto } from "./schemas";

export const getLocalSettingsDto = async (): Promise<LocalSettingsDto> => {
  const { data } = await apiClient.get("/desktop-settings/local-settings");
  return data;
};

export const patchLocalSettingsDto = async (
  params: PatchLocalSettingsParams,
) => {
  const { data } = await apiClient.patch(
    "/desktop-settings/local-settings",
    params,
  );
  return data;
};
