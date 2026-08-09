import { WebsiteCategoryManagementPage } from "@/pages/website-category-management";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/website-categories")({
  component: RouteComponent,
});

// eslint-disable-next-line react-refresh/only-export-components
function RouteComponent() {
  return <WebsiteCategoryManagementPage />;
}
