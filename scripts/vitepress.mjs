import { spawnSync } from 'node:child_process'
import { createHash } from 'node:crypto'
import {
  copyFileSync,
  cpSync,
  existsSync,
  lstatSync,
  mkdirSync,
  rmSync,
  symlinkSync
} from 'node:fs'
import { tmpdir } from 'node:os'
import path from 'node:path'

const command = process.argv[2] ?? 'dev'
const extraArgs = process.argv.slice(3)

const cwd = process.cwd()
const preparedCwd = prepareWorkingDirectory(cwd, command)
const runCwd = preparedCwd.cwd
const vitepressBin = path.join(
  runCwd,
  'node_modules',
  'vitepress',
  'bin',
  'vitepress.js'
)
const nodeArgs = [
  ...preparedCwd.nodeOptions,
  vitepressBin,
  command,
  'docs',
  ...extraArgs
]

let result

try {
  result = spawnSync(
    process.execPath,
    nodeArgs,
    {
      cwd: runCwd,
      env: process.env,
      stdio: 'inherit'
    }
  )
} finally {
  preparedCwd.cleanup()
}

if (typeof result.status === 'number') {
  process.exit(result.status)
}

if (result.error) {
  console.error(result.error.message)
}

process.exit(1)

function prepareWorkingDirectory(target, vitepressCommand) {
  if (!target.includes('#')) {
    return {
      cwd: target,
      nodeOptions: [],
      cleanup() {}
    }
  }

  if (process.platform === 'win32' && vitepressCommand === 'dev') {
    return ensureDevWorkspaceWithoutHash(target)
  }

  return {
    cwd: ensureJunctionWithoutHash(target),
    nodeOptions: [],
    cleanup() {}
  }
}

function ensureDevWorkspaceWithoutHash(target) {
  const hash = createHash('sha256').update(target).digest('hex').slice(0, 16)
  const workspace = path.join(tmpdir(), 'iskypro-vitepress-dev', hash)

  mkdirSync(workspace, { recursive: true })

  for (const file of ['package.json', 'pnpm-lock.yaml', 'pnpm-workspace.yaml']) {
    copyFileSync(path.join(target, file), path.join(workspace, file))
  }

  const workspaceDocs = path.join(workspace, 'docs')
  rmSync(workspaceDocs, { force: true, recursive: true })
  cpSync(path.join(target, 'docs'), workspaceDocs, {
    recursive: true,
    filter: shouldCopyDocsFile
  })

  console.warn(
    `[vitepress-runner] Path contains "#"; synced docs to ${workspace}. Restart dev after editing docs.`
  )
  runPnpmInstall(workspace)

  return {
    cwd: workspace,
    nodeOptions: [],
    cleanup() {}
  }
}

function shouldCopyDocsFile(source) {
  const normalized = source.split(path.sep).join('/')
  return !normalized.includes('/docs/.vitepress/cache') &&
    !normalized.includes('/docs/.vitepress/dist')
}

function runPnpmInstall(target) {
  const pnpmExec = process.env.npm_execpath
  let install

  if (pnpmExec && existsSync(pnpmExec)) {
    install = spawnSync(process.execPath, [pnpmExec, 'install', '--frozen-lockfile'], {
      cwd: target,
      env: process.env,
      stdio: 'inherit'
    })
  } else if (process.platform === 'win32') {
    install = spawnSync('cmd.exe', ['/d', '/s', '/c', 'pnpm install --frozen-lockfile'], {
      cwd: target,
      env: process.env,
      stdio: 'inherit'
    })
  } else {
    install = spawnSync('pnpm', ['install', '--frozen-lockfile'], {
      cwd: target,
      env: process.env,
      stdio: 'inherit'
    })
  }

  if (install.status !== 0) {
    throw new Error('pnpm install --frozen-lockfile failed in the VitePress dev workspace.')
  }
}

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
