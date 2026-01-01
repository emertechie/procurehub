import * as React from "react";
import { createFileRoute, Link } from "@tanstack/react-router";
import { Trash2, AlertCircle, ArrowLeft } from "lucide-react";

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
import { Alert, AlertDescription } from "@/components/ui/alert";

import {
  PurchaseRequestForm,
  PurchaseRequestDetails,
  usePurchaseRequest,
  useEditPurchaseRequest,
  PurchaseRequestStatus,
  type PurchaseRequest,
} from "@/features/purchase-requests";
import { useCategories } from "@/features/categories";
import { useDepartments } from "@/features/departments";

export const Route = createFileRoute("/(auth)/_app-layout/requests/$id/edit")({
  component: EditRequestPage,
});

interface PurchaseRequestReadOnlyViewProps {
  purchaseRequest: PurchaseRequest;
}

function PurchaseRequestReadOnlyView({
  purchaseRequest,
}: PurchaseRequestReadOnlyViewProps) {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">
          {purchaseRequest.title}
        </h1>
        <p className="text-muted-foreground text-sm">
          Request #{purchaseRequest.requestNumber}
        </p>
      </div>

      <PurchaseRequestDetails purchaseRequest={purchaseRequest} />
    </div>
  );
}

function EditRequestPage() {
  const { id } = Route.useParams();

  const {
    data: requestData,
    isPending: isRequestLoading,
    error: requestError,
  } = usePurchaseRequest(id);
  const { data: categoriesData, isPending: isCategoriesLoading } =
    useCategories();
  const { data: departmentsData, isPending: isDepartmentsLoading } =
    useDepartments();

  const {
    handleSaveAsDraft,
    handleSubmitForApproval,
    handleDelete,
    isSaving,
    isSubmitting,
    isDeleting,
    saveError,
    submitError,
  } = useEditPurchaseRequest(id);

  const purchaseRequest = requestData?.data;
  const categories = categoriesData?.data ?? [];
  const departments = departmentsData?.data ?? [];

  const isDraft = purchaseRequest?.status === PurchaseRequestStatus.Draft;

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
    return <PurchaseRequestReadOnlyView purchaseRequest={purchaseRequest} />;
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
            <Button variant="outline" size="sm">
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
                {isDeleting ? "Deleting..." : "Delete"}
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
        isSaving={isSaving}
        isSubmitting={isSubmitting}
        saveError={saveError}
        submitError={submitError}
      />
    </div>
  );
}
