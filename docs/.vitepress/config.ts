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
      { text: '指南', link: '/guide/getting-started' },
      { text: '更新日志', link: '/changelog/' },
      { text: '发布与更新检测', link: '/reference/update-check' }
    ],
    sidebar: [
      {
        text: '开始',
        items: [
          { text: '项目简介', link: '/' },
          { text: '快速开始', link: '/guide/getting-started' }
        ]
      },
      {
        text: '发布',
        items: [
          { text: '更新日志', link: '/changelog/' },
          { text: '2.0.0-preview.1', link: '/changelog/v2-0-0-preview-1' },
          { text: '更新检测约定', link: '/reference/update-check' }
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
