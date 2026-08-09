import {
  WebsiteCategoryIcon,
  websiteCategoryQueries,
  useCreateWebsiteCategory,
  useDeleteWebsiteCategory,
  usePatchWebsiteCategory,
  type WebsiteCategory,
} from "@/entities/website-category";
import { useQuery } from "@tanstack/react-query";
import { LazyColorField } from "@/shared/ui/LazyColorField";
import {
  DataGrid,
  type GridColDef,
  type GridRenderCellParams,
  type GridRowsProp,
} from "@mui/x-data-grid";
import Box from "@mui/material/Box";
import { useSnackbar } from "notistack";
import IconButton from "@mui/material/IconButton";
import DeleteIcon from "@mui/icons-material/Delete";
import Button from "@mui/material/Button";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogContentText from "@mui/material/DialogContentText";
import DialogTitle from "@mui/material/DialogTitle";
import { useState } from "react";
import TextField from "@mui/material/TextField";
import { LazyTextField } from "@/shared/ui/LazyTextField";
import { useTranslation } from "react-i18next";

export const WebsiteCategoryManagementPage = () => {
  const { t } = useTranslation(["page_websiteCategoryManagement"]);
  const { enqueueSnackbar } = useSnackbar();
  const { data: websiteCategoriesData, isLoading } = useQuery(
    websiteCategoryQueries.websiteCategories({}),
  ) as {
    data?: WebsiteCategory[];
    isLoading: boolean;
  };
  const { mutateAsync: createWebsiteCategoryAsync } = useCreateWebsiteCategory();
  const { mutateAsync: patchWebsiteCategoryAsync } = usePatchWebsiteCategory();
  const { mutateAsync: deleteWebsiteCategoryAsync } = useDeleteWebsiteCategory();

  const [deleteWebsiteCategoryComfirmDialogOpen, setDeleteWebsiteCategoryComfirmDialogOpen] =
    useState(false);
  const [createWebsiteCategoryDialogOpen, setCreateWebsiteCategoryDialogOpen] = useState(false);
  const [websiteCategoryToDelete, setWebsiteCategoryToDelete] = useState<WebsiteCategory | null>(
    null,
  );

  const stopGridKeyboardEvent = (e: React.KeyboardEvent) => {
    e.stopPropagation();
  };

  const columns: GridColDef[] = [
    {
      field: "name",
      headerName: t(($) => $.page_websiteCategoryManagement.columns.name),
      width: 150,
      renderCell: (params: GridRenderCellParams<WebsiteCategory, string>) => (
        <Box sx={{ display: "flex", alignItems: "center", height: "100%" }}>
          <LazyTextField
            fullWidth
            size="small"
            value={params.value}
            onKeyDown={stopGridKeyboardEvent}
            onValueChange={async (value) => {
              try {
                await patchWebsiteCategoryAsync({
                  id: params.row.id,
                  body: { name: value },
                });
              } catch {
                enqueueSnackbar(
                  t(($) => $.page_websiteCategoryManagement.errors.updateFailed, {
                    field: t(($) => $.page_websiteCategoryManagement.fields.name),
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
      headerName: t(($) => $.page_websiteCategoryManagement.columns.color),
      width: 70,
      renderCell: (params: GridRenderCellParams<WebsiteCategory, string>) => (
        <Box sx={{ display: "flex", alignItems: "center", height: "100%" }}>
          <LazyColorField
            value={params.value}
            size="small"
            sx={{ width: "3rem" }}
            onValueChange={async (value) => {
              try {
                await patchWebsiteCategoryAsync({
                  id: params.row.id,
                  body: { color: value },
                });
              } catch {
                enqueueSnackbar(
                  t(($) => $.page_websiteCategoryManagement.errors.updateFailed, {
                    field: t(($) => $.page_websiteCategoryManagement.fields.color),
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
      headerName: t(($) => $.page_websiteCategoryManagement.columns.icon),
      width: 50,
      sortable: false,
      filterable: false,
      disableColumnMenu: true,
      renderCell: (params: GridRenderCellParams<WebsiteCategory>) => (
        <Box sx={{ display: "flex", alignItems: "center", height: "100%" }}>
          <WebsiteCategoryIcon
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
      headerName: t(($) => $.page_websiteCategoryManagement.columns.iconPath),
      width: 200,
      renderCell: (params: GridRenderCellParams<WebsiteCategory, string>) => (
        <Box sx={{ display: "flex", alignItems: "center", height: "100%" }}>
          <LazyTextField
            fullWidth
            size="small"
            value={params.value || ""}
            onKeyDown={stopGridKeyboardEvent}
            onValueChange={async (value) => {
              try {
                await patchWebsiteCategoryAsync({
                  id: params.row.id,
                  body: { iconPath: value === "" ? null : value },
                });
              } catch {
                enqueueSnackbar(
                  t(($) => $.page_websiteCategoryManagement.errors.updateFailed, {
                    field: t(($) => $.page_websiteCategoryManagement.fields.iconPath),
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
      field: "action",
      headerName: t(($) => $.page_websiteCategoryManagement.columns.action),
      width: 60,
      sortable: false,
      filterable: false,
      disableColumnMenu: true,
      renderCell: (params: GridRenderCellParams<WebsiteCategory>) => (
        <IconButton
          disabled={params.row.isSystem}
          color="error"
          onClick={() => {
            setWebsiteCategoryToDelete(params.row);
            setDeleteWebsiteCategoryComfirmDialogOpen(true);
          }}
        >
          <DeleteIcon />
        </IconButton>
      ),
    },
  ];

  const rows: GridRowsProp = websiteCategoriesData || [];

  return (
    <>
      <Box sx={{ display: "flex", justifyContent: "end" }}>
        <Button variant="contained" onClick={() => setCreateWebsiteCategoryDialogOpen(true)}>
          {t(($) => $.page_websiteCategoryManagement.buttons.createCategory)}
        </Button>
      </Box>
      <Box sx={{ width: "100%", mt: 1 }}>
        <DataGrid
          showToolbar
          autoHeight
          initialState={{
            pagination: { paginationModel: { pageSize: 10 } },
            columns: {
              columnVisibilityModel: {
                lastAutoUpdated: false,
                processName: false,
                executablePath: false,
              },
            },
          }}
          pageSizeOptions={[5, 10, 20, 40]}
          loading={isLoading}
          rows={rows}
          columns={columns}
        />
      </Box>

      <Dialog
        open={deleteWebsiteCategoryComfirmDialogOpen}
        onClose={() => setDeleteWebsiteCategoryComfirmDialogOpen(false)}
        role="alertdialog"
      >
        <DialogTitle>
          {t(($) => $.page_websiteCategoryManagement.dialogs.delete.title, {
            name: websiteCategoryToDelete?.name,
          })}
        </DialogTitle>
        <DialogContent>
          <DialogContentText>
            {t(($) => $.page_websiteCategoryManagement.dialogs.delete.description)}
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteWebsiteCategoryComfirmDialogOpen(false)} autoFocus>
            {t(($) => $.page_websiteCategoryManagement.actions.cancel)}
          </Button>
          <Button
            onClick={async () => {
              if (websiteCategoryToDelete == null) {
                enqueueSnackbar(
                  t(($) => $.page_websiteCategoryManagement.messages.error.noCategorySelected),
                  {
                    variant: "error",
                  },
                );
                return;
              }
              try {
                await deleteWebsiteCategoryAsync(websiteCategoryToDelete.id);
              } catch {
                enqueueSnackbar(
                  t(($) => $.page_websiteCategoryManagement.errors.deleteFailed, {
                    field: t(($) => $.page_websiteCategoryManagement.fields.category),
                  }),
                  { variant: "error" },
                );
              } finally {
                setDeleteWebsiteCategoryComfirmDialogOpen(false);
                setWebsiteCategoryToDelete(null);
              }
            }}
            color="error"
          >
            {t(($) => $.page_websiteCategoryManagement.actions.confirm)}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={createWebsiteCategoryDialogOpen}
        onClose={() => setCreateWebsiteCategoryDialogOpen(false)}
      >
        <DialogTitle>{t(($) => $.page_websiteCategoryManagement.dialogs.create.title)}</DialogTitle>
        <DialogContent>
          <form
            onSubmit={async (event: React.SyntheticEvent<HTMLFormElement>) => {
              event.preventDefault();
              const formData = new FormData(event.currentTarget);
              const data = Object.fromEntries(formData.entries()) as {
                name: string;
                color: string;
                iconPath: string | null;
              };
              if (data.iconPath === "") data.iconPath = null;
              try {
                await createWebsiteCategoryAsync(data);
                setCreateWebsiteCategoryDialogOpen(false);
              } catch {
                enqueueSnackbar(
                  t(($) => $.page_websiteCategoryManagement.errors.createFailed, {
                    field: t(($) => $.page_websiteCategoryManagement.fields.category),
                  }),
                  { variant: "error" },
                );
              }
            }}
            id="create-form"
          >
            <TextField
              autoFocus
              required
              margin="dense"
              variant="outlined"
              id="name"
              name="name"
              label={t(($) => $.page_websiteCategoryManagement.fields.name)}
              fullWidth
            />
            <TextField
              label={t(($) => $.page_websiteCategoryManagement.fields.color)}
              margin="dense"
              fullWidth
              slotProps={{
                htmlInput: {
                  type: "color",
                  name: "color",
                  defaultValue: "#1976d2",
                },
                inputLabel: {
                  shrink: true,
                },
              }}
            />
            <TextField
              margin="dense"
              variant="outlined"
              id="iconPath"
              name="iconPath"
              label={t(($) => $.page_websiteCategoryManagement.fields.iconPath)}
              fullWidth
            />
          </form>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCreateWebsiteCategoryDialogOpen(false)}>
            {t(($) => $.page_websiteCategoryManagement.actions.cancel)}
          </Button>
          <Button type="submit" form="create-form">
            {t(($) => $.page_websiteCategoryManagement.actions.create)}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};
