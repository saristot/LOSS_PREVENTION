export function getUserLockFromToken(mappings: any[]): { field: string, value: any } | undefined {
    const token = localStorage.getItem('token');
    if (!token) return undefined;

    try {
        const payload = JSON.parse(atob(token.split('.')[1]));

        if (!payload.LockField || payload.LockValue == null) return undefined;

        const field = payload.LockField;
        let value: any = payload.LockValue;

        const mapping = mappings.find(m => m.Name?.toLowerCase() === field.toLowerCase());

        if (mapping) {

            console.log('mapping datatype:', mapping.DataType?.toLowerCase())
            switch (mapping.DataType?.toLowerCase()) {
                case 'integer':
                    value = parseInt(value, 10);
                    break;
                case 'decimal':
                case 'float':
                case 'double':
                    value = parseFloat(value);
                    break;
                case 'boolean':
                    value = value === 'true' || value === true;
                    break;
                case 'date':
                    const date = new Date(value);
                    if (!isNaN(date.getTime())) {
                        value = date;
                    }
                    break;
                default:
                    // Keep as string
                    break;
            }
        }

        return { field, value };

    } catch (err) {
        console.error('Failed to decode JWT lock field:', err);
        return undefined;
    }
}