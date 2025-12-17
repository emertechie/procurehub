import * as React from "react";
import {createFileRoute, redirect} from "@tanstack/react-router";
import { useAuth } from "../auth";

export const Route = createFileRoute("/dashboard")({
  component: DashboardPage,
    beforeLoad: ({ context, location }) => {
        if (!context.auth.isAuthenticated) {
            throw redirect({
                to: '/login',
                search: {
                    redirect: location.href,
                },
            })
        }
    },
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
