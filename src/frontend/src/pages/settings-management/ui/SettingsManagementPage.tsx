import { localSettingsQueries, usePatchLocalSettings } from "@/entities/local-settings";
import { usePatchUserSettings, userSettingsQueries } from "@/entities/user-settings";
import Box from "@mui/material/Box";
import CircularProgress from "@mui/material/CircularProgress";
import IconButton from "@mui/material/IconButton";
import MenuItem from "@mui/material/MenuItem";
import Paper from "@mui/material/Paper";
import Select, { type SelectChangeEvent } from "@mui/material/Select";
import Stack from "@mui/material/Stack";
import Tooltip from "@mui/material/Tooltip";
import Typography from "@mui/material/Typography";
import Switch from "@mui/material/Switch";
import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import HelpIcon from "@mui/icons-material/Help";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogActions from "@mui/material/DialogActions";
import Button from "@mui/material/Button";
import { LazyTextField } from "@/shared/ui/LazyTextField";
import { LazyNumberField } from "@/shared/ui/LazyNumberField";
import { SUPPORTED_LANGUAGES, type LanguageCode } from "@/shared/i18n";
import { useTranslation } from "react-i18next";
import Grid from "@mui/material/Grid";

export const SettingsManagementPage = () => {
  const { t } = useTranslation(["page_settingsManagement"]);
  const { data: userSettingsDtoData, isLoading: isuserSettingsDataLoading } = useQuery(
    userSettingsQueries.userSettings(),
  );
  const { data: localSettingsDtoData, isLoading: isLocalSettingsDataLoading } = useQuery(
    localSettingsQueries.localSettings(),
  );
  const { mutateAsync: patchUserSettingsAsync } = usePatchUserSettings();
  const { mutateAsync: patchLocalSettingsAsync } = usePatchLocalSettings();
  const [autoStartAlertDialogOpen, setAutoStartAlertDialogOpen] = useState(false);

  if (isuserSettingsDataLoading || isLocalSettingsDataLoading)
    return (
      <Box
        sx={{
          display: "flex",
          justifyContent: "center",
        }}
      >
        <CircularProgress />
      </Box>
    );

  if (!userSettingsDtoData || !localSettingsDtoData)
    return (
      <Box
        sx={{
          display: "flex",
          justifyContent: "center",
        }}
      >
        <Typography>{t(($) => $.page_settingsManagement.errors.fetchFailed)}</Typography>
      </Box>
    );

  return (
    <Grid container spacing={2}>
      {/* 第一列 */}
      <Grid size={{ xs: 12, lg: 6 }}>
        <Stack spacing={2} direction="column">
          {/* 本地设置 */}
          <Paper
            variant="outlined"
            sx={{
              p: 2,
            }}
          >
            <Typography sx={{ fontWeight: "bold" }}>
              {t(($) => $.page_settingsManagement.localSettings.title)}
            </Typography>
            <Stack spacing={1} direction="column" sx={{ mt: 2 }}>
              {/* 语言设置 */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Typography>
                  {t(($) => $.page_settingsManagement.localSettings.language.label)}
                </Typography>
                <Select
                  size="small"
                  value={localSettingsDtoData.language}
                  onChange={async (event: SelectChangeEvent<string>) => {
                    await patchLocalSettingsAsync({
                      language: event.target.value as LanguageCode,
                    });
                  }}
                >
                  {SUPPORTED_LANGUAGES.map((language) => (
                    <MenuItem key={language.code} value={language.code}>
                      {language.label}
                    </MenuItem>
                  ))}
                </Select>
              </Stack>
              {/* 打开模式设置 */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Stack direction="row" sx={{ alignItems: "center" }}>
                  <Typography>
                    {t(($) => $.page_settingsManagement.localSettings.defaultUIOpenMode.label)}
                  </Typography>
                  <Tooltip
                    title={t(
                      ($) => $.page_settingsManagement.localSettings.defaultUIOpenMode.tooltip,
                    )}
                  >
                    <IconButton size="small">
                      <HelpIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Stack>
                <Select
                  size="small"
                  value={localSettingsDtoData.defaultUIOpenMode}
                  onChange={async (event: SelectChangeEvent<string>) => {
                    await patchLocalSettingsAsync({
                      defaultUIOpenMode: event.target.value as "Window" | "Browser",
                    });
                  }}
                >
                  <MenuItem value="Window">
                    {" "}
                    {t(
                      ($) =>
                        $.page_settingsManagement.localSettings.defaultUIOpenMode.options.window,
                    )}
                  </MenuItem>
                  <MenuItem value="Browser">
                    {t(
                      ($) =>
                        $.page_settingsManagement.localSettings.defaultUIOpenMode.options.browser,
                    )}
                  </MenuItem>
                </Select>
              </Stack>
              {/* 开机启动设置 */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Stack direction="row" sx={{ alignItems: "center" }}>
                  <Typography>
                    {t(($) => $.page_settingsManagement.localSettings.isAutoStartEnabled.label)}
                  </Typography>
                  <Tooltip
                    title={t(
                      ($) => $.page_settingsManagement.localSettings.isAutoStartEnabled.tooltip,
                    )}
                  >
                    <IconButton size="small">
                      <HelpIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Stack>
                <Switch
                  checked={localSettingsDtoData.isAutoStartEnabled}
                  onChange={async (event: React.ChangeEvent<HTMLInputElement>) => {
                    const {
                      target: { checked },
                    } = event;
                    if (checked === true) setAutoStartAlertDialogOpen(true);

                    await patchLocalSettingsAsync({
                      isAutoStartEnabled: checked,
                    });
                  }}
                />
              </Stack>
              {/* 开机启动警告 Dialog */}
              <Dialog
                open={autoStartAlertDialogOpen}
                onClose={() => setAutoStartAlertDialogOpen(false)}
                role="alertdialog"
              >
                <DialogTitle>
                  {t(($) => $.page_settingsManagement.localSettings.autoStartAlert.title)}
                </DialogTitle>
                <DialogActions>
                  <Button
                    onClick={() => {
                      setAutoStartAlertDialogOpen(false);
                    }}
                  >
                    {t(($) => $.page_settingsManagement.actions.confirm)}
                  </Button>
                </DialogActions>
              </Dialog>
              {/* 静默启动设置 */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Stack direction="row" sx={{ alignItems: "center" }}>
                  <Typography>
                    {t(($) => $.page_settingsManagement.localSettings.isSilentStartEnabled.label)}
                  </Typography>
                  <Tooltip
                    title={t(
                      ($) => $.page_settingsManagement.localSettings.isSilentStartEnabled.tooltip,
                    )}
                  >
                    <IconButton size="small">
                      <HelpIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Stack>
                <Switch
                  checked={localSettingsDtoData.isSilentStartEnabled}
                  onChange={async (event: React.ChangeEvent<HTMLInputElement>) => {
                    await patchLocalSettingsAsync({
                      isSilentStartEnabled: event.target.checked,
                    });
                  }}
                />
              </Stack>
            </Stack>
          </Paper>
          {/* 空闲检测 */}
          <Paper
            variant="outlined"
            sx={{
              p: 2,
            }}
          >
            <Typography sx={{ fontWeight: "bold" }}>
              {t(($) => $.page_settingsManagement.idleDetectionSettings.title)}
            </Typography>
            <Stack spacing={1} direction="column" sx={{ mt: 2 }}>
              {/* 是否启用 */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Stack direction="row" sx={{ alignItems: "center" }}>
                  <Typography>
                    {t(($) => $.page_settingsManagement.idleDetectionSettings.isEnabled.label)}
                  </Typography>
                  <Tooltip
                    title={t(
                      ($) => $.page_settingsManagement.idleDetectionSettings.isEnabled.tooltip,
                    )}
                  >
                    <IconButton size="small">
                      <HelpIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Stack>
                <Switch
                  checked={userSettingsDtoData.idleDetection.isEnabled}
                  onChange={async (event: React.ChangeEvent<HTMLInputElement>) => {
                    await patchUserSettingsAsync({
                      idleDetection: {
                        ...userSettingsDtoData.idleDetection,
                        isEnabled: event.target.checked,
                      },
                    });
                  }}
                />
              </Stack>
              {/* 不活跃阈值设置 */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Stack direction="row" sx={{ alignItems: "center" }}>
                  <Typography>
                    {t(
                      ($) =>
                        $.page_settingsManagement.idleDetectionSettings.inactivityThresholdSeconds
                          .label,
                    )}
                  </Typography>
                  <Tooltip
                    title={t(
                      ($) =>
                        $.page_settingsManagement.idleDetectionSettings.inactivityThresholdSeconds
                          .tooltip,
                    )}
                  >
                    <IconButton size="small">
                      <HelpIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Stack>
                <LazyNumberField
                  size="small"
                  value={userSettingsDtoData.idleDetection.inactivityThresholdSeconds}
                  onValueChange={async (value) => {
                    if (value === undefined) return;
                    await patchUserSettingsAsync({
                      idleDetection: {
                        ...userSettingsDtoData.idleDetection,
                        inactivityThresholdSeconds: value,
                      },
                    });
                  }}
                  min={1}
                  allowDecimal={false}
                  allowEmpty={false}
                />
              </Stack>
              {/* 空闲检测轮询间隔设置 */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Stack direction="row" sx={{ alignItems: "center" }}>
                  <Typography>
                    {t(
                      ($) =>
                        $.page_settingsManagement.idleDetectionSettings.pollingIntervalSeconds
                          .label,
                    )}
                  </Typography>
                  <Tooltip
                    title={t(
                      ($) =>
                        $.page_settingsManagement.idleDetectionSettings.pollingIntervalSeconds
                          .tooltip,
                    )}
                  >
                    <IconButton size="small">
                      <HelpIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Stack>
                <LazyNumberField
                  size="small"
                  value={userSettingsDtoData.idleDetection.pollingIntervalSeconds}
                  onValueChange={async (value) => {
                    if (value === undefined) return;
                    await patchUserSettingsAsync({
                      idleDetection: {
                        ...userSettingsDtoData.idleDetection,
                        pollingIntervalSeconds: value,
                      },
                    });
                  }}
                  min={1}
                  allowDecimal={false}
                  allowEmpty={false}
                />
              </Stack>
            </Stack>
          </Paper>
          {/* 时间边界设置 */}
          <Paper
            variant="outlined"
            sx={{
              p: 2,
            }}
          >
            <Typography sx={{ fontWeight: "bold" }}>
              {t(($) => $.page_settingsManagement.timeBoundarySettings.title)}
            </Typography>
            <Stack spacing={1} direction="column" sx={{ mt: 2 }}>
              {/* 日期切换小时设置 */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Stack direction="row" sx={{ alignItems: "center" }}>
                  <Typography>
                    {t(($) => $.page_settingsManagement.timeBoundarySettings.dayCutoffHour.label)}
                  </Typography>
                  <Tooltip
                    title={t(
                      ($) => $.page_settingsManagement.timeBoundarySettings.dayCutoffHour.tooltip,
                    )}
                  >
                    <IconButton size="small">
                      <HelpIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Stack>
                <LazyNumberField
                  size="small"
                  value={userSettingsDtoData.timeBoundary.dayCutoffHour}
                  onValueChange={async (value) => {
                    if (value === undefined) return;
                    await patchUserSettingsAsync({
                      timeBoundary: {
                        ...userSettingsDtoData.timeBoundary,
                        dayCutoffHour: value,
                      },
                    });
                  }}
                  min={0}
                  allowDecimal={false}
                  allowEmpty={false}
                />
              </Stack>
            </Stack>
          </Paper>
          {/* 区域设置 */}
          <Paper
            variant="outlined"
            sx={{
              p: 2,
            }}
          >
            <Typography sx={{ fontWeight: "bold" }}>
              {t(($) => $.page_settingsManagement.regionalSettings.title)}
            </Typography>
            <Stack spacing={1} direction="column" sx={{ mt: 2 }}>
              {/* IANA 时区 ID */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Stack direction="row" sx={{ alignItems: "center" }}>
                  <Typography>
                    {t(($) => $.page_settingsManagement.regionalSettings.timeZoneId.label)}
                  </Typography>
                  <Tooltip
                    title={t(($) => $.page_settingsManagement.regionalSettings.timeZoneId.tooltip)}
                  >
                    <IconButton size="small">
                      <HelpIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Stack>
                <Select
                  size="small"
                  value={userSettingsDtoData.regional.timeZoneId}
                  onChange={async (event: SelectChangeEvent<string>) => {
                    await patchUserSettingsAsync({
                      regional: {
                        ...userSettingsDtoData.regional,
                        timeZoneId: event.target.value,
                      },
                    });
                  }}
                >
                  {Intl.supportedValuesOf("timeZone").map((timeZoneId) => (
                    <MenuItem key={timeZoneId} value={timeZoneId}>
                      {timeZoneId}
                    </MenuItem>
                  ))}
                </Select>
              </Stack>
            </Stack>
          </Paper>
        </Stack>
      </Grid>
      {/* 第二列 */}
      <Grid size={{ xs: 12, lg: 6 }}>
        <Stack spacing={2} direction="column">
          {/* 应用追踪 */}
          <Paper
            variant="outlined"
            sx={{
              p: 2,
            }}
          >
            <Typography sx={{ fontWeight: "bold" }}>
              {t(($) => $.page_settingsManagement.appTrackingSettings.title)}
            </Typography>
            <Stack spacing={1} direction="column" sx={{ mt: 2 }}>
              {/* 图标目录设置 */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Stack direction="row" sx={{ alignItems: "center" }}>
                  <Typography>
                    {t(($) => $.page_settingsManagement.appTrackingSettings.iconDirectory.label)}
                  </Typography>
                  <Tooltip
                    title={t(
                      ($) => $.page_settingsManagement.appTrackingSettings.iconDirectory.tooltip,
                    )}
                  >
                    <IconButton size="small">
                      <HelpIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Stack>
                <LazyTextField
                  size="small"
                  value={userSettingsDtoData.appTracking.iconDirectory}
                  onValueChange={async (value) => {
                    await patchUserSettingsAsync({
                      appTracking: {
                        ...userSettingsDtoData.appTracking,
                        iconDirectory: value,
                      },
                    });
                  }}
                />
              </Stack>
              {/* 元数据过期阈值设置 */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Stack direction="row" sx={{ alignItems: "center" }}>
                  <Typography>
                    {t(
                      ($) =>
                        $.page_settingsManagement.appTrackingSettings.metadataStaleThresholdMinutes
                          .label,
                    )}
                  </Typography>
                  <Tooltip
                    title={t(
                      ($) =>
                        $.page_settingsManagement.appTrackingSettings.metadataStaleThresholdMinutes
                          .tooltip,
                    )}
                  >
                    <IconButton size="small">
                      <HelpIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Stack>
                <LazyNumberField
                  size="small"
                  value={userSettingsDtoData.appTracking.metadataStaleThresholdMinutes}
                  onValueChange={async (value) => {
                    if (value === undefined) return;
                    await patchUserSettingsAsync({
                      appTracking: {
                        ...userSettingsDtoData.appTracking,
                        metadataStaleThresholdMinutes: value,
                      },
                    });
                  }}
                  min={0}
                  allowDecimal={false}
                  allowEmpty={false}
                />
              </Stack>
              {/* 活跃应用会话自动保存设置 */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Stack direction="row" sx={{ alignItems: "center" }}>
                  <Typography>
                    {t(
                      ($) =>
                        $.page_settingsManagement.appTrackingSettings
                          .activeUsageSessionAutoSaveIntervalSeconds.label,
                    )}
                  </Typography>
                  <Tooltip
                    title={t(
                      ($) =>
                        $.page_settingsManagement.appTrackingSettings
                          .activeUsageSessionAutoSaveIntervalSeconds.tooltip,
                    )}
                  >
                    <IconButton size="small">
                      <HelpIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Stack>
                <LazyNumberField
                  size="small"
                  value={userSettingsDtoData.appTracking.activeUsageSessionAutoSaveIntervalSeconds}
                  onValueChange={async (value) => {
                    if (value === undefined) return;
                    await patchUserSettingsAsync({
                      appTracking: {
                        ...userSettingsDtoData.appTracking,
                        activeUsageSessionAutoSaveIntervalSeconds: value,
                      },
                    });
                  }}
                  min={1}
                  allowDecimal={false}
                  allowEmpty={false}
                />
              </Stack>
              {/* 最小有效会话时长设置 */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Stack direction="row" sx={{ alignItems: "center" }}>
                  <Typography>
                    {t(
                      ($) =>
                        $.page_settingsManagement.appTrackingSettings
                          .minValidUsageSessionDurationSeconds.label,
                    )}
                  </Typography>
                  <Tooltip
                    title={t(
                      ($) =>
                        $.page_settingsManagement.appTrackingSettings
                          .minValidUsageSessionDurationSeconds.tooltip,
                    )}
                  >
                    <IconButton size="small">
                      <HelpIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Stack>
                <LazyNumberField
                  size="small"
                  value={userSettingsDtoData.appTracking.minValidUsageSessionDurationSeconds}
                  onValueChange={async (value) => {
                    if (value === undefined) return;
                    await patchUserSettingsAsync({
                      appTracking: {
                        ...userSettingsDtoData.appTracking,
                        minValidUsageSessionDurationSeconds: value,
                      },
                    });
                  }}
                  min={0}
                  allowDecimal={false}
                  allowEmpty={false}
                />
              </Stack>
              {/* 会话合并容差时间设置 */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Stack direction="row" sx={{ alignItems: "center" }}>
                  <Typography>
                    {t(
                      ($) =>
                        $.page_settingsManagement.appTrackingSettings
                          .usageSessionMergeToleranceSeconds.label,
                    )}
                  </Typography>
                  <Tooltip
                    title={t(
                      ($) =>
                        $.page_settingsManagement.appTrackingSettings
                          .usageSessionMergeToleranceSeconds.tooltip,
                    )}
                  >
                    <IconButton size="small">
                      <HelpIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Stack>
                <LazyNumberField
                  size="small"
                  value={userSettingsDtoData.appTracking.usageSessionMergeToleranceSeconds}
                  onValueChange={async (value) => {
                    if (value === undefined) return;
                    await patchUserSettingsAsync({
                      appTracking: {
                        ...userSettingsDtoData.appTracking,
                        usageSessionMergeToleranceSeconds: value,
                      },
                    });
                  }}
                  min={0}
                  allowDecimal={false}
                  allowEmpty={false}
                />
              </Stack>
              {/* 会话优化间隔设置 */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Stack direction="row" sx={{ alignItems: "center" }}>
                  <Typography>
                    {t(
                      ($) =>
                        $.page_settingsManagement.appTrackingSettings
                          .usageSessionOptimizationIntervalMinutes.label,
                    )}
                  </Typography>
                  <Tooltip
                    title={t(
                      ($) =>
                        $.page_settingsManagement.appTrackingSettings
                          .usageSessionOptimizationIntervalMinutes.tooltip,
                    )}
                  >
                    <IconButton size="small">
                      <HelpIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Stack>
                <LazyNumberField
                  size="small"
                  value={userSettingsDtoData.appTracking.usageSessionOptimizationIntervalMinutes}
                  onValueChange={async (value) => {
                    if (value === undefined) return;
                    await patchUserSettingsAsync({
                      appTracking: {
                        ...userSettingsDtoData.appTracking,
                        usageSessionOptimizationIntervalMinutes: value,
                      },
                    });
                  }}
                  min={1}
                  allowDecimal={false}
                  allowEmpty={false}
                />
              </Stack>
            </Stack>
          </Paper>
          {/* 网站追踪 */}
          <Paper
            variant="outlined"
            sx={{
              p: 2,
            }}
          >
            <Typography sx={{ fontWeight: "bold" }}>
              {t(($) => $.page_settingsManagement.websiteTrackingSettings.title)}
            </Typography>
            <Stack spacing={1} direction="column" sx={{ mt: 2 }}>
              {/* 图标目录设置 */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Stack direction="row" sx={{ alignItems: "center" }}>
                  <Typography>
                    {t(
                      ($) => $.page_settingsManagement.websiteTrackingSettings.iconDirectory.label,
                    )}
                  </Typography>
                  <Tooltip
                    title={t(
                      ($) =>
                        $.page_settingsManagement.websiteTrackingSettings.iconDirectory.tooltip,
                    )}
                  >
                    <IconButton size="small">
                      <HelpIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Stack>
                <LazyTextField
                  size="small"
                  value={userSettingsDtoData.websiteTracking.iconDirectory}
                  onValueChange={async (value) => {
                    await patchUserSettingsAsync({
                      websiteTracking: {
                        ...userSettingsDtoData.websiteTracking,
                        iconDirectory: value,
                      },
                    });
                  }}
                />
              </Stack>
              {/* 活跃网站会话自动保存设置 */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Stack direction="row" sx={{ alignItems: "center" }}>
                  <Typography>
                    {t(
                      ($) =>
                        $.page_settingsManagement.websiteTrackingSettings
                          .activeUsageSessionAutoSaveIntervalSeconds.label,
                    )}
                  </Typography>
                  <Tooltip
                    title={t(
                      ($) =>
                        $.page_settingsManagement.websiteTrackingSettings
                          .activeUsageSessionAutoSaveIntervalSeconds.tooltip,
                    )}
                  >
                    <IconButton size="small">
                      <HelpIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Stack>
                <LazyNumberField
                  size="small"
                  value={
                    userSettingsDtoData.websiteTracking.activeUsageSessionAutoSaveIntervalSeconds
                  }
                  onValueChange={async (value) => {
                    if (value === undefined) return;
                    await patchUserSettingsAsync({
                      websiteTracking: {
                        ...userSettingsDtoData.websiteTracking,
                        activeUsageSessionAutoSaveIntervalSeconds: value,
                      },
                    });
                  }}
                  min={1}
                  allowDecimal={false}
                  allowEmpty={false}
                />
              </Stack>
              {/* 最小有效会话时长设置 */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Stack direction="row" sx={{ alignItems: "center" }}>
                  <Typography>
                    {t(
                      ($) =>
                        $.page_settingsManagement.websiteTrackingSettings
                          .minValidUsageSessionDurationSeconds.label,
                    )}
                  </Typography>
                  <Tooltip
                    title={t(
                      ($) =>
                        $.page_settingsManagement.websiteTrackingSettings
                          .minValidUsageSessionDurationSeconds.tooltip,
                    )}
                  >
                    <IconButton size="small">
                      <HelpIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Stack>
                <LazyNumberField
                  size="small"
                  value={userSettingsDtoData.websiteTracking.minValidUsageSessionDurationSeconds}
                  onValueChange={async (value) => {
                    if (value === undefined) return;
                    await patchUserSettingsAsync({
                      websiteTracking: {
                        ...userSettingsDtoData.websiteTracking,
                        minValidUsageSessionDurationSeconds: value,
                      },
                    });
                  }}
                  min={0}
                  allowDecimal={false}
                  allowEmpty={false}
                />
              </Stack>
              {/* 会话合并容差时间设置 */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Stack direction="row" sx={{ alignItems: "center" }}>
                  <Typography>
                    {t(
                      ($) =>
                        $.page_settingsManagement.websiteTrackingSettings
                          .usageSessionMergeToleranceSeconds.label,
                    )}
                  </Typography>
                  <Tooltip
                    title={t(
                      ($) =>
                        $.page_settingsManagement.websiteTrackingSettings
                          .usageSessionMergeToleranceSeconds.tooltip,
                    )}
                  >
                    <IconButton size="small">
                      <HelpIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Stack>
                <LazyNumberField
                  size="small"
                  value={userSettingsDtoData.websiteTracking.usageSessionMergeToleranceSeconds}
                  onValueChange={async (value) => {
                    if (value === undefined) return;
                    await patchUserSettingsAsync({
                      websiteTracking: {
                        ...userSettingsDtoData.websiteTracking,
                        usageSessionMergeToleranceSeconds: value,
                      },
                    });
                  }}
                  min={0}
                  allowDecimal={false}
                  allowEmpty={false}
                />
              </Stack>
              {/* 会话优化间隔设置 */}
              <Stack
                direction="row"
                sx={{
                  alignItems: "center",
                  justifyContent: "space-between",
                }}
              >
                <Stack direction="row" sx={{ alignItems: "center" }}>
                  <Typography>
                    {t(
                      ($) =>
                        $.page_settingsManagement.websiteTrackingSettings
                          .usageSessionOptimizationIntervalMinutes.label,
                    )}
                  </Typography>
                  <Tooltip
                    title={t(
                      ($) =>
                        $.page_settingsManagement.websiteTrackingSettings
                          .usageSessionOptimizationIntervalMinutes.tooltip,
                    )}
                  >
                    <IconButton size="small">
                      <HelpIcon fontSize="inherit" />
                    </IconButton>
                  </Tooltip>
                </Stack>
                <LazyNumberField
                  size="small"
                  value={
                    userSettingsDtoData.websiteTracking.usageSessionOptimizationIntervalMinutes
                  }
                  onValueChange={async (value) => {
                    if (value === undefined) return;
                    await patchUserSettingsAsync({
                      websiteTracking: {
                        ...userSettingsDtoData.websiteTracking,
                        usageSessionOptimizationIntervalMinutes: value,
                      },
                    });
                  }}
                  min={1}
                  allowDecimal={false}
                  allowEmpty={false}
                />
              </Stack>
            </Stack>
          </Paper>
        </Stack>
      </Grid>
    </Grid>
  );
};
