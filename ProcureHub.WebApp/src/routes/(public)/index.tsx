import { useQuery } from "@tanstack/react-query";
import { createFileRoute } from "@tanstack/react-router";
import * as React from "react";

export const Route = createFileRoute("/(public)/")({
  component: HomeComponent,
});

function HomeComponent() {
  const { data, isPending, error } = useQuery({
    queryKey: ["test"],
    queryFn: () => fetch("/api/test").then((r) => r.json()),
  });

  if (isPending) return <div className="p-2">Loading...</div>;
  if (error) return <div className="p-2">Error: {error.message}</div>;

  return (
    <div className="p-2">
      <h3>Welcome Home!</h3>
      <p>The time is: {JSON.stringify(data)}</p>
    </div>
  );
}
