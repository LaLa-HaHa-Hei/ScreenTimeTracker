import { getAppCategoryIconUrl } from "../api/requests";
import type { SxProps, Theme } from "@mui/material/styles";
import Box from "@mui/material/Box";
import UnknownAppCategory from "@/shared/ui/UnknownAppCategory.svg";

export const AppCategoryIcon = ({
  sx,
  className,
  id,
  iconPath,
  iconPathLastUpdatedAt,
}: {
  sx?: SxProps<Theme>;
  className?: string;
  id: string;
  iconPath: string | null;
  iconPathLastUpdatedAt: Date;
}) => {
  return (
    <Box
      component="img"
      src={
        iconPath === null
          ? UnknownAppCategory
          : getAppCategoryIconUrl(id, iconPathLastUpdatedAt)
      }
      className={className}
      sx={[{}, ...(Array.isArray(sx) ? sx : [sx])]}
    />
  );
};
