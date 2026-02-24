import * as React from "react";
import { Button } from "@/components/ui/button";
import {
  PurchaseRequestStatus,
  type PurchaseRequestStatusValue,
} from "./types";

interface StatusFilterProps {
  selectedStatus: PurchaseRequestStatusValue | null;
  onStatusChange: (status: PurchaseRequestStatusValue | null) => void;
}

const statuses = [
  { status: null, label: "All" },
  { status: PurchaseRequestStatus.Draft, label: PurchaseRequestStatus.Draft },
  {
    status: PurchaseRequestStatus.Pending,
    label: PurchaseRequestStatus.Pending,
  },
  {
    status: PurchaseRequestStatus.Approved,
    label: PurchaseRequestStatus.Approved,
  },
  {
    status: PurchaseRequestStatus.Rejected,
    label: PurchaseRequestStatus.Rejected,
  },
] as const;

export function StatusFilter({
  selectedStatus,
  onStatusChange,
}: StatusFilterProps) {
  return (
    <div className="flex gap-2">
      {statuses.map(({ status, label }) => {
        const isSelected = selectedStatus === status;
        return (
          <Button
            key={label}
            variant={isSelected ? "default" : "outline"}
            size="sm"
            onClick={() => onStatusChange(status)}
          >
            {label}
          </Button>
        );
      })}
    </div>
  );
}
