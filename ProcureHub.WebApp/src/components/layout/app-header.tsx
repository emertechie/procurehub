import React from "react";
import { Link, useLocation } from "@tanstack/react-router";

import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import { Separator } from "@/components/ui/separator";
import { SidebarTrigger } from "@/components/ui/sidebar";
import type { Navigation } from "./nav-data";
import type { AuthUser } from "@/features/auth/types";
import { DemoUserSwitcher } from "./demo-user-switcher";
import { UserMenu } from "./user-menu";

interface DemoUser {
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

interface AppHeaderProps {
  user: AuthUser | null;
  navigation: Navigation;
  hasRole: (role: string) => boolean;
  onLogout: () => void;
  demoUsers?: DemoUser[];
  onDemoLogin?: (email: string) => void;
  isDemoLoginPending?: boolean;
}

function useBreadcrumbs(
  navigation: Navigation,
  hasRole: (role: string) => boolean,
) {
  const location = useLocation();
  const currentPath = location.pathname;

  // Dashboard is always just "Home", no additional breadcrumbs
  if (currentPath === "/dashboard") {
    return { breadcrumbs: [], isOnDashboard: true };
  }

  // Check all navigation groups for matching URL
  const allNavItems = [
    ...navigation.main,
    ...navigation.requests,
    ...(hasRole("Approver") ? navigation.approvals : []),
    ...(hasRole("Admin") ? navigation.admin : []),
  ];

  const matchingItem = allNavItems.find((item) => item.url === currentPath);

  // Handle exact matches (simple pages)
  if (matchingItem) {
    return {
      breadcrumbs: [{ title: matchingItem.title, url: currentPath }],
      isOnDashboard: false,
    };
  }

  // Handle nested routes
  // Check for nested request routes: /requests/{id}/edit
  const requestEditMatch = currentPath.match(/^\/requests\/[^/]+\/edit$/);
  if (requestEditMatch) {
    const requestsItem = allNavItems.find((item) => item.url === "/requests");
    return {
      breadcrumbs: [
        { title: requestsItem?.title || "Requests", url: "/requests" },
        { title: "Purchase Request", url: currentPath },
      ],
      isOnDashboard: false,
    };
  }

  return { breadcrumbs: [], isOnDashboard: false };
}

export function AppHeader({
  user,
  navigation,
  hasRole,
  onLogout,
  demoUsers,
  onDemoLogin,
  isDemoLoginPending,
}: AppHeaderProps) {
  const { breadcrumbs, isOnDashboard } = useBreadcrumbs(navigation, hasRole);

  return (
    <header className="flex h-16 shrink-0 items-center gap-2 border-b px-4">
      <SidebarTrigger className="-ml-1" />
      <Separator orientation="vertical" className="mr-2 h-4" />
      <Breadcrumb>
        <BreadcrumbList>
          <BreadcrumbItem className="hidden md:block">
            <BreadcrumbLink asChild>
              <Link to="/dashboard">Home</Link>
            </BreadcrumbLink>
          </BreadcrumbItem>
          {!isOnDashboard && breadcrumbs.length === 0 && (
            <>
              <BreadcrumbSeparator className="hidden md:block" />
              <BreadcrumbItem>
                <BreadcrumbPage>Current Page</BreadcrumbPage>
              </BreadcrumbItem>
            </>
          )}
          {breadcrumbs.map((crumb, index) => (
            <React.Fragment key={crumb.url}>
              <BreadcrumbSeparator className="hidden md:block" />
              <BreadcrumbItem>
                {index === breadcrumbs.length - 1 ? (
                  <BreadcrumbPage>{crumb.title}</BreadcrumbPage>
                ) : (
                  <BreadcrumbLink asChild>
                    <Link to={crumb.url}>{crumb.title}</Link>
                  </BreadcrumbLink>
                )}
              </BreadcrumbItem>
            </React.Fragment>
          ))}
        </BreadcrumbList>
      </Breadcrumb>
      <div className="ml-auto flex items-center gap-4">
        <UserMenu user={user} onLogout={onLogout} />

        {demoUsers && Array.isArray(demoUsers) && onDemoLogin && (
          <DemoUserSwitcher
            user={user}
            demoUsers={demoUsers}
            onDemoLogin={onDemoLogin}
            isDemoLoginPending={isDemoLoginPending}
          />
        )}
      </div>
    </header>
  );
}
