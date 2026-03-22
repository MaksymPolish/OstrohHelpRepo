import React from "react";

export default function Card({ children, className = "" }) {
  return (
    <div
      className={`bg-white dark:bg-slate-800 rounded-2xl shadow-sm border border-slate-100 dark:border-slate-700 overflow-hidden ${className}`}
    >
      {children}
    </div>
  );
}
