export const appPaths = {
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
    privacy: {
        path: 'privacy'
    }
};

Object.freeze(appPaths);