import React from "react";

export default function Badge({ children, status = "neutral" }) {
  const styles = {
    online:
      "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400",
    offline:
      "bg-slate-100 text-slate-700 dark:bg-slate-700 dark:text-slate-300",
    pending:
      "bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400",
    neutral:
      "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400",
    review:
      "bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400",
  };

  return (
    <span
      className={`px-2.5 py-0.5 rounded-full text-xs font-medium ${styles[status]}`}
    >
      {children}
    </span>
  );
}
