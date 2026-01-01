export function getRequestsAreaTitle(
  hasRole: (role: string) => boolean,
): string {
  if (hasRole("Admin")) {
    return "All Requests";
  }
  if (hasRole("Approver")) {
    return "Department Requests";
  }
  return "My Requests";
}
