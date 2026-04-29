import React, { useEffect } from "react";
import { useLanguage } from "../App";

export default function TermsOfUse() {
  const { t } = useLanguage();

  useEffect(() => {
    const prevTitle = document.title;
    document.title = `${t("termsTitle")} - OA Mind Care`;

    const created = [];

    const upsertMeta = (keyAttr, keyName, content) => {
      const selector = `meta[${keyAttr}="${keyName}"]`;
      const existing = document.querySelector(selector);
      if (existing) {
        const prev = existing.getAttribute("content");
        existing.setAttribute("content", content);
        created.push({ el: existing, prev });
        return;
      }
      const meta = document.createElement("meta");
      meta.setAttribute(keyAttr, keyName);
      meta.setAttribute("content", content);
      document.head.appendChild(meta);
      created.push({ el: meta, prev: null });
    };

    upsertMeta("name", "description", t("termsIntro"));
    upsertMeta("property", "og:title", t("termsTitle"));
    upsertMeta("property", "og:description", t("termsIntro"));
    upsertMeta("property", "og:type", "article");

    return () => {
      document.title = prevTitle;
      for (const item of created) {
        if (!item.prev) item.el.parentNode && item.el.parentNode.removeChild(item.el);
        else item.el.setAttribute("content", item.prev);
      }
    };
  }, [t]);

  return (
    <div className="prose max-w-none text-slate-800 dark:text-slate-200">
      <h1>{t("termsTitle")}</h1>
      <p>{t("termsIntro")}</p>

      <p>{t("termsDescription")}</p>

      <h2>{t("termsUserConductHeading")}</h2>
      <ul>
        <li>{t("termsUserConduct1")}</li>
        <li>{t("termsUserConduct2")}</li>
        <li>{t("termsUserConduct3")}</li>
      </ul>

      <h2>{t("termsLiabilityHeading")}</h2>
      <p>{t("termsLiabilityText")}</p>

      <h2>{t("termsSuspensionHeading")}</h2>
      <p>{t("termsSuspensionText")}</p>

      <h2>{t("termsModificationsHeading")}</h2>
      <p>{t("termsModificationsText")}</p>
    </div>
  );
}
