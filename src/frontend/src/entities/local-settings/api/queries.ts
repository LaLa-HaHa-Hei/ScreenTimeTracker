import {
  queryOptions,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";
import { getLocalSettingsDto, patchLocalSettingsDto } from "./requests";
import type {
  PatchLocalSettingsParams as PatchLocalSettingsParams,
  LocalSettingsDto as LocalSettingsDto,
} from "./schemas";

export const localSettingsQueries = {
  localSettings: () => {
    return queryOptions({
      queryKey: ["local-settings"],
      queryFn: getLocalSettingsDto,
      staleTime: 0,
    });
  },
};

export const usePatchLocalettings = () => {
  const queryClient = useQueryClient();
  return useMutation<LocalSettingsDto, Error, PatchLocalSettingsParams>({
    mutationFn: patchLocalSettingsDto,
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["local-settings"],
      });
    },
  });
};
