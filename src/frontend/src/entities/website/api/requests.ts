import { apiClient, baseApiUrl } from "@/shared/api";
import { websiteDtoSchema, type GetWebsitesParams, type PatchWebsiteParams } from "./schemas";
import type { Website } from "../model/schemas";

export const getWebsites = async (params: GetWebsitesParams): Promise<Partial<Website>[]> => {
  const { data } = await apiClient.get("/screen-time/websites", {
    params,
  });
  return data.map((dto: unknown): Partial<Website> => {
    const validated = websiteDtoSchema.partial().parse(dto);
    return {
      id: validated.id,
      name: validated.name,
      color: validated.color,
      host: validated.host,
      allowMetadataAutoRefresh: validated.allowMetadataAutoRefresh,
      metadataLastRefreshedAt: validated.metadataLastRefreshedAt,
      websiteCategoryId: validated.websiteCategoryId,
      iconPath: validated.iconPath,
      iconPathLastUpdatedAt: validated.iconPathLastUpdatedAt,
      isSystem: validated.isSystem,
    };
  });
};

export const getWebsiteIconUrl = (websiteId: string, iconPathLastUpdatedAt: Date) =>
  `${baseApiUrl}/screen-time/websites/${websiteId}/icon?v=${iconPathLastUpdatedAt.getTime()}`;

export const patchWebsite = async (params: PatchWebsiteParams) => {
  const { data } = await apiClient.patch(`/screen-time/websites/${params.id}`, params.body);
  return data;
};

export const deleteWebsite = async (id: string) => {
  const { data } = await apiClient.delete(`/screen-time/websites/${id}`);
  return data;
};
