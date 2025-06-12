import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import type { ProcessInfo } from '@/types';

export const useProcessStore = defineStore('process', () => {
  const processes = ref<Record<string, ProcessInfo>>({})
})
