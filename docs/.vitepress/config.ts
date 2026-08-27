import fs from 'node:fs'
import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vitepress'

const base = process.env.VITEPRESS_BASE ?? '/ISkyPro.Wiki/'
const docsDir = path.dirname(fileURLToPath(import.meta.url))

type SidebarItem = {
  text: string
  link: string
}

type ParsedVersion = {
  major: number
  minor: number
  patch: number
  prereleaseLabel: string
  prereleaseNumber: number
  hasPrerelease: boolean
}

function slugToVersion(fileName: string): string {
  const slug = fileName.replace(/\.md$/i, '').replace(/^v/i, '')
  const match = slug.match(/^(\d+)-(\d+)-(\d+)(?:-(.+))?$/)
  if (!match) {
    return slug.replace(/-/g, '.')
  }

  const core = `${match[1]}.${match[2]}.${match[3]}`
  if (!match[4]) {
    return core
  }

  // v2-0-0-preview-4 -> 2.0.0-preview.4
  const prerelease = match[4].replace(/-(\d+)$/, '.$1')
  return `${core}-${prerelease}`
}

function parseVersion(version: string): ParsedVersion {
  const [core, prerelease = ''] = version.split('-', 2)
  const [major = 0, minor = 0, patch = 0] = core.split('.').map((part) => Number.parseInt(part, 10) || 0)
  const prereleaseMatch = prerelease.match(/^([a-zA-Z]+)(?:\.?(\d+))?$/)

  return {
    major,
    minor,
    patch,
    prereleaseLabel: prereleaseMatch?.[1] ?? prerelease,
    prereleaseNumber: Number.parseInt(prereleaseMatch?.[2] ?? '0', 10) || 0,
    hasPrerelease: prerelease.length > 0
  }
}

function compareVersionsDesc(left: string, right: string): number {
  const a = parseVersion(left)
  const b = parseVersion(right)

  if (a.major !== b.major) {
    return b.major - a.major
  }

  if (a.minor !== b.minor) {
    return b.minor - a.minor
  }

  if (a.patch !== b.patch) {
    return b.patch - a.patch
  }

  // Stable release sorts above prereleases of the same core version.
  if (a.hasPrerelease !== b.hasPrerelease) {
    return a.hasPrerelease ? 1 : -1
  }

  if (a.prereleaseLabel !== b.prereleaseLabel) {
    return b.prereleaseLabel.localeCompare(a.prereleaseLabel)
  }

  return b.prereleaseNumber - a.prereleaseNumber
}

function buildChangelogSidebarItems(localePrefix: '' | '/en', subdir = ''): SidebarItem[] {
  const subPath = subdir ? `${subdir}/` : ''
  const relativeDir = localePrefix === '/en' ? `../en/changelog/${subPath}` : `../changelog/${subPath}`
  const changelogDir = path.resolve(docsDir, relativeDir)

  if (!fs.existsSync(changelogDir)) {
    return []
  }

  return fs
    .readdirSync(changelogDir)
    .filter((fileName) => /^v.+\.md$/i.test(fileName))
    .map((fileName) => {
      const slug = fileName.replace(/\.md$/i, '')
      const version = slugToVersion(fileName)
      return {
        text: version,
        link: `${localePrefix}/changelog/${subPath}${slug}`,
        version
      }
    })
    .sort((left, right) => compareVersionsDesc(left.version, right.version))
    .map(({ text, link }) => ({ text, link }))
}

// ---------------------------------------------------------------------------
// Top navigation: top-level sections switch the whole sidebar context.
// ---------------------------------------------------------------------------

const zhNav = [
  { text: '指南', link: '/guide/getting-started' },
  { text: '插件 SDK', link: '/plugins/' },
  { text: 'FAQ', link: '/faq' },
  { text: '更新日志', link: '/changelog/' },
  { text: 'GitHub', link: 'https://github.com/Zephdyn/ISkyPro.Wiki' }
]

const enNav = [
  { text: 'Guide', link: '/en/guide/getting-started' },
  { text: 'Plugin SDK', link: '/en/plugins/' },
  { text: 'FAQ', link: '/en/faq' },
  { text: 'Changelog', link: '/en/changelog/' },
  { text: 'GitHub', link: 'https://github.com/Zephdyn/ISkyPro.Wiki' }
]

// ---------------------------------------------------------------------------
// Sidebars: each section shows only its own tree on the left.
// ---------------------------------------------------------------------------

const zhHomeSidebar = [
  {
    text: '入门',
    items: [
      { text: '快速开始', link: '/guide/getting-started' },
      { text: 'QQBot 事件配置', link: '/guide/qqbot-events' },
      { text: 'Webhook 与反向代理', link: '/guide/webhook-and-proxy' }
    ]
  },
  {
    text: '插件开发',
    items: [
      { text: '插件 SDK', link: '/plugins/' },
      { text: 'SDK 下载', link: '/plugins/downloads' }
    ]
  },
  {
    text: '支持',
    items: [
      { text: 'FAQ', link: '/faq' },
      { text: '更新日志', link: '/changelog/' }
    ]
  }
]

