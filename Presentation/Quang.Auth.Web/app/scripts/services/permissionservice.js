'use strict';

/**
 * @ngdoc service
 * @name authclientApp.permissionservice
 * @description
 * # permissionservice
 * Service in the authclientApp.
 */
angular.module('quangauthwebApp')
  .service('permissionService', ['$resource', 'ENV', function ($resource, ENV) {
      
      var rs = $resource('', {}, {

          query: { method: 'POST', url: ENV.urlApiAuth + 'api/Permission/GetAll' },
          read: { method: 'POST', url: ENV.urlApiAuth + 'api/Permission/GetOnePermission' },
          create: { method: 'POST', url: ENV.urlApiAuth + 'api/Permission/CreatePermission' },
          save: { method: 'POST', url: ENV.urlApiAuth + 'api/Permission/UpdatePermission' },
          remove: { method: 'POST', url: ENV.urlApiAuth + 'api/Permission/DeletePermission' },
          listAll: { method: 'GET', url: ENV.urlApiAuth + 'api/Permission/ListAllPermission' },
          getPermissionGrants: { method: 'POST', url: ENV.urlApiAuth + 'api/Permission/GetPermissionGrants' },
          updatePermissionGrants: { method: 'POST', url: ENV.urlApiAuth + 'api/Permission/UpdatePermissionGrants' },
      });

      var serviceFactory = {};

      serviceFactory.listAllOptions = function (callback) {
          rs.listAll().$promise.then(function (res) {
              if (callback) {
                  callback(res.Data);
              }
          });
      }

      serviceFactory.listPermission = function (filter, callback) {
          filter = filter ? filter : {};
          rs.query(filter).$promise.then(function (res) {
              if (callback) {
                  callback({ items: res.Data, totalCount: res.Total });
              }
          });
      }

      serviceFactory.getPermission = function (id, callback) {
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

      serviceFactory.createPermission = function (item, callback) {
          rs.create(item).$promise.then(function (res) {
              if (callback) {
                  callback(res.Status == 0); // Return 0 on success
              }
          });
      }

      serviceFactory.savePermission = function (item, callback) {
          rs.save(item).$promise.then(function (res) {
              if (callback) {
                  callback(res.Status == 0); // Return 0 on success
              }
          });
      }

      serviceFactory.removePermission = function (itemId, callback) {
          rs.remove({ Ids: [itemId] }).$promise.then(function (res) {
              if (callback) {
                  callback(res.Status == 0); // Return 0 on success
              }
          });
      }

      serviceFactory.removeListPermission = function (ids, callback) {
          rs.remove({ Ids: ids }).$promise.then(function (res) {
              if (callback) {
                  callback(res.Status == 0); // Return 0 on success
              }
          });
      }

      serviceFactory.getPermissionGrants = function (permissionId, callback) {
          rs.getPermissionGrants({ Id: permissionId }).$promise.then(function (res) {
              if (callback) {
                  callback({ allowItems: res.AllowGrants, denyItems: res.DenyGrants });
              }
          });
      }

      serviceFactory.updatePermissionGrants = function (permissionId, allowItems, denyItems, callback) {
          rs.updatePermissionGrants({ PermissionId: permissionId, AllowGrants: allowItems, DenyGrants: denyItems }).$promise.then(function (res) {
              if (callback) {
                  callback(res.Status == 0); // Return 0 on success
              }
          });
      }
      

      return serviceFactory;
  }]);
