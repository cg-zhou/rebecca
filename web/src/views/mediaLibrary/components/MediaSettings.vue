<template>
  <div class="media-settings">
    <el-card class="settings-card">
      <template #header>
        <div class="card-header">
          <span>媒体库路径</span>
          <el-button type="primary" @click="saveConfig">保存设置</el-button>
        </div>
      </template>

      <div class="paths-config">
        <div class="path-list">
          <div v-for="(path, index) in config.libraryPaths" :key="index" class="path-item">
            <el-input v-model="config.libraryPaths[index]" placeholder="请输入媒体库路径">
              <template #append>
                <el-button @click="selectFolder(index)">选择</el-button>
                <el-button @click="removePath(index)">删除</el-button>
              </template>
            </el-input>
          </div>
        </div>
        <el-button @click="addPath">添加路径</el-button>
      </div>
    </el-card>

    <el-card class="settings-card">
      <template #header>
        <div class="card-header">
          <span>TMDB API 设置</span>
          <el-button type="primary" @click="saveTmdbConfig">保存设置</el-button>
        </div>
      </template>

      <div class="tmdb-config">
        <el-form label-position="left" label-width="140px">
          <el-form-item label="API Key 类型">
            <el-radio-group v-model="tmdbConfig.apiKeyType">
              <el-radio label="v3">API Key (v3)</el-radio>
              <el-radio label="v4">Bearer Token (v4)</el-radio>
            </el-radio-group>
            <div class="help-text">
              根据您创建的 TMDB API 密钥类型选择对应的认证方式
            </div>
          </el-form-item>
          
          <el-form-item :label="tmdbConfig.apiKeyType === 'v3' ? 'API Key' : 'Bearer Token'">
            <el-input v-model="tmdbConfig.bearerToken" :placeholder="tmdbConfig.apiKeyType === 'v3' ? '请输入 TMDB API Key' : '请输入 TMDB Bearer Token'" show-password />
            <div class="help-text">
              <el-link type="primary" href="https://www.themoviedb.org/settings/api" target="_blank">
                获取 TMDB API 密钥
              </el-link>
            </div>
          </el-form-item>
          
          <el-form-item label="API 地址">
            <el-input v-model="tmdbConfig.baseApiUrl" placeholder="API 地址" />
          </el-form-item>
          
          <el-form-item label="图片地址">
            <el-input v-model="tmdbConfig.baseImageUrl" placeholder="图片地址" />
          </el-form-item>
          
          <el-form-item label="语言">
            <el-select v-model="tmdbConfig.language" placeholder="请选择语言">
              <el-option label="中文 (简体)" value="zh-CN" />
              <el-option label="中文 (繁体)" value="zh-TW" />
              <el-option label="英语 (美国)" value="en-US" />
              <el-option label="日语" value="ja-JP" />
              <el-option label="韩语" value="ko-KR" />
            </el-select>
          </el-form-item>
          
          <el-form-item>
            <el-button type="success" @click="testTmdbApi">测试连接</el-button>
          </el-form-item>
        </el-form>
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'

interface Config {
  libraryPaths: string[]
  tmdbApiKey: string
  tmdbLanguage: string
}

const config = reactive<Config>({
  libraryPaths: [],
  tmdbApiKey: '',
  tmdbLanguage: 'zh-CN'
})

const loadConfig = async () => {
  try {
    const response = await fetch('/api/medialibrary/config')
    const data = await response.json()
    Object.assign(config, data)
  } catch (error) {
    console.error('加载配置失败:', error)
    ElMessage.error('加载配置失败')
  }
}

const saveConfig = async () => {
  try {
    const response = await fetch('/api/medialibrary/config', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(config)
    })
    
    if (response.ok) {
      ElMessage.success('设置已保存')
    } else {
      throw new Error('保存失败')
    }
  } catch (error) {
    console.error('保存配置失败:', error)
    ElMessage.error('保存配置失败')
  }
}

const addPath = () => {
  config.libraryPaths.push('')
}

const removePath = (index: number) => {
  config.libraryPaths.splice(index, 1)
}

const selectFolder = async (index: number) => {
  try {
    const response = await fetch('/api/system/selectFolder', {
      method: 'POST'
    })
    const data = await response.json()
    if (data.path) {
      config.libraryPaths[index] = data.path
    }
  } catch (error) {
    console.error('选择文件夹失败:', error)
    ElMessage.error('选择文件夹失败')
  }
}

// TMDB 配置
const tmdbConfig = reactive({
  bearerToken: '',
  baseApiUrl: 'http://api.tmdb.org/3',
  baseImageUrl: 'https://image.tmdb.org/t/p/original',
  language: 'zh-CN',
  apiKeyType: 'v3'
})

onMounted(async () => {
  await loadConfig()
  await loadTmdbSettings()
})

// 加载 TMDB 配置
const loadTmdbSettings = async () => {
  try {
    const response = await fetch('/api/settings/tmdb')
    const data = await response.json()

    if (data) {
      Object.assign(tmdbConfig, data)
    }
  } catch (error) {
    console.error('加载 TMDB 配置失败:', error)
    ElMessage.error('加载 TMDB 配置失败')
  }
}

// 保存 TMDB 配置
const saveTmdbConfig = async () => {
  if (!tmdbConfig.bearerToken) {
    ElMessage.warning(`请输入 TMDB ${tmdbConfig.apiKeyType === 'v3' ? 'API Key' : 'Bearer Token'}`)
    return
  }
  
  try {
    const response = await fetch('/api/settings/tmdb', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(tmdbConfig)
    })

    if (response.ok) {
      ElMessage.success('TMDB 配置保存成功')
    } else {
      ElMessage.error('TMDB 配置保存失败')
    }
  } catch (error) {
    console.error('保存 TMDB 配置失败:', error)
    ElMessage.error('保存 TMDB 配置失败')
  }
}

// 测试 TMDB API
const testTmdbApi = async () => {
  if (!tmdbConfig.bearerToken) {
    ElMessage.warning(`请先输入 TMDB ${tmdbConfig.apiKeyType === 'v3' ? 'API Key' : 'Bearer Token'}`)
    return
  }
  
  try {
    const response = await fetch('/api/settings/tmdb/test', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(tmdbConfig)
    })

    if (response.ok) {
      const result = await response.json()
      if (result.success) {
        ElMessage.success('TMDB API 连接测试成功！')
      } else {
        ElMessage.error(`TMDB API 连接测试失败: ${result.message}`)
      }
    } else {
      ElMessage.error('TMDB API 连接测试失败')
    }
  } catch (error) {
    console.error('测试 TMDB API 失败:', error)
    ElMessage.error('测试 TMDB API 失败')
  }
}
</script>

<style scoped>
.settings-card {
  margin-bottom: 1rem;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.paths-config {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.path-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.help-text {
  margin-top: 5px;
  font-size: 12px;
  color: #909399;
}

.tmdb-config {
  max-width: 600px;
}
</style>
