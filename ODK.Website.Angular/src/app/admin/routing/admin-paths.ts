export const adminPaths = {
    chapter: {
        path: ''
    },
    events: {
        create: {
            path: 'create'
        },
        event: {
            path: ':id',
            params: {
                id: 'id'
            }
        },
        path: 'events'
    },
    home: {
        path: ''
    },
    login: {
        path: 'login'
    }
};

Object.freeze(adminPaths);