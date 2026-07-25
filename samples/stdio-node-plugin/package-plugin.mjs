import { access, readFile, readdir, mkdir, rm, writeFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { deflateRawSync } from "node:zlib";

const root = path.dirname(fileURLToPath(import.meta.url));
const artifacts = path.join(root, "artifacts");

async function pathExists(candidate) {
  try {
    await access(candidate);
    return true;
  } catch {
    return false;
  }
}

async function resolveSdkRoot() {
  if (process.env.ISKYPRO_NODE_SDK_PATH) {
    return path.resolve(process.env.ISKYPRO_NODE_SDK_PATH);
  }

  const installed = path.join(root, "node_modules/@iskypro/plugin-sdk-v2");
  if (await pathExists(installed)) {
    return installed;
  }

  const repository = path.resolve(root, "../../sdk/node/iskypro-sdk-v2");
  if (await pathExists(repository)) {
    return repository;
  }

  throw new Error(
    "Node.js SDK package not found. Install @iskypro/plugin-sdk-v2 or set ISKYPRO_NODE_SDK_PATH.",
  );
}

const crcTable = new Uint32Array(256);
for (let index = 0; index < crcTable.length; index += 1) {
  let value = index;
  for (let bit = 0; bit < 8; bit += 1) {
    value = (value & 1) !== 0 ? (value >>> 1) ^ 0xedb88320 : value >>> 1;
  }
  crcTable[index] = value >>> 0;
}

function crc32(data) {
  let value = 0xffffffff;
  for (const byte of data) {
    value = (value >>> 8) ^ crcTable[(value ^ byte) & 0xff];
  }
  return (value ^ 0xffffffff) >>> 0;
}

function dosTimestamp(date = new Date()) {
  const year = Math.max(1980, date.getFullYear());
  return {
    time: (date.getHours() << 11) | (date.getMinutes() << 5) | Math.floor(date.getSeconds() / 2),
    date: ((year - 1980) << 9) | ((date.getMonth() + 1) << 5) | date.getDate(),
  };
}

async function collectFiles(directory, archivePrefix) {
  const files = [];
  for (const entry of await readdir(directory, { withFileTypes: true })) {
    if (["bin", "obj", "artifacts", "node_modules"].includes(entry.name)) {
      continue;
    }
    const source = path.join(directory, entry.name);
    const archiveName = path.posix.join(archivePrefix, entry.name);
    if (entry.isDirectory()) {
      files.push(...await collectFiles(source, archiveName));
    } else if (entry.isFile()) {
      files.push({ name: archiveName, data: await readFile(source) });
    }
  }
  return files;
}

function createZip(files) {
  const localParts = [];
  const centralParts = [];
  let offset = 0;
  const timestamp = dosTimestamp();

  for (const file of files) {
    const name = Buffer.from(file.name.replaceAll("\\", "/"), "utf8");
    const compressed = deflateRawSync(file.data, { level: 9 });
    const checksum = crc32(file.data);
    const localHeader = Buffer.alloc(30);
    localHeader.writeUInt32LE(0x04034b50, 0);
    localHeader.writeUInt16LE(20, 4);
    localHeader.writeUInt16LE(0x0800, 6);
    localHeader.writeUInt16LE(8, 8);
    localHeader.writeUInt16LE(timestamp.time, 10);
    localHeader.writeUInt16LE(timestamp.date, 12);
    localHeader.writeUInt32LE(checksum, 14);
    localHeader.writeUInt32LE(compressed.length, 18);
    localHeader.writeUInt32LE(file.data.length, 22);
    localHeader.writeUInt16LE(name.length, 26);

    const centralHeader = Buffer.alloc(46);
    centralHeader.writeUInt32LE(0x02014b50, 0);
    centralHeader.writeUInt16LE(0x0314, 4);
    centralHeader.writeUInt16LE(20, 6);
    centralHeader.writeUInt16LE(0x0800, 8);
    centralHeader.writeUInt16LE(8, 10);
    centralHeader.writeUInt16LE(timestamp.time, 12);
    centralHeader.writeUInt16LE(timestamp.date, 14);
    centralHeader.writeUInt32LE(checksum, 16);
    centralHeader.writeUInt32LE(compressed.length, 20);
    centralHeader.writeUInt32LE(file.data.length, 24);
    centralHeader.writeUInt16LE(name.length, 28);
    centralHeader.writeUInt32LE((0o100644 * 0x10000) >>> 0, 38);
    centralHeader.writeUInt32LE(offset, 42);

    localParts.push(localHeader, name, compressed);
    centralParts.push(centralHeader, name);
    offset += localHeader.length + name.length + compressed.length;
  }

  const centralDirectory = Buffer.concat(centralParts);
  const end = Buffer.alloc(22);
  end.writeUInt32LE(0x06054b50, 0);
  end.writeUInt16LE(files.length, 8);
  end.writeUInt16LE(files.length, 10);
  end.writeUInt32LE(centralDirectory.length, 12);
  end.writeUInt32LE(offset, 16);
  return Buffer.concat([...localParts, centralDirectory, end]);
}

const manifest = JSON.parse(await readFile(path.join(root, "manifest.json"), "utf8"));
if (typeof manifest.pluginId !== "string" || manifest.pluginId.trim().length === 0) {
  throw new Error("manifest.json must contain pluginId");
}

const sdkRoot = await resolveSdkRoot();

const files = [
  { name: "manifest.json", data: await readFile(path.join(root, "manifest.json")) },
  { name: "plugin.mjs", data: await readFile(path.join(root, "plugin.mjs")) },
  { name: "package.json", data: await readFile(path.join(root, "package.json")) },
  ...await collectFiles(sdkRoot, "node_modules/@iskypro/plugin-sdk-v2"),
];
try {
  files.push({ name: "README.md", data: await readFile(path.join(root, "README.md")) });
} catch (error) {
  if (error?.code !== "ENOENT") throw error;
}

await mkdir(artifacts, { recursive: true });
const archivePath = path.join(artifacts, `${manifest.pluginId}.zip`);
await rm(archivePath, { force: true });
await writeFile(archivePath, createZip(files));
console.log(`ISkyPro plugin package: ${archivePath}`);
