import { useQuery } from "@tanstack/react-query";
import ReactECharts from "echarts-for-react";
import {
  appCategoryUsageDistributionQueryOptions,
  appUsageDistributionQueryOptions,
  websiteCategoryUsageDistributionQueryOptions,
  websiteUsageDistributionQueryOptions,
} from "../api/queries";
import type { DateOnly } from "@/shared/lib/date-only";
import { formatSecondsDuration } from "@/shared/lib/time";
import { useMemo } from "react";
import { getAppCategoryIconUrl } from "@/entities/app-category";
import { getAppIconUrl } from "@/entities/app";
import UnknownApp from "@/shared/ui/UnknownApp.svg";
import UnknownCategory from "@/shared/ui/UnknownCategory.svg";
import UnknownWebsite from "@/shared/ui/UnknownWebsite.svg";
import type { Theme } from "@emotion/react";
import { useTheme, type SxProps } from "@mui/material/styles";
import Box from "@mui/material/Box";
import { useTranslation } from "react-i18next";
import { getWebsiteIconUrl } from "@/entities/website";
import { getWebsiteCategoryIconUrl } from "@/entities/website-category";
import dayjs from "dayjs";

export type UsageDistributionPieChartProps = {
  className?: string;
  sx?: SxProps<Theme>;
  type: "app" | "app-category" | "website" | "website-category";
  startDate: DateOnly;
  endDate: DateOnly;
  timeZoneId: string;
  topN: number;
  excludedIds?: string[];
  onItemClick?: (id: string) => void;
};

export const UsageDistributionPieChart = ({
  className,
  sx,
  type,
  startDate,
  endDate,
  timeZoneId,
  topN,
  excludedIds,
  onItemClick,
}: UsageDistributionPieChartProps) => {
  const { t } = useTranslation("feature_usageDistribution");
  const theme = useTheme();
  const isDark = theme.palette.mode === "dark";

  const isDateValid = dayjs(startDate).isSameOrBefore(dayjs(endDate));
  const { data: appUsageDistributionData } = useQuery({
    ...appUsageDistributionQueryOptions({
      startDate: startDate,
      endDate: endDate,
      timeZoneId: timeZoneId,
      topN: topN,
      excludedIds: excludedIds,
    }),
    enabled: isDateValid && type === "app",
  });
  const { data: appCategoryUsageDistributionData } = useQuery({
    ...appCategoryUsageDistributionQueryOptions({
      startDate: startDate,
      endDate: endDate,
      timeZoneId: timeZoneId,
      topN: topN,
      excludedIds: excludedIds,
    }),
    enabled: isDateValid && type === "app-category",
  });
  const { data: websiteUsageDistributionData } = useQuery({
    ...websiteUsageDistributionQueryOptions({
      startDate: startDate,
      endDate: endDate,
      timeZoneId: timeZoneId,
      topN: topN,
      excludedIds: excludedIds,
    }),
    enabled: isDateValid && type === "website",
  });
  const { data: websiteCategoryUsageDistributionData } = useQuery({
    ...websiteCategoryUsageDistributionQueryOptions({
      startDate: startDate,
      endDate: endDate,
      timeZoneId: timeZoneId,
      topN: topN,
      excludedIds: excludedIds,
    }),
    enabled: isDateValid && type === "website-category",
  });
  const usageDistributiondata =
    type === "app"
      ? appUsageDistributionData
      : type === "app-category"
        ? appCategoryUsageDistributionData
        : type === "website"
          ? websiteUsageDistributionData
          : websiteCategoryUsageDistributionData;

  const option = useMemo(() => {
    return {
      backgroundColor: "transparent",
      legend: {
        type: "scroll",
        top: 0,
      },
      label: {
        formatter: (params: { data: { id: string | null; name: string } }) => {
          if (params.data.id !== null) {
            return `{${toRichKey(params.data.id)}|} {name|${params.data.name}}`;
          } else return `${params.data.name}`;
        },
        rich: usageDistributiondata?.items.reduce(
          (acc, v) => {
            const iconUrl =
              type === "app"
                ? getAppIconUrl(v.id, v.iconPathLastUpdatedAt)
                : type === "app-category"
                  ? getAppCategoryIconUrl(v.id, v.iconPathLastUpdatedAt)
                  : type === "website"
                    ? getWebsiteIconUrl(v.id, v.iconPathLastUpdatedAt)
                    : getWebsiteCategoryIconUrl(v.id, v.iconPathLastUpdatedAt);
            const fallbackIconUrl =
              type === "app"
                ? UnknownApp
                : type === "app-category"
                  ? UnknownCategory
                  : type === "website"
                    ? UnknownWebsite
                    : UnknownCategory;
            acc[toRichKey(v.id)] = {
              backgroundColor: {
                image: v.iconPath ? iconUrl : fallbackIconUrl,
              },
              width: 15,
              height: 15,
            };
            return acc;
          },
          {} as Record<
            string,
            {
              backgroundColor: { image: string };
              width: number;
              height: number;
            }
          >,
        ),
      },
      tooltip: {
        trigger: "item",
        formatter: (params: {
          percent: number;
          data: {
            name: string;
            durationSeconds: number;
          };
        }) => {
          const duration = formatSecondsDuration(params.data.durationSeconds);
          return `${params.data.name}<br/>${duration} (${params.percent}%)`;
        },
      },
      series: [
        {
          type: "pie",
          radius: "70%",
          data: [
            ...(usageDistributiondata?.items.map((v) => ({
              name: v.name,
              value: v.durationSeconds,
              id: v.id,
              iconPath: v.iconPath,
              durationSeconds: v.durationSeconds,
              itemStyle: {
                color: v.color,
              },
            })) || []),
            ...(usageDistributiondata && usageDistributiondata.othersDurationSeconds > 0
              ? [
                  {
                    name:
                      type === "app"
                        ? t(($) => $.feature_usageDistribution.pieChart.otherApps, {
                            count: usageDistributiondata.othersCount,
                          })
                        : type === "app-category"
                          ? t(($) => $.feature_usageDistribution.pieChart.otherCategories, {
                              count: usageDistributiondata.othersCount,
                            })
                          : type === "website"
                            ? t(($) => $.feature_usageDistribution.pieChart.otherWebsites, {
                                count: usageDistributiondata.othersCount,
                              })
                            : t(($) => $.feature_usageDistribution.pieChart.otherCategories, {
                                count: usageDistributiondata.othersCount,
                              }),
                    value: usageDistributiondata?.othersDurationSeconds,
                    id: null,
                    itemStyle: {
                      color: isDark ? "#9CA3AF" : "#6B7280",
                    },
                  },
                ]
              : []),
          ],
        },
      ],
    };
  }, [type, usageDistributiondata, isDark, t]);

  return (
    <Box sx={sx} className={className}>
      <ReactECharts
        style={{
          height: "100%",
          width: "100%",
        }}
        theme={isDark ? "dark" : undefined}
        option={option}
        notMerge={true}
        onEvents={{
          click: (params: { data: { id: string | null } }) => {
            if (params.data.id !== null) {
              onItemClick?.(params.data.id);
            }
          },
        }}
      />
    </Box>
  );
};

function toRichKey(id: string): string {
  return `icon_${id.replace(/-/g, "_")}`;
}
