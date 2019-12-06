export const adminPaths = {
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
    members: {
        path: 'members',
        member: {
            path: ':id',
            params: {
                id: 'id'
            }
        }
    },
    venues: {
        create: {
            path: 'create'
        },
        path: 'venues',
        venue: {
            path: ':id',
            params: {
                id: 'id'
            }
        }
    }
};

Object.freeze(adminPaths);