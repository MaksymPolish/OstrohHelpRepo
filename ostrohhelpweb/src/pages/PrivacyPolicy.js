import React, { useEffect } from "react";
import { useLanguage } from "../App";

export default function PrivacyPolicy() {
  const { t } = useLanguage();

  useEffect(() => {
    const prevTitle = document.title;
    document.title = `${t("privacyTitle")} - OA Mind Care`;

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

    upsertMeta("name", "description", t("privacyIntro"));
    upsertMeta("property", "og:title", t("privacyTitle"));
    upsertMeta("property", "og:description", t("privacyIntro"));
    upsertMeta("property", "og:type", "article");

    return () => {
      document.title = prevTitle;
      for (const item of created) {
        if (!item.prev) {
          // we created it
          item.el.parentNode && item.el.parentNode.removeChild(item.el);
        } else {
          item.el.setAttribute("content", item.prev);
        }
      }
    };
  }, [t]);

  return (
    <div className="prose max-w-none text-slate-800 dark:text-slate-200">
      <h1>{t("privacyTitle")}</h1>
      <p>{t("privacyIntro")}</p>

      <h2>{t("privacyInfoHeading")}</h2>

      <h3>{t("privacyAccountHeading")}</h3>
      <p>{t("privacyAccountText")}</p>

      <h3>{t("privacyCommunicationsHeading")}</h3>
      <p>{t("privacyCommunicationsText")}</p>

      <h3>{t("privacyUsageHeading")}</h3>
      <p>{t("privacyUsageText")}</p>

      <h2>{t("privacyHowHeading")}</h2>
      <ul>
        <li>{t("privacyUse1")}</li>
        <li>{t("privacyUse2")}</li>
        <li>{t("privacyUse3")}</li>
        <li>{t("privacyUse4")}</li>
      </ul>

      <h2>{t("privacyStorageHeading")}</h2>
      <p>{t("privacyStorageText")}</p>

      <h2>{t("privacyDeletionHeading")}</h2>
      <p>{t("privacyDeletionText")}</p>

      <h2>{t("privacyRightsHeading")}</h2>
      <ul>
        <li>{t("privacyRights1")}</li>
        <li>{t("privacyRights2")}</li>
        <li>{t("privacyRights3")}</li>
      </ul>

      <h2>Contact</h2>
      <p>
        <a href="mailto:privacy@ostrohhelp.example">{t("privacyContact")}</a>
      </p>
    </div>
  );
}
