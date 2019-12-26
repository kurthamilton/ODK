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
    emailProvider: {
      path: 'email-provider'
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