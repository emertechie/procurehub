export function getRoleBadgeClasses(role: string): string {
  const lowerRole = role.toLowerCase();
  if (lowerRole.includes("manager") || lowerRole.includes("approver")) {
    return "border-green-300 bg-green-50 text-green-700";
  }
  if (lowerRole.includes("admin") || lowerRole.includes("administrator")) {
    return "border-amber-300 bg-amber-50 text-amber-700";
  }
  return "border-gray-300 bg-gray-100 text-gray-700";
}
