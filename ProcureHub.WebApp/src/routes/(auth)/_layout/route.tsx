import React from 'react';
import { Outlet, createFileRoute } from "@tanstack/react-router";

import { ensureAuthenticated } from "@/features/auth/hooks";

export const Route = createFileRoute("/(auth)/_layout")({
  component: AuthenticatedLayout,
  beforeLoad: ({ context, location }) => {
    ensureAuthenticated(context.auth, location.href);
  },
});

function AuthenticatedLayout() {
  return <Outlet />;
}
