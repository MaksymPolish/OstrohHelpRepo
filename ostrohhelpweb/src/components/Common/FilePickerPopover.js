import React from "react";
import { Paperclip } from "lucide-react";

const IMAGE_TYPES = {
  "image/jpeg": [".jpg", ".jpeg"],
  "image/png": [".png"],
  "image/gif": [".gif"],
  "image/webp": [".webp"],
  "image/bmp": [".bmp"],
};

const VIDEO_TYPES = {
  "video/mp4": [".mp4", ".m4v"],
  "video/webm": [".webm"],
  "video/x-msvideo": [".avi"],
  "video/quicktime": [".mov"],
  "video/x-matroska": [".mkv"],
  "video/x-flv": [".flv"],
};

const DOCUMENT_TYPES = {
  "application/pdf": [".pdf"],
  "application/msword": [".doc"],
  "application/vnd.openxmlformats-officedocument.wordprocessingml.document": [".docx"],
  "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet": [".xlsx"],
  "application/vnd.openxmlformats-officedocument.presentationml.presentation": [".pptx"],
  "text/plain": [".txt"],
  "application/zip": [".zip"],
};

const ALL_TYPES = { ...IMAGE_TYPES, ...VIDEO_TYPES, ...DOCUMENT_TYPES };

const FILE_PICKER_TYPES = [
  {
    description: "Усі (Усі дозволені формати)",
    accept: ALL_TYPES,
  },
  {
    description: "Зображення (дозволені формати)",
    accept: IMAGE_TYPES,
  },
  {
    description: "Відео (дозволені формати)",
    accept: VIDEO_TYPES,
  },
  {
    description: "Файли (дозволені формати)",
    accept: DOCUMENT_TYPES,
  },
];

const fallbackAccept = Object.values(ALL_TYPES).flat().join(",");

async function openNativeFilePicker({ multiple, onFilesSelected }) {
  if (typeof window !== "undefined" && typeof window.showOpenFilePicker === "function") {
    const handles = await window.showOpenFilePicker({
      multiple,
      excludeAcceptAllOption: true,
      types: FILE_PICKER_TYPES,
    });

    const files = [];
    for (const handle of handles) {
      files.push(await handle.getFile());
    }

    if (typeof onFilesSelected === "function") {
      onFilesSelected(files);
    }
    return;
  }

  const input = document.createElement("input");
  input.type = "file";
  input.multiple = multiple;
  input.accept = fallbackAccept;
  input.style.display = "none";
  document.body.appendChild(input);

  input.addEventListener("change", () => {
    const files = Array.from(input.files || []);
    if (typeof onFilesSelected === "function") {
      onFilesSelected(files);
    }
    document.body.removeChild(input);
  }, { once: true });

  input.click();
}

export default function FilePickerPopover({ onFilesSelected, multiple = true }) {
  const handleClick = async () => {
    try {
      await openNativeFilePicker({ multiple, onFilesSelected });
    } catch (error) {
      if (error?.name === "AbortError") {
        // Користувач просто закрив вікно вибору, це нормально
        return;
      }
      console.error("Failed to open file picker", error);
    }
  };

  return (
    <button
      type="button"
      onClick={handleClick}
      className="p-2 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 transition-colors"
      aria-label="Додати файл"
      title="Додати файл"
    >
      <Paperclip size={20} />
    </button>
  );
}