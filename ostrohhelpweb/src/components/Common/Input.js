import React from "react";

export default function Input({
  label,
  id,
  type = "text",
  placeholder,
  className = "",
  value,
  onChange,
  disabled = false,
  ...props
}) {
  return (
    <div className={`flex flex-col space-y-1.5 ${className}`}>
      {label && (
        <label
          htmlFor={id}
          className="text-sm font-medium text-slate-700 dark:text-slate-300"
        >
          {label}
        </label>
      )}
      <input
        id={id}
        type={type}
        placeholder={placeholder}
        value={value}
        onChange={onChange}
        disabled={disabled}
        className="px-4 py-2.5 rounded-xl border border-slate-200 dark:border-slate-600 bg-white dark:bg-slate-900 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
        {...props}
      />
    </div>
  );
}
