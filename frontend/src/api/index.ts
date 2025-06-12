import axios from 'axios';
import config from '@/config';
import type { ProcessInfo } from '@/types';

const server = axios.create({
    baseURL: config.baseApiUrl
})

export default {
    // 用进程名获取进程信息
    async GetProcessByName(name: string): Promise<ProcessInfo | null> {
        var response = await server({
            method: 'get',
            url: `/processes/name/${name}`
        })
        if (response.data === '' || response.data === null || response.data === undefined)
            return null;
        else
            return response.data as ProcessInfo;
    },
    // 修改进程的别名
    async UpdateProcessAlias(name: string, alias: string) {
        var response = await server({
            method: 'put',
            url: `/processes/name/${name}/alias`,
            data: { alias }
        })
        return response.data;
    }
}