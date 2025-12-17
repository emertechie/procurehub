import * as React from "react";
import { Link, Outlet, createRootRoute } from "@tanstack/react-router";
import { TanStackRouterDevtools } from "@tanstack/react-router-devtools";
import { useAuth } from "../auth";
import { Button } from "../components/ui/button";

export const Route = createRootRoute({
  component: RootComponent,
});

function RootComponent() {
  const auth = useAuth();

  return (
    <>
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
            <Link
              to="/dashboard"
              activeProps={{
                className: "font-semibold",
              }}
            >
              Dashboard
            </Link>
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
                  onClick={() => auth.logout()}
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
      <main className="mx-auto max-w-5xl px-4 py-6">
        <Outlet />
      </main>
      <TanStackRouterDevtools position="bottom-right" />
    </>
  );
}
