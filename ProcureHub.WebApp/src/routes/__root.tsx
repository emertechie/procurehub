import * as React from "react";
import {
  Link,
  Outlet,
  createRootRouteWithContext,
  useLocation,
} from "@tanstack/react-router";
import { TanStackRouterDevtools } from "@tanstack/react-router-devtools";
import type { AuthContext } from "@/features/auth/types";
import { useAuth, useLogoutMutation } from "@/features/auth/hooks";
import { Button } from "@/components/ui/button";

interface MyRouterContext {
  auth: AuthContext;
}

export const Route = createRootRouteWithContext<MyRouterContext>()({
  component: RootComponent,
});

function RootComponent() {
  const auth = useAuth();
  const logoutMutation = useLogoutMutation();
  const location = useLocation();

  // Hide header on authenticated routes (they have their own layout)
  const isAuthenticatedRoute =
    location.pathname.startsWith("/dashboard") ||
    location.pathname.startsWith("/requests") ||
    location.pathname.startsWith("/approvals") ||
    location.pathname.startsWith("/admin");

  return (
    <>
      {!isAuthenticatedRoute && (
        <div className="border-b border-gray-200 bg-white/80 px-4 py-2 backdrop-blur dark:border-gray-800 dark:bg-gray-900/80">
          <div className="mx-auto flex max-w-5xl items-center justify-between gap-4">
            <div className="flex items-center gap-4 text-sm">
              <Link
                to="/"
                activeProps={{
                  className: "font-semibold",
                }}
                activeOptions={{ exact: true }}
              >
                Home
              </Link>
              <Link
                to="/about"
                activeProps={{
                  className: "font-semibold",
                }}
              >
                About
              </Link>
              {auth.user && (
                <Link
                  to="/dashboard"
                  activeProps={{
                    className: "font-semibold",
                  }}
                >
                  Dashboard
                </Link>
              )}
            </div>
            <div className="flex items-center gap-2 text-sm">
              {auth.user ? (
                <>
                  <span className="hidden text-gray-600 dark:text-gray-300 sm:inline">
                    {auth.user.email}
                  </span>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => logoutMutation.mutate({})}
                  >
                    Sign out
                  </Button>
                </>
              ) : (
                <>
                  <Link to="/login">
                    <Button variant="outline" size="sm">
                      Sign in
                    </Button>
                  </Link>
                  <Link to="/register">
                    <Button size="sm">Register</Button>
                  </Link>
                </>
              )}
            </div>
          </div>
        </div>
      )}
      <main
        className={!isAuthenticatedRoute ? "mx-auto max-w-5xl px-4 py-6" : ""}
      >
        <Outlet />
      </main>
      <TanStackRouterDevtools position="bottom-right" />
    </>
  );
}
