angular.module("quangauthwebApp").service("loginHistoryService", ["$resource", "ENV", function ($resource, ENV) {
    var rs = $resource("", {}, {
        query: { method: "POST", url: ENV.urlApiAuth + "api/LoginHistory/GetAll" },
        queryRequest: { method: "POST", url: ENV.urlApiAuth + "api/LoginHistory/GetAllRequest" },
        read: { method: "POST", url: ENV.urlApiAuth + "api/LoginHistory/GetOneLoginHistory" },
        readByKey: { method: "POST", url: ENV.urlApiAuth + "api/LoginHistory/GetOneLoginHistoryByKey" },
        create: { method: "POST", url: ENV.urlApiAuth + "api/LoginHistory/CreateLoginHistory" },
        save: { method: "POST", url: ENV.urlApiAuth + "api/LoginHistory/UpdateLoginHistory" },
        remove: { method: "POST", url: ENV.urlApiAuth + "api/LoginHistory/DeleteLoginHistory" },
        removeRequest: { method: "POST", url: ENV.urlApiAuth + "api/LoginHistory/DeleteRequestLoginHistory" },
        isExistLoginHistory: { method: "POST", url: ENV.urlApiAuth + "api/LoginHistory/IsExistLoginHistory" }
    });
    serviceFactory = {};
    serviceFactory.listLoginHistory = function (filter, callback) {
        filter = filter ? filter : {};
        rs.query(filter).$promise.then(function (res) {
            if (callback)
                callback({ items: res.DanhSachLoginHistories, totalCount: res.TotalCount });
        })
    };
    serviceFactory.getLoginHistory = function (id, callback) {
        rs.read({ Id: id }).$promise.then(function (res) {
            callback && callback(res.LoginHistory ? res.LoginHistory : {});
        })
    };
    serviceFactory.createLoginHistory = function (item, callback) {
        rs.create(item).$promise.then(function (res) {
            callback && callback(0 == res.Status);
        })
    };
    serviceFactory.saveLoginHistory = function (item, callback) {
        rs.save(item).$promise.then(function (rs) {
            callback && callback(0 == res.Status);
        })
    }
    serviceFactory.removeLoginHistory = function (id, callback) {
        rs.remove({ Ids: [id] }).$promise.then(function (res) {
            callback && callback(0 == res.Status);
        })
    }
    serviceFactory.removeListLoginHistory = function (Ids, callback) {
        rs.remove({ Ids: Ids }).$promise.then(function (res) {
            callback && callback(0 == a.Status);
        })
    }
    return serviceFactory;    
}]);