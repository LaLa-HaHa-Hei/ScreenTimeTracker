import Box from "@mui/material/Box";
import MenuItem from "@mui/material/MenuItem";
import Select, { type SelectChangeEvent } from "@mui/material/Select";
import type { SxProps, Theme } from "@mui/material/styles";
import Typography from "@mui/material/Typography";
import { useQuery } from "@tanstack/react-query";
import type { WebsiteCategory } from "../model/schemas";
import { websiteCategoryQueries } from "../api/queries";
import { WebsiteCategoryIcon } from "./WebsiteCategoryIcon";

type Item = Pick<WebsiteCategory, "id" | "name" | "iconPath" | "iconPathLastUpdatedAt">;

export type WebsiteCategorySelecterProps = {
  className?: string;
  sx?: SxProps<Theme>;
  value: string;
  onValueChange: (value: string) => void;
};

export const WebsiteCategorySelecter = ({
  className,
  sx,
  value,
  onValueChange,
}: WebsiteCategorySelecterProps) => {
  const { data } = useQuery(
    websiteCategoryQueries.websiteCategories({
      fields: "id,name,iconPath,iconPathLastUpdatedAt",
    }),
  ) as {
    data?: Item[];
    isLoading: boolean;
  };

  const handleChange = (event: SelectChangeEvent) => {
    onValueChange(event.target.value);
  };

  if (!data) return null;

  return (
    <Select
      size="small"
      className={className}
      sx={[{}, ...(Array.isArray(sx) ? sx : [sx])]}
      value={value}
      onChange={handleChange}
    >
      {data?.map((item) => {
        return (
          <MenuItem value={item.id} key={item.id}>
            <Box sx={{ display: "flex", alignItems: "center" }}>
              <WebsiteCategoryIcon
                id={item.id}
                iconPath={item.iconPath}
                iconPathLastUpdatedAt={item.iconPathLastUpdatedAt}
                sx={{ width: "1.5rem", height: "1.5rem", mr: 1 }}
              />
              <Typography>{item.name}</Typography>
            </Box>
          </MenuItem>
        );
      })}
    </Select>
  );
};
