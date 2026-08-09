import i18n from "@/shared/i18n";
import enUS from "./i18n/en-US.json";
import zhCN from "./i18n/zh-CN.json";

i18n.addResourceBundle("en-US", "entity_website", enUS, true, true);
i18n.addResourceBundle("zh-CN", "entity_website", zhCN, true, true);

export * from "./model/schemas";

export * from "./api/schemas";
export * from "./api/requests";
export * from "./api/queries";

export * from "./ui/WebsiteIcon";
export * from "./ui/WebsitePicker";
