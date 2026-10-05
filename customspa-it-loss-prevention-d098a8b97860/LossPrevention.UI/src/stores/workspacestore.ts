import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '../api/api'
import type { Workspace } from '../interfaces/workspace'

export const useWorkspaceStore = defineStore('workspace', () => {
    const workspaces = ref<Workspace[]>([])
    const activeWorkspace = ref<Workspace | null>(null)
    const loading = ref(false)

    async function fetchWorkspaces(search = '') {
        loading.value = true
        try {
            const res = await api.get<Workspace[]>('/workspaces')
            if (search) {
                workspaces.value = res.data.filter(ws =>
                    ws.name.toLowerCase().includes(search.toLowerCase())
                )
            } else {
                workspaces.value = res.data
            }
        } catch (err) {
            console.error('Error fetching workspaces', err)
        } finally {
            loading.value = false
        }
    }

    async function addWorkspace(workspace: Partial<Workspace>) {
        const res = await api.post<Workspace>('/workspaces', workspace)
        workspaces.value.push(res.data)
    }

    async function updateWorkspace(id: string, updated: Partial<Workspace>) {
        try {
            const payload = {
                id,
                name: updated.name ?? '',
                description: updated.description ?? '',
                tabs: Array.isArray(updated.tabs) ? updated.tabs : []
            };

            // Follow the exact same pattern as your working rules endpoint
            await api.put(`/workspaces/${id}`, payload);

            // Instead of processing response data, just refetch everything
            await fetchWorkspaces();

            // Update active workspace if needed
            if (activeWorkspace.value?.id === id) {
                const updatedWorkspace = workspaces.value.find(w => w.id === id);
                if (updatedWorkspace) {
                    activeWorkspace.value = updatedWorkspace;
                }
            }

            return true;
        } catch (error) {
            console.error('Error updating workspace:', error);
            throw error;
        }
    }




    async function deleteWorkspace(id: string) {
        await api.delete(`/workspaces/${id}`)
        workspaces.value = workspaces.value.filter(w => w.id !== id)
    }

    function setActiveWorkspace(workspace: Workspace) {
        activeWorkspace.value = workspace
        localStorage.setItem('activeWorkspaceId', workspace.id)
        console.log('Active workspace set to:', workspace)
    }

    async function fetchWorkspaceFromLocalStorageById() {
        const workspaceId = localStorage.getItem('activeWorkspaceId')
        if (workspaceId) {
            try {
                const res = await api.get<Workspace>(`/workspaces/${workspaceId}`)
                activeWorkspace.value = res.data
            } catch (err) {
                console.error('Could not fetch workspace by ID:', err)
            }
        }
    }


    return {
        workspaces,
        loading,
        fetchWorkspaces,
        addWorkspace,
        updateWorkspace,
        deleteWorkspace,
        activeWorkspace,
        setActiveWorkspace,
        fetchWorkspaceFromLocalStorageById
    }
})
