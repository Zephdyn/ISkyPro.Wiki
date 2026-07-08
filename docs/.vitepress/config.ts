import { defineConfig } from 'vitepress'

const base = process.env.VITEPRESS_BASE ?? '/ISkyPro.Wiki/'

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
    },
    nav: [
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
    ],
    sidebar: [
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
    ],
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
})
