import {
  Home,
  FileText,
  FilePlus,
  CheckCircle,
  Users,
  Building2,
  type LucideIcon,
} from "lucide-react";
import { getRequestsAreaTitle } from "@/features/purchase-requests/utils/navigation";

export interface NavItem {
  title: string;
  url: string;
  icon: LucideIcon;
}

export interface Navigation {
  main: NavItem[];
  requests: NavItem[];
  approvals: NavItem[];
  admin: NavItem[];
}

export function getNavigation(hasRole: (role: string) => boolean): Navigation {
  const requestsTitle = getRequestsAreaTitle(hasRole);

  const requestsNav: NavItem[] = [
    {
      title: requestsTitle,
      url: "/requests",
      icon: FileText,
    },
  ];

  // Only show "New Request" link to users with Requester role
  if (hasRole("Requester")) {
    requestsNav.push({
      title: "New Request",
      url: "/requests/new",
      icon: FilePlus,
    });
  }

  return {
    main: [
      {
        title: "Home",
        url: "/dashboard",
        icon: Home,
      },
    ],
    requests: requestsNav,
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
}
