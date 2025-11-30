<template>
    <div class="h-full w-full">
        <DataTable
            :value="processes"
            v-model:filters="filters"
            paginator
            v-model:rows="rowsPerPage"
            :rowsPerPageOptions="[5, 10, 20, 40, 80]"
            showGridlines
            stripedRows
            removableSort
            class="mt-5"
            resizableColumns
            columnResizeMode="expand"
            dataKey="id"
            filterDisplay="menu"
        >
            <Column header="操作">
                <template #body="{ data }">
                    <Button
                        icon="pi pi-pencil"
                        variant="outlined"
                        rounded
                        class="mr-2"
                        @click="editProcess(data)"
                    />
                    <Button
                        rounded
                        icon="pi pi-trash"
                        severity="danger"
                        variant="outlined"
                        @click="confirmDeleteProcess(data)"
                    />
                </template>
                ></Column
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
            <Column field="autoUpdate" header="自动更新信息">
                <template #body="{ data }">
                    <i
                        class="pi"
                        :class="{
                            'pi-check-circle text-green-500': data.autoUpdate,
                            'pi-times-circle text-red-400': !data.autoUpdate,
                        }"
                    ></i>
                </template>
                <template #filter="{ filterModel }">
                    <Checkbox
                        v-model="filterModel.value"
                        :indeterminate="filterModel.value === null"
                        binary
                    />
                </template>
            </Column>
            <Column class="min-w-20" field="lastAutoUpdated" header="上次自动更新时间" sortable />
            <Column sortable field="executablePath" header="可执行文件路径" />
            <Column field="iconPath" header="图标路径" sortable />
        </DataTable>

        <Dialog
            v-model:visible="editProductDialog"
            :style="{ width: '450px' }"
            header="进程信息"
            :modal="true"
        >
            <template #header>
                <span class="text-lg font-bold">{{ editingProcess.name }} 进程信息</span>
            </template>
            <div class="flex flex-col gap-6">
                <div>
                    <label for="alias" class="mb-3 block font-bold">别名</label>
                    <InputText fluid id="alias" v-model="editingProcess.alias" />
                </div>
                <div>
                    <label class="mb-3 block font-bold">自动更新信息</label>
                    <ToggleSwitch v-model="editingProcess.autoUpdate" />
                </div>
                <div>
                    <label for="iconPath" class="mb-3 block font-bold">图标路径</label>
                    <InputText fluid id="iconPath" v-model="editingProcess.iconPath" />
                </div>
            </div>

            <template #footer>
                <Button
                    label="取消"
                    icon="pi pi-times"
                    severity="secondary"
                    @click="editProductDialog = false"
                />
                <Button label="保存" icon="pi pi-check" @click="saveProcess" />
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { FilterMatchMode, FilterOperator } from '@primevue/core/api'
import { getAllProcesses, getProcessIconUrl, updateProcess, deleteProcess } from '@/api'
import { ref, onBeforeMount } from 'vue'
import type { ProcessInfo } from '@/types'
import defaultFileIcon from '@/assets/defaultFileIcon.svg'

const toast = useToast()
const confirm = useConfirm()
const rowsPerPage = ref(10)
const editProductDialog = ref(false)
const editingProcess = ref<ProcessInfo>({} as ProcessInfo)
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
    autoUpdate: { value: null, matchMode: FilterMatchMode.EQUALS },
})

function editProcess(data: ProcessInfo) {
    editingProcess.value = { ...data }
    if (editingProcess.value.alias === null) editingProcess.value.alias = ''
    if (editingProcess.value.iconPath === null) editingProcess.value.iconPath = ''
    editProductDialog.value = true
}

function saveProcess() {
    if (editingProcess.value.alias === '') editingProcess.value.alias = null
    if (editingProcess.value.iconPath === '') editingProcess.value.iconPath = null

    updateProcess(editingProcess.value.id, {
        alias: editingProcess.value.alias,
        autoUpdate: editingProcess.value.autoUpdate,
        iconPath: editingProcess.value.iconPath,
    }).then((result) => {
        if (result.status == 204) {
            toast.add({ severity: 'success', summary: '成功', detail: '进程更新', life: 3000 })
        }
        loadData()
    })

    editProductDialog.value = false
}

function confirmDeleteProcess(data: ProcessInfo) {
    confirm.require({
        message: '删除进程 ' + data.name + ' 也将删除所有相关数据，是否继续？',
        header: '确认删除',
        icon: 'pi pi-info-circle',
        rejectProps: {
            label: '取消',
            severity: 'secondary',
        },
        acceptProps: {
            label: '删除',
            severity: 'danger',
        },
        accept: () => {
            deleteProcess(data.id).then((result) => {
                if (result.status == 204) {
                    toast.add({
                        severity: 'success',
                        summary: '成功',
                        detail: '进程删除',
                        life: 3000,
                    })
                }
                loadData()
            })
        },
        reject: () => {},
    })
}

async function loadData() {
    const response = await getAllProcesses()
    processes.value = response.data
}

onBeforeMount(async () => {
    await loadData()
})
</script>
