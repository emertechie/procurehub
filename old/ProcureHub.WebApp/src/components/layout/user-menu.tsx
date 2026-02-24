import * as React from "react";
import { BadgeCheck, Bell, ChevronsUpDown, LogOut } from "lucide-react";

import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
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
import type { AuthUser } from "@/features/auth/types";

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

interface UserMenuProps {
  user: AuthUser | null;
  onLogout: () => void;
}

export function UserMenu({ user, onLogout }: UserMenuProps) {
  const userInitials = getUserInitials(user?.email);

  return (
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
            <span className="truncate font-semibold">{user?.firstName}</span>
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
                {user?.firstName} {user?.lastName}
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
  );
}
