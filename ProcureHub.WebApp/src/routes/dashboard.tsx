import * as React from "react";
import { createFileRoute, useRouter } from "@tanstack/react-router";
import { useAuth } from "../auth";

export const Route = createFileRoute("/dashboard")({
  component: DashboardPage,
});

function DashboardPage() {
  const { user, loading } = useAuth();
  const router = useRouter();

  React.useEffect(() => {
    if (!loading && !user) {
      router.navigate({ to: "/login" });
    }
  }, [loading, user, router]);

  if (loading) {
    return <div className="p-4">Loading...</div>;
  }

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
