import * as React from "react";
import { createFileRoute, useNavigate, Link } from "@tanstack/react-router";
import { toast } from "sonner";
import { ArrowLeft, Trash2, AlertCircle } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Alert, AlertDescription } from "@/components/ui/alert";

import {
  PurchaseRequestForm,
  usePurchaseRequest,
  useUpdatePurchaseRequest,
  useSubmitPurchaseRequest,
  useDeletePurchaseRequest,
  PurchaseRequestStatus,
  type CreatePurchaseRequestFormData,
  type UpdatePurchaseRequestFormData,
} from "@/features/purchase-requests";
import { useCategories } from "@/features/categories";
import { useDepartments } from "@/features/departments";

export const Route = createFileRoute("/(auth)/_app-layout/requests/$id/edit")({
  component: EditRequestPage,
});

function EditRequestPage() {
  const { id } = Route.useParams();
  const navigate = useNavigate();

  const {
    data: requestData,
    isPending: isRequestLoading,
    error: requestError,
  } = usePurchaseRequest(id);
  const { data: categoriesData, isPending: isCategoriesLoading } =
    useCategories();
  const { data: departmentsData, isPending: isDepartmentsLoading } =
    useDepartments();

  const updateMutation = useUpdatePurchaseRequest();
  const submitMutation = useSubmitPurchaseRequest();
  const deleteMutation = useDeletePurchaseRequest();

  const purchaseRequest = requestData?.data;
  const categories = categoriesData?.data ?? [];
  const departments = departmentsData?.data ?? [];

  const isDraft = purchaseRequest?.status === PurchaseRequestStatus.Draft;
  const isPending = purchaseRequest?.status === PurchaseRequestStatus.Pending;

  const handleSaveAsDraft = async (
    data: CreatePurchaseRequestFormData | UpdatePurchaseRequestFormData,
  ) => {
    try {
      await updateMutation.mutateAsync({
        params: { path: { id } },
        body: {
          id,
          title: data.title,
          description: data.description || null,
          estimatedAmount: data.estimatedAmount,
          businessJustification: data.businessJustification || null,
          categoryId: data.categoryId,
          departmentId: data.departmentId,
        },
      });
      toast.success("Purchase request updated");
      navigate({ to: "/requests" });
    } catch {
      // Error handled via form
    }
  };

  const handleSubmitForApproval = async (
    data: CreatePurchaseRequestFormData | UpdatePurchaseRequestFormData,
  ) => {
    try {
      // First update the request
      await updateMutation.mutateAsync({
        params: { path: { id } },
        body: {
          id,
          title: data.title,
          description: data.description || null,
          estimatedAmount: data.estimatedAmount,
          businessJustification: data.businessJustification || null,
          categoryId: data.categoryId,
          departmentId: data.departmentId,
        },
      });

      // Then submit it for approval
      await submitMutation.mutateAsync({
        params: { path: { id } },
      });
      toast.success("Purchase request submitted for approval");
      navigate({ to: "/requests" });
    } catch {
      // Error handled via form
    }
  };

  const handleDelete = async () => {
    try {
      await deleteMutation.mutateAsync({
        params: { path: { id } },
      });
      toast.success("Purchase request deleted");
      navigate({ to: "/requests" });
    } catch {
      toast.error("Failed to delete purchase request");
    }
  };

  if (isRequestLoading) {
    return (
      <div className="flex items-center justify-center py-12 text-muted-foreground">
        Loading request...
      </div>
    );
  }

  if (requestError || !purchaseRequest) {
    return (
      <div className="space-y-4">
        <Button variant="ghost" size="sm" asChild>
          <Link to="/requests">
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Requests
          </Link>
        </Button>
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            Purchase request not found or you don&apos;t have permission to view
            it.
          </AlertDescription>
        </Alert>
      </div>
    );
  }

  // Show read-only view for non-draft requests
  if (!isDraft) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" asChild>
            <Link to="/requests">
              <ArrowLeft className="mr-2 h-4 w-4" />
              Back to Requests
            </Link>
          </Button>
        </div>

        <div>
          <h1 className="text-2xl font-bold tracking-tight">
            {purchaseRequest.title}
          </h1>
          <p className="text-muted-foreground text-sm">
            Request #{purchaseRequest.requestNumber}
          </p>
        </div>

        <div className="grid gap-6 lg:grid-cols-3">
          <div className="lg:col-span-2">
            <Card>
              <CardHeader>
                <CardTitle>Request Details</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <h4 className="text-sm font-medium text-muted-foreground">
                    Description
                  </h4>
                  <p className="mt-1">
                    {purchaseRequest.description || "No description provided"}
                  </p>
                </div>
                <div className="grid gap-4 sm:grid-cols-2">
                  <div>
                    <h4 className="text-sm font-medium text-muted-foreground">
                      Category
                    </h4>
                    <p className="mt-1">{purchaseRequest.category.name}</p>
                  </div>
                  <div>
                    <h4 className="text-sm font-medium text-muted-foreground">
                      Department
                    </h4>
                    <p className="mt-1">{purchaseRequest.department.name}</p>
                  </div>
                </div>
                <div>
                  <h4 className="text-sm font-medium text-muted-foreground">
                    Estimated Amount
                  </h4>
                  <p className="mt-1 text-lg font-semibold">
                    €
                    {typeof purchaseRequest.estimatedAmount === "string"
                      ? parseFloat(purchaseRequest.estimatedAmount).toFixed(2)
                      : purchaseRequest.estimatedAmount.toFixed(2)}
                  </p>
                </div>
                <div>
                  <h4 className="text-sm font-medium text-muted-foreground">
                    Business Justification
                  </h4>
                  <p className="mt-1">
                    {purchaseRequest.businessJustification ||
                      "No justification provided"}
                  </p>
                </div>
              </CardContent>
            </Card>
          </div>

          <div className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Status</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <Badge
                  variant="outline"
                  className={
                    purchaseRequest.status === PurchaseRequestStatus.Pending
                      ? "border-amber-300 bg-amber-50 text-amber-700"
                      : purchaseRequest.status ===
                          PurchaseRequestStatus.Approved
                        ? "border-green-300 bg-green-50 text-green-700"
                        : purchaseRequest.status ===
                            PurchaseRequestStatus.Rejected
                          ? "border-red-300 bg-red-50 text-red-700"
                          : "border-gray-300 bg-gray-100 text-gray-700"
                  }
                >
                  <span className="mr-1.5 inline-block h-1.5 w-1.5 rounded-full bg-current" />
                  {purchaseRequest.status}
                </Badge>

                {isPending && (
                  <Alert>
                    <AlertCircle className="h-4 w-4" />
                    <AlertDescription>
                      This request is pending approval and cannot be edited.
                    </AlertDescription>
                  </Alert>
                )}
              </CardContent>
            </Card>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">
            Edit Purchase Request
          </h1>
          <p className="text-muted-foreground text-sm">
            Request #{purchaseRequest.requestNumber}
          </p>
        </div>
        <AlertDialog>
          <AlertDialogTrigger asChild>
            <Button variant="destructive" size="sm">
              <Trash2 className="mr-2 h-4 w-4" />
              Delete
            </Button>
          </AlertDialogTrigger>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Delete Purchase Request?</AlertDialogTitle>
              <AlertDialogDescription>
                This action cannot be undone. This will permanently delete the
                purchase request.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancel</AlertDialogCancel>
              <AlertDialogAction
                onClick={handleDelete}
                className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
              >
                {deleteMutation.isPending ? "Deleting..." : "Delete"}
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      </div>

      <PurchaseRequestForm
        purchaseRequest={purchaseRequest}
        categories={categories}
        departments={departments}
        isCategoriesLoading={isCategoriesLoading}
        isDepartmentsLoading={isDepartmentsLoading}
        onSaveAsDraft={handleSaveAsDraft}
        onSubmitForApproval={handleSubmitForApproval}
        isSaving={updateMutation.isPending && !submitMutation.isPending}
        isSubmitting={submitMutation.isPending}
        saveError={updateMutation.error}
        submitError={submitMutation.error || updateMutation.error}
      />
    </div>
  );
}
