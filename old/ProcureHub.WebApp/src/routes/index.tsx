import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/")({
  beforeLoad: ({ context }) => {
    // If authenticated, redirect to dashboard
    // If not authenticated, redirect to login (same as dashboard would do)
    if (context.auth.isAuthenticated) {
      throw redirect({ to: "/dashboard" });
    } else {
      throw redirect({ to: "/login" });
    }
  },
});
