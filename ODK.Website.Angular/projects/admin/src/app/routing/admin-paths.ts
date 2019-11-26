export const adminPaths = {
    chapter: {
        path: ':chapter',
        params: {
            chapter: 'chapter'
        }
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
    }
};

Object.freeze(adminPaths);