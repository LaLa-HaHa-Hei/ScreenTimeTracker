<template>
    <div class="h-full w-full">
        <Button @click="loadProcesses" label="刷新进程列表" />

        <DataTable
            :value="processes"
            v-model:filters="filters"
            paginator
            :rows="5"
            :rowsPerPageOptions="[5, 10, 20, 50]"
            showGridlines
            stripedRows
            removableSort
            class="mt-5"
            resizableColumns
            columnResizeMode="expand"
            dataKey="id"
            filterDisplay="menu"
        >
            <Column header="图标">
                <template #body="{ data }">
                    <Image
                        :src="data.iconPath ? getProcessIconUrl(data.id) : defaultFileIcon"
                        alt="Icon"
                        imageClass="min-w-8 w-8 h-8 min-h-8"
                    />
                </template>
            </Column>
            <Column field="name" header="进程名" sortable>
                <template #filter="{ filterModel }">
                    <InputText v-model="filterModel.value" type="text" placeholder="搜索进程名" />
                </template>
            </Column>
            <Column field="alias" sortable header="别名">
                <template #filter="{ filterModel }">
                    <InputText v-model="filterModel.value" type="text" placeholder="搜索别名" />
                </template>
            </Column>
            <Column field="description" header="描述" sortable></Column>
            <Column field="autoUpdate" header="自动更新信息" sortable>
                <template #body="{ data }">
                    <span>{{ data.autoUpdate ? '是' : '否' }}</span>
                </template>
            </Column>
            <Column class="min-w-20" field="lastAutoUpdated" header="上次自动更新时间" sortable />
            <Column sortable field="executablePath" header="可执行文件路径" />
            <Column field="iconPath" header="图标路径" sortable />
            <Column header="操作">
                <template #body="{ data }">
                    <Button
                        rounded
                        icon="pi pi-trash"
                        severity="danger"
                        variant="outlined"
                        @click="deleteProcess(data)"
                    />
                </template>
                ></Column
            >
        </DataTable>
        <ConfirmDialog />
    </div>
</template>

<script setup lang="ts">
import { useConfirm } from 'primevue/useconfirm'
import { FilterMatchMode, FilterOperator } from '@primevue/core/api'
import { getAllProcesses, getProcessIconUrl, updateProcessById, deleteProcessById } from '@/api'
import { ref, onMounted } from 'vue'
import type { ProcessInfo } from '@/types'
import defaultFileIcon from '@/assets/defaultFileIcon.svg'

const confirm = useConfirm()
const processes = ref<ProcessInfo[]>([])
const filters = ref({
    name: {
        operator: FilterOperator.AND,
        constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },
    alias: {
        operator: FilterOperator.AND,
        constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }],
    },
})

function deleteProcess(data: ProcessInfo) {
    confirm.require({
        message: '删除进程 ' + data.name + ' 也将删除所有相关数据，是否继续？',
        header: '确认删除',
        icon: 'pi pi-info-circle',
        rejectProps: {
            label: '取消',
            severity: 'secondary',
            outlined: true,
        },
        acceptProps: {
            label: '删除',
            severity: 'danger',
        },
        accept: () => {
            deleteProcessById(data.id).then((result) => {
                if (result.status == 204) {
                    processes.value = processes.value.filter((p) => p.id !== data.id)
                }
            })
        },
        reject: () => {},
    })
}

async function loadProcesses() {
    const response = await getAllProcesses()
    processes.value = response.data
}

onMounted(async () => {
    await loadProcesses()
})
</script>
