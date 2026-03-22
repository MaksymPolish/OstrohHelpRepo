import React from "react";
import { X } from "lucide-react";
import Button from "./Button";

export default function Modal({ isOpen, onClose, title, children, actions }) {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-sm animate-in fade-in duration-200">
      <div className="bg-white dark:bg-slate-800 rounded-2xl shadow-xl w-full max-w-md overflow-hidden transform transition-all">
        <div className="px-6 py-4 border-b border-slate-100 dark:border-slate-700 flex justify-between items-center">
          <h3 className="text-lg font-semibold text-slate-900 dark:text-white">
            {title}
          </h3>
          <button
            onClick={onClose}
            className="text-slate-400 hover:text-slate-500 dark:hover:text-slate-300 transition-colors"
          >
            <X size={20} />
          </button>
        </div>
        <div className="p-6">{children}</div>
        {actions && (
          <div className="px-6 py-4 bg-slate-50 dark:bg-slate-800/50 border-t border-slate-100 dark:border-slate-700 flex justify-end space-x-3">
            {actions}
          </div>
        )}
      </div>
    </div>
  );
}