const zhGuideSidebar = [
  {
    text: '指南',
    items: [
      { text: '快速开始', link: '/guide/getting-started' },
      { text: 'QQBot 事件配置', link: '/guide/qqbot-events' },
      { text: 'Webhook 与反向代理', link: '/guide/webhook-and-proxy' }
    ]
  }
]

const zhFaqSidebar = [
  {
    text: 'FAQ',
    items: [
      { text: 'WebUI token 在哪里看？', link: '/faq#webui-token-在哪里看' },
      { text: '应该选 WebSocket 还是 Webhook？', link: '/faq#应该选-websocket-还是-webhook' },
      { text: '为什么登录成功但收不到群消息？', link: '/faq#为什么登录成功但收不到群消息' },
      { text: 'Bot ID / AppID 和 Secret 在哪里获取？', link: '/faq#bot-id-appid-和-secret-在哪里获取' },
      { text: 'Webhook 回调地址应该填什么？', link: '/faq#webhook-回调地址应该填什么' },
      { text: 'ISky v1 插件和 ISkyPro v2+ 插件有什么区别？', link: '/faq#isky-v1-插件和-iskypro-v2-插件有什么区别' },
      { text: 'Linux 支持 ISky v1 插件吗？', link: '/faq#linux-支持-isky-v1-插件吗' },
      { text: '为什么 ISky v1 插件需要 Windows/x86 兼容宿主？', link: '/faq#为什么-isky-v1-插件需要-windows-x86-兼容宿主' },
      { text: 'ISkyPro v2+ 插件 ZIP 上传后为什么没有启动？', link: '/faq#iskypro-v2-插件-zip-上传后为什么没有启动' },
      { text: '更新检测失败是否影响运行？', link: '/faq#更新检测失败是否影响运行' },
      { text: '服务模式下为什么不会自动弹浏览器？', link: '/faq#服务模式下为什么不会自动弹浏览器' },
      { text: '如何从其他机器访问 WebUI？', link: '/faq#如何从其他机器访问-webui' },
      { text: '如何升级 ISkyPro？', link: '/faq#如何升级-iskypro' }
    ]
  }
]

const zhPluginSidebar = [
  {
    text: '插件开发',
    items: [
      { text: '插件概览', link: '/plugins/' },
      { text: 'ISky v1 与 ISkyPro v2+', link: '/plugins/v1-vs-v2' },
      { text: '快速实现', link: '/plugins/sdk-quick-start' },
      { text: '事件详解', link: '/plugins/events' },
      { text: '发布插件', link: '/plugins/publishing' },
      { text: '故障排查', link: '/plugins/troubleshooting' },
      { text: 'SDK 下载', link: '/plugins/downloads' },
      { text: 'SDK 更新日志', link: '/changelog/sdk/' }
    ]
  }
]

const zhChangelogSidebar = [
  {
    text: '更新日志',
    items: [
      { text: '更新日志', link: '/changelog/' },
      ...buildChangelogSidebarItems('')
    ]
  },
  {
    text: 'SDK 更新日志',
    items: [
      { text: 'SDK 更新日志', link: '/changelog/sdk/' },
      ...buildChangelogSidebarItems('', 'sdk')
    ]
  }
]

const zhSidebar = {
  '/guide/': zhGuideSidebar,
  '/plugins/': zhPluginSidebar,
  '/changelog/': zhChangelogSidebar,
  '/faq': zhFaqSidebar,
  '/': zhHomeSidebar
}

const enHomeSidebar = [
  {
    text: 'Getting Started',
    items: [
      { text: 'Getting Started', link: '/en/guide/getting-started' },
      { text: 'QQBot Events', link: '/en/guide/qqbot-events' },
      { text: 'Webhook and Proxy', link: '/en/guide/webhook-and-proxy' }
    ]
  },
  {
    text: 'Plugin Development',
    items: [
      { text: 'Plugin SDK', link: '/en/plugins/' },
      { text: 'SDK Downloads', link: '/en/plugins/downloads' }
    ]
  },
  {
    text: 'Support',
    items: [
      { text: 'FAQ', link: '/en/faq' },
      { text: 'Changelog', link: '/en/changelog/' }
    ]
  }
]

const enGuideSidebar = [
  {
    text: 'Guide',
    items: [
      { text: 'Getting Started', link: '/en/guide/getting-started' },
      { text: 'QQBot Events', link: '/en/guide/qqbot-events' },
      { text: 'Webhook and Proxy', link: '/en/guide/webhook-and-proxy' }
    ]
  }
]

