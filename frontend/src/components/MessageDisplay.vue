<template>
  <div class="message">
    <div class="content">
      <div class="markdown-body" v-html="renderedContent"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import MarkdownIt from 'markdown-it';
import hljs from 'highlight.js';
import mdHighlight from 'markdown-it-highlightjs';
import 'highlight.js/styles/github.css';

const props = defineProps<{
  content: string,
  isUser: boolean
}>();

const md = new MarkdownIt({
  breaks: true,
  linkify: true,
  highlight: function (str, lang) {
    if (lang && hljs.getLanguage(lang)) {
      try {
        return hljs.highlight(str, { language: lang }).value;
      } catch (__) {}
    }
    return '';
  }
}).use(mdHighlight);

const renderedContent = computed(() => {
  return md.render(props.content);
});
</script>

<style>
.message {
  display: flex;
  margin: 1rem 0;
  padding: 12px 16px;
  line-height: 1.5;
}

.content {
  flex: 1;
  min-width: 0;
  text-align: left;
}

/* Markdown 样式 */
.markdown-body {
  color: #24292e;
  line-height: 1.6;
}

.markdown-body pre {
  background-color: #f6f8fa;
  border-radius: 4px;
  padding: 16px;
  overflow: auto;
}

.markdown-body code {
  font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
  font-size: 85%;
  padding: 0.2em 0.4em;
  margin: 0;
  background-color: rgba(27, 31, 35, 0.05);
  border-radius: 4px;
}

.markdown-body pre code {
  padding: 0;
  background-color: transparent;
}
</style>
