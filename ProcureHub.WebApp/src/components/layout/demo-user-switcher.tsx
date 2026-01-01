import { ChevronsUpDown, Sparkles } from "lucide-react";

import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import type { AuthUser } from "@/features/auth/types";

interface DemoUser {
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

interface DemoUserSwitcherProps {
  user: AuthUser | null;
  demoUsers: DemoUser[];
  onDemoLogin: (email: string) => void;
  isDemoLoginPending?: boolean;
}

export function DemoUserSwitcher({
  user,
  demoUsers,
  onDemoLogin,
  isDemoLoginPending,
}: DemoUserSwitcherProps) {
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <button className="flex items-center gap-2 rounded-md border-2 border-amber-400 bg-amber-50 px-3 py-1.5 text-sm font-semibold text-amber-900 shadow-sm hover:bg-amber-100 hover:border-amber-500 transition-colors">
          <Sparkles className="h-4 w-4 text-amber-600" />
          <span>Switch Demo User</span>
          <ChevronsUpDown className="h-4 w-4" />
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
            disabled={isDemoLoginPending || user?.email === demoUser.email}
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
  );
}
