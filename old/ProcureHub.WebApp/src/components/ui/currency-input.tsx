import * as React from "react";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";

export interface CurrencyInputProps extends Omit<
  React.ComponentProps<"input">,
  "type"
> {
  currency?: string;
  currencySymbol?: string;
}

const CurrencyInput = React.forwardRef<HTMLInputElement, CurrencyInputProps>(
  ({ className, currency = "EUR", currencySymbol = "€", ...props }, ref) => {
    return (
      <div className="relative">
        <span
          className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground"
          aria-label={currency}
        >
          {currencySymbol}
        </span>
        <Input
          type="number"
          step="0.01"
          min="0"
          className={cn("pl-7", className)}
          ref={ref}
          {...props}
        />
      </div>
    );
  },
);
CurrencyInput.displayName = "CurrencyInput";

export { CurrencyInput };
