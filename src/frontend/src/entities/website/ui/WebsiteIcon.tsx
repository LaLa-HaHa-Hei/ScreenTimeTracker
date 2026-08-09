import { getWebsiteIconUrl } from "../api/requests";
import type { SxProps, Theme } from "@mui/material/styles";
import Box from "@mui/material/Box";
import UnknownWebsite from "@/shared/ui/UnknownWebsite.svg";

export const WebsiteIcon = ({
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
      src={iconPath === null ? UnknownWebsite : getWebsiteIconUrl(id, iconPathLastUpdatedAt)}
      className={className}
      sx={[{}, ...(Array.isArray(sx) ? sx : [sx])]}
    />
  );
};
