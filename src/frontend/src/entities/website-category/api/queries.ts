import { queryOptions, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  createWebsiteCategory,
  deleteWebsiteCategory,
  getWebsiteCategories,
  patchWebsiteCategory,
} from "./requests";
import type {
  CreateWebsiteCategoryParams,
  GetWebsiteCategoriesParams,
  PatchWebsiteCategoryParams,
} from "./schemas";
import type { WebsiteCategory } from "../model/schemas";

export const websiteCategoryKeys = {
  all: ["website-category"] as const,
  lists: () => [...websiteCategoryKeys.all, "list"] as const,
  list: (params: GetWebsiteCategoriesParams) => [...websiteCategoryKeys.lists(), params] as const,
};

export const websiteCategoryQueries = {
  websiteCategories: (params: GetWebsiteCategoriesParams) => {
    return queryOptions({
      queryKey: websiteCategoryKeys.list(params),
      queryFn: () => getWebsiteCategories(params),
      staleTime: 0,
    });
  },
};

export const useCreateWebsiteCategory = () => {
  const queryClient = useQueryClient();
  return useMutation<WebsiteCategory, Error, CreateWebsiteCategoryParams>({
    mutationFn: createWebsiteCategory,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: websiteCategoryKeys.all });
    },
  });
};

export const usePatchWebsiteCategory = () => {
  const queryClient = useQueryClient();
  return useMutation<WebsiteCategory, Error, PatchWebsiteCategoryParams>({
    mutationFn: patchWebsiteCategory,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: websiteCategoryKeys.all });
    },
  });
};

export const useDeleteWebsiteCategory = () => {
  const queryClient = useQueryClient();
  return useMutation<WebsiteCategory, Error, string>({
    mutationFn: deleteWebsiteCategory,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: websiteCategoryKeys.all });
    },
  });
};
