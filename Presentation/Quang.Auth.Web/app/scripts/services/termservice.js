'use strict';

/**
 * @ngdoc service
 * @name authclientApp.termservice
 * @description
 * # termservice
 * Service in the authclientApp.
 */
angular.module('quangauthwebApp')
  .service('termService', ['$resource', 'ENV', function ($resource, ENV) {
      
      var rs = $resource('', {}, {

          query: { method: 'POST', url: ENV.urlApiAuth + 'api/Term/GetAll' },
          read: { method: 'POST', url: ENV.urlApiAuth + 'api/Term/GetOneTerm' },
          create: { method: 'POST', url: ENV.urlApiAuth + 'api/Term/CreateTerm' },
          save: { method: 'POST', url: ENV.urlApiAuth + 'api/Term/UpdateTerm' },
          remove: { method: 'POST', url: ENV.urlApiAuth + 'api/Term/DeleteTerm' },
          listRole: { method: 'GET', url: ENV.urlApiAuth + 'api/Term/ListRoleOptions' },
          getMissingTerms: { method: 'GET', url: ENV.urlApiAuth + 'api/Term/GetMissingTerms'},
          getGrantUserTerms: { method: 'POST', url: ENV.urlApiAuth + 'api/Term/GetGrantTermsUser' },
          updateUserGrant: { method: 'POST', url: ENV.urlApiAuth + 'api/Term/UpdateUserGrant' },
          getGrantGroupTerms: { method: 'POST', url: ENV.urlApiAuth + 'api/Term/GetGrantTermsGroup'},
          getGrantedUsersByTerm: { method: 'POST', url: ENV.urlApiAuth + 'api/Term/GetGrantedUsersByTerm' },
          updateGroupGrant: { method: 'POST', url: ENV.urlApiAuth + 'api/Term/UpdateGroupGrant' },
      });

      var serviceFactory = {};

      serviceFactory.listRoleOptions = function (callback) {
          rs.listRole().$promise.then(function (res) {
              if (callback) {
                  callback(res.Data);
              }
          });
      }

      serviceFactory.listTerm = function (filter, callback) {
          filter = filter ? filter : {};
          rs.query(filter).$promise.then(function (res) {
              if (callback) {
                 // console.log(res.Data);
                  callback({ items: res.Data, totalCount: res.Total });
              }
          });
      }

      serviceFactory.getGrantUserTerms = function (id, callback) {
          rs.getGrantUserTerms({ Id: id }).$promise.then(function (res) {
              if (callback) {
                  callback(res.Data);
              }
          });
      }

      serviceFactory.getGrantedUsersByTerm = function (id, callback) {
          rs.getGrantedUsersByTerm({ Id: id }).$promise.then(function (users) {
              if (callback) {
                 // console.log(users);
                  callback(users.Data);
              }
          });
      }

      serviceFactory.getGrantGroupTerms = function (id, callback) {
          rs.getGrantGroupTerms({ Id: id }).$promise.then(function (res) {
              if (callback) {
                  callback(res.Data);
              }
          });
      }

      serviceFactory.getTerm = function (id, callback) {
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

      serviceFactory.createTerm = function (item, callback) {
          rs.create(item).$promise.then(function (res) {
              if (callback) {
                  callback(res.Status == 0); // Return 0 on success
              }
          });
      }

      serviceFactory.saveTerm = function (item, callback) {
          rs.save(item).$promise.then(function (res) {
              if (callback) {
                  callback(res.Status == 0); // Return 0 on success
              }
          });
      }

      serviceFactory.removeTerm = function (itemId, callback) {
          rs.remove({ Ids: [itemId] }).$promise.then(function (res) {
              if (callback) {
                  callback(res.Status == 0); // Return 0 on success
              }
          });
      }

      serviceFactory.removeListTerm = function (ids, callback) {
          rs.remove({ Ids: ids }).$promise.then(function (res) {
              if (callback) {
                  callback(res.Status == 0); // Return 0 on success
              }
          });
      }

      serviceFactory.getMissingTerms = function (callback) {
          rs.getMissingTerms().$promise.then(function (res) {
              if (callback) {
                  //console.log(res);
                  callback(res.Data); // Return 0 on success
              }
          });
      }

      serviceFactory.updateUserGrant = function (userId, grants, callback) {
          rs.updateUserGrant({ UserId: userId, UserGrants: grants }).$promise.then(function (res) {
              if (callback) {
                  callback(res.Status == 0); // Return 0 on success
              }
          });
      }

      serviceFactory.updateGroupGrant = function (groupId, grants, callback) {
          rs.updateGroupGrant({ GroupId: groupId, GroupGrants: grants }).$promise.then(function (res) {
              if (callback) {
                  callback(res.Status == 0); // Return 0 on success
              }
          });
      }

      return serviceFactory;
  }]);
