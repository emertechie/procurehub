import * as React from "react";
import { Send, Save } from "lucide-react";

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";

interface PurchaseRequestActionsProps {
  isEditing: boolean;
  isSaving?: boolean;
  isSubmitting?: boolean;
  onSaveAsDraft: () => void;
  onSubmitForApproval: () => void;
}

export function PurchaseRequestActions({
  isEditing,
  isSaving,
  isSubmitting,
  onSaveAsDraft,
  onSubmitForApproval,
}: PurchaseRequestActionsProps) {
  const isPending = isSaving || isSubmitting;

  return (
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
          onClick={onSubmitForApproval}
          disabled={isPending}
        >
          <Send className="mr-2 h-4 w-4" />
          {isSubmitting ? "Submitting..." : "Submit for Approval"}
        </Button>
        <Button
          variant="outline"
          className="w-full"
          onClick={onSaveAsDraft}
          disabled={isPending}
        >
          <Save className="mr-2 h-4 w-4" />
          {isSaving ? "Saving..." : isEditing ? "Save Draft" : "Save as Draft"}
        </Button>
      </CardContent>
    </Card>
  );
}
