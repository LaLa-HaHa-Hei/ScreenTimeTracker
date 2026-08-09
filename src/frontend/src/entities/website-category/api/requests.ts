import { apiClient, baseApiUrl } from "@/shared/api";
import {
  websiteCategoryDtoSchema,
  type CreateWebsiteCategoryParams,
  type GetWebsiteCategoriesParams,
  type PatchWebsiteCategoryParams,
} from "./schemas";
import type { WebsiteCategory } from "../model/schemas";

export const getWebsiteCategories = async (
  params: GetWebsiteCategoriesParams,
): Promise<Partial<WebsiteCategory>[]> => {
  const { data } = await apiClient.get("/screen-time/website-categories", {
    params,
  });
  return data.map((dto: unknown): Partial<WebsiteCategory> => {
    const validated = websiteCategoryDtoSchema.partial().parse(dto);
    return {
      id: validated.id,
      name: validated.name,
      color: validated.color,
      iconPath: validated.iconPath,
      iconPathLastUpdatedAt: validated.iconPathLastUpdatedAt,
      isSystem: validated.isSystem,
    };
  });
};

export const getWebsiteCategoryIconUrl = (websiteCategoryId: string, iconPathLastUpdatedAt: Date) =>
  `${baseApiUrl}/screen-time/website-categories/${websiteCategoryId}/icon?v=${iconPathLastUpdatedAt.getTime()}`;

export const createWebsiteCategory = async (
  params: CreateWebsiteCategoryParams,
): Promise<WebsiteCategory> => {
  const { data } = await apiClient.post("/screen-time/website-categories", params);
  const validated = websiteCategoryDtoSchema.parse(data);
  return {
    id: validated.id,
    name: validated.name,
    color: validated.color,
    iconPath: validated.iconPath,
    iconPathLastUpdatedAt: validated.iconPathLastUpdatedAt,
    isSystem: validated.isSystem,
  };
};

export const patchWebsiteCategory = async (params: PatchWebsiteCategoryParams) => {
  const { data } = await apiClient.patch(
    `/screen-time/website-categories/${params.id}`,
    params.body,
  );
  return data;
};

export const deleteWebsiteCategory = async (id: string) => {
  const { data } = await apiClient.delete(`/screen-time/website-categories/${id}`);
  return data;
};
