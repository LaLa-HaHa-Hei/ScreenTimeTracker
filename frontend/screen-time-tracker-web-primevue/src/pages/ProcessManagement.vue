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
                <template #body="{ data }">
                    <span>{{ data.alias }}</span>
                    <Button
                        icon="pi pi-pen-to-square"
                        variant="text"
                        size="small"
                        @click="editAlias(data)"
                    />
                </template>
                <template #filter="{ filterModel }">
                    <InputText v-model="filterModel.value" type="text" placeholder="搜索别名" />
                </template>
            </Column>
            <Column field="description" header="描述" sortable></Column>
            <Column field="autoUpdate" header="自动更新信息" sortable>
                <template #body="{ data }">
                    <ToggleSwitch v-model="data.autoUpdate" @change="onAutoUpdateChange(data)" />
                </template>
            </Column>
            <Column class="min-w-20" field="lastAutoUpdated" header="上次自动更新时间" sortable />
            <Column sortable field="executablePath" header="可执行文件路径" />
            <Column field="iconPath" header="图标路径" sortable>
                <template #body="{ data }">
                    <span>{{ data.iconPath }}</span>
                    <Button
                        icon="pi pi-pen-to-square"
                        variant="text"
                        size="small"
                        @click="editIconPath(data)"
                    />
                </template>
                ></Column
            >
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

        <Dialog v-model:visible="editAliasDialogVisiable" modal header="编辑别名">
            <div class="flex items-center">
                <InputGroup>
                    <InputText class="flex-auto" v-model="editedAlias" type="text" />
                    <InputGroupAddon>
                        <Button
                            icon="pi pi-times-circle"
                            @click="editedAlias = ''"
                            severity="secondary"
                        />
                    </InputGroupAddon>
                </InputGroup>
            </div>
            <div class="mt-4 flex justify-end gap-2">
                <Button
                    type="button"
                    label="取消"
                    severity="secondary"
                    @click="editAliasDialogVisiable = false"
                />
                <Button type="button" label="保存" @click="saveAlias" />
            </div>
        </Dialog>

        <Dialog v-model:visible="editIconPathDialogVisiable" modal header="编辑图标路径">
            <div class="flex items-center">
                <InputGroup>
                    <InputText class="flex-auto" v-model="editedIconPath" type="text" />
                    <InputGroupAddon>
                        <Button
                            icon="pi pi-times-circle"
                            @click="editedIconPath = ''"
                            severity="secondary"
                        />
                    </InputGroupAddon>
                </InputGroup>
            </div>
            <div class="mt-4 flex justify-end gap-2">
                <Button
                    type="button"
                    label="取消"
                    severity="secondary"
                    @click="editIconPathDialogVisiable = false"
                />
                <Button type="button" label="保存" @click="saveIconPath" />
            </div>
        </Dialog>

        <ConfirmDialog />
    </div>
</template>

<script setup lang="ts">
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import ToggleSwitch from 'primevue/toggleswitch'
import Image from 'primevue/image'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import InputGroup from 'primevue/inputgroup'
import ConfirmDialog from 'primevue/confirmdialog'
import { useConfirm } from 'primevue/useconfirm'
import InputGroupAddon from 'primevue/inputgroupaddon'
import { FilterMatchMode, FilterOperator } from '@primevue/core/api'
import {
    getAllProcesses,
    getProcessIconUrl,
    updateProcessAlias,
    updateProcessAutoUpdate,
    updateProcessIconPath,
    deleteProcessById,
} from '@/api'
import { ref, onMounted } from 'vue'
import type { ProcessInfo } from '@/types'
import defaultFileIcon from '@/assets/defaultFileIcon.svg'

const confirm = useConfirm()
const processes = ref<ProcessInfo[]>([])
const editAliasDialogVisiable = ref<boolean>(false)
const editedAlias = ref<string>('')
const editIconPathDialogVisiable = ref<boolean>(false)
const editedIconPath = ref<string>('')
let editedData: ProcessInfo
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

function onAutoUpdateChange(data: ProcessInfo) {
    updateProcessAutoUpdate(data.id, data.autoUpdate).then((result) => {
        if (result.status !== 204) {
            editedData.autoUpdate = !data.autoUpdate
        }
    })
}

function editIconPath(data: ProcessInfo) {
    editedData = data
    editedIconPath.value = data.iconPath ?? ''
    editIconPathDialogVisiable.value = true
}

function saveIconPath() {
    editIconPathDialogVisiable.value = false
    const newIconPath = editedIconPath.value == '' ? null : editedIconPath.value
    updateProcessIconPath(editedData.id, newIconPath).then((result) => {
        if (result.status == 204) {
            editedData.iconPath = newIconPath
        }
    })
}

function editAlias(data: ProcessInfo) {
    editedData = data
    editedAlias.value = data.alias ?? ''
    editAliasDialogVisiable.value = true
}

function saveAlias() {
    editAliasDialogVisiable.value = false
    const newAlias = editedAlias.value == '' ? null : editedAlias.value
    updateProcessAlias(editedData.id, newAlias).then((result) => {
        if (result.status == 204) {
            editedData.alias = newAlias
        }
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
