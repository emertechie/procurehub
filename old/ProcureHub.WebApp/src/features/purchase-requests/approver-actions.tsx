import * as React from "react";
import { Check, X } from "lucide-react";

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
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

interface ApproverActionsProps {
  isApproving?: boolean;
  isRejecting?: boolean;
  onApprove: () => void;
  onReject: () => void;
}

export function ApproverActions({
  isApproving,
  isRejecting,
  onApprove,
  onReject,
}: ApproverActionsProps) {
  const isPending = isApproving || isRejecting;

  return (
    <Card>
      <CardHeader>
        <CardTitle>Review Actions</CardTitle>
        <CardDescription>
          Approve or reject this purchase request
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-3">
        <AlertDialog>
          <AlertDialogTrigger asChild>
            <Button className="w-full" disabled={isPending}>
              <Check className="mr-2 h-4 w-4" />
              {isApproving ? "Approving..." : "Approve"}
            </Button>
          </AlertDialogTrigger>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Approve Purchase Request?</AlertDialogTitle>
              <AlertDialogDescription>
                This will approve the purchase request and notify the requester.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancel</AlertDialogCancel>
              <AlertDialogAction onClick={onApprove}>Approve</AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>

        <AlertDialog>
          <AlertDialogTrigger asChild>
            <Button variant="outline" className="w-full" disabled={isPending}>
              <X className="mr-2 h-4 w-4" />
              {isRejecting ? "Rejecting..." : "Reject"}
            </Button>
          </AlertDialogTrigger>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Reject Purchase Request?</AlertDialogTitle>
              <AlertDialogDescription>
                This will reject the purchase request and notify the requester.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancel</AlertDialogCancel>
              <AlertDialogAction
                onClick={onReject}
                className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
              >
                Reject
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      </CardContent>
    </Card>
  );
}
