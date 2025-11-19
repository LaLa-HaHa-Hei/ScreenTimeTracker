import { useDark, useToggle } from '@vueuse/core'

export const isDark = useDark({
    disableTransition: false,
})
export const toggleDark = useToggle(isDark)
