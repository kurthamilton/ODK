export const accountPaths = {  
  activate: {
    path: 'activate',
    queryParams: {
      token: 'token'
    }
  },
  delete: {
    path: 'delete'
  },
  emails: {
    path: 'emails'
  },
  join: {
    path: 'join'
  }, 
  logout: {
    path: 'logout'
  },
  password: {
    change: {
      path: 'password/change'
    },
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
  subscription: {
    path: 'subscription'
  },
  updateEmailAddress: {
    path: 'update-email-address',
    queryParams: {
      token: 'token'
    }
  },
};

Object.freeze(accountPaths);