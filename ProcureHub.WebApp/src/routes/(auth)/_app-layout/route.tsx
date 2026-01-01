import React from "react";
import {
  Link,
  Outlet,
  createFileRoute,
  useLocation,
} from "@tanstack/react-router";
import {
  BadgeCheck,
  Bell,
  Building2,
  ChevronsUpDown,
  FilePlus,
  FileText,
  Home,
  LogOut,
  Package,
  Users,
  CheckCircle,
  Sparkles,
} from "lucide-react";

import {
  ensureAuthenticated,
  useAuth,
  useLogoutMutation,
  useDemoUsers,
  useDemoLoginMutation,
} from "@/features/auth/hooks";
import { getRequestsAreaTitle } from "@/features/purchase-requests/utils/navigation";
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
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarInset,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
  SidebarRail,
  SidebarTrigger,
} from "@/components/ui/sidebar";

export const Route = createFileRoute("/(auth)/_app-layout")({
  beforeLoad: ({ context, location }) => {
    ensureAuthenticated(context.auth, location.href);
  },
  component: AuthenticatedLayout,
});

const getNavigation = (hasRole: (role: string) => boolean) => {
  const requestsTitle = getRequestsAreaTitle(hasRole);

  return {
    main: [
      {
        title: "Home",
        url: "/dashboard",
        icon: Home,
      },
    ],
    requests: [
      {
        title: requestsTitle,
        url: "/requests",
        icon: FileText,
      },
      {
        title: "New Request",
        url: "/requests/new",
        icon: FilePlus,
      },
    ],
    approvals: [
      {
        title: "Pending Approvals",
        url: "/approvals",
        icon: CheckCircle,
      },
    ],
    admin: [
      {
        title: "Users",
        url: "/admin/users",
        icon: Users,
      },
      {
        title: "Departments",
        url: "/admin/departments",
        icon: Building2,
      },
    ],
  };
};

function AuthenticatedLayout() {
  const { user, loading, hasRole } = useAuth();
  const logoutMutation = useLogoutMutation();
  const { data: demoUsers } = useDemoUsers();
  const demoLoginMutation = useDemoLoginMutation();
  const location = useLocation();

  const navigation = React.useMemo(() => getNavigation(hasRole), [hasRole]);

  const getCurrentPageTitle = () => {
    const currentPath = location.pathname;

    // Check all navigation groups for matching URL
    const allNavItems = [
      ...navigation.main,
      ...navigation.requests,
      ...(hasRole("Approver") ? navigation.approvals : []),
      ...(hasRole("Admin") ? navigation.admin : []),
    ];

    const matchingItem = allNavItems.find((item) => item.url === currentPath);
    return matchingItem?.title || "Current Page";
  };

  const isOnDashboard = location.pathname === "/dashboard";

  if (loading) {
    return (
      <div className="flex h-screen items-center justify-center text-muted-foreground">
        Checking your session...
      </div>
    );
  }

  const handleLogout = () => {
    logoutMutation.mutate({});

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

  const userInitials = user?.email
    ? user.email
        .split("@")[0]
        ?.split(".")
        .map((n) => n[0])
        .join("")
        .toUpperCase()
        .slice(0, 2)
    : "??";

  return (
    <SidebarProvider>
      <Sidebar collapsible="icon">
        <SidebarHeader>
          <SidebarMenu>
            <SidebarMenuItem>
              <SidebarMenuButton size="lg" asChild>
                <Link to="/dashboard">
                  <div className="flex aspect-square size-8 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground">
                    <Package className="size-4" />
                  </div>
                  <div className="grid flex-1 text-left text-sm leading-tight">
                    <span className="truncate font-semibold">ProcureHub</span>
                    <span className="truncate text-xs">Procurement System</span>
                  </div>
                </Link>
              </SidebarMenuButton>
            </SidebarMenuItem>
          </SidebarMenu>
        </SidebarHeader>

        <SidebarContent>
          <SidebarGroup>
            <SidebarGroupLabel>Overview</SidebarGroupLabel>
            <SidebarMenu>
              {navigation.main.map((item) => (
                <SidebarMenuItem key={item.title}>
                  <SidebarMenuButton asChild>
                    <Link to={item.url}>
                      <item.icon />
                      <span>{item.title}</span>
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroup>

          <SidebarGroup>
            <SidebarGroupLabel>Request Management</SidebarGroupLabel>
            <SidebarMenu>
              {navigation.requests.map((item) => (
                <SidebarMenuItem key={item.title}>
                  <SidebarMenuButton asChild>
                    <Link to={item.url}>
                      <item.icon />
                      <span>{item.title}</span>
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroup>

          {hasRole("Approver") && (
            <SidebarGroup>
              <SidebarGroupLabel>Approvals</SidebarGroupLabel>
              <SidebarMenu>
                {navigation.approvals.map((item) => (
                  <SidebarMenuItem key={item.title}>
                    <SidebarMenuButton asChild>
                      <Link to={item.url}>
                        <item.icon />
                        <span>{item.title}</span>
                      </Link>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                ))}
              </SidebarMenu>
            </SidebarGroup>
          )}

          {hasRole("Admin") && (
            <SidebarGroup>
              <SidebarGroupLabel>Administration</SidebarGroupLabel>
              <SidebarMenu>
                {navigation.admin.map((item) => (
                  <SidebarMenuItem key={item.title}>
                    <SidebarMenuButton asChild>
                      <Link to={item.url}>
                        <item.icon />
                        <span>{item.title}</span>
                      </Link>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                ))}
              </SidebarMenu>
            </SidebarGroup>
          )}
        </SidebarContent>
        <SidebarRail />
      </Sidebar>

      <SidebarInset>
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
              {!isOnDashboard && (
                <>
                  <BreadcrumbSeparator className="hidden md:block" />
                  <BreadcrumbItem>
                    <BreadcrumbPage>{getCurrentPageTitle()}</BreadcrumbPage>
                  </BreadcrumbItem>
                </>
              )}
            </BreadcrumbList>
          </Breadcrumb>
          <div className="ml-auto flex items-center gap-2">
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <button className="flex items-center gap-2 rounded-lg border border-input bg-background px-2 py-1.5 hover:bg-accent hover:text-accent-foreground">
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
                <DropdownMenuItem onClick={handleLogout}>
                  <LogOut className="mr-2 h-4 w-4" />
                  Log out
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
            {demoUsers && Array.isArray(demoUsers) && (
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
                      onClick={() => handleDemoLogin(demoUser.email)}
                      disabled={
                        demoLoginMutation.isPending ||
                        user?.email === demoUser.email
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
        <div className="flex flex-1 flex-col gap-4 py-4 px-6">
          <Outlet />
        </div>
      </SidebarInset>
    </SidebarProvider>
  );
}
