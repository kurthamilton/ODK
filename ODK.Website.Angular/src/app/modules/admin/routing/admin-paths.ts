export const adminPaths = {  

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

  chapter: {    
    emails: {
      email: {
        path: ':type',
        params: {
          type: 'type'
        }      
      },
      path: 'emails'
    },    
    links: {
      path: 'links'
    },
    path: 'chapter',    
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
    questions: {
      create: {
        path: 'create'
      },
      path: 'questions',
      question: {
        path: ':id',
        params: {
          id: 'id'
        }
      }
    }
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

  media: {
    path: 'media'
  },
  
  members: {
    import: {
      path: 'import'
    },
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

  subscriptions: {
    create: {
      path: 'create'
    },
    path: 'subscriptions',
    subscription: {
      path: ':id',
      params: {
        id: 'id'
      }
    },
  },

  superAdmin: {
    emails: {
      email: {
        path: ':type',
        params: {
          type: 'type'
        }
      },
      path: 'emails'
    },
    errorLog: {
      path: 'error-log'
    },
    memberEmails: {
      path: 'members'
    },    
    path: 'super-admin',
    paymentSettings: {
      path: 'payment-settings'
    },
    socialMedia: {
      instagram: {
        path: 'social-media/instagram'
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