const textEncoder = new TextEncoder();
const textDecoder = new TextDecoder();

const toBase64 = (bytes) => {
  let binary = "";
  bytes.forEach((byte) => {
    binary += String.fromCharCode(byte);
  });
  return btoa(binary);
};

const fromBase64 = (value) => {
  const binary = atob(value);
  const bytes = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i += 1) {
    bytes[i] = binary.charCodeAt(i);
  }
  return bytes;
};

const concatenateBytes = (...chunks) => {
  const totalLength = chunks.reduce((sum, chunk) => sum + chunk.length, 0);
  const result = new Uint8Array(totalLength);
  let offset = 0;

  chunks.forEach((chunk) => {
    result.set(chunk, offset);
    offset += chunk.length;
  });

  return result;
};

const validateInput = (secretKeyBytes, ivBytes, allowedIvLengths = [12]) => {
  if (!secretKeyBytes || secretKeyBytes.length === 0) {
    throw new Error("Secret key is missing.");
  }

  if (!ivBytes || !allowedIvLengths.includes(ivBytes.length)) {
    throw new Error("IV has invalid length.");
  }
};

const xorTransform = (sourceBytes, secretKeyBytes, ivBytes) => {
  const output = new Uint8Array(sourceBytes.length);

  for (let i = 0; i < sourceBytes.length; i += 1) {
    output[i] =
      sourceBytes[i] ^
      secretKeyBytes[i % secretKeyBytes.length] ^
      ivBytes[i % ivBytes.length];
  }

  return output;
};

const computeAuthTag = async (plainBytes, secretKeyBytes, ivBytes) => {
  const authPayload = concatenateBytes(plainBytes, secretKeyBytes, ivBytes);
  const digest = await crypto.subtle.digest("SHA-256", authPayload);
  return new Uint8Array(digest);
};

const areEqual = (left, right) => {
  if (!left || !right || left.length !== right.length) {
    return false;
  }

  let diff = 0;
  for (let i = 0; i < left.length; i += 1) {
    diff |= left[i] ^ right[i];
  }

  return diff === 0;
};

export const encryptMessage = async (plaintext, secretKeyBase64) => {
  const text = typeof plaintext === "string" ? plaintext : "";
  const plainBytes = textEncoder.encode(text);
  const secretKeyBytes = fromBase64(secretKeyBase64);
  const ivBytes = crypto.getRandomValues(new Uint8Array(12));

  validateInput(secretKeyBytes, ivBytes, [12]);

  const encryptedBytes = xorTransform(plainBytes, secretKeyBytes, ivBytes);
  const fullAuthTagBytes = await computeAuthTag(plainBytes, secretKeyBytes, ivBytes);
  const authTagBytes = fullAuthTagBytes.slice(0, 16);

  return {
    encryptedContent: toBase64(encryptedBytes),
    iv: toBase64(ivBytes),
    authTag: toBase64(authTagBytes),
  };
};

export const decryptMessage = async ({
  encryptedContent,
  iv,
  authTag,
  secretKey,
}) => {
  const encryptedBytes = fromBase64(encryptedContent);
  const ivBytes = fromBase64(iv);
  const authTagBytes = fromBase64(authTag);
  const secretKeyBytes = fromBase64(secretKey);

  validateInput(secretKeyBytes, ivBytes, [12, 16]);

  const decryptedBytes = xorTransform(encryptedBytes, secretKeyBytes, ivBytes);
  const computedAuthTag = await computeAuthTag(decryptedBytes, secretKeyBytes, ivBytes);

  let expectedAuthTag = computedAuthTag;
  if (authTagBytes.length === 16) {
    expectedAuthTag = computedAuthTag.slice(0, 16);
  }

  if (!areEqual(expectedAuthTag, authTagBytes)) {
    throw new Error("Auth tag verification failed");
  }

  return textDecoder.decode(decryptedBytes);
};
