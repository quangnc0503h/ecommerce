'use strict';
angular.module('quangauthwebApp').service('groupService', ['$resource', 'ENV', function ($resource, ENV) {
      
    var rs = $resource('', {}, {

        query: { method: "POST", url: ENV.urlApiAuth + "api/Group/GetAll" },
        read: { method: 'POST', url: ENV.urlApiAuth + 'api/Group/GetOneGroup' },
        create: { method: 'POST', url: ENV.urlApiAuth + 'api/Group/CreateGroup' },
        save: { method: 'POST', url: ENV.urlApiAuth + 'api/Group/UpdateGroup' },
        remove: { method: 'POST', url: ENV.urlApiAuth + 'api/Group/DeleteGroup' },
        listAll: { method: 'GET', url: ENV.urlApiAuth + 'api/Group/ListAllGroup' },
        getGroupPermissions: { method: 'POST', url: ENV.urlApiAuth + 'api/Permission/GetGroupPermissions', isArray: true },
        updateGroupPermissions: { method: 'POST', url: ENV.urlApiAuth + 'api/Permission/UpdateGroupPermissions' },
    });

    var serviceFactory = {};

    serviceFactory.listAllOptions = function (callback) {
        rs.listAll().$promise.then(function (res) {
            if (callback) {
                callback(res.DanhSachGroups);
            }
        });
    }

    serviceFactory.listGroup = function (filter, callback) {
        filter = filter ? filter : {};
        rs.query(filter).$promise.then(function (res) {
            if (callback) {
                callback({ items: res.DanhSachGroups, totalCount: res.TotalCount });
            }
        });
    }

    serviceFactory.getGroup = function (id, callback) {
        rs.read({ Id: id }).$promise.then(function (res) {
            if (callback) {
                if (res.Group) {
                    callback(res.Group);
                } else {
                    callback({});
                }
            }
        });
    }

    serviceFactory.createGroup = function (item, callback) {
        rs.create(item).$promise.then(function (res) {
            if (callback) {
                callback(res.Status == 0); // Return 0 on success
            }
        });
    }

    serviceFactory.saveGroup = function (item, callback) {
        rs.save(item).$promise.then(function (res) {
            if (callback) {
                callback(res.Status == 0); // Return 0 on success
            }
        });
    }

    serviceFactory.removeGroup = function (itemId, callback) {
        rs.remove({ Ids: [itemId] }).$promise.then(function (res) {
            if (callback) {
                callback(res.Status == 0); // Return 0 on success
            }
        });
    }

    serviceFactory.removeListGroup = function (ids, callback) {
        rs.remove({ Ids: ids }).$promise.then(function (res) {
            if (callback) {
                callback(res.Status == 0); // Return 0 on success
            }
        });
    }

    serviceFactory.getGroupPermissions = function (groupId, callback) {
        rs.getGroupPermissions({ Id: groupId }).$promise.then(function (res) {
            if (callback) {
                callback(res);
            }
        });
    }

    serviceFactory.updateGroupPermissions = function (groupId, permissionIds, callback) {
        rs.updateGroupPermissions({ GroupId: groupId, PermissionIds: permissionIds }).$promise.then(function (res) {
            if (callback) {
                callback(res);
            }
        });
    }

    return serviceFactory;
}]);