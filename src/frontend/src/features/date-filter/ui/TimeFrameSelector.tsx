import ToggleButtonGroup from "@mui/material/ToggleButtonGroup";
import type { TimeFrame } from "../model/schemas";
import type { Theme } from "@emotion/react";
import type { SxProps } from "@mui/material/styles";
import ToggleButton from "@mui/material/ToggleButton";
import { useTranslation } from "react-i18next";

type TimeFrameOption = { value: TimeFrame; label: string };

export type TimeFrameSelectorProps = {
  sx?: SxProps<Theme>;
  className?: string;
  value: TimeFrame;
  onValueChange: (value: TimeFrame) => void;
};

export const TimeFrameSelector = ({
  sx,
  className,
  value,
  onValueChange,
}: TimeFrameSelectorProps) => {
  const { t } = useTranslation(["feature_dateFilter"]);
  const options: TimeFrameOption[] = [
    {
      value: "day",
      label: t(($) => $.feature_dateFilter.timeFrame.day),
    },
    {
      value: "week",
      label: t(($) => $.feature_dateFilter.timeFrame.week),
    },
    {
      value: "month",
      label: t(($) => $.feature_dateFilter.timeFrame.month),
    },
    {
      value: "custom",
      label: t(($) => $.feature_dateFilter.timeFrame.custom),
    },
  ];

  return (
    <ToggleButtonGroup
      size="small"
      exclusive
      sx={sx}
      className={className}
      value={value}
      onChange={(_, newTimeFrame: TimeFrame | null) => {
        if (newTimeFrame !== null) {
          onValueChange(newTimeFrame);
        }
      }}
    >
      {options.map((item) => (
        <ToggleButton key={item.value} value={item.value}>
          {item.label}
        </ToggleButton>
      ))}
    </ToggleButtonGroup>
  );
};
