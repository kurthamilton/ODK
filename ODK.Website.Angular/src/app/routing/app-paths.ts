export const appPaths = {
  account: {
    path: 'account'
  },
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
      account: {
        path: 'account'
      },
      contact: {
        path: 'contact'
      },
      event: {
        path: 'events/:id',
        params: {
          id: 'id'
        },
        queryParams: {
          rsvp: 'rsvp'
        }
      },
      events: {
        path: 'events'
      },      
      login: {
        path: 'login'
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
    path: 'login',
    queryParams: {
      returnUrl: 'returnUrl'
    }
  },
  logout: {
    path: 'logout'
  },
  password: {
    forgotten: {
      path: 'password/forgotten'
    },
    reset: {
      path: 'password/reset',
      queryParams: {
        token: 'token'
      }
    }
  },
  privacy: {
    path: 'privacy'
  }
};

Object.freeze(appPaths);