import { api } from "@/lib/api/client";

export function useCategories() {
  return api.useQuery("get", "/categories");
}
