import { localSettingsQueries, usePatchLocalettings } from "@/entities/local-settings";
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

export const SettingsManagementPage = () => {
  const { t } = useTranslation(["page_settingsManagement"]);
  const { data: userSettingsDtoData, isLoading: isuserSettingsDataLoading } = useQuery(
    userSettingsQueries.userSettings(),
  );
  const { data: localSettingsDtoData, isLoading: isLocalSettingsDataLoading } = useQuery(
    localSettingsQueries.localSettings(),
  );
  const { mutateAsync: patchUserSettingsAsync } = usePatchUserSettings();
  const { mutateAsync: patchLocalSettingsAsync } = usePatchLocalettings();
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
    <Stack spacing={2} direction="column">
      <Paper
        variant="outlined"
        sx={{
          p: 2,
        }}
      >
        <Stack spacing={1} direction="column">
          <Typography sx={{ fontWeight: "bold" }}>{t(($) => $.page_settingsManagement.localSettings.title)}</Typography>
          {/* 语言设置 */}
          <Stack
            direction="row"
            sx={{
              alignItems: "center",
              justifyContent: "space-between",
            }}
          >
            <Typography>{t(($) => $.page_settingsManagement.localSettings.language.label)}</Typography>
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
              <Typography>{t(($) => $.page_settingsManagement.localSettings.defaultUIOpenMode.label)}</Typography>
              <Tooltip title={t(($) => $.page_settingsManagement.localSettings.defaultUIOpenMode.tooltip)}>
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
                {t(($) => $.page_settingsManagement.localSettings.defaultUIOpenMode.options.window)}
              </MenuItem>
              <MenuItem value="Browser">
                {t(($) => $.page_settingsManagement.localSettings.defaultUIOpenMode.options.browser)}
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
              <Typography>{t(($) => $.page_settingsManagement.localSettings.isAutoStartEnabled.label)}</Typography>
              <Tooltip title={t(($) => $.page_settingsManagement.localSettings.isAutoStartEnabled.tooltip)}>
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
            <DialogTitle>{t(($) => $.page_settingsManagement.localSettings.autoStartAlert.title)}</DialogTitle>
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
              <Typography>{t(($) => $.page_settingsManagement.localSettings.isSilentStartEnabled.label)}</Typography>
              <Tooltip title={t(($) => $.page_settingsManagement.localSettings.isSilentStartEnabled.tooltip)}>
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
      <Paper
        variant="outlined"
        sx={{
          p: 2,
        }}
      >
        <Stack spacing={1} direction="column">
          <Typography sx={{ fontWeight: "bold" }}>{t(($) => $.page_settingsManagement.screenTimeSettings.title)}</Typography>
          {/* 应用图标目录设置 */}
          <Stack
            direction="row"
            sx={{
              alignItems: "center",
              justifyContent: "space-between",
            }}
          >
            <Stack direction="row" sx={{ alignItems: "center" }}>
              <Typography>{t(($) => $.page_settingsManagement.screenTimeSettings.appIconDirectory.label)}</Typography>
              <Tooltip title={t(($) => $.page_settingsManagement.screenTimeSettings.appIconDirectory.tooltip)}>
                <IconButton size="small">
                  <HelpIcon fontSize="inherit" />
                </IconButton>
              </Tooltip>
            </Stack>
            <LazyTextField
              size="small"
              value={userSettingsDtoData.appIconDirectory}
              onValueChange={async (value) => {
                await patchUserSettingsAsync({
                  appIconDirectory: value,
                });
              }}
            />
          </Stack>
          {/* 应用信息过期阈值设置 */}
          <Stack
            direction="row"
            sx={{
              alignItems: "center",
              justifyContent: "space-between",
            }}
          >
            <Stack direction="row" sx={{ alignItems: "center" }}>
              <Typography>
                {t(($) => $.page_settingsManagement.screenTimeSettings.appMetadataStaleThresholdMinutes.label)}
              </Typography>
              <Tooltip title={t(($) => $.page_settingsManagement.screenTimeSettings.appMetadataStaleThresholdMinutes.tooltip)}>
                <IconButton size="small">
                  <HelpIcon fontSize="inherit" />
                </IconButton>
              </Tooltip>
            </Stack>
            <LazyNumberField
              size="small"
              value={userSettingsDtoData.appMetadataStaleThresholdMinutes}
              onValueChange={async (value) => {
                await patchUserSettingsAsync({
                  appMetadataStaleThresholdMinutes: value,
                });
              }}
              min={0}
              allowDecimal={false}
              allowEmpty={false}
            />
          </Stack>
          {/* 活动会话自动保存设置 */}
          <Stack
            direction="row"
            sx={{
              alignItems: "center",
              justifyContent: "space-between",
            }}
          >
            <Stack direction="row" sx={{ alignItems: "center" }}>
              <Typography>
                {t(($) => $.page_settingsManagement.screenTimeSettings.activeAppUsageSessionAutoSaveIntervalSeconds.label)}
              </Typography>
              <Tooltip
                title={t(($) => $.page_settingsManagement.screenTimeSettings.activeAppUsageSessionAutoSaveIntervalSeconds.tooltip)}
              >
                <IconButton size="small">
                  <HelpIcon fontSize="inherit" />
                </IconButton>
              </Tooltip>
            </Stack>
            <LazyNumberField
              size="small"
              value={userSettingsDtoData.activeAppUsageSessionAutoSaveIntervalSeconds}
              onValueChange={async (value) => {
                await patchUserSettingsAsync({
                  activeAppUsageSessionAutoSaveIntervalSeconds: value,
                });
              }}
              min={1}
              allowDecimal={false}
              allowEmpty={false}
            />
          </Stack>
          {/* 空闲检测设置 */}
          <Stack
            direction="row"
            sx={{
              alignItems: "center",
              justifyContent: "space-between",
            }}
          >
            <Stack direction="row" sx={{ alignItems: "center" }}>
              <Typography>{t(($) => $.page_settingsManagement.screenTimeSettings.isIdleDetectionEnabled.label)}</Typography>
              <Tooltip title={t(($) => $.page_settingsManagement.screenTimeSettings.isIdleDetectionEnabled.tooltip)}>
                <IconButton size="small">
                  <HelpIcon fontSize="inherit" />
                </IconButton>
              </Tooltip>
            </Stack>
            <Switch
              checked={userSettingsDtoData.isIdleDetectionEnabled}
              onChange={async (event: React.ChangeEvent<HTMLInputElement>) => {
                await patchUserSettingsAsync({
                  isIdleDetectionEnabled: event.target.checked,
                });
              }}
            />
          </Stack>
          {/* 空闲阈值设置 */}
          <Stack
            direction="row"
            sx={{
              alignItems: "center",
              justifyContent: "space-between",
            }}
          >
            <Stack direction="row" sx={{ alignItems: "center" }}>
              <Typography>{t(($) => $.page_settingsManagement.screenTimeSettings.idleThresholdSeconds.label)}</Typography>
              <Tooltip title={t(($) => $.page_settingsManagement.screenTimeSettings.idleThresholdSeconds.tooltip)}>
                <IconButton size="small">
                  <HelpIcon fontSize="inherit" />
                </IconButton>
              </Tooltip>
            </Stack>
            <LazyNumberField
              size="small"
              value={userSettingsDtoData.idleThresholdSeconds}
              onValueChange={async (value) => {
                await patchUserSettingsAsync({
                  idleThresholdSeconds: value,
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
                {t(($) => $.page_settingsManagement.screenTimeSettings.idleDetectionPollingIntervalSeconds.label)}
              </Typography>
              <Tooltip title={t(($) => $.page_settingsManagement.screenTimeSettings.idleDetectionPollingIntervalSeconds.tooltip)}>
                <IconButton size="small">
                  <HelpIcon fontSize="inherit" />
                </IconButton>
              </Tooltip>
            </Stack>
            <LazyNumberField
              size="small"
              value={userSettingsDtoData.idleDetectionPollingIntervalSeconds}
              onValueChange={async (value) => {
                await patchUserSettingsAsync({
                  idleDetectionPollingIntervalSeconds: value,
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
                {t(($) => $.page_settingsManagement.screenTimeSettings.minValidAppUsageSessionDurationSeconds.label)}
              </Typography>
              <Tooltip
                title={t(($) => $.page_settingsManagement.screenTimeSettings.minValidAppUsageSessionDurationSeconds.tooltip)}
              >
                <IconButton size="small">
                  <HelpIcon fontSize="inherit" />
                </IconButton>
              </Tooltip>
            </Stack>
            <LazyNumberField
              size="small"
              value={userSettingsDtoData.minValidAppUsageSessionDurationSeconds}
              onValueChange={async (value) => {
                await patchUserSettingsAsync({
                  minValidAppUsageSessionDurationSeconds: value,
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
                {t(($) => $.page_settingsManagement.screenTimeSettings.appUsageSessionMergeToleranceSeconds.label)}
              </Typography>
              <Tooltip title={t(($) => $.page_settingsManagement.screenTimeSettings.appUsageSessionMergeToleranceSeconds.tooltip)}>
                <IconButton size="small">
                  <HelpIcon fontSize="inherit" />
                </IconButton>
              </Tooltip>
            </Stack>
            <LazyNumberField
              size="small"
              value={userSettingsDtoData.appUsageSessionMergeToleranceSeconds}
              onValueChange={async (value) => {
                await patchUserSettingsAsync({
                  appUsageSessionMergeToleranceSeconds: value,
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
                {t(($) => $.page_settingsManagement.screenTimeSettings.appUsageSessionOptimizationIntervalMinutes.label)}
              </Typography>
              <Tooltip
                title={t(($) => $.page_settingsManagement.screenTimeSettings.appUsageSessionOptimizationIntervalMinutes.tooltip)}
              >
                <IconButton size="small">
                  <HelpIcon fontSize="inherit" />
                </IconButton>
              </Tooltip>
            </Stack>
            <LazyNumberField
              size="small"
              value={userSettingsDtoData.appUsageSessionOptimizationIntervalMinutes}
              onValueChange={async (value) => {
                await patchUserSettingsAsync({
                  appUsageSessionOptimizationIntervalMinutes: value,
                });
              }}
              min={1}
              allowDecimal={false}
              allowEmpty={false}
            />
          </Stack>
          {/* 日期切换小时设置 */}
          <Stack
            direction="row"
            sx={{
              alignItems: "center",
              justifyContent: "space-between",
            }}
          >
            <Stack direction="row" sx={{ alignItems: "center" }}>
              <Typography>{t(($) => $.page_settingsManagement.screenTimeSettings.dayCutoffHour.label)}</Typography>
              <Tooltip title={t(($) => $.page_settingsManagement.screenTimeSettings.dayCutoffHour.tooltip)}>
                <IconButton size="small">
                  <HelpIcon fontSize="inherit" />
                </IconButton>
              </Tooltip>
            </Stack>
            <LazyNumberField
              size="small"
              value={userSettingsDtoData.dayCutoffHour}
              onValueChange={async (value) => {
                await patchUserSettingsAsync({
                  dayCutoffHour: value,
                });
              }}
              min={0}
              allowDecimal={false}
              allowEmpty={false}
            />
          </Stack>
        </Stack>
      </Paper>
    </Stack>
  );
};
