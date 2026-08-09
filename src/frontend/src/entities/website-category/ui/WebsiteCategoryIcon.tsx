import { getWebsiteCategoryIconUrl } from "../api/requests";
import type { SxProps, Theme } from "@mui/material/styles";
import Box from "@mui/material/Box";
import UnknownCategory from "@/shared/ui/UnknownCategory.svg";

export const WebsiteCategoryIcon = ({
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
        iconPath === null ? UnknownCategory : getWebsiteCategoryIconUrl(id, iconPathLastUpdatedAt)
      }
      className={className}
      sx={[{}, ...(Array.isArray(sx) ? sx : [sx])]}
    />
  );
};
