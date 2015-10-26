'use strict';
angular.module('quangcatewebApp').service('languageService', ['$resource', 'ENV', function ($resource, ENV) {
    var rs = $resource('', {}, {

        query: { method: "POST", url: ENV.urlApiCate + "api/Language/GetAll" },
        queryResources: { method: "POST", url: ENV.urlApiCate + "api/Language/GetAllResources" },
        read: { method: "POST", url: ENV.urlApiCate + "api/Language/GetOneLanguage" },
        readByKey: { method: "POST", url: ENV.urlApiCate + "api/Language/GetOneDeviceByKey" },
        create: { method: "POST", url: ENV.urlApiCate + "api/Language/CreateLanguage" },
        save: { method: "POST", url: ENV.urlApiCate + "api/Language/UpdateLanguage" },
        remove: { method: "POST", url: ENV.urlApiCate + "api/Language/DeleteLanguage" },
        removeRequest: { method: "POST", url: ENV.urlApiCate + "api/Language/DeleteResources" },
        isExistDevice: { method: "POST", url: ENV.urlApiCate + "api/Language/IsExistDevice" }
    });
    var serviceFactory = {};
    serviceFactory.listLanguage = function (filter, callback) {
        filter = filter ? filter : {};
        rs.query(filter).$promise.then(function (res) {
            if (callback) {
                callback({ items: res.Data, totalCount: res.TotalCount });
            }
        });
    }
    serviceFactory.listRequestDevice = function (filter, callback) {
        filter = filter ? filter : {};
        rs.queryRequest(filter).$promise.then(function (res) {
            if (callback) {
                callback({ items: res.Data, totalCount: res.TotalCount });
            }


        });
    }

    serviceFactory.getLanguage = function (id, callback) {
        rs.read({ Id: id }).$promise.then(function (res) {
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
        rs.readByKey({ DeviceKey: key }).$promise.then(function (res) {
            if (callback) {
                if (res.Divice) {
                    callback(res.Divice);
                } else {
                    callback({});
                }
            }
        });
    }

    serviceFactory.createLanguage = function (item, callback) {
        rs.create(item).$promise.then(function (res) {
            if (callback) {
                callback(res.Status == 0); // Return 0 on success
            }
        });
    }
    serviceFactory.saveLanguage = function (item, callback) {
        rs.save(item).$promise.then(function (res) {
            if (callback) {
                callback(res.Status == 0); // Return 0 on success
            }
        });
    }
    serviceFactory.removeLanguage = function (itemId, callback) {
        rs.remove({ Ids: [itemId] }).$promise.then(function (res) {
            if (callback) {
                callback(res.Status == 0); // Return 0 on success
            }
        });
    }

    serviceFactory.removeListLanguage = function (ids, callback) {
        rs.remove({ Ids: ids }).$promise.then(function (res) {
            if (callback) {
                callback(res.Status == 0); // Return 0 on success
            }
        });
    }
    serviceFactory.removeRequestDevice = function (itemId, callback) {
        rs.removeRequest({ Ids: [itemId] }).$promise.then(function (res) {
            if (callback) {
                callback(res.Status == 0); // Return 0 on success
            }
        });
    }
    serviceFactory.removeListRequestDevice = function (ids, callback) {
        rs.removeRequest({ Ids: ids }).$promise.then(function (res) {
            if (callback) {
                callback(res.Status == 0); // Return 0 on success
            }
        });
    }
    serviceFactory.isExistDevice = function (key, id, callback) {
        rs.isExistDevice({ DeviceKey: key, Id: Id }).$promise.then(function (res) {
            if (callback) {
                callback(res.Check); // Return 0 on success
            }
        });
    }
    return serviceFactory;
}]);