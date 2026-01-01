import * as React from "react";
import { createFileRoute, useNavigate, Link } from "@tanstack/react-router";
import { Plus } from "lucide-react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import {
  PurchaseRequestTable,
  StatusFilter,
  usePurchaseRequests,
  type PurchaseRequestStatusValue,
} from "@/features/purchase-requests";
import { useDebouncedValue } from "@/hooks/use-debounced-value";
import { useAuth } from "@/features/auth/hooks";

export const Route = createFileRoute("/(auth)/_app-layout/requests/")({
  component: MyRequestsPage,
});

function MyRequestsPage() {
  const { hasRole } = useAuth();
  const navigate = useNavigate();
  const [searchQuery, setSearchQuery] = React.useState("");
  const debouncedSearch = useDebouncedValue(searchQuery);
  const [selectedStatus, setSelectedStatus] =
    React.useState<PurchaseRequestStatusValue | null>(null);
  const [page, setPage] = React.useState(1);
  const pageSize = 20;

  const { data, isPending, isError, error } = usePurchaseRequests(
    selectedStatus ?? undefined,
    debouncedSearch,
    page,
    pageSize,
  );

  const handleStatusChange = (status: PurchaseRequestStatusValue | null) => {
    setSelectedStatus(status);
    setPage(1);
  };

  const handleViewRequest = (request: { id: string }) => {
    navigate({ to: "/requests/$id/edit", params: { id: request.id } });
  };

  const totalCount =
    typeof data?.pagination?.totalCount === "string"
      ? parseInt(data.pagination.totalCount, 10)
      : (data?.pagination?.totalCount ?? 0);

  const currentPageSize =
    typeof data?.pagination?.pageSize === "string"
      ? parseInt(data.pagination.pageSize, 10)
      : (data?.pagination?.pageSize ?? pageSize);

  const totalPages = Math.ceil(totalCount / currentPageSize);

  const pageTitle = hasRole("Admin")
    ? "All Requests"
    : hasRole("Approver")
      ? "Department Requests"
      : "My Requests";

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{pageTitle}</h1>
          <p className="text-muted-foreground text-sm">
            View and manage your purchase requests
          </p>
        </div>
        <Button asChild>
          <Link to="/requests/new">
            <Plus className="mr-2 h-4 w-4" />
            New Request
          </Link>
        </Button>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex gap-2">
          <Input
            placeholder="Search by title or ID..."
            value={searchQuery}
            onChange={(e) => {
              setSearchQuery(e.target.value);
              setPage(1);
            }}
            className="w-64 bg-white"
          />
          {searchQuery && (
            <Button
              variant="outline"
              onClick={() => {
                setSearchQuery("");
                setPage(1);
              }}
            >
              Clear
            </Button>
          )}
        </div>
      </div>

      <StatusFilter
        selectedStatus={selectedStatus}
        onStatusChange={handleStatusChange}
      />

      <Card>
        <CardHeader>
          <CardTitle>Purchase Requests</CardTitle>
          <CardDescription>
            {isPending
              ? "Loading..."
              : `Showing ${data?.data?.length ?? 0} of ${totalCount} requests`}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {isPending ? (
            <div className="flex items-center justify-center py-8 text-muted-foreground">
              Loading requests...
            </div>
          ) : isError ? (
            <div className="flex items-center justify-center py-8 text-red-500">
              Error loading requests: {error?.detail ?? "Unknown error"}
            </div>
          ) : (
            <>
              <PurchaseRequestTable
                requests={data?.data ?? []}
                onViewRequest={handleViewRequest}
              />
              {totalPages > 1 && (
                <div className="mt-4 flex items-center justify-between">
                  <div className="text-sm text-muted-foreground">
                    Page {page} of {totalPages}
                  </div>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => setPage((p) => Math.max(1, p - 1))}
                      disabled={page <= 1}
                    >
                      Previous
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() =>
                        setPage((p) => Math.min(totalPages, p + 1))
                      }
                      disabled={page >= totalPages}
                    >
                      Next
                    </Button>
                  </div>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
