import * as React from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { ArrowLeft, Send, Save } from "lucide-react";
import { Link } from "@tanstack/react-router";

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { FormErrorAlert } from "@/components/form-error-alert";
import {
  useProblemDetails,
  setFormFieldErrors,
} from "@/hooks/use-problem-details";

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

  const isPending = isSaving || isSubmitting;

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

                  <FormField
                    control={form.control}
                    name="title"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Title *</FormLabel>
                        <FormControl>
                          <Input
                            placeholder="e.g., MacBook Pro for Development Team"
                            {...field}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="description"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Description</FormLabel>
                        <FormControl>
                          <Textarea
                            placeholder="Describe what you need to purchase and why..."
                            className="min-h-[100px] resize-y"
                            {...field}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <div className="grid gap-4 sm:grid-cols-2">
                    <FormField
                      control={form.control}
                      name="categoryId"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Category *</FormLabel>
                          <Select
                            onValueChange={field.onChange}
                            value={field.value}
                            disabled={isCategoriesLoading}
                          >
                            <FormControl>
                              <SelectTrigger className="w-full">
                                <SelectValue placeholder="Select category" />
                              </SelectTrigger>
                            </FormControl>
                            <SelectContent>
                              {categories.map((category) => (
                                <SelectItem
                                  key={category.id}
                                  value={category.id}
                                >
                                  {category.name}
                                </SelectItem>
                              ))}
                            </SelectContent>
                          </Select>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="departmentId"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Department *</FormLabel>
                          <Select
                            onValueChange={field.onChange}
                            value={field.value}
                            disabled={isDepartmentsLoading}
                          >
                            <FormControl>
                              <SelectTrigger className="w-full">
                                <SelectValue placeholder="Select department" />
                              </SelectTrigger>
                            </FormControl>
                            <SelectContent>
                              {departments.map((department) => (
                                <SelectItem
                                  key={department.id}
                                  value={department.id}
                                >
                                  {department.name}
                                </SelectItem>
                              ))}
                            </SelectContent>
                          </Select>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>

                  <FormField
                    control={form.control}
                    name="estimatedAmount"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Estimated Amount (EUR) *</FormLabel>
                        <FormControl>
                          <div className="relative">
                            <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground">
                              €
                            </span>
                            <Input
                              type="number"
                              step="0.01"
                              min="0"
                              placeholder="0.00"
                              className="pl-7"
                              {...field}
                            />
                          </div>
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="businessJustification"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Business Justification</FormLabel>
                        <FormControl>
                          <Textarea
                            placeholder="Explain why this purchase is necessary and how it benefits the organization..."
                            className="min-h-[100px] resize-y"
                            {...field}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </form>
              </Form>
            </CardContent>
          </Card>
        </div>

        <div className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Actions</CardTitle>
              <CardDescription>
                {isEditing
                  ? "Update your draft request or submit for approval"
                  : "Save as draft or submit for approval"}
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-3">
              <Button
                className="w-full"
                onClick={handleSubmitForApproval}
                disabled={isPending}
              >
                <Send className="mr-2 h-4 w-4" />
                {isSubmitting ? "Submitting..." : "Submit for Approval"}
              </Button>
              <Button
                variant="outline"
                className="w-full"
                onClick={handleSaveAsDraft}
                disabled={isPending}
              >
                <Save className="mr-2 h-4 w-4" />
                {isSaving
                  ? "Saving..."
                  : isEditing
                    ? "Save Draft"
                    : "Save as Draft"}
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
