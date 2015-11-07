'use strict';
angular.module('quangauthwebApp').service('deviceService', ['$resource', 'ENV', function ($resource, ENV) {
    var rs = $resource('', {}, {

        query: { method: "POST", url: ENV.urlApiAuth + "api/Device/GetAll" },
        queryRequest: { method: "POST", url: ENV.urlApiAuth + "api/Device/GetAllRequest" },
        read: { method: "POST", url: ENV.urlApiAuth + "api/Device/GetOneDevice" },
        readByKey: { method: "POST", url: ENV.urlApiAuth + "api/Device/GetOneDeviceByKey" },
        create: { method: "POST", url: ENV.urlApiAuth + "api/Device/CreateDevice" },
        save: { method: "POST", url: ENV.urlApiAuth + "api/Device/UpdateDevice" },
        remove: { method: "POST", url: ENV.urlApiAuth + "api/Device/DeleteDevice" },
        removeRequest: { method: "POST", url: ENV.urlApiAuth + "api/Device/DeleteRequestDevice" },
        isExistDevice: { method: "POST", url: ENV.urlApiAuth + "api/Device/IsExistDevice" },
        listAllClients: { method: "GET", url: ENV.urlApiAuth + "api/Device/GetAllClients", isArray: true }
    });
    var serviceFactory = {};
    serviceFactory.listDevice = function(filter, callback) {
        filter = filter ? filter : {};
        rs.query(filter).$promise.then(function(res) {
            if (callback) {
                callback({ items: res.DanhSachDevices, totalCount: res.TotalCount });
            }
        });
    }
    serviceFactory.listRequestDevice = function (filter, callback) {
        filter = filter ? filter : {};
        rs.queryRequest(filter).$promise.then(function (res) {
            if (callback) {
                callback({ items: res.DanhSachRequestDevices, totalCount: res.TotalCount });
            }

            
        });
    }

    serviceFactory.getDevice = function (id, callback) {
        rs.read({ Id: id }).$promise.then(function(res) {
            if (callback) {
                if (res.Divice) {
                    callback(res.Divice);
                } else {
                    callback({});
                }
            }
        });
    }

    serviceFactory.getDeviceByKey = function (key, callback) {
        rs.readByKey({ DeviceKey: key }).$promise.then(function(res) {
            if (callback) {
                if (res.Divice) {
                    callback(res.Divice);
                } else {
                    callback({});
                }
            }
        });
    }

    serviceFactory.createDevice = function (item, callback) {
        rs.create(item).$promise.then(function(res) {
            if (callback) {
                callback(res.Status == 0); // Return 0 on success
            }
        });
    }
    serviceFactory.saveDevice = function (item, callback) {
        rs.save(item).$promise.then(function(res) {
            if (callback) {
                callback(res.Status == 0); // Return 0 on success
            }
        });
    }
    serviceFactory.removeDevice = function (itemId, callback) {
        rs.remove({ Ids: [itemId] }).$promise.then(function(res) {
            if (callback) {
                callback(res.Status == 0); // Return 0 on success
            }
        });
    }

    serviceFactory.removeListDevice = function(ids, callback) {
        rs.remove({ Ids: ids }).$promise.then(function(res) {
            if (callback) {
                callback(res.Status == 0); // Return 0 on success
            }
        });
    }
    serviceFactory.removeRequestDevice = function (itemId, callback) {
        rs.removeRequest({ Ids: [itemId] }).$promise.then(function(res) {
            if (callback) {
                callback(res.Status == 0); // Return 0 on success
            }
        });
    }
    serviceFactory.removeListRequestDevice = function(ids, callback) {
        rs.removeRequest({ Ids: ids }).$promise.then(function(res) {
            if (callback) {
                callback(res.Status == 0); // Return 0 on success
            }
        });
    }
    serviceFactory.isExistDevice = function(key, id, callback) {
        rs.isExistDevice({ DeviceKey: key, Id: Id }).$promise.then(function(res) {
            if (callback) {
                callback(res.Check); // Return 0 on success
            }
        });
    }
    serviceFactory.listAllClients = function (callback) {
        rs.listAllClients().$promise.then(function (res) {
            if (callback)
                callback(res);
            else
                callback({});
            
        })
    }
    return serviceFactory;
}]);