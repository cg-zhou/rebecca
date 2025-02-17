import Home from '../views/Home.vue'
import Settings from '../views/Settings.vue'
import About from '../views/About.vue'
import MediaLibrary from '../views/MediaLibrary.vue'
import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    name: 'home',
    component: Home,
    meta: { title: '首页' }
  },
  {
    path: '/settings',
    name: 'settings',
    component: Settings,
    meta: { title: '设置' }
  },
  {
    path: '/media-library',
    name: 'media-library',
    component: MediaLibrary,
    meta: { title: '媒体库' }
  },
  {
    path: '/about',
    name: 'about',
    component: About,
    meta: { title: '关于' }
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

// 更新页面标题
router.beforeEach((to, _from, next) => {
  document.title = to.meta.title ? `${to.meta.title} - Rebecca` : 'Rebecca'
  next()
})

export default router