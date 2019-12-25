export const adminPaths = {
  chapter: {
    about: {
      path: 'about'
    },
    emails: {
      path: 'emails'
    },
    path: 'chapter',
    payments: {
      path: 'payments'
    }
  },
  events: {
    create: {
      path: 'create',
      queryParams: {
        venue: 'venue'
      }
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
    path: 'chapter'
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