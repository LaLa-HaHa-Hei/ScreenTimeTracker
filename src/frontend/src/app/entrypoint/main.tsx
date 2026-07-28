import { StrictMode, useEffect, type FC, type PropsWithChildren } from "react";
import { createRoot } from "react-dom/client";
import CssBaseline from "@mui/material/CssBaseline";
import "@fontsource/roboto/300.css";
import "@fontsource/roboto/400.css";
import "@fontsource/roboto/500.css";
import "@fontsource/roboto/700.css";
import dayjs from "@/shared/lib/dayjs";
import { SnackbarProvider } from "notistack";
import i18n, { type LanguageCode } from "@/shared/i18n";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { QueryClient, QueryClientProvider, useQuery } from "@tanstack/react-query";
import { createTheme, ThemeProvider } from "@mui/material/styles";
import { createRouter, RouterProvider } from "@tanstack/react-router";
import { routeTree } from "../routeTree.gen";
import { I18nextProvider } from "react-i18next";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { localSettingsQueries } from "@/entities/local-settings";
import Box from "@mui/material/Box";
import CircularProgress from "@mui/material/CircularProgress";
import Typography from "@mui/material/Typography";
import "dayjs/locale/zh-cn";
import "dayjs/locale/en";
import enUS from "../i18n/en-US.json";
import zhCN from "../i18n/zh-CN.json";
import { useTranslation } from "react-i18next";

i18n.addResourceBundle("en-US", "app", enUS, true, true);
i18n.addResourceBundle("zh-CN", "app", zhCN, true, true);

const dayjsLocaleMap: Record<LanguageCode, string> = {
  "zh-CN": "zh-cn",
  "en-US": "en",
};

const AppLocalizationProvider: FC<PropsWithChildren> = ({ children }) => {
  const { t } = useTranslation(["app"]);
  const { data: appSettingsDtoData, isLoading: isAppSettingsDtoDataLoading } = useQuery(
    localSettingsQueries.localSettings(),
  );

  useEffect(() => {
    if (appSettingsDtoData?.language) {
      i18n.changeLanguage(appSettingsDtoData.language);
      dayjs.locale(dayjsLocaleMap[appSettingsDtoData.language]);
    }
  }, [appSettingsDtoData]);

  if (isAppSettingsDtoDataLoading) {
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
  }

  if (!appSettingsDtoData)
    return (
      <Box
        sx={{
          display: "flex",
          justifyContent: "center",
        }}
      >
        <Typography>{t(($) => $.app.errors.fetchFailed)}</Typography>
      </Box>
    );

  return (
    <I18nextProvider i18n={i18n}>
      <SnackbarProvider>
        <LocalizationProvider
          dateAdapter={AdapterDayjs}
          adapterLocale={dayjsLocaleMap[appSettingsDtoData.language]}
        >
          {children}
        </LocalizationProvider>
      </SnackbarProvider>
    </I18nextProvider>
  );
};

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

const theme = createTheme({
  colorSchemes: {
    dark: true,
  },
});

const router = createRouter({
  routeTree,
  context: {
    queryClient,
  },
});

declare module "@tanstack/react-router" {
  interface Register {
    router: typeof router;
  }
}

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <ReactQueryDevtools />
      <SnackbarProvider>
        <ThemeProvider theme={theme}>
          <CssBaseline />
          <AppLocalizationProvider>
            <RouterProvider router={router} />
          </AppLocalizationProvider>
        </ThemeProvider>
      </SnackbarProvider>
    </QueryClientProvider>
  </StrictMode>,
);
