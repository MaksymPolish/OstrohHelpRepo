import React, { useEffect } from "react";
import { useLanguage } from "../App";

export default function CookiePolicy() {
  const { t } = useLanguage();

  useEffect(() => {
    const prevTitle = document.title;
    document.title = `${t("cookiesTitle")} - OA Mind Care`;

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

    upsertMeta("name", "description", t("cookiesIntro"));
    upsertMeta("property", "og:title", t("cookiesTitle"));
    upsertMeta("property", "og:description", t("cookiesIntro"));
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
      <h1>{t("cookiesTitle")}</h1>
      <p>{t("cookiesIntro")}</p>

      <p>{t("cookiesWhatAre")}</p>

      <h2>{t("cookiesTypesTitle")}</h2>
      <ul>
        <li>{t("cookiesFunctional")}</li>
        <li>{t("cookiesAnalytical")}</li>
        <li>{t("cookiesThirdParty")}</li>
      </ul>

      <h2>{t("cookiesManage")}</h2>
      <p>{t("cookiesManage")}</p>

      <h2>Contact</h2>
      <p>
        <a href="mailto:privacy@ostrohhelp.example">{t("cookiesContact")}</a>
      </p>
    </div>
  );
}
