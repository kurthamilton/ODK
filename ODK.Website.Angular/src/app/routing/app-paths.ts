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
      activateAccount: {
        path: 'account/activate',
        queryParams: {
          token: 'token'
        }
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
      join: {
        path: 'join'
      },
      login: {
        path: 'login'
      },
      logout: {
        path: 'logout'
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
      profile: {
        password: {
          change: {
            path: 'password/change'
          }
        },
        path: 'profile',
        subscription: {
          path: 'subscription'
        }
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