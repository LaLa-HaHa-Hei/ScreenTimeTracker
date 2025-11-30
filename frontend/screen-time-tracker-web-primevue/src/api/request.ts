import axios from 'axios'

const server = axios.create({
    baseURL: import.meta.env.DEV ? 'http://localhost:5124/' : '/',
    timeout: 1000,
    headers: {
        'Cache-Control': 'no-cache',
        Pragma: 'no-cache',
    },
})

export default server
