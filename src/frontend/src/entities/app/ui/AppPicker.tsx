import { useQuery } from "@tanstack/react-query";
import { type SyntheticEvent, useEffect, useMemo } from "react";
import type { App } from "../model/schemas";
import { appQueries } from "../api/queries";
import { AppIcon } from "./AppIcon";
import Autocomplete, { type AutocompleteProps } from "@mui/material/Autocomplete";
import Box from "@mui/material/Box";
import Chip from "@mui/material/Chip";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { useTranslation } from "react-i18next";
type Item = Pick<App, "id" | "name" | "iconPath" | "iconPathLastUpdatedAt">;

export type AppPickerProps = {
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

export const AppPicker = (props: AppPickerProps) => {
  const { t } = useTranslation(["entity_app"]);
  let commonProps;
  if (props.mode === "multiple") {
    const { maxDisplayCount: _, ...tempProps } = props;
    commonProps = tempProps;
  } else commonProps = props;
  const { mode, value, onValueChange, placeholder, ...autocompleteProps } = commonProps;

  const { data: appsData, isLoading: isAppsDataLoading } = useQuery(
    appQueries.apps({ fields: "id,name,iconPath,iconPathLastUpdatedAt" }),
  ) as {
    data?: Item[];
    isLoading: boolean;
  };

  // 将外部的 string/string[] 转换为 Autocomplete 需要的对象数组
  const selectedOptions = useMemo(() => {
    if (!appsData) return [];

    if (mode === "single") {
      const found = appsData.find((item) => item.id === value);
      return found ? [found] : [];
    } else {
      const valueSet = new Set(value as string[]);
      return appsData.filter((item) => valueSet.has(item.id));
    }
  }, [appsData, value, mode]);

  // 数据校验：自动移除不存在的 ID
  useEffect(() => {
    if (!appsData) return;
    const validIdSet = new Set(appsData.map((e) => e.id));

    if (mode === "single") {
      if (value && !validIdSet.has(value as string)) onValueChange(null);
    } else {
      const nextValue = (value as string[]).filter((id) => validIdSet.has(id));
      if (nextValue.length !== (value as string[]).length) onValueChange(nextValue);
    }
  }, [appsData, value, mode, onValueChange]);

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
      options={appsData || []}
      loading={isAppsDataLoading}
      loadingText={t(($) => $.entity_app.ui.state.loading)}
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
          return <Chip variant="outlined" size="small" label={t(($) => $.entity_app.ui.picker.selectedCount, { count: value.length })} />;
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
            <AppIcon
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
      noOptionsText={t(($) => $.entity_app.ui.state.noOptions)}
    />
  );
};
