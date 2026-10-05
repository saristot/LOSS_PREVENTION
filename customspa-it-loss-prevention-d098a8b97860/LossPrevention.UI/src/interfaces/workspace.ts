// src/interfaces/workspace.ts
import type { Tab } from './tab'

export interface Workspace {
  id: string
  name: string
  description?: string
  tabs: Tab[]
}
