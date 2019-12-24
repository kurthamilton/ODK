export const adminPaths = {
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