import React from "react";
import { Link, useLocation } from "@tanstack/react-router";
import {
  BadgeCheck,
  Bell,
  ChevronsUpDown,
  LogOut,
  Sparkles,
} from "lucide-react";

import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { Separator } from "@/components/ui/separator";
import { SidebarTrigger } from "@/components/ui/sidebar";
import type { Navigation } from "./nav-data";
import type { AuthUser } from "@/features/auth/types";

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

function getUserInitials(email?: string): string {
  if (!email) return "??";
  return (
    email
      .split("@")[0]
      ?.split(".")
      .map((n) => n[0])
      .join("")
      .toUpperCase()
      .slice(0, 2) || "??"
  );
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
  const userInitials = getUserInitials(user?.email);
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
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <button className="flex items-center gap-2 rounded-lg border border-input bg-background px-2 py-1 hover:bg-accent hover:text-accent-foreground">
              <Avatar className="h-7 w-7 rounded-lg">
                <AvatarImage src="" alt={user?.email} />
                <AvatarFallback className="rounded-lg text-xs">
                  {userInitials}
                </AvatarFallback>
              </Avatar>
              <div className="hidden md:grid text-left text-sm leading-tight">
                <span className="truncate font-semibold">
                  {user?.email?.split("@")[0] || "User"}
                </span>
              </div>
              <ChevronsUpDown className="h-4 w-4" />
            </button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-56">
            <DropdownMenuLabel className="p-0 font-normal">
              <div className="flex items-center gap-2 px-1 py-1.5 text-left text-sm">
                <Avatar className="h-8 w-8 rounded-lg">
                  <AvatarImage src="" alt={user?.email} />
                  <AvatarFallback className="rounded-lg">
                    {userInitials}
                  </AvatarFallback>
                </Avatar>
                <div className="grid flex-1 text-left text-sm leading-tight">
                  <span className="truncate font-semibold">
                    {user?.email?.split("@")[0] || "User"}
                  </span>
                  <span className="truncate text-xs">{user?.email}</span>
                </div>
              </div>
            </DropdownMenuLabel>
            <DropdownMenuSeparator />
            <DropdownMenuGroup>
              <Tooltip>
                <TooltipTrigger asChild>
                  <DropdownMenuItem onSelect={(e) => e.preventDefault()}>
                    <BadgeCheck className="mr-2 h-4 w-4" />
                    Profile
                  </DropdownMenuItem>
                </TooltipTrigger>
                <TooltipContent side="left">
                  <p className="text-xs">Not implemented</p>
                </TooltipContent>
              </Tooltip>
              <Tooltip>
                <TooltipTrigger asChild>
                  <DropdownMenuItem onSelect={(e) => e.preventDefault()}>
                    <Bell className="mr-2 h-4 w-4" />
                    Notifications
                  </DropdownMenuItem>
                </TooltipTrigger>
                <TooltipContent side="left">
                  <p className="text-xs">Not implemented</p>
                </TooltipContent>
              </Tooltip>
            </DropdownMenuGroup>
            <DropdownMenuSeparator />
            <DropdownMenuItem onClick={onLogout}>
              <LogOut className="mr-2 h-4 w-4" />
              Log out
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
        {demoUsers && Array.isArray(demoUsers) && onDemoLogin && (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <button className="flex items-center gap-2 rounded-md border-2 border-amber-400 bg-amber-50 px-3 py-1.5 text-sm font-semibold text-amber-900 shadow-sm hover:bg-amber-100 hover:border-amber-500 transition-colors">
                <Sparkles className="h-4 w-4 text-amber-600" />
                <span>Switch Demo User</span>
              </button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-64">
              <DropdownMenuLabel className="flex items-center gap-2 text-amber-900">
                <Sparkles className="h-4 w-4 text-amber-600" />
                <span>Demo Users</span>
              </DropdownMenuLabel>
              <DropdownMenuSeparator />
              {demoUsers.map((demoUser) => (
                <DropdownMenuItem
                  key={demoUser.email}
                  onClick={() => onDemoLogin(demoUser.email)}
                  disabled={
                    isDemoLoginPending || user?.email === demoUser.email
                  }
                  className="cursor-pointer"
                >
                  <div className="flex flex-col">
                    <span className="font-medium">
                      {demoUser.firstName} {demoUser.lastName}
                    </span>
                    <span className="text-xs text-muted-foreground">
                      {demoUser.role} • {demoUser.email}
                    </span>
                  </div>
                </DropdownMenuItem>
              ))}
            </DropdownMenuContent>
          </DropdownMenu>
        )}
      </div>
    </header>
  );
}
