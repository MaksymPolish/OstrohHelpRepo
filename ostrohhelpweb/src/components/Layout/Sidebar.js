import React from "react";
import { Brain } from "lucide-react";

export default function Sidebar({
  isOpen,
  onClose,
  navItems,
  currentView,
  onNavigate,
  onLogout,
}) {
  return (
    <>
      {/* Mobile Overlay */}
      {isOpen && (
        <div
          className="fixed inset-0 bg-slate-900/50 backdrop-blur-sm z-40 lg:hidden"
          onClick={onClose}
        />
      )}

      {/* Sidebar */}
      <aside
        className={`fixed top-0 left-0 z-50 h-screen w-64 bg-white dark:bg-slate-800 border-r border-slate-100 dark:border-slate-700 transform transition-transform duration-300 ease-in-out ${
          isOpen ? "translate-x-0" : "-translate-x-full lg:translate-x-0"
        }`}
      >
        <div className="h-full flex flex-col">
          {/* Logo */}
          <div className="p-6 flex items-center space-x-3">
            <div className="w-10 h-10 rounded-xl bg-blue-600 text-white flex items-center justify-center shadow-md">
              <Brain size={24} />
            </div>
            <span className="text-xl font-bold text-slate-800 dark:text-white tracking-tight">
              OA Mind Care
            </span>
          </div>

          {/* Navigation */}
          <nav className="flex-1 px-4 space-y-1.5 overflow-y-auto mt-4">
            {navItems.map((item) => {
              const Icon = item.icon;
              const isActive = currentView === item.id;
              return (
                <button
                  key={item.id}
                  onClick={() => {
                    onNavigate(item.id);
                    onClose();
                  }}
                  className={`w-full flex items-center space-x-3 px-4 py-3 rounded-xl group ${
                    isActive
                      ? "bg-blue-50 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400 font-semibold"
                      : "text-slate-600 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-slate-800 hover:text-slate-900 dark:hover:text-white"
                  }`}
                >
                  {Icon && typeof Icon === 'function' ? (
                    <Icon
                      size={20}
                      className={
                        isActive
                          ? "text-blue-600 dark:text-blue-400"
                          : "text-slate-400 group-hover:text-slate-600 dark:group-hover:text-slate-300"
                      }
                    />
                  ) : null}
                  <span>{item.label}</span>
                  {isActive && (
                    <div className="ml-auto w-1.5 h-5 bg-blue-600 dark:bg-blue-400 rounded-full"></div>
                  )}
                </button>
              );
            })}
          </nav>
        </div>
      </aside>
    </>
  );
}
