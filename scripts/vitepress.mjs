import { spawnSync } from 'node:child_process'
import { createHash } from 'node:crypto'
import { existsSync, lstatSync, mkdirSync, rmSync, symlinkSync } from 'node:fs'
import { tmpdir } from 'node:os'
import path from 'node:path'

const command = process.argv[2] ?? 'dev'
const extraArgs = process.argv.slice(3)

const cwd = process.cwd()
const runCwd = cwd.includes('#') ? ensureJunctionWithoutHash(cwd) : cwd
const vitepressBin = path.join(
  runCwd,
  'node_modules',
  'vitepress',
  'bin',
  'vitepress.js'
)

const result = spawnSync(
  process.execPath,
  [vitepressBin, command, 'docs', ...extraArgs],
  {
    cwd: runCwd,
    env: process.env,
    stdio: 'inherit'
  }
)

if (typeof result.status === 'number') {
  process.exit(result.status)
}

if (result.error) {
  console.error(result.error.message)
}

process.exit(1)

function ensureJunctionWithoutHash(target) {
  const root = path.join(tmpdir(), 'iskypro-vitepress-links')
  const hash = createHash('sha256').update(target).digest('hex').slice(0, 16)
  const link = path.join(root, hash)

  mkdirSync(root, { recursive: true })

  if (existsSync(link)) {
    const stat = lstatSync(link)
    if (!stat.isSymbolicLink()) {
      throw new Error(`Refusing to replace non-link path: ${link}`)
    }

    rmSync(link, { force: true, recursive: true })
  }

  symlinkSync(target, link, process.platform === 'win32' ? 'junction' : 'dir')
  return link
}
