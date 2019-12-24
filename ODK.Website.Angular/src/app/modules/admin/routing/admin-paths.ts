export const adminPaths = {
  chapter: {
    path: 'chapter'
  },
  events: {
    create: {
      path: 'create'
    },
    event: {
      invites: {
        path: 'invites'
      },
      path: ':id',
      params: {
        id: 'id'
      },
      responses: {
        path: 'responses'
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
      image: {
        path: 'image'
      },
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
      events: {
        path: 'events'
      },
      path: ':id',
      params: {
        id: 'id'
      }
    }
  }
};

Object.freeze(adminPaths);