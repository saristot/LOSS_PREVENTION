export interface User {
    id?: string
    username: string
    password: string
    email: string
    firstName: string
    lastName: string
    registrationDate: string
    isActive: boolean
    roles: string[]
    lockField?: string // e.g., "storeId"
    lockValue?: string // e.g., "1"
}