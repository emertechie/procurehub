import * as React from "react";
import { createFileRoute } from "@tanstack/react-router";
import { Info } from "lucide-react";
import { useAuth } from "@/features/auth/hooks";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Alert, AlertDescription } from "@/components/ui/alert";

export const Route = createFileRoute("/(auth)/_app-layout/dashboard")({
  component: DashboardPage,
});

function DashboardPage() {
  const { user } = useAuth();

  if (!user) {
    return <div>Redirecting to login...</div>;
  }

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Dashboard</h1>
        <p className="text-muted-foreground">Welcome back {user.firstName}</p>
      </div>

      <Alert className="border-amber-200 bg-amber-50">
        <Info className="h-4 w-4 !text-amber-800" />
        <AlertDescription className="text-amber-800">
          Dashboard not implemented yet. Demo data shown below.
        </AlertDescription>
      </Alert>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        <Card>
          <CardHeader>
            <CardTitle>My Requests</CardTitle>
            <CardDescription>Active purchase requests</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">12</div>
            <p className="text-xs text-muted-foreground">+2 from last month</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Pending Approvals</CardTitle>
            <CardDescription>Awaiting your review</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">5</div>
            <p className="text-xs text-muted-foreground">3 high priority</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Total Spend</CardTitle>
            <CardDescription>This quarter</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">€24,500</div>
            <p className="text-xs text-muted-foreground">65% of budget used</p>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Recent Activity</CardTitle>
          <CardDescription>Latest procurement updates</CardDescription>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">
            Activity feed will appear here
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
