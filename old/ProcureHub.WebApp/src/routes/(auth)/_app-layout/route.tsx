import React from "react";
import { Outlet, createFileRoute } from "@tanstack/react-router";

import {
  ensureAuthenticated,
  useAuth,
  useLogoutMutation,
  useDemoUsers,
  useDemoLoginMutation,
} from "@/features/auth/hooks";
import { SidebarInset, SidebarProvider } from "@/components/ui/sidebar";
import { AppSidebar, AppHeader, getNavigation } from "@/components/layout";

export const Route = createFileRoute("/(auth)/_app-layout")({
  beforeLoad: ({ context, location }) => {
    ensureAuthenticated(context.auth, location.href);
  },
  component: AuthenticatedLayout,
});

function AuthenticatedLayout() {
  const { user, loading, hasRole } = useAuth();
  const logoutMutation = useLogoutMutation();
  const { data: demoUsers } = useDemoUsers();
  const demoLoginMutation = useDemoLoginMutation();

  const navigation = React.useMemo(() => getNavigation(hasRole), [hasRole]);

  if (loading) {
    return (
      <div className="flex h-screen items-center justify-center text-muted-foreground">
        Checking your session...
      </div>
    );
  }

  const handleLogout = () => {
    logoutMutation.mutate(
      {},
      {
        onSuccess: () => {
          // Can't use `router.invalidate()` and then `navigate({ to: "/login" })` because
          // the auth context update happens in a React re-render cycle, which occurs AFTER
          // `beforeLoad` above has already evaluated with stale context. So the user would
          // stay on this route but (eventually) be in a logged-out state.
          // So using hard redirect instead.
          window.location.href = "/login";
        },
      },
    );
  };

  const handleDemoLogin = (email: string) => {
    demoLoginMutation.mutate(
      {
        body: { email },
      },
      {
        onSuccess: () => {
          // Full page reload to refresh auth context and stay on current page
          window.location.reload();
        },
      },
    );
  };

  return (
    <SidebarProvider>
      <AppSidebar navigation={navigation} hasRole={hasRole} />
      <SidebarInset>
        <AppHeader
          user={user}
          navigation={navigation}
          hasRole={hasRole}
          onLogout={handleLogout}
          demoUsers={demoUsers}
          onDemoLogin={handleDemoLogin}
          isDemoLoginPending={demoLoginMutation.isPending}
        />
        <div className="flex flex-1 flex-col gap-4 py-4 px-6">
          <Outlet />
        </div>
      </SidebarInset>
    </SidebarProvider>
  );
}
