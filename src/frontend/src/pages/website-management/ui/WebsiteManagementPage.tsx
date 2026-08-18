import {
  WebsiteIcon,
  websiteQueries,
  useDeleteWebsite,
  usePatchWebsite,
  type Website,
} from "@/entities/website";
import DeleteIcon from "@mui/icons-material/Delete";
import { LazyColorField } from "@/shared/ui/LazyColorField";
import { useQuery } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import Box from "@mui/material/Box";
import { useSnackbar } from "notistack";
import Switch from "@mui/material/Switch";
import IconButton from "@mui/material/IconButton";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogContentText from "@mui/material/DialogContentText";
import DialogActions from "@mui/material/DialogActions";
import Button from "@mui/material/Button";
import {
  DataGrid,
  type GridColDef,
  type GridRenderCellParams,
  type GridRowsProp,
} from "@mui/x-data-grid";
import {
  websiteCategoryQueries,
  WebsiteCategorySelecter,
  type WebsiteCategory,
} from "@/entities/website-category";
import { LazyTextField } from "@/shared/ui/LazyTextField";
import dayjs from "@/shared/lib/dayjs";
import { useTranslation } from "react-i18next";

export const WebsiteManagementPage = () => {
  const { t } = useTranslation(["page_websiteManagement"]);
  const { enqueueSnackbar } = useSnackbar();
  const { data: websitesData, isLoading: isWebsitesDataLoading } = useQuery(
    websiteQueries.websites({}),
  ) as {
    isLoading: boolean;
    data?: Website[];
  };
  const { mutateAsync: patchWebsiteAsync } = usePatchWebsite();
  const { mutateAsync: deleteWebsiteAsync } = useDeleteWebsite();
  const { data: websiteCategoriesData, isLoading: isWebsiteCategoriesDataLoading } = useQuery(
    websiteCategoryQueries.websiteCategories({ fields: "id,name" }),
  ) as {
    isLoading: boolean;
    data?: Pick<WebsiteCategory, "id" | "name">[];
  };
  const categoryMap = useMemo(() => {
    return new Map(websiteCategoriesData?.map((item) => [item.id, item.name]) ?? []);
  }, [websiteCategoriesData]);

  const [deleteWebsiteComfirmDialogOpen, setDeleteWebsiteComfirmDialogOpen] = useState(false);
  const [websiteToDelete, setWebsiteToDelete] = useState<Website | null>(null);

  const stopGridKeyboardEvent = (e: React.KeyboardEvent) => {
    e.stopPropagation();
  };

  const columns: GridColDef[] = [
    {
      field: "name",
      headerName: t(($) => $.page_websiteManagement.columns.name),
      width: 150,
      renderCell: (params: GridRenderCellParams<Website, string>) => (
        <Box sx={{ display: "flex", alignItems: "center", height: "100%" }}>
          <LazyTextField
            fullWidth
            size="small"
            value={params.value}
            onKeyDown={stopGridKeyboardEvent}
            onValueChange={async (value) => {
              try {
                await patchWebsiteAsync({
                  id: params.row.id,
                  body: { name: value },
                });
              } catch {
                enqueueSnackbar(
                  t(($) => $.page_websiteManagement.errors.updateFailed, {
                    field: t(($) => $.page_websiteManagement.columns.name),
                  }),
                  { variant: "error" },
                );
              }
            }}
          />
        </Box>
      ),
    },
    {
      field: "color",
      headerName: t(($) => $.page_websiteManagement.columns.color),
      width: 70,
      renderCell: (params: GridRenderCellParams<Website, string>) => (
        <Box sx={{ display: "flex", alignItems: "center", height: "100%" }}>
          <LazyColorField
            size="small"
            sx={{ width: "3rem" }}
            value={params.value}
            onValueChange={async (value) => {
              try {
                await patchWebsiteAsync({
                  id: params.row.id,
                  body: { color: value },
                });
              } catch {
                enqueueSnackbar(
                  t(($) => $.page_websiteManagement.errors.updateFailed, {
                    field: t(($) => $.page_websiteManagement.columns.color),
                  }),
                  { variant: "error" },
                );
              }
            }}
          />
        </Box>
      ),
    },
    {
      field: "category",
      headerName: t(($) => $.page_websiteManagement.columns.category),
      width: 210,
      valueGetter: (_, row) => {
        return categoryMap.get(row.ctegoryId) ?? "";
      },
      renderCell: (params: GridRenderCellParams<Website>) => (
        <Box sx={{ display: "flex", alignItems: "center", height: "100%" }}>
          <WebsiteCategorySelecter
            value={params.row.websiteCategoryId}
            onValueChange={async (value) => {
              try {
                await patchWebsiteAsync({
                  id: params.row.id,
                  body: { websiteCategoryId: value },
                });
              } catch {
                enqueueSnackbar(
                  t(($) => $.page_websiteManagement.errors.updateFailed, {
                    field: t(($) => $.page_websiteManagement.columns.category),
                  }),
                  { variant: "error" },
                );
              }
            }}
          />
        </Box>
      ),
    },
    {
      field: "icon",
      headerName: t(($) => $.page_websiteManagement.columns.icon),
      width: 50,
      sortable: false,
      filterable: false,
      disableColumnMenu: true,
      renderCell: (params: GridRenderCellParams<Website>) => (
        <Box sx={{ display: "flex", alignItems: "center", height: "100%" }}>
          <WebsiteIcon
            id={params.row.id}
            iconPath={params.row.iconPath}
            iconPathLastUpdatedAt={params.row.iconPathLastUpdatedAt}
            sx={{
              width: "2rem",
              height: "2rem",
            }}
          />
        </Box>
      ),
    },
    {
      field: "iconPath",
      headerName: t(($) => $.page_websiteManagement.columns.iconPath),
      width: 150,
      renderCell: (params: GridRenderCellParams<Website, string>) => (
        <Box sx={{ display: "flex", alignItems: "center", height: "100%" }}>
          <LazyTextField
            fullWidth
            size="small"
            value={params.value || ""}
            onKeyDown={stopGridKeyboardEvent}
            onValueChange={async (value) => {
              try {
                await patchWebsiteAsync({
                  id: params.row.id,
                  body: { iconPath: value === "" ? null : value },
                });
              } catch {
                enqueueSnackbar(
                  t(($) => $.page_websiteManagement.errors.updateFailed, {
                    field: t(($) => $.page_websiteManagement.columns.iconPath),
                  }),
                  { variant: "error" },
                );
              }
            }}
          />
        </Box>
      ),
    },
    {
      field: "allowMetadataAutoRefresh",
      headerName: t(($) => $.page_websiteManagement.columns.allowMetadataAutoRefresh),
      width: 80,
      renderCell: (params: GridRenderCellParams<Website, boolean>) => (
        <Switch
          checked={params.value}
          onChange={async (event: React.ChangeEvent<HTMLInputElement>) => {
            try {
              await patchWebsiteAsync({
                id: params.row.id,
                body: { allowMetadataAutoRefresh: event.target.checked },
              });
            } catch {
              enqueueSnackbar(
                t(($) => $.page_websiteManagement.errors.updateFailed, {
                  field: t(($) => $.page_websiteManagement.columns.allowMetadataAutoRefresh),
                }),
                { variant: "error" },
              );
            }
          }}
        />
      ),
    },
    {
      field: "metadataLastRefreshedAt",
      headerName: t(($) => $.page_websiteManagement.columns.metadataLastRefreshedAt),
      width: 140,
      valueFormatter: (value) => dayjs(value).format("L LT"),
    },
    {
      field: "host",
      headerName: t(($) => $.page_websiteManagement.columns.host),
      width: 150,
    },
    {
      field: "action",
      headerName: t(($) => $.page_websiteManagement.columns.action),
      width: 60,
      sortable: false,
      filterable: false,
      disableColumnMenu: true,
      renderCell: (params: GridRenderCellParams<Website>) => (
        <IconButton
          disabled={params.row.isSystem}
          color="error"
          onClick={() => {
            setWebsiteToDelete(params.row);
            setDeleteWebsiteComfirmDialogOpen(true);
          }}
        >
          <DeleteIcon />
        </IconButton>
      ),
    },
  ];

  const rows: GridRowsProp = websitesData || [];

  return (
    <>
      <Box sx={{ width: "100%" }}>
        <DataGrid
          showToolbar
          autoHeight
          initialState={{
            pagination: { paginationModel: { pageSize: 10 } },
            columns: {
              columnVisibilityModel: {},
            },
          }}
          pageSizeOptions={[5, 10, 20, 40]}
          loading={isWebsitesDataLoading || isWebsiteCategoriesDataLoading}
          rows={rows}
          columns={columns}
        />
      </Box>

      <Dialog
        open={deleteWebsiteComfirmDialogOpen}
        onClose={() => setDeleteWebsiteComfirmDialogOpen(false)}
        role="alertdialog"
      >
        <DialogTitle>
          {t(($) => $.page_websiteManagement.deleteConfirm.title, { name: websiteToDelete?.name })}
        </DialogTitle>
        <DialogContent>
          <DialogContentText id="alert-dialog-description">
            {t(($) => $.page_websiteManagement.deleteConfirm.description)}
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteWebsiteComfirmDialogOpen(false)} autoFocus>
            {t(($) => $.page_websiteManagement.actions.cancel)}
          </Button>
          <Button
            onClick={async () => {
              if (websiteToDelete == null) {
                enqueueSnackbar(
                  t(($) => $.page_websiteManagement.errors.noWebsiteSelected),
                  {
                    variant: "error",
                  },
                );
                return;
              }
              try {
                await deleteWebsiteAsync(websiteToDelete.id);
              } catch {
                enqueueSnackbar(
                  t(($) => $.page_websiteManagement.errors.deleteFailed, {
                    field: t(($) => $.page_websiteManagement.entityName),
                  }),
                  { variant: "error" },
                );
              } finally {
                setDeleteWebsiteComfirmDialogOpen(false);
                setWebsiteToDelete(null);
              }
            }}
            color="error"
          >
            {t(($) => $.page_websiteManagement.actions.confirm)}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};
