import React from "react";
import { Link } from "react-router-dom";
import Card from "../components/Common/Card";
import Button from "../components/Common/Button";
import { useLanguage } from "../App";

export default function NotFoundPage() {
  const { t } = useLanguage();

  return (
    <div className="min-h-[calc(100vh-14rem)] flex items-center justify-center">
      <Card className="max-w-xl w-full text-center p-10">
        <p className="text-blue-600 dark:text-blue-400 font-semibold text-sm tracking-wide uppercase">
          404
        </p>
        <h1 className="mt-2 text-3xl font-bold text-slate-900 dark:text-white">
          {t("notFoundTitle")}
        </h1>
        <p className="mt-3 text-slate-600 dark:text-slate-400">
          {t("notFoundDescription")}
        </p>

        <div className="mt-8">
          <Link to="/">
            <Button>{t("goHome")}</Button>
          </Link>
        </div>
      </Card>
    </div>
  );
}
