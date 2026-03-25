import React from "react";
import { useLanguage } from "../../App";

export default function Footer() {
  const { t } = useLanguage();
  const currentYear = new Date().getFullYear();

  return (
    <footer className="fixed bottom-0 left-0 right-0 bg-slate-900 dark:bg-slate-950 border-t border-slate-700 py-2 px-6 z-40">
      <div className="max-w-full mx-auto">
        <div className="flex flex-col md:flex-row justify-between items-center gap-2 text-xs">
          <p className="text-slate-400">
            {t("footerCopyright").replace("{year}", currentYear)}
          </p>
          
          <div className="flex flex-wrap justify-center gap-4 text-slate-400">
            <a href="/" className="hover:text-blue-400 transition-colors">
              {t("footerLegalPrivacy")}
            </a>
            <span className="text-slate-600">•</span>
            <a href="/" className="hover:text-blue-400 transition-colors">
              {t("footerLegalTerms")}
            </a>
            <span className="text-slate-600">•</span>
            <a href="/" className="hover:text-blue-400 transition-colors">
              {t("footerLegalCookies")}
            </a>
            <span className="text-slate-600">•</span>
            <a href="/" className="hover:text-blue-400 transition-colors">
              {t("home")}
            </a>
          </div>

          <div className="flex gap-3">
            <a
              href="https://www.facebook.com"
              className="text-slate-400 hover:text-blue-400 transition-colors"
              title="Facebook"
              target="_blank"
              rel="noreferrer"
            >
              <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
                <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z" />
              </svg>
            </a>
            <a
              href="https://x.com"
              className="text-slate-400 hover:text-blue-400 transition-colors"
              title="Twitter"
              target="_blank"
              rel="noreferrer"
            >
              <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
                <path d="M8.29 20a11.08 11.08 0 0011.08-11.08 7.72 7.72 0 00-.24-1.62A7.9 7.9 0 0020 6.18a7.81 7.81 0 01-2.26.62 3.95 3.95 0 001.73-2.18 7.87 7.87 0 01-2.5.95 3.94 3.94 0 00-6.82 3.59 11.18 11.18 0 01-8.11-4.12 3.94 3.94 0 001.22 5.26 3.93 3.93 0 01-1.79-.49v.05a3.94 3.94 0 003.16 3.86 3.88 3.88 0 01-1.78.07 3.95 3.95 0 003.68 2.74A7.9 7.9 0 010 17.33 11.09 11.09 0 008.29 20" />
              </svg>
            </a>
          </div>
        </div>
      </div>
    </footer>
  );
}

