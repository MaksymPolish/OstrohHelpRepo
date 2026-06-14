import React, { useState, useEffect } from "react";
import { getCachedImageBlob, setCachedImageBlob } from "../../services/cacheService";

export default function CachedImage({ src, alt, className, ...props }) {
  const [imgSrc, setImgSrc] = useState(null);

  useEffect(() => {
    let isMounted = true;
    let objectUrl = null;

    const loadImage = async () => {
      if (!src) return;

      try {
        // Try to get from IndexedDB cache
        const cachedBlob = await getCachedImageBlob(src);
        if (cachedBlob && isMounted) {
          objectUrl = URL.createObjectURL(cachedBlob);
          setImgSrc(objectUrl);
          return;
        }

        // Fetch from network
        const response = await fetch(src, { cache: "no-cache" }); // bypass browser cache for fresh fetch
        if (response.ok) {
          const blob = await response.blob();
          await setCachedImageBlob(src, blob);
          if (isMounted) {
            objectUrl = URL.createObjectURL(blob);
            setImgSrc(objectUrl);
          }
        } else {
          // Fallback to original src if fetch fails
          if (isMounted) setImgSrc(src);
        }
      } catch (err) {
        // CORS or network error, fallback to original src
        if (isMounted) setImgSrc(src);
      }
    };

    loadImage();

    return () => {
      isMounted = false;
      if (objectUrl) {
        URL.revokeObjectURL(objectUrl);
      }
    };
  }, [src]);

  return <img src={imgSrc || src} alt={alt} className={className} {...props} />;
}
