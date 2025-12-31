import * as React from "react";
import { AlertCircle } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { PurchaseRequestStatus, type PurchaseRequest } from "./types";

interface PurchaseRequestDetailsProps {
  purchaseRequest: PurchaseRequest;
}

export function PurchaseRequestDetails({
  purchaseRequest,
}: PurchaseRequestDetailsProps) {
  const isPending = purchaseRequest.status === PurchaseRequestStatus.Pending;

  return (
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
                  : purchaseRequest.status === PurchaseRequestStatus.Approved
                    ? "border-green-300 bg-green-50 text-green-700"
                    : purchaseRequest.status === PurchaseRequestStatus.Rejected
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
  );
}
