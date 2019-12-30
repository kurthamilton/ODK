export const adminPaths = {
  chapter: {
    about: {
      path: 'about'
    },
    adminMembers: {
      add: {
        path: 'add'
      },
      adminMember: {
        path: ':memberId',
        params: {
          memberId: 'memberId'
        }
      },
      path: 'admin-members'
    },    
    path: 'chapter',
    payments: {
      path: 'payments'
    },
    properties: {
      path: 'properties',
      property: {
        path: ':id',
        params: {
          id: 'id'
        }
      }
    }
  },
  emails: {
    default: {
      path: 'default'
    },
    emailProvider: {
      path: 'email-provider'
    },  
    path: 'emails'
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
      events: {
        path: 'events'
      },
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