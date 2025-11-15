import axios from 'axios'

const server = axios.create({
    baseURL: import.meta.env.DEV ? 'http://localhost:5124/api' : '/api',
    timeout: 1000,
})

export default server