const enFaqSidebar = [
  {
    text: 'FAQ',
    items: [
      { text: 'Where can I find the WebUI token?', link: '/en/faq#where-can-i-find-the-webui-token' },
      { text: 'Should I use WebSocket or Webhook?', link: '/en/faq#should-i-use-websocket-or-webhook' },
      { text: 'Why did login succeed but group messages are missing?', link: '/en/faq#why-did-login-succeed-but-group-messages-are-missing' },
      { text: 'Where do I get Bot ID / AppID and Secret?', link: '/en/faq#where-do-i-get-bot-id-appid-and-secret' },
      { text: 'What should the Webhook callback URL be?', link: '/en/faq#what-should-the-webhook-callback-url-be' },
      { text: 'What is the difference between ISky v1 and ISkyPro v2+ plugins?', link: '/en/faq#what-is-the-difference-between-isky-v1-and-iskypro-v2-plugins' },
      { text: 'Does Linux support ISky v1 plugins?', link: '/en/faq#does-linux-support-isky-v1-plugins' },
      { text: 'Why do ISky v1 plugins need a Windows/x86 compatibility host?', link: '/en/faq#why-do-isky-v1-plugins-need-a-windows-x86-compatibility-host' },
      { text: 'Why did a newly uploaded ISkyPro v2+ plugin not start?', link: '/en/faq#why-did-a-newly-uploaded-iskypro-v2-plugin-not-start' },
      { text: 'Does update-check failure affect runtime?', link: '/en/faq#does-update-check-failure-affect-runtime' },
      { text: 'Why does service mode not open a browser automatically?', link: '/en/faq#why-does-service-mode-not-open-a-browser-automatically' },
      { text: 'How do I access the WebUI from another machine?', link: '/en/faq#how-do-i-access-the-webui-from-another-machine' },
      { text: 'How do I upgrade ISkyPro?', link: '/en/faq#how-do-i-upgrade-iskypro' }
    ]
  }
]

const enPluginSidebar = [
  {
    text: 'Plugin Development',
    items: [
      { text: 'Overview', link: '/en/plugins/' },
      { text: 'ISky v1 and ISkyPro v2+', link: '/en/plugins/v1-vs-v2' },
      { text: 'Quick Start', link: '/en/plugins/sdk-quick-start' },
      { text: 'Events', link: '/en/plugins/events' },
      { text: 'Publishing', link: '/en/plugins/publishing' },
      { text: 'Troubleshooting', link: '/en/plugins/troubleshooting' },
      { text: 'SDK Downloads', link: '/en/plugins/downloads' },
      { text: 'SDK Changelog', link: '/en/changelog/sdk/' }
    ]
  }
]

const enChangelogSidebar = [
  {
    text: 'Changelog',
    items: [
      { text: 'Changelog', link: '/en/changelog/' },
      ...buildChangelogSidebarItems('/en')
    ]
  },
  {
    text: 'SDK Changelog',
    items: [
      { text: 'SDK Changelog', link: '/en/changelog/sdk/' },
      ...buildChangelogSidebarItems('/en', 'sdk')
    ]
  }
]

const enSidebar = {
  '/en/guide/': enGuideSidebar,
  '/en/plugins/': enPluginSidebar,
  '/en/changelog/': enChangelogSidebar,
  '/en/faq': enFaqSidebar,
  '/en/': enHomeSidebar
}

export default defineConfig({
  lang: 'zh-CN',
  title: 'ISkyPro Wiki',
  description: 'ISkyPro 使用、部署、插件开发和发布说明',
  base,
  cleanUrls: true,
  lastUpdated: true,
  head: [
    ['link', { rel: 'icon', href: `${base}ico/ISkyPro.ico` }],
    ['meta', { name: 'theme-color', content: '#256f5a' }]
  ],
  themeConfig: {
    logo: {
      src: '/assets/Icon.png',
      alt: 'ISkyPro'
    },
    search: {
      provider: 'local'
    }
  },
  locales: {
    root: {
      label: '简体中文',
      lang: 'zh-CN',
      title: 'ISkyPro Wiki',
      description: 'ISkyPro 使用、部署、插件开发和发布说明',
      themeConfig: {
        nav: zhNav,
        sidebar: zhSidebar,
        outline: {
          level: [2, 3],
          label: '本页目录'
        },
        docFooter: {
          prev: '上一页',
          next: '下一页'
        },
        lastUpdated: {
          text: '最后更新',
          formatOptions: {
            dateStyle: 'medium',
            timeStyle: 'short'
          }
        }
      }
    },
    en: {
      label: 'English',
      lang: 'en-US',
      title: 'ISkyPro Wiki',
      description: 'Usage, deployment, plugin development, and release notes for ISkyPro',
      themeConfig: {
        nav: enNav,
        sidebar: enSidebar,
        outline: {
          level: [2, 3],
          label: 'On this page'
        },
        docFooter: {
          prev: 'Previous page',
          next: 'Next page'
        },
        lastUpdated: {
          text: 'Last updated',
          formatOptions: {
            dateStyle: 'medium',
            timeStyle: 'short'
          }
        }
      }
    }
  }
})