'use strict';

/**
 * @ngdoc service
 * @name authclientApp.userservice
 * @description
 * # userservice
 * Service in the authclientApp.
 */
angular.module('quangauthwebApp')
  .service('userService', ['$resource', 'ENV', function ($resource, ENV) {
      
      var rs = $resource('', {}, {

          query: { method: 'POST', url: ENV.urlApiAuth + 'api/User/GetAll' },
          read: { method: 'POST', url: ENV.urlApiAuth + 'api/User/GetOneUser' },
          readCurrentUser: { method: 'POST', url: ENV.urlApiAuth + 'api/User/GetCurrentUser' },
          create: { method: 'POST', url: ENV.urlApiAuth + 'api/User/CreateUser' },
          save: { method: 'POST', url: ENV.urlApiAuth + 'api/User/UpdateUser' },
          saveCurrentUser: { method: 'POST', url: ENV.urlApiAuth + 'api/User/UpdateCurrentUser' },
          remove: { method: 'POST', url: ENV.urlApiAuth + 'api/User/DeleteUser' },
          existUserName: { method: 'POST', url: ENV.urlApiAuth + 'api/User/CheckUserName' },
          existEmail: { method: 'POST', url: ENV.urlApiAuth + 'api/User/CheckEmail' },
          getUserClientApp: { method: 'POST', url: ENV.urlApiAuth + 'api/User/GetUserClientApp' },
          updateUserApp: { method: 'POST', url: ENV.urlApiAuth + 'api/User/UpdateUserClientApp' },
          generateApiKey: { method: 'POST', url: ENV.urlApiAuth + 'api/User/GenerateUserAppApiKey' },
          changepass: { method: 'POST', url: ENV.urlApiAuth + 'api/Account/ChangePassword' },
          setpassword: { method: 'POST', url: ENV.urlApiAuth + 'api/Account/SetPassword' },
          getUserPermissions: { method: 'POST', url: ENV.urlApiAuth + 'api/Permission/GetUserPermissions', isArray: true },
          updateUserPermissions: { method: 'POST', url: ENV.urlApiAuth + 'api/Permission/UpdateUserPermissions' },
      });

      var serviceFactory = {};

      serviceFactory.listUser = function (filter, callback) {
          filter = filter ? filter : {};
          rs.query(filter).$promise.then(function (res) {
              if (callback) {
                  callback({ items: res.Data, totalCount: res.Total });
              }
          });
      }

      serviceFactory.getUser = function (id, callback) {
          rs.read({ Id: id }).$promise.then(function (res) {
              if (callback) {
                  if (res) {
                      callback(res);
                  } else {
                      callback({});
                  }
              }
          });
      }

      serviceFactory.getCurrentUser = function (callback) {
          rs.readCurrentUser().$promise.then(function (res) {
              if (callback) {
                  if (res) {
                      callback(res);
                  } else {
                      callback({});
                  }
              }
          });
      }

      serviceFactory.createUser = function (item, callback) {
          rs.create(item).$promise.then(function (res) {
              if (callback) {
                  callback(res.Status == 0); // Return 0 on success
              }
          });
      }

      serviceFactory.saveUser = function (item, callback) {
          rs.save(item).$promise.then(function (res) {
              if (callback) {
                  callback(res.Status == 0); // Return 0 on success
              }
          });
      }

      serviceFactory.saveCurrentUser = function (item, callback) {
          rs.saveCurrentUser(item).$promise.then(function (res) {
              if (callback) {
                  callback(res.Status == 0); // Return 0 on success
              }
          });
      }

      serviceFactory.removeUser = function (itemId, callback) {
          rs.remove({ Ids: [itemId] }).$promise.then(function (res) {
              if (callback) {
                  callback(res.Status == 0); // Return 0 on success
              }
          });
      }

      serviceFactory.removeListUser = function (ids, callback) {
          rs.remove({ Ids: ids }).$promise.then(function (res) {
              if (callback) {
                  callback(res.Status == 0); // Return 0 on success
              }
          });
      }

      serviceFactory.existUserName = function (username, userId, callback) {
          rs.existUserName({ UserName: username, Id: userId }).$promise.then(function (res) {
              if (callback) {
                  callback(res.Check);
              }
          });
      }

      serviceFactory.existEmail = function (email, userId, callback) {
          rs.existEmail({ Email: email, Id: userId }).$promise.then(function (res) {
              if (callback) {
                  callback(res.Check);
              }
          });
      }
      
      serviceFactory.getUserClientApp = function (userId, callback) {
          rs.getUserClientApp({ Id: userId }).$promise.then(function (res) {
              if (callback) {
                  callback(res.UserApp);
              }
          });
      }
      
      serviceFactory.updateUserApp = function (userApp, callback) {
          rs.updateUserApp(userApp).$promise.then(function (res) {
              if (callback) {
                  callback(res.Status == 0); // Return 0 on success
              }
          });
      }
      
      serviceFactory.generateApiKey = function (callback) {
          rs.generateApiKey().$promise.then(function (res) {
              if (callback) {
                  callback(res);
              }
          });
      }

      serviceFactory.changepass = function (item, callback) {
          rs.changepass(item).$promise.then(function (res) {
              if (callback) {
                  callback(true); // Return 0 on success
              }
          }, function (reason) {
              if (callback) {
                  callback(false); // Return 0 on success
              }
          });
      }

      serviceFactory.setpassword = function (item, callback) {
          rs.setpassword(item).$promise.then(function (res) {
              if (callback) {
                  callback(true); // Return 0 on success
              }
          }, function (reason) {
              if (callback) {
                  callback(false); // Return 0 on success
              }
          });
      }
      
      serviceFactory.getUserPermissions = function (userId, callback) {
          rs.getUserPermissions({ Id: userId }).$promise.then(function (res) {
              if (callback) {
                  callback(res);
              }
          });
      }

      serviceFactory.updateUserPermissions = function (userId, permissionIds, callback) {
          rs.updateUserPermissions({ UserId: userId, PermissionIds: permissionIds }).$promise.then(function (res) {
              if (callback) {
                  callback(res);
              }
          });
      }

      return serviceFactory;
  }]);
