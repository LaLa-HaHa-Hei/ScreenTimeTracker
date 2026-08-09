import Autocomplete, { type AutocompleteProps } from "@mui/material/Autocomplete";
import Box from "@mui/material/Box";
import Chip from "@mui/material/Chip";
import type { SxProps, Theme } from "@mui/material/styles";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { useQuery } from "@tanstack/react-query";
import { useEffect, useMemo, type SyntheticEvent } from "react";
import { useTranslation } from "react-i18next";
import type { WebsiteCategory } from "../model/schemas";
import { websiteCategoryQueries } from "../api/queries";
import { WebsiteCategoryIcon } from "./WebsiteCategoryIcon";

type Item = Pick<WebsiteCategory, "id" | "name" | "iconPath" | "iconPathLastUpdatedAt">;

export type WebsiteCategoryPickerProps = {
  className?: string;
  sx?: SxProps<Theme>;
  placeholder?: string;
} & (
  | {
      mode: "single";
      value: string | null;
      onValueChange: (value: string | null) => void;
    }
  | {
      mode: "multiple";
      value: string[];
      onValueChange: (value: string[]) => void;
      maxDisplayCount?: number;
    }
) &
  Omit<
    AutocompleteProps<Item, true, true, false>,
    | "multiple"
    | "options"
    | "loading"
    | "loadingText"
    | "value"
    | "onChange"
    | "getOptionLabel"
    | "isOptionEqualToValue"
    | "renderValue"
    | "renderOption"
    | "renderInput"
    | "disableCloseOnSelect"
    | "noOptionsText"
  >;

export const WebsiteCategoryPicker = (props: WebsiteCategoryPickerProps) => {
  const { t } = useTranslation(["entity_websiteCategory"]);
  let commonProps;
  if (props.mode === "multiple") {
    const { maxDisplayCount: _, ...tempProps } = props;
    commonProps = tempProps;
  } else commonProps = props;
  const { mode, value, onValueChange, placeholder, ...autocompleteProps } = commonProps;
  const { data: websiteCategoriesData, isLoading: iswebsiteCategoriesDataLoading } = useQuery(
    websiteCategoryQueries.websiteCategories({
      fields: "id,name,iconPath,iconPathLastUpdatedAt",
    }),
  ) as {
    data?: Item[];
    isLoading: boolean;
  };

  // 将外部的 string/string[] 转换为 Autocomplete 需要的对象数组
  const selectedOptions = useMemo(() => {
    if (!websiteCategoriesData) return [];

    if (mode === "single") {
      const found = websiteCategoriesData.find((item) => item.id === value);
      return found ? [found] : [];
    } else {
      const valueSet = new Set(value as string[]);
      return websiteCategoriesData.filter((item) => valueSet.has(item.id));
    }
  }, [websiteCategoriesData, value, mode]);

  // 数据校验：自动移除不存在的 ID
  useEffect(() => {
    if (!websiteCategoriesData) return;
    const validIdSet = new Set(websiteCategoriesData.map((e) => e.id));

    if (mode === "single") {
      if (value && !validIdSet.has(value as string)) onValueChange(null);
    } else {
      const nextValue = (value as string[]).filter((id) => validIdSet.has(id));
      if (nextValue.length !== (value as string[]).length) onValueChange(nextValue);
    }
  }, [websiteCategoriesData, value, mode, onValueChange]);

  const handleChange = (_: SyntheticEvent, newValue: Item[]) => {
    if (mode === "single") {
      const lastSelected = newValue.length > 0 ? newValue[newValue.length - 1] : null;
      onValueChange(lastSelected ? lastSelected.id : null);
    } else {
      onValueChange(newValue.map((v) => v.id));
    }
  };

  return (
    <Autocomplete
      {...autocompleteProps}
      multiple
      options={websiteCategoriesData || []}
      loading={iswebsiteCategoriesDataLoading}
      loadingText={t(($) => $.entity_websiteCategory.ui.state.loading)}
      value={selectedOptions}
      onChange={handleChange}
      getOptionLabel={(option) => option.name}
      isOptionEqualToValue={(option, v) => option.id === v.id}
      renderValue={(value: readonly Item[], getItemProps) => {
        if (
          props.mode === "multiple" &&
          props.maxDisplayCount !== undefined &&
          value.length > props.maxDisplayCount
        )
          return (
            <Chip
              variant="outlined"
              size="small"
              label={t(($) => $.entity_websiteCategory.ui.picker.selectedCount, {
                count: value.length,
              })}
            />
          );
        else
          return value.map((option: Item, index: number) => {
            const { key: _, ...itemProps } = getItemProps({ index });
            return (
              <Chip
                variant="outlined"
                size="small"
                label={option.name}
                key={option.id}
                {...itemProps}
              />
            );
          });
      }}
      renderOption={(props, option) => {
        const { key: _, ...otherProps } = props;
        return (
          <Box component="li" key={option.id} {...otherProps}>
            <WebsiteCategoryIcon
              sx={{
                width: "1.5rem",
                height: "1.r5em",
                mr: 1,
              }}
              id={option.id}
              iconPath={option.iconPath}
              iconPathLastUpdatedAt={option.iconPathLastUpdatedAt}
            />
            <Typography>{option.name}</Typography>
          </Box>
        );
      }}
      renderInput={(params) => <TextField {...params} size="small" placeholder={placeholder} />}
      disableCloseOnSelect={mode === "multiple"}
      noOptionsText={t(($) => $.entity_websiteCategory.ui.state.noOptions)}
    />
  );
};
