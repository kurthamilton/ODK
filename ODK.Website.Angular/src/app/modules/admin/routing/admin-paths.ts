export const adminPaths = {
  admin: {
    path: 'admin',
    log: {
      path: ''
    }
  },
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
      create: {
        path: 'create'
      },
      path: 'properties',
      property: {
        path: ':id',
        params: {
          id: 'id'
        }
      }
    },
    subscription: {      
      path: ':id',
      params: {
        id: 'id'
      }
    },
    subscriptions: {
      create: {
        path: 'create'
      },
      path: 'subscriptions'
    }
  },
  emails: {
    default: {
      path: 'default'
    },
    emailProviders: {
      create: {
        path: 'create'
      },     
      path: 'email-providers',
      emailProvider: {
        params: {
          id: 'id'
        },
        path: ':id'
      }
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
      attendees: {
        path: 'attendees'
      },
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
      },
      sendEmail: {
        path: 'send-email'
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