const DB_NAME = "OstrohHelpCacheDB";
const DB_VERSION = 1;
const API_STORE = "api-cache";
const IMAGE_STORE = "image-cache";

let dbPromise = null;

const initDB = () => {
  if (!dbPromise) {
    dbPromise = new Promise((resolve, reject) => {
      const request = indexedDB.open(DB_NAME, DB_VERSION);

      request.onupgradeneeded = (event) => {
        const db = event.target.result;
        if (!db.objectStoreNames.contains(API_STORE)) {
          db.createObjectStore(API_STORE);
        }
        if (!db.objectStoreNames.contains(IMAGE_STORE)) {
          db.createObjectStore(IMAGE_STORE);
        }
      };

      request.onsuccess = (event) => resolve(event.target.result);
      request.onerror = (event) => reject(event.target.error);
    });
  }
  return dbPromise;
};

const getFromStore = async (storeName, key) => {
  try {
    const db = await initDB();
    return new Promise((resolve, reject) => {
      const transaction = db.transaction(storeName, "readonly");
      const store = transaction.objectStore(storeName);
      const request = store.get(key);

      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error);
    });
  } catch (error) {
    console.error(`Error reading from ${storeName}:`, error);
    return null;
  }
};

const setToStore = async (storeName, key, value) => {
  try {
    const db = await initDB();
    return new Promise((resolve, reject) => {
      const transaction = db.transaction(storeName, "readwrite");
      const store = transaction.objectStore(storeName);
      const request = store.put(value, key);

      request.onsuccess = () => resolve();
      request.onerror = () => reject(request.error);
    });
  } catch (error) {
    console.error(`Error writing to ${storeName}:`, error);
  }
};

// API Cache Helpers
export const getCachedConsultations = (userId) => getFromStore(API_STORE, `consultations_${userId}`);
export const setCachedConsultations = (userId, data) => setToStore(API_STORE, `consultations_${userId}`, data);

export const getCachedMessages = (consultationId) => getFromStore(API_STORE, `messages_${consultationId}`);
export const setCachedMessages = (consultationId, data) => setToStore(API_STORE, `messages_${consultationId}`, data);

// Image Cache Helpers
export const getCachedImageBlob = (url) => getFromStore(IMAGE_STORE, url);
export const setCachedImageBlob = (url, blob) => setToStore(IMAGE_STORE, url, blob);
