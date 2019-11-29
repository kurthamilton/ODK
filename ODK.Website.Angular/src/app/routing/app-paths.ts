export const appPaths = {
    admin: {
        path: ':chapter/admin',
        params: {
            chapter: 'chapter'
        }
    },
    chapter: {
        childPaths: {
            about: {
                path: 'about'
            },
            blog: {
                path: 'blog'
            },
            contact: {
                path: 'contact'
            },
            event: {
                path: 'events/:id',
                params: {
                    id: 'id'
                }
            },
            events: {
                path: 'events'            
            },            
            member: {
                path: 'knitwits/:id',
                params: {
                    id: 'id'
                }
            },            
            members: {
                path: 'knitwits'
            },
            profile: {
                path: 'profile'
            },
            subscription: {
                path: 'subscription'
            }
        },        
        path: ':chapter',
        params: {
            chapter: 'chapter'
        }
    },
    home: {
        path: ''
    },
    login: {
        path: 'login'
    },
    logout: {
        path: 'logout'
    },
    privacy: {
        path: 'privacy'
    }
};

Object.freeze(appPaths);