import * as React from "react";
import { Link } from "@tanstack/react-router";
import type { PurchaseRequest, PurchaseRequestStatusValue } from "./types";
import { PurchaseRequestStatus } from "./types";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { MoreHorizontal, Eye } from "lucide-react";

interface PurchaseRequestTableProps {
  requests: PurchaseRequest[];
  onViewRequest?: (request: PurchaseRequest) => void;
}

function getStatusBadgeClasses(status: PurchaseRequestStatusValue): string {
  switch (status) {
    case PurchaseRequestStatus.Draft:
      return "border-gray-300 bg-gray-100 text-gray-700";
    case PurchaseRequestStatus.Pending:
      return "border-amber-300 bg-amber-50 text-amber-700";
    case PurchaseRequestStatus.Approved:
      return "border-green-300 bg-green-50 text-green-700";
    case PurchaseRequestStatus.Rejected:
      return "border-red-300 bg-red-50 text-red-700";
    default:
      return "border-gray-300 bg-gray-100 text-gray-700";
  }
}

function formatCurrency(amount: number | string): string {
  const numAmount = typeof amount === "string" ? parseFloat(amount) : amount;
  return new Intl.NumberFormat("en-IE", {
    style: "currency",
    currency: "EUR",
  }).format(numAmount);
}

function formatDate(dateString: string): string {
  return new Intl.DateTimeFormat("en-IE", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(new Date(dateString));
}

export function PurchaseRequestTable({
  requests,
  onViewRequest,
}: PurchaseRequestTableProps) {
  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>ID</TableHead>
          <TableHead>Title</TableHead>
          <TableHead>Category</TableHead>
          <TableHead className="text-right">Amount</TableHead>
          <TableHead>Status</TableHead>
          <TableHead>Updated</TableHead>
          <TableHead className="w-12"></TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {requests.length === 0 ? (
          <TableRow>
            <TableCell
              colSpan={7}
              className="text-center text-muted-foreground"
            >
              No purchase requests found
            </TableCell>
          </TableRow>
        ) : (
          requests.map((request) => (
            <TableRow key={request.id}>
              <TableCell className="font-mono text-sm text-muted-foreground">
                <Link
                  to="/requests/$id/edit"
                  params={{ id: request.id }}
                  className="hover:underline"
                >
                  {request.requestNumber}
                </Link>
              </TableCell>
              <TableCell className="font-medium">{request.title}</TableCell>
              <TableCell>{request.category.name}</TableCell>
              <TableCell className="text-right font-medium">
                {formatCurrency(request.estimatedAmount)}
              </TableCell>
              <TableCell>
                <Badge
                  variant="outline"
                  className={getStatusBadgeClasses(request.status)}
                >
                  <span className="mr-1.5 inline-block h-1.5 w-1.5 rounded-full bg-current" />
                  {request.status}
                </Badge>
              </TableCell>
              <TableCell className="text-muted-foreground">
                {formatDate(request.updatedAt)}
              </TableCell>
              <TableCell>
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="ghost" size="icon">
                      <MoreHorizontal className="h-4 w-4" />
                      <span className="sr-only">Open menu</span>
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end">
                    <DropdownMenuItem onClick={() => onViewRequest?.(request)}>
                      <Eye className="mr-2 h-4 w-4" />
                      View Details
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              </TableCell>
            </TableRow>
          ))
        )}
      </TableBody>
    </Table>
  );
}
