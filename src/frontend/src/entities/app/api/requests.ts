import { apiClient, baseApiUrl } from "@/shared/api";
import { appDtoSchema, type GetAppsParams, type PatchAppParams } from "./schemas";
import type { App } from "../model/schemas";

export const getApps = async (params: GetAppsParams): Promise<Partial<App>[]> => {
  const { data } = await apiClient.get("/screen-time/apps", {
    params,
  });
  return data.map((dto: unknown): Partial<App> => {
    const validated = appDtoSchema.partial().parse(dto);
    return {
      id: validated.id,
      name: validated.name,
      color: validated.color,
      processName: validated.processName,
      allowMetadataAutoRefresh: validated.allowMetadataAutoRefresh,
      metadataLastRefreshedAt: validated.metadataLastRefreshedAt,
      categoryId: validated.categoryId,
      executablePath: validated.executablePath,
      iconPath: validated.iconPath,
      iconPathLastUpdatedAt: validated.iconPathLastUpdatedAt,
      isSystem: validated.isSystem,
    };
  });
};

export const getAppIconUrl = (appId: string, iconPathLastUpdatedAt: Date) =>
  `${baseApiUrl}/screen-time/apps/${appId}/icon?v=${iconPathLastUpdatedAt.getTime()}`;

export const patchApp = async (params: PatchAppParams) => {
  const { data } = await apiClient.patch(`/screen-time/apps/${params.id}`, params.body);
  return data;
};

export const deleteApp = async (id: string) => {
  const { data } = await apiClient.delete(`/screen-time/apps/${id}`);
  return data;
};
