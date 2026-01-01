import * as React from "react";
import { type UseFormReturn } from "react-hook-form";

import {
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { CurrencyInput } from "@/components/ui/currency-input";

import type {
  CreatePurchaseRequestFormData,
  UpdatePurchaseRequestFormData,
  Category,
  Department,
} from "./types";

interface PurchaseRequestFormFieldsProps {
  form: UseFormReturn<
    CreatePurchaseRequestFormData | UpdatePurchaseRequestFormData
  >;
  categories: Category[];
  departments: Department[];
  isCategoriesLoading?: boolean;
  isDepartmentsLoading?: boolean;
}

export function PurchaseRequestFormFields({
  form,
  categories,
  departments,
  isCategoriesLoading,
  isDepartmentsLoading,
}: PurchaseRequestFormFieldsProps) {
  return (
    <>
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
                    <SelectItem key={category.id} value={category.id}>
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
                    <SelectItem key={department.id} value={department.id}>
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
              <CurrencyInput placeholder="0.00" {...field} />
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
    </>
  );
}
