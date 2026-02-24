import * as React from "react";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";

interface UserAvatarProps {
  firstName: string;
  lastName: string;
  email: string;
}

function getInitials(firstName: string, lastName: string): string {
  return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase();
}

export function UserAvatar({ firstName, lastName, email }: UserAvatarProps) {
  const fullName = `${firstName} ${lastName}`;
  return (
    <div className="flex items-center gap-2">
      <Avatar className="h-8 w-8">
        <AvatarFallback className="bg-primary/10 text-primary text-xs font-medium">
          {getInitials(firstName, lastName)}
        </AvatarFallback>
      </Avatar>
      <div className="flex flex-col">
        <span className="text-sm font-medium leading-tight">{fullName}</span>
        <span className="text-xs text-muted-foreground leading-tight">
          {email}
        </span>
      </div>
    </div>
  );
}
