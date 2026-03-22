import React from "react";

export default function Button({
  children,
  onClick,
  variant = "primary",
  className = "",
  type = "button",
  disabled = false,
  size = "md",
  fullWidth = false,
  ...props
}) {
  const baseStyle =
    "inline-flex items-center justify-center px-4 py-2 rounded-xl font-medium transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed";

  const variants = {
    primary:
      "bg-blue-600 text-white hover:bg-blue-700 focus:ring-blue-500 shadow-sm dark:bg-blue-600 dark:hover:bg-blue-700",
    secondary:
      "bg-teal-500 text-white hover:bg-teal-600 focus:ring-teal-400 shadow-sm dark:bg-teal-500 dark:hover:bg-teal-600",
    outline:
      "border border-slate-300 dark:border-slate-600 text-slate-700 dark:text-slate-200 hover:bg-slate-50 dark:hover:bg-slate-800",
    ghost:
      "text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800",
    danger:
      "bg-red-500 text-white hover:bg-red-600 focus:ring-red-400 shadow-sm dark:bg-red-600 dark:hover:bg-red-700",
  };

  const sizeStyles = {
    sm: "px-3 py-1.5 text-sm",
    md: "px-4 py-2 text-base",
    lg: "px-6 py-3 text-lg",
  };

  return (
    <button
      type={type}
      onClick={onClick}
      className={`${baseStyle} ${variants[variant]} ${sizeStyles[size]} ${
        fullWidth ? "w-full" : ""
      } ${className}`}
      disabled={disabled}
      {...props}
    >
      {children}
    </button>
  );
}
