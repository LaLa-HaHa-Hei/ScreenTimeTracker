import { queryOptions, useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteWebsite, getWebsites, patchWebsite } from "./requests";
import type { GetWebsitesParams, PatchWebsiteParams } from "./schemas";
import type { Website } from "../model/schemas";

export const websiteKeys = {
  all: ["website"] as const,
  lists: () => [...websiteKeys.all, "list"] as const,
  list: (params: GetWebsitesParams) => [...websiteKeys.lists(), params] as const,
};

export const websiteQueries = {
  websites: (params: GetWebsitesParams) => {
    return queryOptions({
      queryKey: websiteKeys.list(params),
      queryFn: () => getWebsites(params),
      staleTime: 0,
    });
  },
};

export const usePatchWebsite = () => {
  const queryClient = useQueryClient();
  return useMutation<Website, Error, PatchWebsiteParams>({
    mutationFn: patchWebsite,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: websiteKeys.all });
    },
  });
};

export const useDeleteWebsite = () => {
  const queryClient = useQueryClient();
  return useMutation<Website, Error, string>({
    mutationFn: deleteWebsite,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: websiteKeys.all });
    },
  });
};
