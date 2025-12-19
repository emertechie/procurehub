import * as React from "react";
import {createFileRoute} from "@tanstack/react-router";
import {useAuth} from "@/features/auth/hooks";

export const Route = createFileRoute("/(auth)/_layout/dashboard")({
  component: DashboardPage,
});

function DashboardPage() {
  const { user } = useAuth();
  
  if (!user) {
    return <div className="p-4">Redirecting to login...</div>;
  }

  return (
    <div className="p-4 space-y-2">
      <h3 className="text-xl font-semibold">Protected dashboard</h3>
      <p className="text-sm text-gray-600 dark:text-gray-400">
        You are signed in as <span className="font-medium">{user.email}</span>.
      </p>
      <p className="text-sm text-gray-600 dark:text-gray-400">
        This page is only accessible when authenticated.
      </p>
    </div>
  );
}
