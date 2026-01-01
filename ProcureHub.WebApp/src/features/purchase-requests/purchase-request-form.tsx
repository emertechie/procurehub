import * as React from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Form } from "@/components/ui/form";
import { FormErrorAlert } from "@/components/form-error-alert";
import {
  useProblemDetails,
  setFormFieldErrors,
} from "@/hooks/use-problem-details";

import { PurchaseRequestFormFields } from "./purchase-request-form-fields";
import { PurchaseRequestActions } from "./purchase-request-actions";
import {
  createPurchaseRequestSchema,
  updatePurchaseRequestSchema,
  type CreatePurchaseRequestFormData,
  type UpdatePurchaseRequestFormData,
  type PurchaseRequestDetail,
  type Category,
  type Department,
} from "./types";

interface PurchaseRequestFormProps {
  purchaseRequest?: PurchaseRequestDetail | null;
  categories: Category[];
  departments: Department[];
  isCategoriesLoading?: boolean;
  isDepartmentsLoading?: boolean;
  onSaveAsDraft: (
    data: CreatePurchaseRequestFormData | UpdatePurchaseRequestFormData,
  ) => Promise<void>;
  onSubmitForApproval: (
    data: CreatePurchaseRequestFormData | UpdatePurchaseRequestFormData,
  ) => Promise<void>;
  isSaving?: boolean;
  isSubmitting?: boolean;
  saveError?: unknown;
  submitError?: unknown;
}

export function PurchaseRequestForm({
  purchaseRequest,
  categories,
  departments,
  isCategoriesLoading,
  isDepartmentsLoading,
  onSaveAsDraft,
  onSubmitForApproval,
  isSaving,
  isSubmitting,
  saveError,
  submitError,
}: PurchaseRequestFormProps) {
  const isEditing = !!purchaseRequest;

  const form = useForm<
    CreatePurchaseRequestFormData | UpdatePurchaseRequestFormData
  >({
    resolver: zodResolver(
      isEditing ? updatePurchaseRequestSchema : createPurchaseRequestSchema,
    ),
    defaultValues: isEditing
      ? {
          id: purchaseRequest.id,
          title: purchaseRequest.title,
          description: purchaseRequest.description ?? "",
          estimatedAmount:
            typeof purchaseRequest.estimatedAmount === "string"
              ? parseFloat(purchaseRequest.estimatedAmount)
              : purchaseRequest.estimatedAmount,
          businessJustification: purchaseRequest.businessJustification ?? "",
          categoryId: purchaseRequest.category.id,
          departmentId: purchaseRequest.department.id,
        }
      : {
          title: "",
          description: "",
          estimatedAmount: 0,
          businessJustification: "",
          categoryId: "",
          departmentId: "",
        },
  });

  // Reset form when purchase request data changes (e.g., after refetch)
  React.useEffect(() => {
    if (isEditing && purchaseRequest) {
      form.reset({
        id: purchaseRequest.id,
        title: purchaseRequest.title,
        description: purchaseRequest.description ?? "",
        estimatedAmount:
          typeof purchaseRequest.estimatedAmount === "string"
            ? parseFloat(purchaseRequest.estimatedAmount)
            : purchaseRequest.estimatedAmount,
        businessJustification: purchaseRequest.businessJustification ?? "",
        categoryId: purchaseRequest.category.id,
        departmentId: purchaseRequest.department.id,
      });
    }
  }, [purchaseRequest, isEditing, form]);

  // Extract ProblemDetails from errors
  const saveProblem = useProblemDetails(saveError);
  const submitProblem = useProblemDetails(submitError);

  // Sync field errors to form
  React.useEffect(() => {
    if (saveProblem.hasFieldErrors) {
      setFormFieldErrors(form, saveProblem.fieldErrors);
    }
  }, [saveProblem.fieldErrors, saveProblem.hasFieldErrors, form]);

  React.useEffect(() => {
    if (submitProblem.hasFieldErrors) {
      setFormFieldErrors(form, submitProblem.fieldErrors);
    }
  }, [submitProblem.fieldErrors, submitProblem.hasFieldErrors, form]);

  const handleSaveAsDraft = async () => {
    const isValid = await form.trigger();
    if (isValid) {
      const data = form.getValues();
      await onSaveAsDraft(data);
    }
  };

  const handleSubmitForApproval = async () => {
    const isValid = await form.trigger();
    if (isValid) {
      const data = form.getValues();
      await onSubmitForApproval(data);
    }
  };

  return (
    <div className="space-y-6">
      <div className="grid gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2">
          <Card>
            <CardHeader>
              <CardTitle>Request Details</CardTitle>
              <CardDescription>
                Provide information about your purchase request
              </CardDescription>
            </CardHeader>
            <CardContent>
              <Form {...form}>
                <form className="space-y-6">
                  <FormErrorAlert
                    message={saveProblem.summary || submitProblem.summary}
                  />

                  <PurchaseRequestFormFields
                    form={form}
                    categories={categories}
                    departments={departments}
                    isCategoriesLoading={isCategoriesLoading}
                    isDepartmentsLoading={isDepartmentsLoading}
                  />
                </form>
              </Form>
            </CardContent>
          </Card>
        </div>

        <div className="space-y-4">
          <PurchaseRequestActions
            isEditing={isEditing}
            isSaving={isSaving}
            isSubmitting={isSubmitting}
            onSaveAsDraft={handleSaveAsDraft}
            onSubmitForApproval={handleSubmitForApproval}
          />
        </div>
      </div>
    </div>
  );
}
