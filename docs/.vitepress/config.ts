import { defineConfig } from 'vitepress'

const base = process.env.VITEPRESS_BASE ?? '/ISkyPro.Wiki/'

const zhNav = [
  {
    text: '指南',
    items: [
      { text: '快速开始', link: '/guide/getting-started' },
      { text: 'QQBot 事件配置', link: '/guide/qqbot-events' },
      { text: 'Webhook 与反向代理', link: '/guide/webhook-and-proxy' }
    ]
  },
  {
    text: '插件 SDK',
    items: [
      { text: '插件概览', link: '/plugins/' },
      { text: '旧插件与新插件', link: '/plugins/legacy-vs-modern' },
      { text: '快速实现', link: '/plugins/sdk-quick-start' },
      { text: '事件详解', link: '/plugins/events' },
      { text: '发布插件', link: '/plugins/publishing' },
      { text: '故障排查', link: '/plugins/troubleshooting' },
      { text: 'SDK 下载', link: '/plugins/downloads' }
    ]
  },
  { text: 'FAQ', link: '/faq' },
  { text: '更新日志', link: '/changelog/' },
  { text: 'GitHub', link: 'https://github.com/Zephdyn/ISkyPro.Wiki' }
]

const zhSidebar = [
  {
    text: '指南',
    items: [
      { text: '项目简介', link: '/' },
      { text: '快速开始', link: '/guide/getting-started' },
      { text: 'QQBot 事件配置', link: '/guide/qqbot-events' },
      { text: 'Webhook 与反向代理', link: '/guide/webhook-and-proxy' }
    ]
  },
  {
    text: '插件 SDK',
    items: [
      { text: '插件概览', link: '/plugins/' },
      { text: '旧插件与新插件', link: '/plugins/legacy-vs-modern' },
      { text: '快速实现', link: '/plugins/sdk-quick-start' },
      { text: '事件详解', link: '/plugins/events' },
      { text: '发布插件', link: '/plugins/publishing' },
      { text: '故障排查', link: '/plugins/troubleshooting' },
      { text: 'SDK 下载', link: '/plugins/downloads' }
    ]
  },
  {
    text: '支持',
    items: [
      { text: 'FAQ', link: '/faq' }
    ]
  },
  {
    text: '发布',
    items: [
      { text: '更新日志', link: '/changelog/' },
      { text: '2.0.0-preview.3', link: '/changelog/v2-0-0-preview-3' },
      { text: '2.0.0-preview.2', link: '/changelog/v2-0-0-preview-2' },
      { text: '2.0.0-preview.1', link: '/changelog/v2-0-0-preview-1' }
    ]
  }
]

const enNav = [
  {
    text: 'Guide',
    items: [
      { text: 'Getting Started', link: '/en/guide/getting-started' },
      { text: 'QQBot Events', link: '/en/guide/qqbot-events' },
      { text: 'Webhook and Proxy', link: '/en/guide/webhook-and-proxy' }
    ]
  },
  {
    text: 'Plugin SDK',
    items: [
      { text: 'Overview', link: '/en/plugins/' },
      { text: 'Legacy and Modern', link: '/en/plugins/legacy-vs-modern' },
      { text: 'Quick Start', link: '/en/plugins/sdk-quick-start' },
      { text: 'Events', link: '/en/plugins/events' },
      { text: 'Publishing', link: '/en/plugins/publishing' },
      { text: 'Troubleshooting', link: '/en/plugins/troubleshooting' },
      { text: 'SDK Downloads', link: '/en/plugins/downloads' }
    ]
  },
  { text: 'FAQ', link: '/en/faq' },
  { text: 'Changelog', link: '/en/changelog/' },
  { text: 'GitHub', link: 'https://github.com/Zephdyn/ISkyPro.Wiki' }
]

const enSidebar = [
  {
    text: 'Guide',
    items: [
      { text: 'Overview', link: '/en/' },
      { text: 'Getting Started', link: '/en/guide/getting-started' },
      { text: 'QQBot Events', link: '/en/guide/qqbot-events' },
      { text: 'Webhook and Proxy', link: '/en/guide/webhook-and-proxy' }
    ]
  },
  {
    text: 'Plugin SDK',
    items: [
      { text: 'Overview', link: '/en/plugins/' },
      { text: 'Legacy and Modern', link: '/en/plugins/legacy-vs-modern' },
      { text: 'Quick Start', link: '/en/plugins/sdk-quick-start' },
      { text: 'Events', link: '/en/plugins/events' },
      { text: 'Publishing', link: '/en/plugins/publishing' },
      { text: 'Troubleshooting', link: '/en/plugins/troubleshooting' },
      { text: 'SDK Downloads', link: '/en/plugins/downloads' }
    ]
  },
  {
    text: 'Support',
    items: [
      { text: 'FAQ', link: '/en/faq' }
    ]
  },
  {
    text: 'Release',
    items: [
      { text: 'Changelog', link: '/en/changelog/' },
      { text: '2.0.0-preview.3', link: '/en/changelog/v2-0-0-preview-3' },
      { text: '2.0.0-preview.2', link: '/en/changelog/v2-0-0-preview-2' },
      { text: '2.0.0-preview.1', link: '/en/changelog/v2-0-0-preview-1' }
    ]
  }
]

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
